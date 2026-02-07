using MessagePack;
using UnityEngine;

[MessagePackObject]
public struct PacketSummonEntity : IPacket
{
    [Key(0)] public int EntityId { get ; set; }
    [Key(1)] public Team Team { get ; set; }
    [Key(2)] public int RaceId { get ; set; }
    [Key(3)] public Vector2Int GridPosition { get ; set; }
    [Key(4)] public bool IsPlayer { get ; set; }
    [Key(5)] public int SummonerId { get ; set; }
    
    private Entity _createdEntity;
    
    public void Apply(GameState state, Map map)
    {
        Entity summoner = state.GetEntityById(SummonerId);
        Race race = RaceDatabase.GetById(RaceId);
        
        _createdEntity = new Entity
        {
            Id = EntityId,
            Team = Team,
            GridPosition = GridPosition,
            Race = race,
            Hp = race.Hp,
            Pa = race.Pa,
            Pm = race.Pm,
            IsPlayer = IsPlayer,
            Summoner = summoner
        };
        
        if (_createdEntity.Summoner != null)
        {
            state.Entities.Insert(state.CurrentEntityIndex + 1, _createdEntity);
        }
        else
        {
            state.Entities.Add(_createdEntity);
        }
    }

    public void Undo(GameState state, Map map)
    {
        state.Entities.Remove(_createdEntity);
    }
}