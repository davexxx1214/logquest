using System.Collections.Generic;

namespace LogQuest.Data;

public sealed class GameData
{
	public Dictionary<string, ClassDefinition> Classes { get; init; } = new();
	public Dictionary<string, List<ClassSkill>> Skills { get; init; } = new();
	public List<RegionDefinition> Regions { get; init; } = new();
	public Dictionary<string, MonsterDefinition> Monsters { get; init; } = new();
	public ItemData Items { get; init; } = new();
	public List<ExplorationEventDefinition> Events { get; init; } = new();
}

public sealed class ClassesFile
{
	public Dictionary<string, ClassDefinition> ClassData { get; set; } = new();
	public Dictionary<string, List<ClassSkill>> ClassSkills { get; set; } = new();
}

public sealed class ClassDefinition
{
	public string Name { get; set; } = "";
	public string Mark { get; set; } = "";
	public int SpriteIndex { get; set; }
	public string Unlock { get; set; } = "";
	public string Role { get; set; } = "";
	public string ColorA { get; set; } = "";
	public string ColorB { get; set; } = "";
	public StatBlock Base { get; set; } = new();
	public StatBlock Growth { get; set; } = new();
}

public sealed class ClassSkill
{
	public string Id { get; set; } = "";
	public int Level { get; set; }
	public string Name { get; set; } = "";
	public string Text { get; set; } = "";
}

public sealed class RegionDefinition
{
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public string Tone { get; set; } = "";
	public string Description { get; set; } = "";
	public List<string> Unlocks { get; set; } = new();
	public List<RegionReward> Rewards { get; set; } = new();
	public List<string> Scene { get; set; } = new();
	public List<string> Icon { get; set; } = new();
	public List<DungeonDefinition> Dungeons { get; set; } = new();
}

public sealed class RegionReward
{
	public int Floor { get; set; }
	public List<string> Classes { get; set; } = new();
	public List<string> Regions { get; set; } = new();
	public string Message { get; set; } = "";
}

public sealed class DungeonDefinition
{
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public int Floors { get; set; }
	public List<int> LevelRange { get; set; } = new();
	public List<string> MonsterIds { get; set; } = new();
	public List<string> LootTable { get; set; } = new();
}

public sealed class MonsterDefinition
{
	public string Name { get; set; } = "";
	public string Mark { get; set; } = "";
	public int SpriteIndex { get; set; }
	public string Family { get; set; } = "";
	public string ColorA { get; set; } = "";
	public string ColorB { get; set; } = "";
	public StatBlock Base { get; set; } = new();
}

public sealed class ItemData
{
	public List<RarityDefinition> Rarity { get; set; } = new();
	public Dictionary<string, string> SlotNames { get; set; } = new();
	public Dictionary<string, ItemDefinition> ItemCatalog { get; set; } = new();
	public List<AffixDefinition> AffixPool { get; set; } = new();
}

public sealed class RarityDefinition
{
	public string Name { get; set; } = "";
	public string ClassName { get; set; } = "";
	public double Chance { get; set; }
	public double Power { get; set; }
}

public sealed class ItemDefinition
{
	public string Slot { get; set; } = "";
	public List<string> Stats { get; set; } = new();
	public string Icon { get; set; } = "";
	public int SpriteIndex { get; set; }
}

public sealed class AffixDefinition
{
	public string Name { get; set; } = "";
	public StatBlock Stats { get; set; } = new();
	public double Weight { get; set; }
}

public sealed class ExplorationEventDefinition
{
	public string Id { get; set; } = "";
	public string Name { get; set; } = "";
	public string Type { get; set; } = "";
	public string Text { get; set; } = "";
	public int XpBonus { get; set; }
	public double LootChance { get; set; }
}

public sealed class StatBlock
{
	public int Hp { get; set; }
	public int Atk { get; set; }
	public int Mag { get; set; }
	public int Def { get; set; }
	public int Spd { get; set; }
	public int Crit { get; set; }
}
