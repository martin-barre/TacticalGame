using TMPro;
using VContainer;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private GameObject panelContent;
    [SerializeField] private GameObject chatMessageUI;

    [Inject] private GameplayClientState _clientState;

    private void Start()
    {
        _clientState.OnChatMessage += OnChatMessage;
    }
    
    private void OnDestroy()
    {
        _clientState.OnChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(string message)
    {
        TMP_Text instance = Instantiate(chatMessageUI, panelContent.transform).GetComponent<TMP_Text>();
        instance.text = message;
    }
}
