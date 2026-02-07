using System.Collections.Generic;
using System.Linq;
using ZLinq;

public static class AiManager
{
    public static List<IPacket> PlayOnAction(Entity entity, GameState gameState, Map map)
    {
        IList<IAiAction> actions = GetBestActions(entity.Id, gameState, map);
        List<IPacket> packets = actions.SelectMany(a => a.Apply(gameState, map)).ToList();
        packets.AddRange(GameServerAction.NextTurn(gameState, map));
        return packets;
    }
    
    private static IList<IAiAction> GetBestActions(int entityId, GameState gameState, Map map)
    {
        List<IAiAction> bestActions = new();
        int bestActionsScore = int.MinValue;
        List<AiActionMove> moveActions = GetAllMoveActions(entityId, gameState, map);
        foreach (AiActionMove moveAction in moveActions)
        {
            List<IPacket> movePackets = moveAction.Apply(gameState, map);
            
            int score = CalculScore(movePackets, entityId, gameState, map);

            if (score > bestActionsScore)
            {
                bestActionsScore = score;
                bestActions.Clear();
                bestActions.AddRange(new List<IAiAction> { moveAction });
            }
            
            List<AiActionLaunchSpell> spellActions = GetAllSpellActions(entityId, gameState, map);
            foreach (AiActionLaunchSpell aiActionLaunchSpell in spellActions)
            {
                List<IPacket> spellPackets = aiActionLaunchSpell.Apply(gameState, map);
                
                int score2 = CalculScore(movePackets.Concat(spellPackets), entityId, gameState, map);

                if (score2 > bestActionsScore)
                {
                    bestActionsScore = score2;
                    bestActions.Clear();
                    bestActions.AddRange(new List<IAiAction> { moveAction, aiActionLaunchSpell });
                }
                
                spellPackets.ForEach(spellPacket => spellPacket.Undo(gameState, map));
            }
            movePackets.ForEach(movePacket => movePacket.Undo(gameState, map));
        }

        return bestActions;
    }

    private static int CalculScore(IEnumerable<IPacket> packets, int entityId, GameState gameState, Map map)
    {
        Entity entity = gameState.GetEntityById(entityId);
        int score = 0;
        score += entity.Pa * 3;
        score += entity.Pm * 2;
        foreach (IPacket packet in packets)
        {
            score += packet switch
            {
                PacketDamage p => (gameState.GetEntityById(p.TargetId).Team == entity.Team ? -p.Value : p.Value) * 100,
                PacketHeal p => (gameState.GetEntityById(p.TargetId).Team == entity.Team ? p.Value : -p.Value) * 100,
                PacketKillEntity p => gameState.GetEntityById(p.TargetId).Team == entity.Team ? -500 : 500,
                _ => 0
            };
        }

        int minDistance = gameState.Entities
            .AsValueEnumerable()
            .Where(e => e.Team != entity.Team)
            .Select(e => BFS.GetPath(entity.GridPosition, e.GridPosition, gameState, map, true))
            .Where(path => path != null)
            .Select(path => path.Count)
            .OrderBy(d => d)
            .First();

        score += minDistance * 3;
        
        return score;
    }

    private static List<AiActionMove> GetAllMoveActions(int entityId, GameState gameState, Map map)
    {
        Entity entity = gameState.GetEntityById(entityId);
        List<AiActionMove> actions = BFS.GetDisplacement(entity.GridPosition, entity.Pm, gameState, map)
            .Select(n => new AiActionMove { GridPosition = n.GridPosition })
            .ToList();
        actions.Add(new AiActionMove { GridPosition = entity.GridPosition });
        return actions;
    }
    
    private static List<AiActionLaunchSpell> GetAllSpellActions(int entityId, GameState gameState, Map map)
    {
        Entity entity = gameState.GetEntityById(entityId);
        return entity.Race.Spells
            .AsValueEnumerable()
            .Where(s => entity.Pa >= s.paCost)
            .SelectMany(s => FOV.GetDisplacement(entity, s, gameState, map)
                .AsValueEnumerable()
                .Select(n => new AiActionLaunchSpell { SpellId = s.Id, GridPosition = n.GridPosition }))
            .ToList();
    }
}
