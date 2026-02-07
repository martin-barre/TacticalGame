using System.Collections.Generic;
using UnityEngine;

public enum FovMode
{
    NORMAL,
    SQUARE,
    LINE,
    DIAGONAL,
    LINE_DIAGONAL
}

public static class FOV
{
    public static List<Node> GetDisplacement(Entity entity, Spell spell, GameState gameState, Map map)
    {
        List<Node> nodes = new();
        if (spell.poMin == 0)
        {
            nodes.Add(map.GetNode(entity.GridPosition));
        }

        if (spell.fovMode == FovMode.NORMAL)
        {
            if (spell.xRay) nodes.AddRange(DoXRay(entity, spell, gameState, map));
            else nodes.AddRange(DoFOV(entity, spell, gameState, map));
        }
        else if (spell.fovMode == FovMode.SQUARE)
        {
            if (spell.xRay) nodes.AddRange(DoXRay(entity, spell, gameState, map));
            else nodes.AddRange(DoFOV(entity, spell, gameState, map));
        }
        else if (spell.fovMode == FovMode.LINE)
        {
            nodes.AddRange(DoLineOnly(entity, spell, gameState, map));
        }
        else if (spell.fovMode == FovMode.DIAGONAL)
        {
            nodes.AddRange(DoDiagonalOnly(entity, spell, gameState, map));
        }
        else if (spell.fovMode == FovMode.LINE_DIAGONAL)
        {
            nodes.AddRange(DoLineOnly(entity, spell, gameState, map));
            nodes.AddRange(DoDiagonalOnly(entity, spell, gameState, map));
        }
        return nodes;
    }

    private static List<Node> DoFOV(Entity entity, Spell spell, GameState gameState, Map map)
    {
        Vector3Int[] directions = {
            new(1, 1, 0),   // UP UP RIGHT
            new(1, 1, 1),   // UP UP LEFT
            new(1, -1, 0),  // RIGHT RIGHT UP
            new(1, -1, 1),  // RIGHT RIGHT DOWN
            new(-1, -1, 1), // DOWN DOWN RIGHT
            new(-1, -1, 0), // DOWN DOWN LEFT
            new(-1, 1, 0),  // LEFT LEFT DOWN
            new(-1, 1, 1),  // LEFT LEFT UP
        };

        List<Node> nodes = new();
        foreach (Vector3Int direction in directions)
        {
            nodes.AddRange(DoFOVRecursive(entity, spell, 0, 0.0f, 1.0f, direction, gameState, map));
        }
        return nodes;
    }

    private static List<Node> DoFOVRecursive(Entity entity, Spell spell, int startLigne, float angleMin, float angleMax, Vector3Int direction, GameState gameState, Map map)
    {
        List<Node> nodes = new();

        for (int x = startLigne; x <= spell.poMax; x++)
        {
            bool blocked = false;
            for (int y = 0; y <= spell.poMax; y++)
            {
                if (angleMin > angleMax) return nodes;
                if (x == 0 && y == 0) continue;
                if (y == 0 && direction.y > 0 && direction.x > 0) continue;
                if (y == 0 && direction.y < 0 && direction.x < 0) continue;
                if (spell.fovMode != FovMode.SQUARE && x + y > spell.poMax) continue;

                // RECUPERATION DU NODE
                int realX = direction.z == 0 ? x : y;
                int realY = direction.z == 0 ? y : x;
                realX = entity.GridPosition.x + realX * direction.x;
                realY = entity.GridPosition.y + realY * direction.y;
                Node node = map.GetNode(new Vector2Int(realX, realY));

                float angle = (float)y / x;

                // DESACTIVE LA CASE CAR ELLE N'EST PAS VISIBLE
                bool active = !(angle < angleMin || angle > angleMax);

                if (node.NodeType is NodeType.Invalid or NodeType.Wall || gameState.GetEntityByGridPosition(node.GridPosition) != null)
                {
                    if (!blocked)
                    {
                        float newAngleMax = (y - .5f) / (x + .5f);
                        nodes.AddRange(DoFOVRecursive(entity, spell, x + 1, angleMin, newAngleMax, direction, gameState, map));
                        blocked = true;
                    }
                    float newAngleMin = (y + .5f) / (x - .5f);
                    angleMin = Mathf.Max(angleMin, newAngleMin);
                }
                else
                {
                    blocked = false;
                }

                if (angle > angleMax) break;

                if (active && node is { NodeType: NodeType.Ground })
                {
                    if (spell.fovMode == FovMode.SQUARE && Mathf.Abs(x) < spell.poMin && Mathf.Abs(y) < spell.poMin) continue;
                    if (spell.fovMode != FovMode.SQUARE && x + y < spell.poMin) continue;
                    if (direction.z == 0 && x == y) continue;
                    if (!spell.canLaunchOnEntity && gameState.GetEntityByGridPosition(node.GridPosition) != null) continue;
                    nodes.Add(node);
                }
            }
        }
        return nodes;
    }

