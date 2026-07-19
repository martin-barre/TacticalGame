using VContainer;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpriteHoverDetector : NetworkClientBehaviour
{
    [SerializeField] private EntityInfoWindow entityInfoWindow;

    [Inject] private MapManager _mapManager;
    [Inject] private GameplayClientState _clientState;

    private void Update()
    {
        if (!_clientState.IsInitialized)
        {
            return;
        }

        TooltipUI.Instance.Hide<EntityOverUI>();
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Node node = _clientState.Map.GetNode(_mapManager.WorldToGrid(mousePosition));
        if (node.NodeType != NodeType.Invalid)
        {
            Entity entity = _clientState.GameState.GetEntityByGridPosition(node.GridPosition);
            if (entity != null)
            {
                EntityPrefabController entityPrefabController = _clientState.GetEntityPrefab(entity.Id);
                if (entityPrefabController != null)
                {
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(entityPrefabController.overHeadPosition.transform.position);
                    TooltipUI.Instance.Show<EntityOverUI>(screenPos, TooltipPosition.Top, tooltip => tooltip.SetUI(entity));

                    if (Mouse.current.rightButton.wasPressedThisFrame)
                    {
                        EntityInfoWindow window = WindowManager.Instance.CreateWindow(entityInfoWindow);
                        window.Bind(entity);
                    }
                }
            }
        }
    }
}
