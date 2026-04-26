using LogQuest.Data;

namespace LogQuest.Runtime;

public sealed class GameState
{
    public List<HeroState> Heroes { get; set; } = new();
    public List<string> SelectedHeroIds { get; set; } = new();
    public List<string> UnlockedRegions { get; set; } = new();
    public List<string> UnlockedClasses { get; set; } = new();
    public Dictionary<string, int> FloorCaps { get; set; } = new();
    public Dictionary<string, int> CompletedFloors { get; set; } = new();
    public List<ExpeditionRun> ActiveRuns { get; set; } = new();
    public List<LootItem> Inventory { get; set; } = new();
    public List<CombatLogEntry> LastLogs { get; set; } = new();
    public List<string> History { get; set; } = new();
}

public sealed class HeroState
{
    public string Id { get; set; } = "";
    public int Level { get; set; } = 1;
    public int Xp { get; set; }
    public bool Unlocked { get; set; }
}

public sealed class ExpeditionRun
{
    public string Id { get; set; } = "";
    public string RegionId { get; set; } = "";
    public string DungeonId { get; set; } = "";
    public int Floor { get; set; }
    public List<string> HeroIds { get; set; } = new();
    public long StartedAt { get; set; }
    public long EndAt { get; set; }
    public long DurationMs { get; set; }
    public CombatSimulation Simulation { get; set; } = new();
    public ExplorationPlan Exploration { get; set; } = new();
}

public sealed class ExplorationPlan
{
    public List<ExplorationEventRoll> Events { get; set; } = new();
    public List<ExplorationSegment> Segments { get; set; } = new();
}

public sealed class ExplorationEventRoll
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Text { get; set; } = "";
    public double At { get; set; }
    public int XpBonus { get; set; }
    public double LootChance { get; set; }
}

public sealed class ExplorationSegment
{
    public string Kind { get; set; } = "";
    public double Start { get; set; }
    public double End { get; set; }
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
}

public sealed class CombatSimulation
{
    public bool Success { get; set; }
    public int Xp { get; set; }
    public int Rounds { get; set; }
    public List<LootItem> Loot { get; set; } = new();
    public List<CombatLogEntry> Logs { get; set; } = new();
    public List<UnitSnapshot> Encounter { get; set; } = new();
    public List<UnitSnapshot> Team { get; set; } = new();
    public List<UnitSnapshot> EnemyTeam { get; set; } = new();
}

public sealed class UnitSnapshot
{
    public string Side { get; set; } = "";
    public string Id { get; set; } = "";
    public string ClassId { get; set; } = "";
    public string MonsterId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public int SpriteIndex { get; set; }
    public int MaxHp { get; set; }
    public int Hp { get; set; }
    public StatBlock Stats { get; set; } = new();
}

public sealed class CombatLogEntry
{
    public string Text { get; set; } = "";
    public string Type { get; set; } = "";
    public double RevealAt { get; set; }
    public int Amount { get; set; }
    public string TargetSide { get; set; } = "";
    public string TargetId { get; set; } = "";
}

public sealed class LootItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Rarity { get; set; } = "";
    public string Slot { get; set; } = "";
    public int SpriteIndex { get; set; }
    public StatBlock Stats { get; set; } = new();
}
