using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int GridDirection(Vector2Int a, Vector2Int b)
    {
        int dx = b.x - a.x;
        int dy = b.y - a.y;

        if (dx == 0 && dy == 0)
        {
            return Vector2Int.zero;
        }

        int absX = math.abs(dx);
        int absY = math.abs(dy);
        int signX = math.sign(dx);
        int signY = math.sign(dy);

        // is diagonal
        if (absX == absY)
        {
            return new Vector2Int(signX, signY);
        }
        
        return absX > absY ? new Vector2Int(signX, 0) : new Vector2Int(0, signY);
    }
}
