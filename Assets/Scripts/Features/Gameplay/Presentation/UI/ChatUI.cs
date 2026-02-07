using TMPro;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private GameObject panelContent;
    [SerializeField] private GameObject chatMessageUI;

    private void Start()
    {
        GameManagerClient.Instance.OnChatMessage += OnChatMessage;
    }
    
    private void OnDestroy()
    {
        GameManagerClient.Instance.OnChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(string message)
    {
        TMP_Text instance = Instantiate(chatMessageUI, panelContent.transform).GetComponent<TMP_Text>();
        instance.text = message;
    }
}
