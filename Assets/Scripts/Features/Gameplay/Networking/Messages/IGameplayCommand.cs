using MessagePack;

[Union(0, typeof(GameplayCommandNextTurn))]
[Union(1, typeof(GameplayCommandMove))]
[Union(2, typeof(GameplayCommandLaunchSpell))]
[Union(3, typeof(GameplayCommandClientReady))]
public interface IGameplayCommand {}
