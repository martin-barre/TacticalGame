using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Unity.Netcode;
using Debug = UnityEngine.Debug;

public class InteractionManager : MonoSingleton<InteractionManager>
{
    [SerializeField] private GameObject txtDamageUI;
    
    private Spell _selectedSpell;
    private List<Node> _activeNodes = new();
    private GameStateViewModel _gameStateViewModel;

    private void Start()
    {
        _gameStateViewModel = ViewModelFactory.Game.GetOrCreate(GameManagerClient.Instance.GameState);
        _gameStateViewModel.CurrentEntityIndex.OnValueChanged += OnCurrentEntityIndexChanged;
        
        // for (int i = 0; i < GameManagerClient.Instance.Map.Width; i++)
        // {
        //     for (int j = 0; j < GameManagerClient.Instance.Map.Height; j++)
        //     {
        //         Node node = GameManagerClient.Instance.Map.GetNode(new Vector2Int(i, j));
        //         if (node != null)
        //         {
        //             CreateText($"{i},{j}", node.WorldPosition);
        //         }
        //     }
        // }
    }
    
    private void OnDestroy()
    {
        ViewModelFactory.Game.Release(GameManagerClient.Instance.GameState);
        _gameStateViewModel.CurrentEntityIndex.OnValueChanged -= OnCurrentEntityIndexChanged;
    }

    private void OnCurrentEntityIndexChanged(int entityIndex)
    {
        DisplayMovementNode();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) DisplaySpellNodeByIndex(0);
        if (Input.GetKeyDown(KeyCode.W)) DisplaySpellNodeByIndex(1);
        if (Input.GetKeyDown(KeyCode.E)) DisplaySpellNodeByIndex(2);
        if (Input.GetKeyDown(KeyCode.R)) DisplaySpellNodeByIndex(3);
        
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Node node = GameManagerClient.Instance.Map.GetNode(mousePosition);

        if (Input.GetButtonDown("Fire1") && !EventSystem.current.IsPointerOverGameObject())
        {
            if (_selectedSpell != null) // SI JE SUIS EN TRAIN DE LANCER UN SORT
            {
                if (_activeNodes.Contains(node)) // SI JE CLICK SUR UNE CASE ACTIVE -> LANCE LE SORT
                {
                    ActionRequestSender.Instance.LaunchSpellServerRpc(_selectedSpell.Id, node.GridPosition);
                    _selectedSpell = null;
                    MapManager.Instance.SetOverlay1();
                }
            }
            else // SI JE SUIS EN MODE DEPLACEMENT
            {
                if (_activeNodes.Contains(node)) // SI JE CLICK SUR UNE CASE ACTIVE -> DEPLACE LE JOUEUR
                {
                    ActionRequestSender.Instance.MoveServerRpc(node.GridPosition);
                }
            }
            DisplayMovementNode();
        }

        MapManager.Instance.SetOverlay2(); 
        
        if (_activeNodes.Contains(node))
        {
            Entity entity = GameManagerClient.Instance.GameState.CurrentEntity;
            if (_selectedSpell != null)
            {
                List<Node> zone = _selectedSpell.GetZoneNodes(entity.GridPosition, node.GridPosition, GameManagerClient.Instance.Map);
                MapManager.Instance.SetOverlay2(zone.ToArray());
            }
            else
            {
                List<Node> path = BFS.GetPath(entity.GridPosition, node.GridPosition, GameManagerClient.Instance.GameState, GameManagerClient.Instance.Map);
                MapManager.Instance.SetOverlay2(path.ToArray());
            }
        }
    }

    public void DisplayMovementNode()
    {
        Entity entity = GameManagerClient.Instance.GameState.CurrentEntity; 
        _selectedSpell = null;
        if (GameManagerClient.Instance.Team == entity.Team && entity.IsPlayer)
        {
            _activeNodes = BFS.GetReachableCells(entity.GridPosition, entity.Pm, GameManagerClient.Instance.GameState, GameManagerClient.Instance.Map)
                .Select(rc => GameManagerClient.Instance.Map.GetNode(rc.Position))
                .ToList();
            MapManager.Instance.SetOverlay1(_activeNodes.ToArray());
        }
        else
        {
            _activeNodes.Clear();
            MapManager.Instance.SetOverlay1();
        }
    }
    
    public void DisplaySpellNodeByIndex(int spellIndex)
    {
        Entity entity = GameManagerClient.Instance.GameState.CurrentEntity;
        if (spellIndex < 0 || spellIndex >= entity.Race.Spells.Count) return;
        DisplaySpellNode(entity.Race.Spells[spellIndex].Id);
    }
    
    public void DisplaySpellNode(int spellId)
    {
        Spell spell = SpellDatabase.GetById(spellId);
        if (spell == null) return;
        
        Entity entity = GameManagerClient.Instance.GameState.CurrentEntity;
        if (GameManagerClient.Instance.Team != entity.Team) return;
        if (spell == null) return;
        if (entity.Pa < spell.paCost) return;

        _selectedSpell = spell;
        _activeNodes = FOV.GetDisplacement(entity, _selectedSpell, GameManagerClient.Instance.GameState, GameManagerClient.Instance.Map);
        MapManager.Instance.SetOverlay1(_activeNodes.ToArray());
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
    
    public static void CreateText(string message, Vector3 position)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
    
        // Crée un nouvel objet texte
        GameObject textObject = new("InfoText");
        textObject.transform.SetParent(canvas.transform, false);

        // Ajoute TextMeshPro et configure
        TextMeshProUGUI textMesh = textObject.AddComponent<TextMeshProUGUI>();
        textMesh.text = message;
        textMesh.color = Color.black;
        textMesh.fontSize = 22;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.raycastTarget = false;

        // Ajoute RectTransform et place correctement l'objet dans le Canvas
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(300, 100);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Convertit la position monde en UI
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPosition, canvas.worldCamera, out Vector2 uiPosition);
        rectTransform.anchoredPosition = uiPosition;
    }
}
