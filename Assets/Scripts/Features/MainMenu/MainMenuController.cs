using System;
using VContainer;
using Unity.Services.Authentication;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Inject] private IAuthServiceFacade _authServiceFacade;
    
    private void Awake()
    {
        TrySignIn();
    }
    
    private async void TrySignIn() 
    {
        try
        {
            await _authServiceFacade.InitializeAndSignInAsync();
            Debug.Log($"Signed in. Unity Player ID {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception)
        {
            Debug.LogError("Failed to sign in.");
        }
    }
}
