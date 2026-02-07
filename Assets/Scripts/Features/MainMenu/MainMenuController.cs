using System;
using Reflex.Attributes;
using Unity.Services.Authentication;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Inject] private readonly AuthenticationServiceFacade _authenticationServiceFacade;
    
    private void Awake()
    {
        TrySignIn();
    }
    
    private async void TrySignIn() 
    {
        try
        {
            await _authenticationServiceFacade.InitializeAndSignInAsync();
            Debug.Log($"Signed in. Unity Player ID {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception)
        {
            Debug.LogError("Failed to sign in.");
        }
    }
}