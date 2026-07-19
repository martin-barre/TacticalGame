using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LineSpellZone : SpellZone
{
    public bool horizontal = true;
    
    public override List<Vector2Int> GetZonePositions(Vector2Int launcherPosition, Vector2Int targetPosition)
    {
        return horizontal
            ? GetPositionLineHorizontal(launcherPosition, targetPosition)
            : GetPositionLineVertical(launcherPosition, targetPosition);
    }
    
    private List<Vector2Int> GetPositionLineVertical(Vector2Int launcherPosition, Vector2Int targetPosition)
    {
        Vector2Int direction = Utils.GridDirection(launcherPosition, targetPosition);
        List<Vector2Int> positions = new(size);
        for (int i = 0; i < size; i++)
        {
            positions.Add(direction * i);
        }
        return positions;
    }

    private List<Vector2Int> GetPositionLineHorizontal(Vector2Int launcherPosition, Vector2Int targetPosition)
    {
        Vector2Int direction = Utils.GridDirection(launcherPosition, targetPosition);
        List<Vector2Int> positions = new(1 + 2 * (size - 1)) { Vector2Int.zero };
        for (int i = 1; i < size; i++)
        {
            positions.Add(new Vector2Int(direction.y * i, -direction.x * i));
            positions.Add(new Vector2Int(-direction.y * i, direction.x * i));
        }
        return positions;
    }
}