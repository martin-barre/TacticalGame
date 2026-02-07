using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameServerAction
{
    public static PacketMove Move(Vector2Int gridPosition, GameState gameState, Map map)
    {
        Entity entity = gameState.CurrentEntity;
        List<Node> path = BFS.GetPath(entity.GridPosition, gridPosition, gameState, map);

        PacketMove packetMove = new() {
            TargetId = entity.Id,
            PmCost = path.Count,
            Path = path.Select(n => n.GridPosition).ToArray()
        };
        
        packetMove.Apply(gameState, map);

        return packetMove;
    }
    
    public static List<IPacket> LaunchSpell(int spellId, Vector2Int targetPos, GameState gameState, Map map)
    {
        List<IPacket> clientEffects = new();
        Spell spell = SpellDatabase.GetById(spellId);
        if (spell == null) return clientEffects;
        
        Entity entity = gameState.CurrentEntity;
        if (entity.Pa < spell.paCost) return clientEffects;
        
        Race race = RaceDatabase.GetById(entity.Race.Id);
        if (race.Spells.All(s => s.Id != spellId)) return clientEffects;
        
        List<Node> fovNodes = FOV.GetDisplacement(entity, spell, gameState, map);
        if(fovNodes.All(n => n.GridPosition != targetPos))
            return clientEffects;

        PacketLaunchSpell packetLaunchSpell = new() { LauncherId = entity.Id, SpellId = spellId, TargetPos = targetPos };
        packetLaunchSpell.Apply(gameState, map);
        clientEffects.Add(packetLaunchSpell);
        clientEffects.AddRange(spell.Launch(entity, targetPos, gameState, map));

        return clientEffects;
    }
    
    public static List<IPacket> NextTurn(GameState gameState, Map map)
    {
        List<IPacket> packets = new();

        PacketNextTurn packetNextTurn = new();
        packetNextTurn.Apply(gameState, map);
        packets.Add(packetNextTurn);
        
        // APPLY START TURN BUFF EFFECTS
        List<ActiveBuff> serverEffects = gameState.CurrentEntity.Buffs
            .Where(b => b.Buff.StartTurnEffects.Any())
            .ToList();

        foreach (ActiveBuff activeBuff in serverEffects)
        {
            foreach (ServerEffectBase startTurnEffect in activeBuff.Buff.StartTurnEffects)
            {
                packets.AddRange(startTurnEffect.Apply(
                    activeBuff.Launcher,
                    new List<Entity> { gameState.CurrentEntity },
                    gameState.CurrentEntity.GridPosition,
                    gameState,
                    map));
            }
        }
        
        return packets;
    }
}
