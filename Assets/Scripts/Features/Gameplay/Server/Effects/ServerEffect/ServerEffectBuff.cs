using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ServerEffectBuff : ServerEffectBase
{
    [SerializeField] private Buff buff;

    public override List<IPacket> Apply(Entity launcher, List<Entity> entities, Vector2Int targetPos, GameState gameState, Map map)
    {
        List<IPacket> packets = new();
        List<Entity> filteredEntities = GetFilteredEntities(launcher, entities);
        
        foreach (Entity entity in filteredEntities)
        {
            // Test if the buff can be added
            if (entity.Buffs.Count(b => b.Buff.Id == buff.Id) < buff.MaxStack)
            {
                PacketBuff packetBuff = new()
                {
                    TargetId = entity.Id,
                    LauncherId = launcher.Id,
                    BuffId = buff.Id
                };
                packetBuff.Apply(gameState, map);
                packets.Add(packetBuff);
            }
        }

        return packets;
    }
}
