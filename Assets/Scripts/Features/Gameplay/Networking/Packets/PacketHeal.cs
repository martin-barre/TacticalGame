using MessagePack;

[MessagePackObject]
public struct PacketHeal : IPacket
{
    [Key(0)] public int TargetId { get; set; }
    [Key(1)] public int Value { get; set; }
    
    public void Apply(GameState state, Map map)
    {
        Entity entity = state.GetEntityById(TargetId);
        entity.Hp += Value;
    }

    public void Undo(GameState state, Map map)
    {
        Entity entity = state.GetEntityById(TargetId);
        entity.Hp -= Value;
    }
}