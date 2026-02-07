using System;
using System.Text;
using System.Threading.Tasks;
using Reflex.Attributes;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class AuthenticationServiceFacade
{
    [Inject] private readonly IMessageChannel<UnityServiceErrorMessage> _unityServiceErrorMessagePublisher;

    public async Task InitializeAndSignInAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await AuthenticationService.Instance.UpdatePlayerNameAsync("Guest");
                
                byte[] payload = Encoding.UTF8.GetBytes(AuthenticationService.Instance.PlayerId);
                NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;
            }
        }
        catch (Exception e)
        {
            string reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
            _unityServiceErrorMessagePublisher.Publish(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
            throw;
        }
    }

    public async Task<bool> EnsurePlayerIsAuthorized()
    {
        if (AuthenticationService.Instance.IsAuthorized)
        {
            return true;
        }

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            return true;
        }
        catch (AuthenticationException e)
        {
            string reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
            _unityServiceErrorMessagePublisher.Publish(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
            // not rethrowing for authentication exceptions - any failure to authenticate is considered "handled failure"
            return false;
        }
        catch (Exception e)
        {
            // all other exceptions should still bubble up as unhandled ones
            string reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
            _unityServiceErrorMessagePublisher.Publish(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
            throw;
        }
    }
}
