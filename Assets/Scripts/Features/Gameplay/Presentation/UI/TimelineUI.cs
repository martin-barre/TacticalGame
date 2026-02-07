using System.Collections;
using DG.Tweening;
using UnityEngine;

public class TimelineUI : MonoBehaviour
{
    [SerializeField] private RectTransform contentEntities;
    [SerializeField] private RectTransform panelCurrentEntity;
    [SerializeField] private TimelineEntityUI timelineEntityUI;

    private GameStateViewModel _gameStateViewModel;
    private void Start()
    {
        _gameStateViewModel = ViewModelFactory.Game.GetOrCreate(GameManagerClient.Instance.GameState);
        _gameStateViewModel.CurrentEntityIndex.OnValueChanged += OnCurrentEntityIndexChanged;
        _gameStateViewModel.Entities.OnListChanged += OnEntitiesChanged;
    }
    
    private void OnDestroy()
    {
        _gameStateViewModel.CurrentEntityIndex.OnValueChanged -= OnCurrentEntityIndexChanged;
        _gameStateViewModel.Entities.OnListChanged -= OnEntitiesChanged;
    }

    private void OnEntitiesChanged()
    {
        // Destroy all
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        // Recreate
        foreach (Entity entity in GameManagerClient.Instance.GameState.Entities)
        {
            TimelineEntityUI instance = Instantiate(timelineEntityUI, transform);
            instance.Bind(entity);
        }

        StartCoroutine(WaitForNextFrame());
    }

    private void OnCurrentEntityIndexChanged(int entityIndex)
    {
        if (entityIndex < 0 || entityIndex >= transform.childCount) return;
        
        RectTransform canvasRectTransform = FindAnyObjectByType<Canvas>().GetComponent<RectTransform>();
        
        // Récupérer la position mondiale de l'élément UI
        Vector3 worldPosition = contentEntities.GetChild(entityIndex).transform.position;
        // Convertir la position mondiale en position locale par rapport au Canvas
        Vector3 localPosition = canvasRectTransform.InverseTransformPoint(worldPosition);
        
        panelCurrentEntity.DOLocalMove(localPosition, 0.5f).SetEase(Ease.OutSine);
    }

    IEnumerator WaitForNextFrame()
    {
        yield return null;
        OnCurrentEntityIndexChanged(_gameStateViewModel.CurrentEntityIndex.Value);
    }
}
