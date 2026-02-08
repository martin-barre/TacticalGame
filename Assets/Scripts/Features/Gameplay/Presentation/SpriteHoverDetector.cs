using UnityEngine;
using UnityEngine.InputSystem;

public class SpriteHoverDetector : MonoSingleton<SpriteHoverDetector>
{
    [SerializeField] private EntityInfoWindow entityInfoWindow;
    
    private void Update()
    {
        TooltipUI.Instance.Hide<EntityOverUI>();
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Node node = GameManagerClient.Instance.Map.GetNode(mousePosition);
        if (node.NodeType != NodeType.Invalid)
        {
            Entity entity = GameManagerClient.Instance.GameState.GetEntityByGridPosition(node.GridPosition);
            if (entity != null)
            {
                EntityPrefabController entityPrefabController = GameManagerClient.Instance.GetEntityPrefab(entity.Id);
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
