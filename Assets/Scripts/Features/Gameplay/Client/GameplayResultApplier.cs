using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public sealed class GameplayResultApplier : IDisposable
{
    private readonly GameplayClientState _clientState;
    private readonly InteractionManager _interactionManager;

    private readonly Queue<IPacket[]> _bufferedPackets = new();
    private readonly IDisposable _packetsSubscription;
    private readonly IDisposable _teamSubscription;

    private bool _isProcessing;

    public GameplayResultApplier(
        GameplayClientState clientState,
        InteractionManager interactionManager,
        ISubscriber<IPacket[]> packetsSubscriber,
        ISubscriber<Team> teamSubscriber)
    {
        _clientState = clientState;
        _interactionManager = interactionManager;

        _packetsSubscription = packetsSubscriber.Subscribe(OnPacketsReceived);
        _teamSubscription = teamSubscriber.Subscribe(OnTeamReceived);
    }

    private void OnPacketsReceived(IPacket[] packets)
    {
        _bufferedPackets.Enqueue(packets);
        if (!_isProcessing)
        {
            _ = ProcessQueueAsync();
        }
    }

    private void OnTeamReceived(Team team)
    {
        _clientState.Team = team;
    }

    private async Task ProcessQueueAsync()
    {
        _isProcessing = true;
        try
        {
            while (_bufferedPackets.Count > 0)
            {
                foreach (IPacket packet in _bufferedPackets.Dequeue())
                {
                    try
                    {
                        packet.Apply(_clientState.GameState, _clientState.Map);
                        await _clientState.PacketRendererRegistry.RenderAsync(packet);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
        finally
        {
            _isProcessing = false;
        }

        if (_clientState.Team != null && _clientState.GameState.Entities.Any())
        {
            _interactionManager.DisplayMovementNode();
        }
    }

    public void Dispose()
    {
        _packetsSubscription.Dispose();
        _teamSubscription.Dispose();
    }
}
