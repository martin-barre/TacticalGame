using MessagePack;

[MessagePackObject]
public struct PacketSetGameLogic : IPacket
{
    [Key(0)] public bool IsStarted { get; set; }
    [Key(1)] public int CurrentPlayer { get; set; }

    private bool _oldIsStarted;
    private int _oldCurrentEntityIndex;

    public void Apply(GameState state, Map map)
    {
        _oldIsStarted = state.IsStarted;
        _oldCurrentEntityIndex = state.CurrentEntityIndex;
        
        state.IsStarted = IsStarted;
        state.CurrentEntityIndex = CurrentPlayer;
    }

    public void Undo(GameState state, Map map)
    {
        state.IsStarted = _oldIsStarted;
        state.CurrentEntityIndex = _oldCurrentEntityIndex;
    }
}