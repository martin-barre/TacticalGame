using System.Collections.Generic;
using UnityEngine;

public interface IAiAction
{
    public List<IPacket> Apply(GameState gameState, Map map);
}

public class AiActionMove : IAiAction
{
    public Vector2Int GridPosition { get; set; }
    
    public List<IPacket> Apply(GameState gameState, Map map)
    {
        return new List<IPacket> { GameServerAction.Move(GridPosition, gameState, map) };
    }
}

public class AiActionLaunchSpell : IAiAction
{
    public int SpellId { get; set; }
    public Vector2Int GridPosition { get; set; }
    
    public List<IPacket> Apply(GameState gameState, Map map)
    {
        Spell spell = SpellDatabase.GetById(SpellId);
        if (spell == null) return new List<IPacket>();
        return spell.Launch(gameState.CurrentEntity, GridPosition, gameState, map);
    }
}
