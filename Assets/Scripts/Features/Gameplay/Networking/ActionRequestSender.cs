using System.Collections.Generic;
using MessagePack;
using Unity.Netcode;
using UnityEngine;

public class ActionRequestSender : NetworkSingleton<ActionRequestSender>
{
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void NotifyClientReadyRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameManagerServer.Instance.SetupClientData(clientId);
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void MoveRpc(Vector2Int gridPosition, RpcParams rpcParams = default)
    {
        if (!CanDoAction(rpcParams)) return;
        IPacket packet = GameServerAction.Move(gridPosition, GameManagerServer.Instance.GameState, GameManagerServer.Instance.Map);
        ActionResultSender.Instance.SendPacketClientRpc(MessagePackSerializer.Serialize(packet));
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void LaunchSpellRpc(int spellId, Vector2Int targetPos, RpcParams rpcParams = default)
    {
        if (!CanDoAction(rpcParams)) return;
        List<IPacket> clientEffects = GameServerAction.LaunchSpell(spellId, targetPos, GameManagerServer.Instance.GameState, GameManagerServer.Instance.Map);
        ActionResultSender.Instance.SendPacketsClientRpc(MessagePackSerializer.Serialize(clientEffects));
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void NextTurnRpc(RpcParams rpcParams = default)
    {
        if (!CanDoAction(rpcParams)) return;
        List<IPacket> packets = GameServerAction.NextTurn(GameManagerServer.Instance.GameState, GameManagerServer.Instance.Map);
        ActionResultSender.Instance.SendPacketsClientRpc(MessagePackSerializer.Serialize(packets));
        GameManagerServer.Instance.TestPlayIA();
    }
    
    private bool CanDoAction(RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        SessionPlayerData? sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
        if (sessionData == null) return false;
        if (sessionData.Value.Team != GameManagerServer.Instance.GameState.CurrentEntity.Team) return false;
        return true;
    }
}
