using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CrossSpellZone : SpellZone
{
    public override List<Vector2Int> GetZonePositions(Vector2Int launcherPosition, Vector2Int targetPosition)
    {
        return GetPositionCross();
    }

    private List<Vector2Int> GetPositionCross()
    {
        List<Vector2Int> positions = new(1 + 4 * (size - 1))
        {
            Vector2Int.zero
        };
        for (int i = 1; i < size; i++)
        {
            positions.Add(new Vector2Int(i, 0));
            positions.Add(new Vector2Int(-i, 0));
            positions.Add(new Vector2Int(0, i));
            positions.Add(new Vector2Int(0, -i));
        }
        return positions;
    }
}