namespace ServiceLibrary.Helpers;

public class JwtSection
{
    public string? Key { get; set; }
    public string? Audience { get; set; }
    public string? Issuer { get; set; }
    public int Lifetime { get; set; }
    
}