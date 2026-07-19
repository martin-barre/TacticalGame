using MessagePack;
using UnityEngine;

[MessagePackObject]
public struct GameplayCommandMove : IGameplayCommand
{
    [Key(0)] public Vector2Int Position { get; set; }
}
