using System.Net.Http.Json;
using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ClientLibrary.Helpers;
using ClientLibrary.Services.Contracts;

namespace ClientLibrary.Services.Implementations;

public class UserAccountService : IUserAccountService
{
    private readonly GetHttpClient _getHttpClient;

    // Constructor
    public UserAccountService(GetHttpClient getHttpClient)
    {
        _getHttpClient = getHttpClient;
    }

    public const string AuthUrl = "/api/authentication";
    
    public async Task<GeneralResponse> CreateAsync(Register user)
    {
        var httpClient = _getHttpClient.GetPublicHttpClient();
        var result = await httpClient.PostAsJsonAsync($"{AuthUrl}", user);
        if (!result.IsSuccessStatusCode)
        {
            return new GeneralResponse("401", "Unauthorized, Error occured", null);
        }
        return await result.Content.ReadFromJsonAsync<GeneralResponse>();
    }

    public async Task<GeneralResponse> SignInAsync(Login user)
    {
        var httpClient = _getHttpClient.GetPublicHttpClient();
        var result = await httpClient.PostAsJsonAsync($"{AuthUrl}/login", user);
        if (!result.IsSuccessStatusCode)
        {
            return new GeneralResponse("401", "Unauthorized, Error occured", null);
        }
        return await result.Content.ReadFromJsonAsync<GeneralResponse>();
    }

    public Task<GeneralResponse> RefreshTokenAsync(RefreshToken token)
    {
        throw new NotImplementedException();
    }
}