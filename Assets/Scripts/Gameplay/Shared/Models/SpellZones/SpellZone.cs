using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[Serializable]
public abstract class SpellZone
{
    public int size = 1;
    
    public abstract List<Vector2Int> GetZonePositions(Vector2Int launcherPosition, Vector2Int targetPosition);
}
