using System;

[Serializable]
public class ActiveBuff
{
    public Buff Buff;
    public int TurnDuration;
    public Entity Launcher;
    
    public ActiveBuff Clone()
    {
        return new ActiveBuff
        {
            Buff = Buff,
            TurnDuration = TurnDuration,
            Launcher = Launcher
        };
    }
}
