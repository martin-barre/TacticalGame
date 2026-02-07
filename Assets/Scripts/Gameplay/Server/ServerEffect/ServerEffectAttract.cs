using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ServerEffectAttract : ServerEffectBase
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
        List<IPacket> clientEffects = new();
        List<Entity> filteredEntities = GetFilteredEntities(launcher, entities);

        Vector2Int launcherPosition = centerPoint == CenterPoint.LAUNCHER ? launcher.GridPosition : targetPos;
        
        foreach (Entity entity in filteredEntities)
        {
            Vector2Int targetPosition = entity.GridPosition;
            Vector2Int direction = Utils.GridDirection(targetPosition, launcherPosition);
            bool isDiagonal = Mathf.Abs(direction.x) + Mathf.Abs(direction.y) == 2;

            Node node = Node.Invalid;
            for (int i = 1; i <= nbOfTile; i++)
            {
                Node tmp = map.GetNode(targetPosition + direction * i);
                if (tmp is not { NodeType: NodeType.Ground } || gameState.GetEntityByGridPosition(tmp.GridPosition) != null) break;

                if (isDiagonal)
                {
                    Node node1 = map.GetNode(targetPosition + direction * i - new Vector2Int(direction.x, 0));
                    Node node2 = map.GetNode(targetPosition + direction * i - new Vector2Int(0, direction.y));
                    if (node1 is not { NodeType: NodeType.Ground } || gameState.GetEntityByGridPosition(node1.GridPosition) != null) break;
                    if (node2 is not { NodeType: NodeType.Ground } || gameState.GetEntityByGridPosition(node2.GridPosition) != null) break;
                }

                node = tmp;
            }

            if (node.NodeType != NodeType.Invalid && node.GridPosition != entity.GridPosition)
            {
                PacketMove packetMove = new()
                {
                    TargetId = entity.Id,
                    PmCost = 0,
                    Path = new[] { node.GridPosition }
                };
                packetMove.Apply(gameState, map);
                clientEffects.Add(packetMove);
            }
        }

        return clientEffects;
    }
}
