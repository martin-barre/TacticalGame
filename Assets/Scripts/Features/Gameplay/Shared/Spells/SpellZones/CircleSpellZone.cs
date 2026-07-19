using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CircleSpellZone : SpellZone
{
    public bool fill = true;

    public override List<Vector2Int> GetZonePositions(Vector2Int launcherPosition, Vector2Int targetPosition)
    {
        return fill ? GetPositionCircleFill() : GetPositionCircleLine();
    }

    private List<Vector2Int> GetPositionCircleFill()
    {
        List<Vector2Int> positions = new( 1 + 2 * size * (size - 1)) { Vector2Int.zero };
        
        for (int d = 1; d < size; d++)
        {
            int x = d;

            // Quart supérieur droit → x diminue, y augmente
            for (; x > 0; x--) positions.Add(new Vector2Int(x, d - x));

            // Quart supérieur gauche → x diminue encore, y diminue
            for (; x > -d; x--) positions.Add(new Vector2Int(x, d + x));

            // Quart inférieur gauche → x augmente, y diminue
            for (; x < 0; x++) positions.Add(new Vector2Int(x, -d - x));

            // Quart inférieur droit → x augmente encore, y augmente
            for (; x < d; x++) positions.Add(new Vector2Int(x, -d + x));
        }
        return positions;
    }

    private List<Vector2Int> GetPositionCircleLine()
    {
        List<Vector2Int> positions = new(4 * (size - 1));
        
        int x = size;

        // Quart supérieur droit → x diminue, y augmente
        for (; x > 0; x--) positions.Add(new Vector2Int(x, size - x));

        // Quart supérieur gauche → x diminue encore, y diminue
        for (; x > -size; x--) positions.Add(new Vector2Int(x, size + x));

        // Quart inférieur gauche → x augmente, y diminue
        for (; x < 0; x++) positions.Add(new Vector2Int(x, -size - x));

        // Quart inférieur droit → x augmente encore, y augmente
        for (; x < size; x++) positions.Add(new Vector2Int(x, -size + x));
        
        return positions;
    }
}