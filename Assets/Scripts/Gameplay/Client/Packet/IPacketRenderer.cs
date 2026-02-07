using System.Threading.Tasks;

public interface IPacketRenderer<in T> where T : IPacket
{
    Task RenderAsync(T packet);
}