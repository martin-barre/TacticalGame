using System;


[Serializable]
public abstract class BuffEffect {}

[Serializable]
public class BuffEffectAddStats : BuffEffect
{
    public Stats Stats;
    public int Value;
}
