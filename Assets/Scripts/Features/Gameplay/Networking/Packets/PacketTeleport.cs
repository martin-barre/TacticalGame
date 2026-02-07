using MessagePack;
using UnityEngine;

[MessagePackObject]
public struct PacketTeleport : IPacket
{
    [Key(0)] public int TargetId { get; set; }
    [Key(1)] public Vector2Int GridPosition { get; set; }

    private Vector2Int _startPosition;
    
    public void Apply(GameState state, Map map)
    {
        Entity entity = state.GetEntityById(TargetId);
        _startPosition = entity.GridPosition;
        state.MoveOrSwapEntity(entity, GridPosition);
    }

    public void Undo(GameState state, Map map)
    {
        Entity entity = state.GetEntityById(TargetId);
        state.MoveOrSwapEntity(entity, _startPosition);
    }
}