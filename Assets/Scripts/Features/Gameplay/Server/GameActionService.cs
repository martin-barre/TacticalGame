using System;
using System.Collections.Generic;

public sealed class GameActionService : IDisposable
{
    private readonly SessionManager<SessionPlayerData> _sessionManager;
    private readonly GameManagerServer _gameManagerServer;
    private readonly IPublisher<PacketsNetwork> _packetsNetworkPublisher;

    private readonly IDisposable _gameplayCommandNetworkSubscription;

    public GameActionService(
        SessionManager<SessionPlayerData> sessionManager,
        GameManagerServer gameManagerServer,
        ISubscriber<GameplayCommandNetwork> gameplayCommandSubscriber,
        IPublisher<PacketsNetwork> packetsNetworkPublisher)
    {
        _sessionManager = sessionManager;
        _gameManagerServer = gameManagerServer;
        _packetsNetworkPublisher = packetsNetworkPublisher;

        _gameplayCommandNetworkSubscription = gameplayCommandSubscriber.Subscribe(OnGameplayCommandNetwork);
    }

    private void OnGameplayCommandNetwork(GameplayCommandNetwork gameplayCommandNetwork)
    {
        if (gameplayCommandNetwork.GameplayCommand is GameplayCommandClientReady)
        {
            _gameManagerServer.SetupClientData(gameplayCommandNetwork.ClientId);
            return;
        }

        if (!CanDoAction(gameplayCommandNetwork.ClientId)) return;

        if (gameplayCommandNetwork.GameplayCommand is GameplayCommandMove commandMove)
            OnMoveRequested(commandMove);
        if (gameplayCommandNetwork.GameplayCommand is GameplayCommandLaunchSpell commandLaunchSpell)
            OnLaunchSpellRequested(commandLaunchSpell);
        if (gameplayCommandNetwork.GameplayCommand is GameplayCommandNextTurn commandNextTurn)
            OnNextTurnRequested(commandNextTurn);
    }
    
    private void OnMoveRequested(GameplayCommandMove command)
    {
        IPacket packet = GameServerAction.Move(command.Position, _gameManagerServer.GameState, _gameManagerServer.Map);
        _packetsNetworkPublisher.Publish(new PacketsNetwork { Packets = new[] { packet } });
    }

    private void OnLaunchSpellRequested(GameplayCommandLaunchSpell command)
    {
        List<IPacket> packets = GameServerAction.LaunchSpell(command.SpellId, command.Position, _gameManagerServer.GameState, _gameManagerServer.Map);
        _packetsNetworkPublisher.Publish(new PacketsNetwork { Packets = packets.ToArray() });
    }

    private void OnNextTurnRequested(GameplayCommandNextTurn _)
    {
        List<IPacket> packets = GameServerAction.NextTurn(_gameManagerServer.GameState, _gameManagerServer.Map);
        _packetsNetworkPublisher.Publish(new PacketsNetwork { Packets = packets.ToArray() });
        _gameManagerServer.AdvanceAiTurns();
    }

    private bool CanDoAction(ulong clientId)
    {
        SessionPlayerData? sessionData = _sessionManager.GetPlayerData(clientId);
        if (sessionData == null) return false;
        if (sessionData.Value.Team != _gameManagerServer.GameState.CurrentEntity.Team) return false;
        return true;
    }

    public void Dispose()
    {
        _gameplayCommandNetworkSubscription.Dispose();
    }
}
