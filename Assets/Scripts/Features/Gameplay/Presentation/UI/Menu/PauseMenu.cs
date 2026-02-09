using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnQuit;

    [Header("Parameters")]
    [SerializeField] private string mainMenuSceneName;
    
    private void OnEnable()
    {
        btnResume.onClick.AddListener(OnResume);
        btnQuit.onClick.AddListener(OnQuit);
    }

    private void OnDisable()
    {
        btnResume.onClick.RemoveAllListeners();
        btnQuit.onClick.RemoveAllListeners();
    }

    private void OnResume() => Debug.Log("OnResume");
    
    private void OnQuit() => SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
}
