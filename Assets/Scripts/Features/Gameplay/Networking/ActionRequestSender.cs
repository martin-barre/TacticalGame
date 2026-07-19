using System.Collections.Generic;
using MessagePack;
using VContainer;
using Unity.Netcode;
using UnityEngine;

public class ActionRequestSender : NetworkBehaviour
{
    [Inject] private SessionManager<SessionPlayerData> _sessionManager;
    [Inject] private GameManagerServer _gameManagerServer;
    [Inject] private ActionResultSender _actionResultSender;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void NotifyClientReadyRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        _gameManagerServer.SetupClientData(clientId);
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void MoveRpc(Vector2Int gridPosition, RpcParams rpcParams = default)
    {
        if (!CanDoAction(rpcParams)) return;
        IPacket packet = GameServerAction.Move(gridPosition, _gameManagerServer.GameState, _gameManagerServer.Map);
        _actionResultSender.SendPacketClientRpc(MessagePackSerializer.Serialize(packet));
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void LaunchSpellRpc(int spellId, Vector2Int targetPos, RpcParams rpcParams = default)
    {
        if (!CanDoAction(rpcParams)) return;
        List<IPacket> clientEffects = GameServerAction.LaunchSpell(spellId, targetPos, _gameManagerServer.GameState, _gameManagerServer.Map);
        _actionResultSender.SendPacketsClientRpc(MessagePackSerializer.Serialize(clientEffects));
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void NextTurnRpc(RpcParams rpcParams = default)
    {
        if (!CanDoAction(rpcParams)) return;
        List<IPacket> packets = GameServerAction.NextTurn(_gameManagerServer.GameState, _gameManagerServer.Map);
        _actionResultSender.SendPacketsClientRpc(MessagePackSerializer.Serialize(packets));
        _gameManagerServer.AdvanceAiTurns();
    }
    
    private bool CanDoAction(RpcParams rpcParams)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        SessionPlayerData? sessionData = _sessionManager.GetPlayerData(clientId);
        if (sessionData == null) return false;
        if (sessionData.Value.Team != _gameManagerServer.GameState.CurrentEntity.Team) return false;
        return true;
    }
}
