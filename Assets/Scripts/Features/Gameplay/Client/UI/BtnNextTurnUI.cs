using VContainer;
using UnityEngine;
using UnityEngine.UI;

public class BtnNextTurnUI : MonoBehaviour
{
    [SerializeField] private Button btnNextTurn;

    [Inject] private ActionRequestSender _actionRequestSender;

    private void Start()
    {
        btnNextTurn.onClick.AddListener(() => _actionRequestSender.NextTurnRpc());
    }

    private void OnDestroy()
    {
        btnNextTurn.onClick.RemoveAllListeners();
    }
}
