using System.Text.Json;
using System.Text.Json.Serialization;

public static class JsonMessageLoader
{
    public static async Task<IReadOnlyList<EventMessage>> LoadAllAsync(string? explicitPath = null)
    {
        var path = explicitPath ?? "zdrojovy_dokument.json";

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Could not find 'zdrojovy_dokument.json' at '{path}'.");
        }

        var json = await File.ReadAllTextAsync(path);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.Strict
        };

        var messages = JsonSerializer.Deserialize<List<EventMessage>>(json, options)
                       ?? new List<EventMessage>();

        return messages;
    }
}
