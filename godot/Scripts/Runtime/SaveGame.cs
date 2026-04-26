using System.Text.Json;
using Godot;

namespace LogQuest.Runtime;

public static class SaveGame
{
    private const string SavePath = "user://logquest-save.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static GameState? Load()
    {
        if (!Godot.FileAccess.FileExists(SavePath))
        {
            return null;
        }

        var json = Godot.FileAccess.GetFileAsString(SavePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<GameState>(json, JsonOptions);
    }

    public static void Save(GameState state)
    {
        using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(state, JsonOptions));
    }

    public static void Reset()
    {
        if (Godot.FileAccess.FileExists(SavePath))
        {
            DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(SavePath));
        }
    }
}
