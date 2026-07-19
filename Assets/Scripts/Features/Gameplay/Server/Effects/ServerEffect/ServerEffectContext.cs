using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServerEffectContext
{
    public static PacketSummonEntity? SpawnEntity(GameState gameState, Map map, Team team, int raceId, Vector2Int gridPosition, bool isPlayer, Entity summoner = null)
    {
        Node node = map.GetNode(gridPosition);

        if (node.NodeType != NodeType.Ground || gameState.GetEntityByGridPosition(node.GridPosition) != null) return null;

        PacketSummonEntity packetSummonEntity = new()
        {
            EntityId = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0),
            Team = team,
            RaceId = raceId,
            GridPosition = gridPosition,
            IsPlayer = isPlayer,
            SummonerId = summoner?.Id ?? -1
        };
        packetSummonEntity.Apply(gameState, map);

        return packetSummonEntity;
    }

    public static  IList<IPacket> KillEntity(GameState gameState, Map map, Entity entity)
    {
        List<IPacket> packets = new();

        PacketKillEntity packetKillEntity = new() { TargetId = entity.Id };
        packetKillEntity.Apply(gameState, map);
        packets.Add(packetKillEntity);

        if (entity == gameState.CurrentEntity)
        {
            packets.AddRange(GameServerAction.NextTurn(gameState, map));
        }

        return packets;
    }
}
