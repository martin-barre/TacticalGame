using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapController : MonoBehaviour
{
    [SerializeField] private string sceneNameToLoad;

    private void Start()
    {
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
        yield return null;
        Application.Quit();
    }

    private bool OnWantToQuit()
    {
        Application.wantsToQuit -= OnWantToQuit;

        StartCoroutine(LeaveBeforeQuit());

        return true;
    }
}
