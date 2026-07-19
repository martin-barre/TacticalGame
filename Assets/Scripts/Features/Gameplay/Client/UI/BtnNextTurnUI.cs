using VContainer;
using UnityEngine;
using UnityEngine.UI;

public class BtnNextTurnUI : MonoBehaviour
{
    [SerializeField] private Button btnNextTurn;

    [Inject] private IPublisher<IGameplayCommand> _gameplayCommandsReceivedPublisher;

    private void Start()
    {
        btnNextTurn.onClick.AddListener(() => _gameplayCommandsReceivedPublisher.Publish(new GameplayCommandNextTurn()));
    }

    private void OnDestroy()
    {
        btnNextTurn.onClick.RemoveAllListeners();
    }
}