    private static List<Node> DoXRay(Entity entity, Spell spell, GameState gameState, Map map)
    {
        List<Node> nodes = new();

        for (int x = -spell.poMax; x <= spell.poMax; x++)
        {
            int absX = Mathf.Abs(x);
            for (int y = -spell.poMax; y <= spell.poMax; y++)
            {
                int absY = Mathf.Abs(y);
                if (spell.fovMode == FovMode.SQUARE && absX < spell.poMin && absY < spell.poMin) continue;
                if (spell.fovMode != FovMode.SQUARE && (absX + absY < spell.poMin || absX + absY > spell.poMax)) continue;
                Node node = map.GetNode(entity.GridPosition + new Vector2Int(x, y));
                if (node is not { NodeType: NodeType.Ground }) continue;
                if (!spell.canLaunchOnEntity && gameState.GetEntityByGridPosition(node.GridPosition) != null) continue;
                nodes.Add(node);
            }
        }

        return nodes;
    }

    private static List<Node> DoLineOnly(Entity entity, Spell spell, GameState gameState, Map map)
    {
        Vector2Int[] directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

        List<Node> nodes = new();
        foreach (Vector2Int direction in directions)
        {
            for (int i = 1; i <= spell.poMax; i++)
            {
                Node node = map.GetNode(entity.GridPosition + direction * i);

                if (node.NodeType is NodeType.Ground && i >= spell.poMin && i <= spell.poMax)
                {
                    if (!spell.canLaunchOnEntity && gameState.GetEntityByGridPosition(node.GridPosition) != null) continue;
                    nodes.Add(node);
                }

                if (!spell.xRay)
                {
                    if (node.NodeType is NodeType.Wall || (node.NodeType != NodeType.Invalid && gameState.GetEntityByGridPosition(node.GridPosition) != null))
                    {
                        break;
                    }
                }
            }
        }
        return nodes;
    }

    private static List<Node> DoDiagonalOnly(Entity entity, Spell spell, GameState gameState, Map map)
    {
        Vector2Int[] directions = {
            new(1, 1),
            new(-1, 1),
            new(-1, -1),
            new(1, -1)
        };

        List<Node> nodes = new();
        foreach (Vector2Int direction in directions)
        {
            for (int i = 1; i <= spell.poMax; i++)
            {
                int realX = entity.GridPosition.x + i * direction.x;
                int realY = entity.GridPosition.y + i * direction.y;
                Node node = map.GetNode(Vector2Int.CeilToInt(new Vector2(realX, realY)));

                if (node.NodeType == NodeType.Ground && i >= spell.poMin && i <= spell.poMax)
                {
                    if (!spell.canLaunchOnEntity && gameState.GetEntityByGridPosition(node.GridPosition) != null) continue;
                    nodes.Add(node);
                }

                if (!spell.xRay && (node.NodeType is NodeType.Invalid or NodeType.Wall || gameState.GetEntityByGridPosition(node.GridPosition) != null))
                {
                    break;
                }
            }
        }
        return nodes;
    }
}
