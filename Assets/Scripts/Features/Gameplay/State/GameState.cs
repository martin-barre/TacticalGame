using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public sealed class GameState
{
    public bool IsStarted;
    public int CurrentEntityIndex;
    public List<Entity> Entities = new();
    
    public Entity CurrentEntity => CurrentEntityIndex >= 0 && CurrentEntityIndex < Entities.Count ? Entities[CurrentEntityIndex] : null;
    
    [CanBeNull]
    public Entity GetEntityByGridPosition(Vector2Int gridPosition)
    {
        return Entities.SingleOrDefault(e => e.GridPosition == gridPosition);
    }
    
    public Entity GetEntityByIndex(int entityIndex)
    {
        return Entities[entityIndex];
    }
    
    public Entity GetEntityById(int entityId)
    {
        return Entities.FirstOrDefault(e => e.Id == entityId);
    }
    
    public void MoveOrSwapEntity(Entity entity, Vector2Int gridPosition)
    {
        Entity entityToSwap = GetEntityByGridPosition(gridPosition);
        Vector2Int oldPosition = entity.GridPosition;
        
        entity.GridPosition = gridPosition;

        if (entityToSwap != null)
        {
            entityToSwap.GridPosition = oldPosition;
        }
    }
    
    public GameState Clone()
    {
        return new GameState
        {
            IsStarted = IsStarted,
            CurrentEntityIndex = CurrentEntityIndex,
            Entities = Entities.Select(e => e.Clone()).ToList()
        };
    }
}
