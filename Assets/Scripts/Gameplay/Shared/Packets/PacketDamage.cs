using MessagePack;

[MessagePackObject]
public struct PacketDamage : IPacket
{
    [Key(0)] public int TargetId { get; set; }
    [Key(1)] public int Value { get; set; }
    
    public void Apply(GameState state, Map map)
    {
        Entity e = state.GetEntityById(TargetId);
        e.Hp -= Value;
    }

    public void Undo(GameState state, Map map)
    {
        Entity e = state.GetEntityById(TargetId);
        e.Hp += Value;
    }
}