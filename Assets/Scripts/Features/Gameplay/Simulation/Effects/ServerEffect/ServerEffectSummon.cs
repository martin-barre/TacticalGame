using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ServerEffectSummon : ServerEffectBase
{
    public Race Race;

    public override List<IPacket> Apply(Entity launcher, List<Entity> entities, Vector2Int targetPos, GameState gameState, Map map)
    {
        List<IPacket> packets = new ();

        Node node = map.GetNode(targetPos);
        if (node.NodeType == NodeType.Ground && gameState.GetEntityByGridPosition(node.GridPosition) == null)
        {
            PacketSummonEntity? summonedEntity = GameManagerServer.Instance.SpawnEntity(launcher.Team, Race.Id, targetPos, launcher.IsPlayer, gameState, launcher);
            if (summonedEntity.HasValue)
            {
                packets.Add(summonedEntity.Value);
            }
        }

        return packets.ToList();
    }
}