using Reflex.Attributes;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject isHostIcon;
    [SerializeField] private TMP_Text textName;
    [SerializeField] private Button btnRemove;

    [Inject] private readonly SessionServiceFacade _sessionServiceFacade;
    
    private IReadOnlyPlayer _player;
    
    public void SetInfo(IReadOnlyPlayer player, ISession session)
    {
        _player = player;
        isHostIcon.SetActive(player.Id == session.Host);
        btnRemove.gameObject.SetActive(session.IsHost && player.Id != session.Host);
        textName.text = player.Properties["playerName"].Value;
    }

    public void Remove()
    {
        _sessionServiceFacade.RemovePlayerAsync(_player.Id);
    }
}
