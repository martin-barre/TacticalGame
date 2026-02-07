using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ServerEffectTeleport : ServerEffectBase
{
    [SerializeField] private bool canSwap;

    public override List<IPacket> Apply(Entity launcher, List<Entity> entities, Vector2Int targetPos, GameState gameState, Map map)
    {
        List<IPacket> packets = new();
        Node node = map.GetNode(targetPos);
        
        if (!canSwap && gameState.GetEntityByGridPosition(node.GridPosition) != null)
            return packets;

        PacketTeleport packetTeleport = new()
        {
            TargetId = launcher.Id,
            GridPosition = targetPos
        };
        packetTeleport.Apply(gameState, map);
        packets.Add(packetTeleport);
        
        return packets;
    }
}
