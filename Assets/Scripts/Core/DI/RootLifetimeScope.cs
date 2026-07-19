using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IMessageChannel<UnityServiceErrorMessage>, MessageChannel<UnityServiceErrorMessage>>(Lifetime.Singleton);
        builder.Register<IAuthServiceFacade, AuthServiceFacade>(Lifetime.Singleton);
        builder.Register<ISessionServiceFacade, SessionServiceFacade>(Lifetime.Singleton);
        builder.Register<SessionManager<SessionPlayerData>>(Lifetime.Singleton);
    }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
