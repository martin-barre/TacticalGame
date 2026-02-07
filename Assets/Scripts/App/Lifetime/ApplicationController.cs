using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private string sceneNameToLoad;
    
    private void Start()
    {
        // TODO -> Subscribe to quit game message event
        Application.wantsToQuit += OnWantToQuit;
        Application.targetFrameRate = 120;
        SceneManager.LoadScene(sceneNameToLoad);
    }

    /// <summary>
    /// In builds, if we are in a Session and try to send a Leave request on application quit, it won't go through if we're quitting on the same frame.
    /// So, we need to delay just briefly to let the request happen (though we don't need to wait for the result).
    /// </summary>
    private IEnumerator LeaveBeforeQuit()
    {
        // We want to quit anyways, so if anything happens while trying to leave the Session, log the exception then carry on
        // try
        // {
        //     m_MultiplayerServicesFacade.EndTracking();
        // }
        // catch (Exception e)
        // {
        //     Debug.LogError(e.Message);
        // }

        yield return null;
        Application.Quit();
    }
    
    private bool OnWantToQuit()
    {
        Application.wantsToQuit -= OnWantToQuit;

        bool canQuit = true; // m_LocalSession != null && string.IsNullOrEmpty(m_LocalSession.SessionID);
        if (!canQuit)
        {
            StartCoroutine(LeaveBeforeQuit());
        }

        return canQuit;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
