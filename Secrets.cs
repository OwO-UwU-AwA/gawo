using Newtonsoft.Json;

namespace GaWo;

public class Secrets
{
    [JsonProperty("namespace")] public string Namespace { get; private set; } = string.Empty;

    [JsonProperty("database")] public string Database { get; private set; } = string.Empty;

    [JsonProperty("username")] public string Username { get; private set; } = string.Empty;

    [JsonProperty("password")] public string Password { get; private set; } = string.Empty;

    [JsonProperty("emailpassword")] public string EmailPassword { get; private set; } = string.Empty;

    public static async Task<Secrets> Get()
    {
        using StreamReader r = new("~/secrets.json");
        var json = await r.ReadToEndAsync();
        var secrets = JsonConvert.DeserializeObject<Secrets>(json)!;

        return secrets;
    }
}