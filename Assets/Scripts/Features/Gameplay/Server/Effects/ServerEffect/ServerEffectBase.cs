using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class ServerEffectBase
{
    public bool canTouchLauncher;
    public bool canTouchMate;
    public bool canTouchEnemy;

    public bool launcherMustBeInZone;
    
    public abstract List<IPacket> Apply(Entity launcher, List<Entity> entities, Vector2Int targetPos, GameState gameState, Map map);

    protected List<Entity> GetFilteredEntities(Entity launcher, List<Entity> entities)
    {
        List<Entity> filteredEntities = entities
            .Where(entity =>
                entity != null &&
                (canTouchLauncher || launcher != entity) &&
                (canTouchMate || launcher.Team != entity.Team) &&
                (canTouchEnemy || launcher.Team == entity.Team))
            .ToList();

        if (canTouchLauncher && !launcherMustBeInZone && filteredEntities.All(e => e.Id != launcher.Id))
        {
            filteredEntities.Add(launcher);
        }

        return filteredEntities;
    }

    protected static Node WalkUntilBlocked(Vector2Int origin, Vector2Int direction, int maxTiles, GameState gameState, Map map)
    {
        bool isDiagonal = Mathf.Abs(direction.x) + Mathf.Abs(direction.y) == 2;

        Node node = Node.Invalid;
        for (int i = 1; i <= maxTiles; i++)
        {
            Node tmp = map.GetNode(origin + direction * i);
            if (tmp is not { NodeType: NodeType.Ground } || gameState.GetEntityByGridPosition(tmp.GridPosition) != null) break;

            if (isDiagonal)
            {
                Node node1 = map.GetNode(origin + direction * i - new Vector2Int(direction.x, 0));
                Node node2 = map.GetNode(origin + direction * i - new Vector2Int(0, direction.y));
                if (node1 is not { NodeType: NodeType.Ground } || gameState.GetEntityByGridPosition(node1.GridPosition) != null) break;
                if (node2 is not { NodeType: NodeType.Ground } || gameState.GetEntityByGridPosition(node2.GridPosition) != null) break;
            }

            node = tmp;
        }

        return node;
    }
}
