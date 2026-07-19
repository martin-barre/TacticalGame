using System.Threading.Tasks;

public interface IAuthServiceFacade
{
    Task InitializeAndSignInAsync();
    Task<bool> EnsurePlayerIsAuthorized();
}
