using System;
using VContainer;

public class GameManagerClient
{
    private readonly IObjectResolver _container;
    private readonly GameplayClientState _clientState;

    public GameManagerClient(IObjectResolver container, GameplayClientState clientState)
    {
        _container = container;
        _clientState = clientState;

        _clientState.PacketRendererRegistry.Register(CreateRenderer<PacketRendererBuff>());
        _clientState.PacketRendererRegistry.Register(CreateRenderer<PacketRendererDamage>());
        _clientState.PacketRendererRegistry.Register(CreateRenderer<PacketRendererHeal>());
        _clientState.PacketRendererRegistry.Register(CreateRenderer<PacketRendererKillEntity>());
        _clientState.PacketRendererRegistry.Register(CreateRenderer<PacketRendererLaunchSpell>());
        _clientState.PacketRendererRegistry.Register(CreateRenderer<PacketRendererMove>());
        _clientState.PacketRendererRegistry.Register(CreateRenderer<PacketRendererNextTurn>());
        _clientState.PacketRendererRegistry.Register(CreateRenderer<PacketRendererSetGameLogic>());
        _clientState.PacketRendererRegistry.Register(CreateRenderer<PacketRendererSummonEntity>());
        _clientState.PacketRendererRegistry.Register(CreateRenderer<PacketRendererTeleport>());
        _clientState.Initialize();
    }

    private TRenderer CreateRenderer<TRenderer>() where TRenderer : class
    {
        TRenderer instance = Activator.CreateInstance<TRenderer>();
        _container.Inject(instance);
        return instance;
    }
}
