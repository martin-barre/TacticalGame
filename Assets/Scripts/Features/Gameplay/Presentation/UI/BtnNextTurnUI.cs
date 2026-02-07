using UnityEngine;
using UnityEngine.UI;

public class BtnNextTurnUI : MonoBehaviour
{
    [SerializeField] private Button btnNextTurn;

    private void Start()
    {
        btnNextTurn.onClick.AddListener(() =>
        {
            ActionRequestSender.Instance.NextTurnServerRpc();
        });
    }

    private void OnDestroy()
    {
        btnNextTurn.onClick.RemoveAllListeners();
    }
}
