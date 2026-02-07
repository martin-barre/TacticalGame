using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class ArcSpellZone : SpellZone
{
    public override List<Vector2Int> GetZonePositions(Vector2Int launcherPosition, Vector2Int targetPosition)
    {
        return GetPositionArc(launcherPosition, targetPosition);
    }
    
    private List<Vector2Int> GetPositionArc(Vector2Int launcherGridPosition, Vector2Int targetGridPosition)
    {
        Vector2Int direction = Utils.GridDirection(launcherGridPosition, targetGridPosition);
        List<Vector2Int> positions = new(1 + 2 * (size - 1)) { Vector2Int.zero };
        int signX = math.sign(direction.x);
        int signY = math.sign(direction.x);
        bool isDiagonal = math.abs(direction.x) == math.abs(direction.y);
        for (int i = 1; i < size; i++)
        {
            if (isDiagonal)
            {
                positions.Add(new Vector2Int(direction.x - signX, signY * -i));
                positions.Add(new Vector2Int(signX * -i, direction.y - signY));
            }
            else
            {
                positions.Add(new Vector2Int(direction.x * -i + direction.y * i, direction.y * -i + direction.x * i));
                positions.Add(new Vector2Int(direction.x * -i - direction.y * i, direction.y * -i - direction.x * i));
            }
        }
        return positions;
    }
}