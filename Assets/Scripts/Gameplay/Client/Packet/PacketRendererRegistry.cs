using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class PacketRendererRegistry
{
    private interface IBox
    {
        Task RenderAsync(IPacket packet);
    }

    private sealed class Box<T> : IBox where T : IPacket
    {
        private readonly IPacketRenderer<T> _renderer;
        public Box(IPacketRenderer<T> renderer) => _renderer = renderer;
        public Task RenderAsync(IPacket packet) => _renderer.RenderAsync((T)packet);
    }

    private readonly Dictionary<Type, IBox> _registry = new();

    public void Register<T>(IPacketRenderer<T> renderer) where T : IPacket
        => _registry[typeof(T)] = new Box<T>(renderer);

    public Task RenderAsync(IPacket packet)
    {
        if (packet is null)
            throw new ArgumentNullException(nameof(packet));

        var type = packet.GetType();
        if (_registry.TryGetValue(type, out var box))
            return box.RenderAsync(packet);

        throw new InvalidOperationException($"No renderer registered for {type.Name}");
    }
}