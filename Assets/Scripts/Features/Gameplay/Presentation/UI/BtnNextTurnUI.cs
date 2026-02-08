using UnityEngine;
using UnityEngine.UI;

public class BtnNextTurnUI : MonoBehaviour
{
    [SerializeField] private Button btnNextTurn;

    private void Start()
    {
        btnNextTurn.onClick.AddListener(() =>
        {
            ActionRequestSender.Instance.NextTurnRpc();
        });
    }

    private void OnDestroy()
    {
        btnNextTurn.onClick.RemoveAllListeners();
    }
}
