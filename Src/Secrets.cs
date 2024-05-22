using Newtonsoft.Json;

namespace GaWo;

public class Secrets
{
    [JsonProperty("username")] public string Username { get; private set; } = string.Empty;

    [JsonProperty("password")] public string Password { get; private set; } = string.Empty;

    [JsonProperty("emailpassword")] public string EmailPassword { get; private set; } = string.Empty;

    // Cannot use constructor because JSON deserialisation would create a new instance causing an endless loop
    public static async Task<Secrets> Get()
    {
        // Read secrets from text/secrets.json into attributes
        using StreamReader r = new(Constants.SecretsPath);
        var secrets = JsonConvert.DeserializeObject<Secrets>(await r.ReadToEndAsync());

        if (secrets is null)
        {
            throw new Exception($"Failed to read secrets from {Constants.SecretsPath}");
        }

        return secrets;
    }
}