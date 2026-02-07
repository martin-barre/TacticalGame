using System.Linq;
using MessagePack;
using UnityEngine;

[MessagePackObject]
public struct PacketMove : IPacket
{
    [Key(0)] public int TargetId { get; set; }
    [Key(1)] public int PmCost { get; set; }
    [Key(2)] public Vector2Int[] Path { get; set; }
    
    private Vector2Int _startPosition;
    
    public void Apply(GameState state, Map map)
    {
        Entity entity = state.GetEntityById(TargetId);
        _startPosition = entity.GridPosition;
        entity.Pm -= PmCost;
        if(Path.Any())
        {
            state.MoveOrSwapEntity(entity, Path.Last());
        }
    }

    public void Undo(GameState state, Map map)
    {
        Entity entity = state.GetEntityById(TargetId);
        entity.Pm += PmCost;
        state.MoveOrSwapEntity(entity, _startPosition);
    }
}