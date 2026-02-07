using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Unity.Netcode;

public class ActionResultSender : NetworkSingleton<ActionResultSender>
{
    private readonly Queue<IPacket[]> _bufferPackets = new();
    private bool _isProcessing;

    private void Update()
    {
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
        GameManagerClient.Instance.Team = team;
    }

    private IEnumerator ApplyPacketsCoroutine(IPacket[] packets)
    {
        _isProcessing = true;
        foreach (IPacket packet in packets)
        {
            packet.Apply(GameManagerClient.Instance.GameState, GameManagerClient.Instance.Map);
            Task task = GameManagerClient.Instance.PacketRendererRegistry.RenderAsync(packet);
            while (!task.IsCompleted) yield return null;
        }
        _isProcessing = false;

        if (GameManagerClient.Instance.Team != null && GameManagerClient.Instance.GameState.Entities.Any())
        {
            InteractionManager.Instance.DisplayMovementNode();
        }
    }
}
