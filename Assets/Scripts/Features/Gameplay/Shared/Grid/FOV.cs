using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public enum FovMode
{
    Normal,
    Square,
    Line,
    Diagonal,
    LineDiagonal
}

public static class FOV
{
    public static List<Node> GetDisplacement(Entity entity, Spell spell, GameState gameState, Map map, bool forceXRay = false)
    {
        List<Node> nodes = new();
        if (spell.poMin == 0)
        {
            nodes.Add(map.GetNode(entity.GridPosition));
        }

        if (spell.fovMode == FovMode.Normal)
        {
            if (spell.xRay || forceXRay) nodes.AddRange(DoXRay(entity, spell, gameState, map));
            else nodes.AddRange(DoFov(entity.GridPosition, spell, gameState, map));
        }
        else if (spell.fovMode == FovMode.Square)
        {
            if (spell.xRay || forceXRay) nodes.AddRange(DoXRay(entity, spell, gameState, map));
            else nodes.AddRange(DoFov(entity.GridPosition, spell, gameState, map));
        }
        else if (spell.fovMode == FovMode.Line)
        {
            nodes.AddRange(DoLineOnly(entity, spell, gameState, map, forceXRay));
        }
        else if (spell.fovMode == FovMode.Diagonal)
        {
            nodes.AddRange(DoDiagonalOnly(entity, spell, gameState, map, forceXRay));
        }
        else if (spell.fovMode == FovMode.LineDiagonal)
        {
            nodes.AddRange(DoLineOnly(entity, spell, gameState, map, forceXRay));
            nodes.AddRange(DoDiagonalOnly(entity, spell, gameState, map, forceXRay));
        }
        return nodes;
    }
    
    // Octant transform multipliers: xx, xy, yx, yy
    private static readonly int[][] OctantMultipliers =
    {
        new[] { 1,  0,  0, -1}, // N-NE
        new[] { 0,  1, -1,  0}, // E-NE
        new[] { 0,  1,  1,  0}, // E-SE
        new[] { 1,  0,  0,  1}, // S-SE
        new[] {-1,  0,  0,  1}, // S-SW
        new[] { 0, -1,  1,  0}, // W-SW
        new[] { 0, -1, -1,  0}, // W-NW
        new[] {-1,  0,  0, -1}, // N-NW
    };

