using System.Net.Http.Headers;
using BaseLibrary.DTOs;
using Microsoft.Extensions.Logging;

namespace ClientLibrary.Helpers;

// Kelas ini menyediakan metode untuk mengelola HttpClient, baik untuk permintaan yang memerlukan autentikasi (private) 
// maupun tanpa autentikasi (public).
public class GetHttpClient
{
    // Field untuk menyimpan instance IHttpClientFactory yang digunakan untuk membuat HttpClient.
    private readonly IHttpClientFactory _httpClientFactory;

    // Field untuk menyimpan instance LocalStorageService yang digunakan untuk mengelola token autentikasi.
    private readonly LocalStorageService _localStorageService;
    
    private readonly ILogger<GetHttpClient> _logger;

    // Konstruktor untuk menginisialisasi dependensi (IHttpClientFactory dan LocalStorageService).
    public GetHttpClient(IHttpClientFactory httpClientFactory, LocalStorageService localStorageService, ILogger<GetHttpClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _localStorageService = localStorageService;
        _logger = logger;
    }
    
    // Kunci header yang digunakan untuk menambahkan autentikasi.
    private const string HeaderKey = "Authorization";

    // Metode untuk mendapatkan HttpClient yang memerlukan autentikasi menggunakan token.
    public async Task<HttpClient> GetPrivateHttpClient()
    {
        // Membuat HttpClient baru menggunakan IHttpClientFactory.
        var client = _httpClientFactory.CreateClient("SystemApiClient");
        
        try
        {
            // Mengambil token autentikasi dari local storage.
            var stringToken = await _localStorageService.GetTokenAsync();

            // Jika token tidak ditemukan, kembalikan HttpClient tanpa header Authorization.
            if (string.IsNullOrEmpty(stringToken))
            {
                _logger.LogWarning("Token is empty. Returning HttpClient without Authorization header.");
                return client;
            }

            // Deserialize token ke dalam objek UserSession.
            var deserializeToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
            if (deserializeToken == null)
            {
                _logger.LogWarning("Failed to deserialize token. Returning HttpClient without Authorization header.");
                return client;
            }

            // Periksa validitas token (misalnya expired).
            if (deserializeToken.Expires <= DateTime.UtcNow)
            {
                _logger.LogWarning("Token has expired. Returning HttpClient without Authorization header.");
                return client;
            }

            // Tambahkan header Authorization dengan format Bearer Token.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", deserializeToken.Token);
            _logger.LogInformation("Authorization header added to HttpClient.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while configuring HttpClient with Authorization header.");
        }

        return client; // Kembalikan HttpClient dengan atau tanpa header Authorization.

    }

    // Metode untuk mendapatkan HttpClient tanpa autentikasi (public).
    public HttpClient GetPublicHttpClient()
    {
        var client = _httpClientFactory.CreateClient("SystemApiClient");

        try
        {
            // Menghapus header Authorization jika ada.
            client.DefaultRequestHeaders.Remove(HeaderKey);
            _logger.LogInformation("Authorization header removed from HttpClient.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while removing Authorization header from HttpClient.");
        }

        return client; // Kembalikan HttpClient tanpa header Authorization.
    }
}
