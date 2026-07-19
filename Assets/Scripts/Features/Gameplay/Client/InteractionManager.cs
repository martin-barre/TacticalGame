using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using VContainer;
using UnityEngine.InputSystem;

public class InteractionManager : ClientStateBoundBehaviour
{
    [SerializeField] private GameObject txtDamageUI;

    private Spell _selectedSpell;
    private List<Node> _activeNodes = new();
    private List<Node> _unselectableNodes = new();
    private GameStateViewModel _gameStateViewModel;

    [Inject] private MapManager _mapManager;
    [Inject] private IPublisher<IGameplayCommand> _gameplayCommandsReceivedPublisher;

    protected override void BindClientState()
    {
        _gameStateViewModel = ViewModelFactory.Game.GetOrCreate(ClientState.GameState);
        _gameStateViewModel.CurrentEntityIndex.OnValueChanged += OnCurrentEntityIndexChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (ClientState.IsInitialized)
        {
            ViewModelFactory.Game.Release(ClientState.GameState);
        }

        if (_gameStateViewModel != null)
        {
            _gameStateViewModel.CurrentEntityIndex.OnValueChanged -= OnCurrentEntityIndexChanged;
        }
    }

    private void OnCurrentEntityIndexChanged(int entityIndex)
    {
        DisplayMovementNode();
    }

    private void Update()
    {
        if (!ClientState.IsInitialized)
        {
            return;
        }

        if (Keyboard.current.qKey.wasPressedThisFrame) DisplaySpellNodeByIndex(0);
        if (Keyboard.current.wKey.wasPressedThisFrame) DisplaySpellNodeByIndex(1);
        if (Keyboard.current.eKey.wasPressedThisFrame) DisplaySpellNodeByIndex(2);
        if (Keyboard.current.rKey.wasPressedThisFrame) DisplaySpellNodeByIndex(3);

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Node node = ClientState.Map.GetNode(_mapManager.WorldToGrid(mousePosition));

        if (Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
        {
            if (_selectedSpell != null) // SI JE SUIS EN TRAIN DE LANCER UN SORT
            {
                if (_activeNodes.Contains(node)) // SI JE CLICK SUR UNE CASE ACTIVE -> LANCE LE SORT
                {
                    _gameplayCommandsReceivedPublisher.Publish(new GameplayCommandLaunchSpell
                    {
                        SpellId = _selectedSpell.Id,
                        Position = node.GridPosition
                    });
                    _selectedSpell = null;
                    _mapManager.SetSelectableCells();
                    _mapManager.SetBlockedCells();
                }
            }
            else // SI JE SUIS EN MODE DEPLACEMENT
            {
                if (_activeNodes.Contains(node)) // SI JE CLICK SUR UNE CASE ACTIVE -> DEPLACE LE JOUEUR
                {
                    _gameplayCommandsReceivedPublisher.Publish(new GameplayCommandMove
                    {
                        Position = node.GridPosition
                    });
                }
            }
            DisplayMovementNode();
        }

        _mapManager.SetPreviewCells();

        if (_activeNodes.Contains(node))
        {
            Entity entity = ClientState.GameState.CurrentEntity;
            if (_selectedSpell != null)
            {
                List<Node> zone = _selectedSpell.GetZoneNodes(entity.GridPosition, node.GridPosition, ClientState.Map);
                _mapManager.SetPreviewCells(zone.ToArray());
            }
            else
            {
                List<Node> path = BFS.GetPath(entity.GridPosition, node.GridPosition, ClientState.GameState, ClientState.Map);
                _mapManager.SetPreviewCells(path.ToArray());
            }
        }
    }

    public void DisplayMovementNode()
    {
        if (!ClientState.IsInitialized || ClientState.GameState.Entities.Count == 0)
        {
            return;
        }

        Entity entity = ClientState.GameState.CurrentEntity;
        _selectedSpell = null;
        if (ClientState.Team == entity.Team && entity.IsPlayer)
        {
            _activeNodes = BFS.GetReachableCells(entity.GridPosition, entity.Pm, ClientState.GameState, ClientState.Map)
                .Select(rc => ClientState.Map.GetNode(rc.Position))
                .ToList();
            _mapManager.SetSelectableCells(_activeNodes.ToArray());
            _mapManager.SetBlockedCells();
        }
        else
        {
            _activeNodes.Clear();
            _mapManager.SetSelectableCells();
            _mapManager.SetBlockedCells();
        }
    }

    public void DisplaySpellNodeByIndex(int spellIndex)
    {
        if (!ClientState.IsInitialized || ClientState.GameState.Entities.Count == 0)
        {
            return;
        }

        Entity entity = ClientState.GameState.CurrentEntity;
        if (spellIndex < 0 || spellIndex >= entity.Race.Spells.Count) return;
        DisplaySpellNode(entity.Race.Spells[spellIndex].Id);
    }

    public void DisplaySpellNode(int spellId)
    {
        if (!ClientState.IsInitialized || ClientState.GameState.Entities.Count == 0)
        {
            return;
        }

        Spell spell = SpellDatabase.GetById(spellId);
        if (spell == null) return;

        Entity entity = ClientState.GameState.CurrentEntity;
        if (ClientState.Team != entity.Team) return;
        if (spell == null) return;
        if (entity.Pa < spell.paCost) return;

        _selectedSpell = spell;
        _activeNodes = FOV.GetDisplacement(entity, _selectedSpell, ClientState.GameState, ClientState.Map);

        if (_selectedSpell.xRay) _unselectableNodes.Clear();
        if (_selectedSpell.xRay) _unselectableNodes.Clear();
        else _unselectableNodes = FOV.GetDisplacement(entity, _selectedSpell, ClientState.GameState, ClientState.Map, true);

        _mapManager.SetSelectableCells(_activeNodes.ToArray());
        _mapManager.SetBlockedCells(_unselectableNodes.ToArray());
    }

    public static void ShowInfo(string message, Vector3 position, Color color, float duration = 1f)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, position);

        // Crée un nouvel objet texte
        GameObject textObject = new("InfoText");
        textObject.transform.SetParent(canvas.transform, false);

        // Ajoute TextMeshPro et configure
        TextMeshProUGUI textMesh = textObject.AddComponent<TextMeshProUGUI>();
        textMesh.text = message;
        textMesh.color = color;
        textMesh.fontSize = 32;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.raycastTarget = false;

        // Ajoute RectTransform et place correctement l'objet dans le Canvas
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(300, 100);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Convertit la position monde en UI
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPosition, canvas.worldCamera, out Vector2 uiPosition);
        rectTransform.anchoredPosition = uiPosition;

        // Anime la montée, le scale et le fade-out
        textObject.transform.localScale = Vector3.zero;
        textObject.transform.DOScale(1f, duration).SetEase(Ease.OutElastic,0.5f);
        rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + 50f, duration).SetEase(Ease.OutElastic, 0.5f);
        textMesh.DOFade(0, duration).SetEase(Ease.OutSine).SetDelay(0.2f).OnComplete(() => Destroy(textObject));
    }
}
