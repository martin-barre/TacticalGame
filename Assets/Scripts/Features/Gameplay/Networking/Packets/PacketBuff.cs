using MessagePack;

[MessagePackObject]
public struct PacketBuff : IPacket
{
    [Key(0)] public int TargetId { get; set; }
    [Key(1)] public int LauncherId { get; set; }
    [Key(2)] public int BuffId { get; set; }

    private ActiveBuff _activeBuff;
    
    public void Apply(GameState state, Map map)
    {
        Entity target = state.GetEntityById(TargetId);
        Entity launcher = state.GetEntityById(LauncherId);
        Buff buff = BuffDatabase.GetById(BuffId);
        
        _activeBuff = new ActiveBuff
        {
            Buff = buff,
            TurnDuration = buff.TurnDuration,
            Launcher = launcher
        };
        
        target.Buffs.Add(_activeBuff);
    }

    public void Undo(GameState state, Map map)
    {
        Entity target = state.GetEntityById(TargetId);
        target.Buffs.Add(_activeBuff);
    }
}