using System.Collections.Generic;
using System.Linq;
using MessagePack;

[MessagePackObject]
public struct PacketKillEntity : IPacket
{
    [Key(0)] public int TargetId { get; set; }
    
    private List<Entity> _entities;
    private int _currentEntityIndex;
    
    public void Apply(GameState state, Map map)
    {
        Entity entity = state.GetEntityById(TargetId);

        _entities = state.Entities.Select(e => e.Clone()).ToList();
        _currentEntityIndex = state.CurrentEntityIndex;
        
        if (state.CurrentEntityIndex >= state.Entities.IndexOf(entity))
        {
            state.CurrentEntityIndex--;
        }
        
        state.Entities.ForEach(e => e.Buffs.RemoveAll(b => b.Launcher.Id == entity.Id));
        state.Entities.Remove(entity);
    }

    public void Undo(GameState state, Map map)
    {
        state.Entities = _entities;
        state.CurrentEntityIndex = _currentEntityIndex;
    }
}