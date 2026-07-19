using System;
using MessagePack;
using Unity.Netcode;
using VContainer;

public class GameplayCommandNetwork
{
    public IGameplayCommand GameplayCommand { get; set; }
    public ulong ClientId { get; set; }
}

public class ActionRequestSender : NetworkBehaviour
{
    [Inject] private ISubscriber<IGameplayCommand> _gameplayCommandSubscriber;
    [Inject] private IPublisher<GameplayCommandNetwork> _gameplayCommandReceivedPublisher;
    
    private IDisposable _gameplayCommandSubscription;
    
    public override void OnNetworkSpawn()
    {
        _gameplayCommandSubscription = _gameplayCommandSubscriber.Subscribe(OnGameplayCommand);

        if (IsClient)
        {
            OnGameplayCommand(new GameplayCommandClientReady());
        }
    }

    public override void OnNetworkDespawn()
    {
        _gameplayCommandSubscription?.Dispose();
        _gameplayCommandSubscription = null;
    }
    
    private void OnGameplayCommand(IGameplayCommand gameplayCommand)
    {
        byte[] payload = MessagePackSerializer.Serialize(gameplayCommand);
        SendGameplayCommandServerRpc(payload);
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SendGameplayCommandServerRpc(byte[] serializedMessage, RpcParams rpcParams = default)
    {
        IGameplayCommand gameplayCommand = MessagePackSerializer.Deserialize<IGameplayCommand>(serializedMessage);
        _gameplayCommandReceivedPublisher.Publish(new GameplayCommandNetwork
        {
            GameplayCommand = gameplayCommand,
            ClientId = rpcParams.Receive.SenderClientId
        });
    }
}
