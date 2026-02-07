using MessagePack;

[Union(0, typeof(PacketBuff))]
[Union(1, typeof(PacketDamage))]
[Union(2, typeof(PacketHeal))]
[Union(3, typeof(PacketKillEntity))]
[Union(4, typeof(PacketLaunchSpell))]
[Union(5, typeof(PacketMove))]
[Union(6, typeof(PacketNextTurn))]
[Union(7, typeof(PacketSetGameLogic))]
[Union(8, typeof(PacketSummonEntity))]
[Union(9, typeof(PacketTeleport))]
public interface IPacket
{
    public void Apply(GameState state, Map map);
    public void Undo(GameState state, Map map);
}
