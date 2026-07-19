using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using VContainer;
using Unity.Netcode;
using UnityEngine;

public class ActionResultSender : NetworkBehaviour
{
    private readonly Queue<IPacket[]> _bufferPackets = new();
    private bool _isProcessing;

    [Inject] private GameplayClientState _clientState;
    [Inject] private InteractionManager _interactionManager;
    [Inject] private GameManagerClient _gameManagerClient;

    private void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        {
            return;
        }

        if (!_isProcessing && _bufferPackets.Count > 0)
        {
            StartCoroutine(ApplyPacketsCoroutine(_bufferPackets.Dequeue()));
        }
    }

    [ClientRpc]
    public void SendPacketClientRpc(byte[] serializedPacket, ClientRpcParams _ = default)
    {
        IPacket packet = MessagePackSerializer.Deserialize<IPacket>(serializedPacket);
        _bufferPackets.Enqueue(new []{ packet });
    }
    
    [ClientRpc]
    public void SendPacketsClientRpc(byte[] serializedPackets, ClientRpcParams _ = default)
    {
        IPacket[] packets = MessagePackSerializer.Deserialize<IPacket[]>(serializedPackets);
        _bufferPackets.Enqueue(packets);
    }
    
    [ClientRpc]
    public void SendTeamClientRpc(Team team, ClientRpcParams _ = default)
    {
        _clientState.Team = team;
    }

    private IEnumerator ApplyPacketsCoroutine(IPacket[] packets)
    {
        _isProcessing = true;
        try
        {
            foreach (IPacket packet in packets)
            {
                Task task;
                try
                {
                    packet.Apply(_clientState.GameState, _clientState.Map);
                    task = _clientState.PacketRendererRegistry.RenderAsync(packet);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    continue;
                }
                while (!task.IsCompleted) yield return null;
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
}
