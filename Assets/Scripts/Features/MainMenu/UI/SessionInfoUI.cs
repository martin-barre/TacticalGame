using VContainer;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionInfoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textName;
    [SerializeField] private TMP_Text textPlayerCount;
    
    [Inject] private ISessionServiceFacade _sessionServiceFacade;
    
    private ISessionInfo _sessionInfo;

    public void SetInfo(ISessionInfo sessionInfo)
    {
        _sessionInfo = sessionInfo;
        textName.text = sessionInfo.Name;
        textPlayerCount.text = sessionInfo.MaxPlayers - sessionInfo.AvailableSlots + " / " + sessionInfo.MaxPlayers;
    }

    public void JoinSession()
    {
        _ = _sessionServiceFacade.TryJoinSessionByIdAsync(_sessionInfo.Id);
    }
}
