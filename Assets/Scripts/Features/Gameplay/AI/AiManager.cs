using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AiManager
{
    public enum AiBehaviorType
    {
        Aggressive,
        Distance,
        Support
    }

    public static List<IPacket> PlayOnAction(Entity entity, GameState gameState, Map map, AiBehaviorType behavior)
    {
        List<IAiAction> bestActions = GetBestActions(entity, gameState, map, behavior);
        
        List<IPacket> packets = new List<IPacket>();
        foreach (IAiAction action in bestActions)
        {
            packets.AddRange(action.Apply(gameState, map));
        }

        packets.AddRange(GameServerAction.NextTurn(gameState, map));
        return packets;
    }

    private static List<IAiAction> GetBestActions(Entity entity, GameState gameState, Map map, AiBehaviorType behavior)
    {
        // 1. Prepare Targets and Distance Map
        // This map tells us "How far is the nearest interesting target?" for any cell.
        List<Vector2Int> targets = GetTargets(entity, gameState, behavior);
        int[] distanceMap = BFS.GetDistanceMap(targets, map);
        int mapWidth = map.Width;

        List<IAiAction> bestActions = new List<IAiAction>();
        int bestScore = int.MinValue;

        // 2. Get Reachable Moves (respecting obstacles)
        // We include the current position (Cost 0) in this list.
        List<BFS.ReachableNode> reachableNodes = BFS.GetReachableCells(entity.GridPosition, entity.Pm, gameState, map);

        Vector2Int originalPos = entity.GridPosition;
        int originalPm = entity.Pm;

        // 3. Iterate through all possible move destinations
        foreach (BFS.ReachableNode node in reachableNodes)
        {
            // --- SIMULATE MOVE ---
            // We manually update GameState to avoid the overhead of generating PacketMove and full pathfinding
            gameState.MoveOrSwapEntity(entity, node.Position);
            entity.Pm -= node.Cost;

            // Calculate Base Score from Positioning
            int currentScore = GetPositionScore(node.Position, distanceMap, mapWidth, behavior);
            
            // We also add score for unused PM/PA (heuristic to prefer efficiency or conservation?)
            // Original AI: score += entity.Pa * 3 + entity.Pm * 2;
            currentScore += entity.Pa * 3 + entity.Pm * 2;

            IAiAction bestSpellAction = null;
            int bestSpellScoreToAdd = 0;

            // --- SIMULATE SPELLS (from new position) ---
            // Iterate over all spells available to the entity
            foreach (Spell spell in entity.Race.Spells)
            {
                if (entity.Pa < spell.paCost) continue;

                // Get valid target cells for this spell from the CURRENT simulated position
                List<Node> validTargetNodes = FOV.GetDisplacement(entity, spell, gameState, map);

                foreach (Node targetNode in validTargetNodes)
                {
                    // Try to launch the spell and see what happens
                    List<IPacket> spellPackets = GameServerAction.LaunchSpell(spell.Id, targetNode.GridPosition, gameState, map);
                    
                    if (spellPackets.Count > 0)
                    {
                        int spellScore = EvaluateSpellPackets(spellPackets, entity, gameState);
                        
                        if (spellScore > bestSpellScoreToAdd)
                        {
                            bestSpellScoreToAdd = spellScore;
                            bestSpellAction = new AiActionLaunchSpell 
                            { 
                                SpellId = spell.Id, 
                                GridPosition = targetNode.GridPosition 
                            };
                        }

                        // UNDO SPELL EFFECTS
                        // We must revert the state to test the next spell/target cleanly
                        for (int i = spellPackets.Count - 1; i >= 0; i--)
                        {
                            spellPackets[i].Undo(gameState, map);
                        }
                    }
                }
            }

            // Combine Move Score + Best Spell Score
            int totalScore = currentScore + bestSpellScoreToAdd;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestActions.Clear();
                // If we moved, add move action
                if (node.Position != originalPos)
                {
                    bestActions.Add(new AiActionMove { GridPosition = node.Position });
                }
                // If we found a good spell, add spell action
                if (bestSpellAction != null)
                {
                    bestActions.Add(bestSpellAction);
                }
            }

            // --- UNDO MOVE ---
            gameState.MoveOrSwapEntity(entity, originalPos);
            entity.Pm = originalPm;
        }
        
        // Fallback: If no actions generated (shouldn't happen if we include current pos), stay put.
        if (bestActions.Count == 0 && reachableNodes.Count > 0)
        {
             // Do nothing (NextTurn will be added by PlayOnAction)
        }

        return bestActions;
    }

    private static List<Vector2Int> GetTargets(Entity entity, GameState gameState, AiBehaviorType behavior)
    {
        List<Vector2Int> targets = new List<Vector2Int>();
        
        // Find relevant entities based on behavior
        foreach (Entity other in gameState.Entities)
        {
            if (other.Id == entity.Id) continue; // Ignore self

            bool isEnemy = other.Team != entity.Team;
            bool isAlly = !isEnemy;

            if (behavior == AiBehaviorType.Aggressive || behavior == AiBehaviorType.Distance)
            {
                if (isEnemy) targets.Add(other.GridPosition);
            }
            else if (behavior == AiBehaviorType.Support)
            {
                if (isAlly) targets.Add(other.GridPosition);
            }
        }
        return targets;
    }

    private static int GetPositionScore(Vector2Int pos, int[] distanceMap, int mapWidth, AiBehaviorType behavior)
    {
        int index = pos.x + pos.y * mapWidth;
        if (index < 0 || index >= distanceMap.Length) return -1000;
        
        int distance = distanceMap[index];
        
        // Unreachable target
        if (distance == int.MaxValue) return -100; 

        // Scoring Logic
        switch (behavior)
        {
            case AiBehaviorType.Aggressive:
                // Prefer being close (0 distance is best)
                // Score decreases as distance increases
                return -distance * 10; 

            case AiBehaviorType.Distance:
                // Prefer keeping a safe range (e.g., 4-6 cells)
                // If too close (<3), penalty.
                // If too far (>7), penalty.
                int optimalRange = 5;
                int diff = Mathf.Abs(distance - optimalRange);
                return -diff * 8;

            case AiBehaviorType.Support:
                // Prefer being close to allies
                return -distance * 10;

            default:
                return 0;
        }
    }

    private static int EvaluateSpellPackets(List<IPacket> packets, Entity me, GameState gameState)
    {
        int score = 0;

        foreach (IPacket packet in packets)
        {
            switch (packet)
            {
                case PacketDamage p:
                {
                    Entity target = gameState.GetEntityById(p.TargetId);
                    if (target != null)
                    {
                        bool isEnemy = target.Team != me.Team;
                        // Positive score for damaging enemies, negative for damaging allies
                        score += (isEnemy ? p.Value : -p.Value) * 10;
                    }
                    break;
                }
                case PacketHeal p:
                {
                    Entity target = gameState.GetEntityById(p.TargetId);
                    if (target != null)
                    {
                        bool isAlly = target.Team == me.Team;
                        // Positive score for healing allies, negative for healing enemies
                        score += (isAlly ? p.Value : -p.Value) * 10;
                    }
                    break;
                }
                case PacketKillEntity p:
                {
                    Entity target = gameState.GetEntityById(p.TargetId);
                    if (target != null)
                    {
                        bool isEnemy = target.Team != me.Team;
                        // Huge bonus for killing enemy, huge penalty for killing ally
                        score += isEnemy ? 1000 : -1000;
                    }
                    break;
                }
                case PacketBuff p:
                    // Simple heuristic: Buffing is generally good if on ally
                    // We might need to check if buff is positive/negative, but assuming positive for now
                     Entity targetBuff = gameState.GetEntityById(p.TargetId);
                     if (targetBuff != null)
                     {
                         bool isAlly = targetBuff.Team == me.Team;
                         score += isAlly ? 50 : -50;
                     }
                    break;
            }
        }
        return score;
    }
}
