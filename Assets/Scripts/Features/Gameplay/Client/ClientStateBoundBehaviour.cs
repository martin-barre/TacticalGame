using VContainer;

public abstract class ClientStateBoundBehaviour : NetworkClientBehaviour
{
    [Inject] protected GameplayClientState ClientState;

    protected override void Start()
    {
        base.Start();
        if (!enabled) return;

        if (!ClientState.IsInitialized)
        {
            ClientState.Initialized += OnClientStateInitializedInternal;
            return;
        }

        BindClientState();
    }

    private void OnClientStateInitializedInternal()
    {
        ClientState.Initialized -= OnClientStateInitializedInternal;
        BindClientState();
    }

    protected virtual void OnDestroy()
    {
        ClientState.Initialized -= OnClientStateInitializedInternal;
    }

    protected abstract void BindClientState();
}
