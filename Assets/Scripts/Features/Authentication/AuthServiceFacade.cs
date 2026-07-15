using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class AuthServiceFacade
{
    public async Task InitializeAndSignInAsync()
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

    public async Task<bool> EnsurePlayerIsAuthorized()
    {
        if (AuthenticationService.Instance.IsAuthorized) return true;

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await AuthenticationService.Instance.UpdatePlayerNameAsync("Guest");
            return true;
        }
        catch
        {
            return false;
        }
    }
}
