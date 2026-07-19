using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private ActionRequestSender actionRequestSender;
    [SerializeField] private ActionResultSender actionResultSender;
    [SerializeField] private InteractionManager interactionManager;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(mapManager);
        builder.RegisterComponent(actionRequestSender);
        builder.RegisterComponent(actionResultSender);
        builder.RegisterComponent(interactionManager);

        builder.Register<GameManagerClient>(Lifetime.Singleton);
        builder.Register<GameManagerServer>(Lifetime.Singleton);
        builder.Register<InjectedPrefabFactory>(Lifetime.Singleton);
        builder.Register<PacketRendererRegistry>(Lifetime.Singleton);
        builder.Register<GameplayClientState>(Lifetime.Singleton);
        builder.Register<GameplayServerState>(Lifetime.Singleton);
    }
}
