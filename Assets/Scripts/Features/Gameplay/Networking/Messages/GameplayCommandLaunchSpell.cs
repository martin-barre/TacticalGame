using MessagePack;
using UnityEngine;

[MessagePackObject]
public struct GameplayCommandLaunchSpell : IGameplayCommand
{
    [Key(0)] public int SpellId { get; set; }
    [Key(1)] public Vector2Int Position { get; set; }
}
