using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClientLibrary.Helpers;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly LocalStorageService _localStorageService;
    private readonly ClaimsPrincipal _anonymous = new (new ClaimsIdentity());

    public CustomAuthenticationStateProvider(LocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var stringToken = await _localStorageService.GetTokenAsync();
        if (string.IsNullOrEmpty(stringToken))
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
        var deserializeToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
        if (deserializeToken == null)
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
        var getUserClaims = DecyptToken(deserializeToken.Token!);
        if (getUserClaims == null)
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
        var claimsPrincipal = SetClaimPrincipal(getUserClaims);
        return await Task.FromResult(new AuthenticationState(claimsPrincipal));
    }

    public async Task UpdateAuthenticationState(UserSession userSession)
    {
        var claimsPrincipal = new ClaimsPrincipal();
        if (userSession.Token != null || userSession.RefreshToken != null)
        {
            var serializeSession = Serializations.SerializeObj(userSession);
            await _localStorageService.SetTokenAsync(serializeSession);
            var getUserClaims = DecyptToken(userSession.Token!);
            claimsPrincipal = SetClaimPrincipal(getUserClaims);
        }
        else
        {
            await _localStorageService.RemoveTokenAsync();
        }
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    private static CustomUserClaims DecyptToken(string jwtToken)
    {
        if (string.IsNullOrEmpty(jwtToken))
        {
            return new CustomUserClaims();
        }

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);
        
        var userId = token.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
        var name = token.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Name)?.Value;
        var email = token.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Email)?.Value;
        var role = token.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Role)?.Value;
        return new CustomUserClaims(userId!, name, email, role);
    }
    
    public static ClaimsPrincipal SetClaimPrincipal(CustomUserClaims claims)
    {
        if (claims.Email == null)
        {
            return new ClaimsPrincipal();
        }

        return new ClaimsPrincipal(new ClaimsIdentity(
            new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, claims.Id),
                new Claim(ClaimTypes.Name, claims.Name!),
                new Claim(ClaimTypes.Email, claims.Email!),
                new Claim(ClaimTypes.Role, claims.Role!)
            }, "JwtAuth"));
    }
}