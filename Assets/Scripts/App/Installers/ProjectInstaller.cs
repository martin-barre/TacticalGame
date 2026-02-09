using Reflex.Core;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder builder)
    {
        builder.AddSingleton(typeof(MessageChannel<UnityServiceErrorMessage>), typeof(IMessageChannel<UnityServiceErrorMessage>));
        builder.AddSingleton(typeof(AuthServiceFacade));
        builder.AddSingleton(typeof(SessionServiceFacade));
    }
}
