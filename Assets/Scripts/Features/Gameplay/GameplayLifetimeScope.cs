using Unity.Netcode;
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

        builder.Register<InjectedPrefabFactory>(Lifetime.Singleton);
        builder.Register<PacketRendererRegistry>(Lifetime.Singleton);

        RegisterMessageChannels(builder);

        bool isServer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
        bool isClient = NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient;

        if (isServer)
        {
            builder.Register<GameplayServerState>(Lifetime.Singleton);
            builder.Register<GameManagerServer>(Lifetime.Singleton);
            builder.Register<GameActionService>(Lifetime.Singleton);
        }

        if (isClient)
        {
            builder.Register<GameplayClientState>(Lifetime.Singleton);
            builder.Register<GameManagerClient>(Lifetime.Singleton);
            builder.Register<GameplayResultApplier>(Lifetime.Singleton);
        }

        // GameManagerServer/GameActionService/GameManagerClient/GameplayResultApplier are only
        // referenced through message channels (no NetworkBehaviour injects them directly anymore),
        // so they must be force-resolved for their constructors (and channel subscriptions) to run.
        builder.RegisterBuildCallback(resolver =>
        {
            if (isServer)
            {
                resolver.Resolve<GameManagerServer>();
                resolver.Resolve<GameActionService>();
            }

            if (isClient)
            {
                resolver.Resolve<GameManagerClient>();
                resolver.Resolve<GameplayResultApplier>();
            }
        });
    }

    private static void RegisterMessageChannels(IContainerBuilder builder)
    {
        RegisterChannel<IGameplayCommand>(builder);
        RegisterChannel<IPacket[]>(builder);
        RegisterChannel<GameplayCommandNetwork>(builder);
        RegisterChannel<PacketsNetwork>(builder);
        RegisterChannel<TeamNetwork>(builder);
        RegisterChannel<Team>(builder);
    }

    // Registered as three interfaces sharing a single instance so both IPublisher<T> and
    // ISubscriber<T> can be injected independently (RPC adapters only publish, services only subscribe).
    private static void RegisterChannel<T>(IContainerBuilder builder)
    {
        builder.Register<MessageChannel<T>>(Lifetime.Singleton)
            .As<IMessageChannel<T>, IPublisher<T>, ISubscriber<T>>();
    }
}
