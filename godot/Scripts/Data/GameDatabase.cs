using System.Text.Json;
using Godot;

namespace LogQuest.Data;

public static class GameDatabase
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	public static GameData Load()
	{
		var classesFile = ReadJson<ClassesFile>("res://Data/classes.json");
		return new GameData
		{
			Classes = classesFile.ClassData,
			Skills = classesFile.ClassSkills,
			Regions = ReadJson<List<RegionDefinition>>("res://Data/regions.json"),
			Monsters = ReadJson<Dictionary<string, MonsterDefinition>>("res://Data/monsters.json"),
			Items = ReadJson<ItemData>("res://Data/items.json"),
			Events = ReadJson<List<ExplorationEventDefinition>>("res://Data/events.json")
		};
	}

	private static T ReadJson<T>(string path)
	{
		var json = Godot.FileAccess.GetFileAsString(path);
		if (string.IsNullOrWhiteSpace(json))
		{
			throw new InvalidOperationException($"Data file is empty or missing: {path}");
		}

		return JsonSerializer.Deserialize<T>(json, JsonOptions)
			?? throw new InvalidOperationException($"Could not parse data file: {path}");
	}
}
