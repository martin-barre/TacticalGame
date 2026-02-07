using MessagePack;
using UnityEngine;

[MessagePackObject]
public struct PacketLaunchSpell : IPacket
{
    [Key(0)] public int LauncherId { get; set; }
    [Key(1)] public int SpellId { get; set; }
    [Key(2)] public Vector2Int TargetPos { get; set; }
    
    public void Apply(GameState state, Map map)
    {
        Spell spell = SpellDatabase.GetById(SpellId);
        Entity entity = state.GetEntityById(LauncherId);
        entity.Pa -= spell.paCost;
    }

    public void Undo(GameState state, Map map)
    {
        Spell spell = SpellDatabase.GetById(SpellId);
        Entity entity = state.GetEntityById(LauncherId);
        entity.Pa += spell.paCost;
    }
}