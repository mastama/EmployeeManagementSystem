using Blazored.LocalStorage;

namespace ClientLibrary.Helpers;

public class LocalStorageService
{
    private readonly ILocalStorageService _localStorageService;
    private const string StorageKey = "authentication-token";

    public LocalStorageService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async Task<string?> GetTokenAsync() => 
        await _localStorageService.GetItemAsStringAsync(StorageKey);

    public async Task SetTokenAsync(string token) => 
        await _localStorageService.SetItemAsStringAsync(StorageKey, token);

    public async Task RemoveTokenAsync() => 
        await _localStorageService.RemoveItemAsync(StorageKey);
}
