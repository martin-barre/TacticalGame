using MessagePack;

[MessagePackObject]
public struct PacketNextTurn : IPacket
{
    public void Apply(GameState state, Map map)
    {
        Entity entity = state.CurrentEntity;
        entity.Pa = entity.Race.Pa;
        entity.Pm = entity.Race.Pm;
        entity.Buffs.RemoveAll(s => s.TurnDuration is 0 or 1);
        entity.Buffs.ForEach(s =>
        {
            if (s.TurnDuration != -1)
            {
                s.TurnDuration--;
            }
        });
        
        state.CurrentEntityIndex = state.CurrentEntityIndex >= state.Entities.Count - 1 ? 0 : state.CurrentEntityIndex + 1;
    }

    public void Undo(GameState state, Map map)
    {
        throw new System.NotImplementedException();
    }
}
