using UnityEngine;
using VContainer;
using VContainer.Unity;

public class CharacterSelectLifetimeScope : LifetimeScope
{
    [SerializeField] private RaceSelectionManagerClient raceSelectionManagerClient;
    [SerializeField] private RaceSelectionManagerServer raceSelectionManagerServer;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(raceSelectionManagerClient);
        builder.RegisterComponent(raceSelectionManagerServer);
    }
}
