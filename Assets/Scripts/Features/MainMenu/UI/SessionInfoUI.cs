using Reflex.Attributes;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionInfoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text textName;
    [SerializeField] private TMP_Text textPlayerCount;
    
    [Inject] private readonly SessionServiceFacade _sessionServiceFacade;
    
    private ISessionInfo _sessionInfo;

    public void SetInfo(ISessionInfo sessionInfo)
    {
        _sessionInfo = sessionInfo;
        textName.text = sessionInfo.Name;
        textPlayerCount.text = sessionInfo.MaxPlayers - sessionInfo.AvailableSlots + " / " + sessionInfo.MaxPlayers;
    }

    public void JoinSession()
    {
        _ = _sessionServiceFacade.JoinSessionById(_sessionInfo.Id);
    }
}
