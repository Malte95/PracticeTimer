using System.IO;
using System.Text.Json;


public static class PresetLoader
{
    public static Preset Load(string path)
    {
        var json = File.ReadAllText(path);

        var preset = JsonSerializer.Deserialize<Preset>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });


        if (preset == null)
            throw new InvalidDataException("Preset file is invalid.");

        return preset;
    }
}
