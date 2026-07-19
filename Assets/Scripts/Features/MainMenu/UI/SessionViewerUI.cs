using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

public class SessionViewerUI : MonoBehaviour
{
    [Header("Session List")]
    [SerializeField] private GameObject sessionListPanel;
    [SerializeField] private SessionInfoUI sessionInfoUI;
    [SerializeField] private GameObject sessionListContent;
    
    [Header("Session Creation")]
    [SerializeField] private GameObject sessionCreationPanel;
    [SerializeField] private TMP_InputField inputFieldName;
    [SerializeField] private TMP_InputField inputFieldPassword;
    [SerializeField] private Button btnCreate;
    
    [Header("Current Session")]
    [SerializeField] private GameObject currentSessionPanel;
    [SerializeField] private TMP_Text sessionName;
    [SerializeField] private GameObject playerListContent;
    [SerializeField] private PlayerInfoUI playerInfoUI;
    [SerializeField] private Button btnLaunchGame;
    [SerializeField] private Button btnQuitSession;

    [Inject] private ISessionServiceFacade _sessionServiceFacade;
    [Inject] private IObjectResolver _resolver;
    
    private ISession _currentSession;
    private Coroutine _coroutineRefreshSessionList;
    
    private void Awake()
    {
        _sessionServiceFacade.CurrentSession.OnValueChanged += SetCurrentSession;
        
        btnCreate.interactable = false;
        inputFieldName.onValueChanged.AddListener(_ => ValidateCreateButton());
        inputFieldPassword.onValueChanged.AddListener(_ => ValidateCreateButton());
        btnCreate.onClick.AddListener(CreateSession);
        
        btnLaunchGame.onClick.AddListener(LaunchGame);
        btnQuitSession.onClick.AddListener(QuitSession);
    }

    private void Start()
    {
        ShowSessionListPanel();
    }

    private void ValidateCreateButton() => btnCreate.interactable = inputFieldName.text.Length >= 4 && inputFieldPassword.text.Length is 0 or >= 8;

    private void ShowSessionListPanel()
    {
        sessionListPanel.SetActive(true);
        sessionCreationPanel.SetActive(true);
        currentSessionPanel.SetActive(false);
        
        _coroutineRefreshSessionList = StartCoroutine(RefreshSessionList());
    }
    
    private void ShowCurrentSessionPanel()
    {
        sessionListPanel.SetActive(false);
        sessionCreationPanel.SetActive(false);
        currentSessionPanel.SetActive(true);

        inputFieldName.text = "";
        
        if (_coroutineRefreshSessionList != null)
        {
            StopCoroutine(_coroutineRefreshSessionList);
        }
    }

    private void SetCurrentSession(ISession session)
    {
        if (_currentSession != null)
        {
            _currentSession.Changed -= RefreshPlayerList;
        }
        
        _currentSession = session;
        
        if (_currentSession != null)
        {
            _currentSession.Changed += RefreshPlayerList;
            
            RefreshPlayerList();
            sessionName.text = "Session : " + _currentSession.Name;
            btnLaunchGame.interactable = _currentSession.IsHost && _currentSession.PlayerCount >= _currentSession.MaxPlayers;
            ShowCurrentSessionPanel();
        }
        else
        {
            ShowSessionListPanel();
        }
    }

    private void RefreshPlayerList()
    {
        btnLaunchGame.interactable = _currentSession.IsHost && _currentSession.PlayerCount >= _currentSession.MaxPlayers;
        
        foreach (Transform child in playerListContent.transform)
            Destroy(child.gameObject);

        foreach (IReadOnlyPlayer player in _currentSession?.Players ?? new List<IReadOnlyPlayer>())
        {
            PlayerInfoUI instance = Instantiate(playerInfoUI, playerListContent.transform);
            _resolver.InjectGameObject(instance.gameObject);
            instance.SetInfo(player, _currentSession);
        }
    }
    
    private IEnumerator RefreshSessionList()
    {
        while (true)
        {
            Task<IList<ISessionInfo>> task = _sessionServiceFacade.GetAllSessions();
            yield return new WaitUntil(() => task.IsCompleted);
            
            IList<ISessionInfo> sessionInfos = task.Result;
            foreach (Transform child in sessionListContent.transform)
            {
                Destroy(child.gameObject);
            }
            
            foreach (ISessionInfo sessionInfo in sessionInfos)
            {
                SessionInfoUI instance = Instantiate(sessionInfoUI, sessionListContent.transform);
                _resolver.InjectGameObject(instance.gameObject);
                instance.SetInfo(sessionInfo);
            }

            yield return new WaitForSeconds(1);
        }
    }
    
    private void CreateSession()
    {
        _ = _sessionServiceFacade.TryCreateSessionAsync(inputFieldName.text, inputFieldPassword.text, 2);
    }

    private void QuitSession()
    {
        _ = _sessionServiceFacade.LeaveSessionAsync();
    }

    private void LaunchGame()
    {
        _sessionServiceFacade.LaunchGame();
    }
}
