using System;
using System.Linq;
using MessagePack;
using Unity.Netcode;
using VContainer;

public class PacketsNetwork
{
    public IPacket[] Packets { get; set; }
    public ulong? TargetClientId { get; set; }
}

public class ActionResultSender : NetworkBehaviour
{
    [Inject] private ISubscriber<PacketsNetwork> _packetsSubscriber;
    [Inject] private IPublisher<IPacket[]> _packetsReceivedPublisher;
    [Inject] private ISubscriber<TeamNetwork> _teamSubscriber;
    [Inject] private IPublisher<Team> _teamReceivedPublisher;

    private IDisposable _packetsSubscription;
    private IDisposable _teamSubscription;

    public override void OnNetworkSpawn()
    {
        _packetsSubscription = _packetsSubscriber.Subscribe(OnPacketsNetwork);
        _teamSubscription = _teamSubscriber.Subscribe(OnTeamNetwork);
    }

    public override void OnNetworkDespawn()
    {
        _packetsSubscription?.Dispose();
        _packetsSubscription = null;
        _teamSubscription?.Dispose();
        _teamSubscription = null;
    }

    private void OnPacketsNetwork(PacketsNetwork packetsNetwork)
    {
        byte[] payload = MessagePackSerializer.Serialize(packetsNetwork.Packets);
        if (packetsNetwork.TargetClientId.HasValue)
        {
            SendPacketsClientRpc(payload, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { packetsNetwork.TargetClientId.Value } }
            });
        }
        else
        {
            SendPacketsClientRpc(payload);
        }
    }

    private void OnTeamNetwork(TeamNetwork teamNetwork)
    {
        SendTeamClientRpc(teamNetwork.Team, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { teamNetwork.TargetClientId } }
        });
    }

    [ClientRpc]
    private void SendPacketsClientRpc(byte[] serializedPackets, ClientRpcParams _ = default)
    {
        IPacket[] packets = MessagePackSerializer.Deserialize<IPacket[]>(serializedPackets);
        _packetsReceivedPublisher.Publish(packets);
    }

    [ClientRpc]
    private void SendTeamClientRpc(Team team, ClientRpcParams _ = default)
    {
        _teamReceivedPublisher.Publish(team);
    }
}
