using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ServerEffectPush : ServerEffectBase
{
    private enum CenterPoint
    {
        LAUNCHER,
        TARGET
    }

    [SerializeField] private int nbOfTile;
    [SerializeField] private CenterPoint centerPoint;

    public override List<IPacket> Apply(Entity launcher, List<Entity> entities, Vector2Int targetPos, GameState gameState, Map map)
    {
        List<IPacket> packets = new();
        List<Entity> filteredEntities = GetFilteredEntities(launcher, entities);
        
        Vector2Int launcherPosition = centerPoint == CenterPoint.LAUNCHER ? launcher.GridPosition : targetPos;
        
        foreach (Entity entity in filteredEntities.AsEnumerable().Reverse())
        {
            Vector2Int targetPosition = entity.GridPosition;
            Vector2Int direction = Utils.GridDirection(launcherPosition, targetPosition);

            Node node = WalkUntilBlocked(targetPosition, direction, nbOfTile, gameState, map);

            if (node.NodeType != NodeType.Invalid && node.GridPosition != entity.GridPosition)
            {
                PacketMove packetMove = new()
                {
                    TargetId = entity.Id,
                    PmCost = 0,
                    Path = new[] { node.GridPosition }
                };
                packetMove.Apply(gameState, map);
                packets.Add(packetMove);
            }
        }

        return packets;
    }
}