    private readonly struct Slope
    {
        public readonly long Num;
        public readonly long Den;

        public Slope(long num, long den)
        {
            // On veut den > 0 pour simplifier les comparaisons
            if (den < 0)
            {
                num = -num;
                den = -den;
            }
            Num = num;
            Den = den;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Less(in Slope a, in Slope b) => a.Num * b.Den <  b.Num * a.Den;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Greater(in Slope a, in Slope b) => a.Num * b.Den >  b.Num * a.Den;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LessOrEqual(in Slope a, in Slope b) => a.Num * b.Den <= b.Num * a.Den;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GreaterOrEqual(in Slope a, in Slope b) => a.Num * b.Den >= b.Num * a.Den;

        public static readonly Slope One  = new(1, 1);
        public static readonly Slope Zero = new(0, 1);
    }
    
    public static List<Node> DoFov(Vector2Int startPosition, Spell spell, GameState gameState, Map map)
    {
        List<Node> nodes = new();
        
        for (int i = 0; i < 8; i++)
        {
            CastLight(startPosition, spell, 1, Slope.One, Slope.Zero, OctantMultipliers[i], gameState, map, nodes, i);
        }

        return nodes;
    }

    private static void CastLight(Vector2Int startPosition, Spell spell, int row, Slope start, Slope end, int[] mult, GameState gameState, Map map, List<Node> nodes, int octantIndex)
    {
        if (Slope.Less(start, end)) return;

        int xx = mult[0];
        int xy = mult[1];
        int yx = mult[2];
        int yy = mult[3];

        bool isOdd = octantIndex % 2 != 0;

        for (int j = row; j <= spell.poMax; j++)
        {
            int dx = -j - 1;
            int dy = -j;
            bool blocked = false;
            Slope newStart = start;
            int kLimit = spell.fovMode == FovMode.Square ? spell.poMax : spell.poMax - j;

            // Start loop from kLimit instead of j to avoid radius checks
            for (int k = kLimit; k >= 0; k--)
            {
                dx = k;
                dy = j; // depth

                Slope lSlope = new(2L * dx - 1, 2L * dy + 1);
                Slope rSlope = new(2L * dx + 1, 2L * dy - 1);

                if (Slope.LessOrEqual(start, lSlope)) continue;
                if (Slope.GreaterOrEqual(end, rSlope)) break;

                int sax = dx * xx + dy * xy;
                int say = dx * yx + dy * yy;
                
                int realX = startPosition.x + sax;
                int realY = startPosition.y + say;

                // Bounds check optimization using unsigned cast
                if ((uint)realX < (uint)map.Width && (uint)realY < (uint)map.Height)
                {
                    // Restrictive FOV Check basé sur centerSlope = dx/dy :
                    // start >= dx/dy  <=> start.Num/start.Den >= dx/dy <=> start.Num * dy >= dx * start.Den
                    // end   <= dx/dy  <=> end.Num/end.Den   <= dx/dy <=> end.Num   * dy <= dx * end.Den
                    //
                    // (dy > 0 garanti)
                    long dyL = dy;
                    long dxL = dx;

                    if (start.Num * dyL >= dxL * start.Den && end.Num * dyL <= dxL * end.Den)
                    {
                        if (spell.fovMode == FovMode.Square ? (dx >= spell.poMin && dy >= spell.poMin) : dx + dy >= spell.poMin)
                        {
                            if (map.IsWalkable(realY * map.Width + realX))
                            {
                                // Avoid double-visiting edges
                                if (!isOdd || (k > 0 && k < j))
                                {
                                    nodes.Add(map.GetNode(new Vector2Int(realX, realY)));
                                }
                            }
                        }
                    }

                    // Direct grid access
                    bool isOpaque = map.IsOpaque(realY * map.Width + realX);
                    bool hasEntity = gameState.GetEntityByGridPosition(new Vector2Int(realX, realY)) != null;
                    
                    if (blocked)
                    {
                        if (isOpaque || hasEntity)
                        {
                            newStart = lSlope;
                            continue;
                        }
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else
                    {
                        if ((isOpaque || hasEntity) && j < spell.poMax)
                        {
                            blocked = true;
                            CastLight(startPosition, spell, j + 1, start, rSlope, mult, gameState, map, nodes, octantIndex);
                            newStart = lSlope;
                        }
                    }
                }
            }

            if (blocked) break;
        }
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
                if (spell.fovMode == FovMode.Square && absX < spell.poMin && absY < spell.poMin) continue;
                if (spell.fovMode != FovMode.Square && (absX + absY < spell.poMin || absX + absY > spell.poMax)) continue;
                Node node = map.GetNode(entity.GridPosition + new Vector2Int(x, y));
                if (node is not { NodeType: NodeType.Ground }) continue;
                if (!spell.canLaunchOnEntity && gameState.GetEntityByGridPosition(node.GridPosition) != null) continue;
                nodes.Add(node);
            }
        }

        return nodes;
    }

    private static List<Node> DoLineOnly(Entity entity, Spell spell, GameState gameState, Map map, bool forceXRay = false)
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

                if (!spell.xRay && !forceXRay)
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

    private static List<Node> DoDiagonalOnly(Entity entity, Spell spell, GameState gameState, Map map, bool forceXRay = false)
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

                if (!spell.xRay && !forceXRay && (node.NodeType is NodeType.Invalid or NodeType.Wall || gameState.GetEntityByGridPosition(node.GridPosition) != null))
                {
                    break;
                }
            }
        }
        return nodes;
    }
}
