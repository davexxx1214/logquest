using LogQuest.Data;

namespace LogQuest.Runtime;

public sealed class GameLogic
{
    public const int TeamLimit = 3;
    public const int MaxActiveRuns = 2;

    private const int InitialFloorCap = 3;
    private static readonly string[] InitialRegions = ["meadow"];
    private static readonly string[] InitialClasses = ["warrior", "monk"];

    private readonly GameData _data;
    private readonly Random _rng = new();

    public GameLogic(GameData data)
    {
        _data = data;
    }

    public GameState CreateInitialState()
    {
        var state = new GameState
        {
            SelectedHeroIds = ["warrior", "monk"],
            UnlockedRegions = InitialRegions.ToList(),
            UnlockedClasses = InitialClasses.ToList()
        };

        foreach (var entry in _data.Classes)
        {
            state.Heroes.Add(new HeroState
            {
                Id = entry.Key,
                Level = InitialClasses.Contains(entry.Key) ? 2 : 1,
                Unlocked = InitialClasses.Contains(entry.Key)
            });
        }

        foreach (var region in _data.Regions)
        {
            foreach (var dungeon in region.Dungeons)
            {
                state.FloorCaps[dungeon.Id] = InitialRegions.Contains(region.Id) ? Math.Min(InitialFloorCap, dungeon.Floors) : 0;
            }
        }

        state.History.Add("营地建立。战士与僧侣已经待命。");
        return state;
    }

    public void NormalizeState(GameState state)
    {
        foreach (var entry in _data.Classes)
        {
            if (state.Heroes.All(hero => hero.Id != entry.Key))
            {
                state.Heroes.Add(new HeroState { Id = entry.Key, Level = 1, Unlocked = state.UnlockedClasses.Contains(entry.Key) });
            }
        }

        foreach (var region in _data.Regions)
        {
            foreach (var dungeon in region.Dungeons)
            {
                state.FloorCaps.TryAdd(dungeon.Id, state.UnlockedRegions.Contains(region.Id) ? Math.Min(InitialFloorCap, dungeon.Floors) : 0);
            }
        }

        state.SelectedHeroIds = state.SelectedHeroIds
            .Where(id => state.Heroes.Any(hero => hero.Id == id && hero.Unlocked))
            .Take(TeamLimit)
            .ToList();
        state.ActiveRuns = state.ActiveRuns.Take(MaxActiveRuns).ToList();
        state.Inventory = state.Inventory.TakeLast(48).ToList();
        state.History = state.History.TakeLast(80).ToList();
    }

    public ExpeditionRun StartRun(GameState state, string regionId, string dungeonId, int floor)
    {
        var region = _data.Regions.First(item => item.Id == regionId);
        var dungeon = region.Dungeons.First(item => item.Id == dungeonId);
        var heroIds = state.SelectedHeroIds.ToList();
        var simulation = SimulateBattle(state, region, dungeon, floor, heroIds);
        var exploration = CreateExplorationPlan(region, dungeon, floor, simulation);
        ApplyExplorationRewards(simulation, exploration, dungeon, floor);
        simulation.Logs = CreateTimedLogs(region, dungeon, floor, exploration, simulation);

        var now = NowMs();
        var run = new ExpeditionRun
        {
            Id = Guid.NewGuid().ToString("N"),
            RegionId = region.Id,
            DungeonId = dungeon.Id,
            Floor = floor,
            HeroIds = heroIds,
            StartedAt = now,
            DurationMs = GetDurationMs(floor),
            EndAt = now + GetDurationMs(floor),
            Simulation = simulation,
            Exploration = exploration
        };

        state.ActiveRuns.Add(run);
        state.SelectedHeroIds.Clear();
        state.History.Add($"{string.Join(" / ", heroIds.Select(HeroName))} 出发前往 {region.Name} 第 {floor} 层。");
        return run;
    }

    public List<ExpeditionRun> FinishReadyRuns(GameState state)
    {
        var now = NowMs();
        var finished = state.ActiveRuns.Where(run => now >= run.EndAt).ToList();
        foreach (var run in finished)
        {
            FinishRun(state, run);
        }

        state.ActiveRuns = state.ActiveRuns.Where(run => now < run.EndAt).ToList();
        return finished;
    }

    public bool CanStart(GameState state, string regionId, string dungeonId, int floor)
    {
        if (state.ActiveRuns.Count >= MaxActiveRuns || state.SelectedHeroIds.Count == 0)
        {
            return false;
        }

        if (!state.UnlockedRegions.Contains(regionId) || !state.FloorCaps.TryGetValue(dungeonId, out var cap) || floor > cap)
        {
            return false;
        }

        var busy = BusyHeroIds(state);
        return state.SelectedHeroIds.All(id => !busy.Contains(id));
    }

    public HashSet<string> BusyHeroIds(GameState state) => state.ActiveRuns.SelectMany(run => run.HeroIds).ToHashSet();

    public int GetDurationMs(int floor) => (18 + floor * 8) * 1000;

    public static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public int RequiredXp(int level) => 85 + level * level * 48;

    public StatBlock HeroStats(HeroState hero)
    {
        var data = _data.Classes[hero.Id];
        var levelBonus = Math.Max(0, hero.Level - 1);
        return Add(data.Base, Scale(data.Growth, levelBonus));
    }

    private void FinishRun(GameState state, ExpeditionRun run)
    {
        var region = _data.Regions.First(item => item.Id == run.RegionId);
        var dungeon = region.Dungeons.First(item => item.Id == run.DungeonId);
        var outcome = run.Simulation.Success ? "成功" : "失败";
        state.LastLogs = run.Simulation.Logs;
        state.History.Add($"{region.Name} / {dungeon.Name} 第 {run.Floor} 层探索{outcome}。");

        foreach (var heroId in run.HeroIds)
        {
            var hero = state.Heroes.First(item => item.Id == heroId);
            hero.Xp += run.Simulation.Xp;
            while (hero.Xp >= RequiredXp(hero.Level))
            {
                hero.Xp -= RequiredXp(hero.Level);
                hero.Level += 1;
                state.History.Add($"{HeroName(hero.Id)} 提升到 Lv.{hero.Level}。");
            }
        }

        if (run.Simulation.Success)
        {
            state.CompletedFloors[dungeon.Id] = Math.Max(state.CompletedFloors.GetValueOrDefault(dungeon.Id), run.Floor);
            state.FloorCaps[dungeon.Id] = Math.Min(dungeon.Floors, Math.Max(state.FloorCaps.GetValueOrDefault(dungeon.Id), run.Floor + 1));
            state.Inventory.AddRange(run.Simulation.Loot);
            state.Inventory = state.Inventory.TakeLast(48).ToList();

            foreach (var reward in region.Rewards.Where(reward => reward.Floor <= run.Floor))
            {
                foreach (var classId in reward.Classes)
                {
                    if (!state.UnlockedClasses.Contains(classId))
                    {
                        state.UnlockedClasses.Add(classId);
                        var hero = state.Heroes.First(item => item.Id == classId);
                        hero.Unlocked = true;
                        state.History.Add($"{HeroName(classId)} 加入营地。");
                    }
                }

                foreach (var unlockedRegion in reward.Regions)
                {
                    if (!state.UnlockedRegions.Contains(unlockedRegion))
                    {
                        state.UnlockedRegions.Add(unlockedRegion);
                        var nextRegion = _data.Regions.First(item => item.Id == unlockedRegion);
                        foreach (var nextDungeon in nextRegion.Dungeons)
                        {
                            state.FloorCaps[nextDungeon.Id] = Math.Max(InitialFloorCap, state.FloorCaps.GetValueOrDefault(nextDungeon.Id));
                        }
                        state.History.Add(nextRegion.Name + " 已解锁。");
                    }
                }
            }
        }
    }

    private CombatSimulation SimulateBattle(GameState state, RegionDefinition region, DungeonDefinition dungeon, int floor, List<string> heroIds)
    {
        var logs = new List<CombatLogEntry>
        {
            new() { Text = $"队伍进入 {region.Name} / {dungeon.Name} 第 {floor} 层。" }
        };
        var team = heroIds.Select(id =>
        {
            var hero = state.Heroes.First(item => item.Id == id);
            var stats = HeroStats(hero);
            return new UnitSnapshot
            {
                Side = "hero",
                Id = id,
                ClassId = id,
                Name = HeroName(id),
                Level = hero.Level,
                SpriteIndex = _data.Classes[id].SpriteIndex,
                MaxHp = stats.Hp,
                Hp = stats.Hp,
                Stats = stats
            };
        }).ToList();

        var enemies = CreateEncounter(dungeon, floor);
        foreach (var enemy in enemies)
        {
            logs.Add(new CombatLogEntry { Text = $"遭遇 {enemy.Name} Lv.{enemy.Level}。", Type = "danger" });
        }

        var round = 1;
        while (round <= 36 && Alive(team).Any() && Alive(enemies).Any())
        {
            logs.Add(new CombatLogEntry { Text = $"-- 第 {round} 轮 --" });
            var actors = Alive(team).Concat(Alive(enemies)).OrderByDescending(unit => unit.Stats.Spd).ToList();
            foreach (var actor in actors)
            {
                if (actor.Hp <= 0 || !Alive(team).Any() || !Alive(enemies).Any())
                {
                    continue;
                }

                if (actor.Side == "hero")
                {
                    HeroAction(actor, team, enemies, round, logs);
                }
                else
                {
                    MonsterAction(actor, team, logs);
                }
            }

            round += 1;
        }

        var success = Alive(team).Any() && !Alive(enemies).Any();
        logs.Add(new CombatLogEntry { Text = success ? "迷宫层主被击退，远征完成。" : "队伍被迫撤退，挑战失败。", Type = success ? "success" : "danger" });

        var xp = (int)Math.Round((double)((success ? 34 : 14) * floor + enemies.Sum(enemy => enemy.Level * 5)));
        var loot = success ? RollLoot(dungeon, floor) : [];
        foreach (var item in loot)
        {
            logs.Add(new CombatLogEntry { Text = $"发现掉落：{item.Rarity} {item.Name}（{SlotName(item.Slot)}）", Type = "loot" });
        }

        return new CombatSimulation
        {
            Success = success,
            Xp = xp,
            Loot = loot,
            Logs = logs,
            Rounds = round - 1,
            Encounter = enemies.Select(CloneUnit).ToList(),
            Team = team.Select(CloneUnit).ToList(),
            EnemyTeam = enemies.Select(CloneUnit).ToList()
        };
    }

    private List<UnitSnapshot> CreateEncounter(DungeonDefinition dungeon, int floor)
    {
        var (minLevel, maxLevel) = FloorLevelBand(dungeon, floor);
        var count = Math.Min(4, 2 + (int)Math.Floor((floor + 1) / 4.0));
        var result = new List<UnitSnapshot>();
        for (var index = 0; index < count; index += 1)
        {
            var monsterId = dungeon.MonsterIds[(floor + index + _rng.Next(dungeon.MonsterIds.Count)) % dungeon.MonsterIds.Count];
            var level = _rng.Next(minLevel, maxLevel + 1);
            var template = _data.Monsters[monsterId];
            var stats = MonsterStats(monsterId, level);
            result.Add(new UnitSnapshot
            {
                Side = "monster",
                Id = $"{monsterId}-{index}",
                MonsterId = monsterId,
                Name = template.Name,
                Level = level,
                SpriteIndex = template.SpriteIndex,
                MaxHp = stats.Hp,
                Hp = stats.Hp,
                Stats = stats
            });
        }

        return result;
    }

    private ExplorationPlan CreateExplorationPlan(RegionDefinition region, DungeonDefinition dungeon, int floor, CombatSimulation simulation)
    {
        var eventCount = floor >= 8 ? 2 : 1;
        var events = new List<ExplorationEventRoll>();
        for (var index = 0; index < eventCount; index += 1)
        {
            if (_rng.NextDouble() > 0.72 && index > 0)
            {
                continue;
            }

            var template = _data.Events[_rng.Next(_data.Events.Count)];
            events.Add(new ExplorationEventRoll
            {
                Id = template.Id,
                Name = template.Name,
                Type = template.Type,
                Text = template.Text,
                XpBonus = template.XpBonus,
                LootChance = template.LootChance,
                At = 0.22 + index * 0.15
            });
        }

        var segments = new List<ExplorationSegment>
        {
            new()
            {
                Kind = "travel",
                Start = 0,
                End = events.FirstOrDefault() is { } first ? Math.Max(0.18, first.At - 0.04) : 0.34,
                Title = "行军推进",
                Text = $"{region.Name} 的地形正在展开。"
            }
        };

        foreach (var item in events.Select((value, index) => new { value, index }))
        {
            var eventEnd = Math.Min(0.54, item.value.At + 0.08);
            segments.Add(new ExplorationSegment { Kind = "event", Start = item.value.At, End = eventEnd, Title = item.value.Name, Text = item.value.Text });
            var next = events.ElementAtOrDefault(item.index + 1);
            var nextStart = next != null ? Math.Max(eventEnd + 0.02, next.At - 0.04) : 0.42;
            if (nextStart > eventEnd)
            {
                segments.Add(new ExplorationSegment { Kind = "travel", Start = eventEnd, End = nextStart, Title = "继续前进", Text = $"{dungeon.Name} 的第 {floor} 层仍有动静。" });
            }
        }

        var battleStart = Math.Max(0.42, segments.Last().End);
        segments.Add(new ExplorationSegment { Kind = "battle", Start = battleStart, End = 0.9, Title = "遭遇战", Text = $"{string.Join("、", simulation.Encounter.Select(enemy => enemy.Name))} 出现。" });
        segments.Add(new ExplorationSegment { Kind = "aftermath", Start = 0.9, End = 1, Title = simulation.Success ? "清点战利品" : "撤退途中", Text = simulation.Success ? "队伍正在返回营地。" : "队伍保留经验并撤回营地。" });
        return new ExplorationPlan { Events = events, Segments = NormalizeSegments(segments) };
    }

    private void ApplyExplorationRewards(CombatSimulation simulation, ExplorationPlan exploration, DungeonDefinition dungeon, int floor)
    {
        foreach (var evt in exploration.Events)
        {
            if (evt.XpBonus > 0)
            {
                simulation.Xp += floor * evt.XpBonus;
            }

            if (evt.LootChance > 0 && simulation.Success && _rng.NextDouble() < evt.LootChance)
            {
                simulation.Loot.Add(CreateLoot(dungeon.LootTable[_rng.Next(dungeon.LootTable.Count)], PickRarity(), floor));
            }
        }
    }

    private List<CombatLogEntry> CreateTimedLogs(RegionDefinition region, DungeonDefinition dungeon, int floor, ExplorationPlan exploration, CombatSimulation simulation)
    {
        var logs = new List<CombatLogEntry>
        {
            new() { Text = $"队伍离开营地，前往 {region.Name} / {dungeon.Name} 第 {floor} 层。", RevealAt = 0.01 },
            new() { Text = "探索开始：队伍进入横版路线，自动搜索前进。", RevealAt = 0.04 }
        };

        logs.AddRange(exploration.Events.Select(evt => new CombatLogEntry
        {
            Text = $"随机事件：{evt.Name}。{evt.Text}",
            Type = evt.Type == "hazard" ? "danger" : "success",
            RevealAt = Math.Max(0.01, evt.At)
        }));

        var battle = exploration.Segments.FirstOrDefault(segment => segment.Kind == "battle") ?? new ExplorationSegment { Start = 0.42, End = 0.9 };
        var span = Math.Max(0.08, battle.End - battle.Start);
        for (var index = 0; index < simulation.Logs.Count; index += 1)
        {
            var entry = simulation.Logs[index];
            entry.RevealAt = battle.Start + span * ((index + 1.0) / Math.Max(1, simulation.Logs.Count + 1));
            logs.Add(entry);
        }

        return logs;
    }

    private void HeroAction(UnitSnapshot hero, List<UnitSnapshot> team, List<UnitSnapshot> enemies, int round, List<CombatLogEntry> logs)
    {
        var target = Alive(enemies).OrderBy(unit => unit.Hp / (double)unit.MaxHp).FirstOrDefault();
        if (target == null)
        {
            return;
        }

        if (hero.ClassId == "monk" && (round % 3 == 0 || Alive(team).Min(unit => unit.Hp / (double)unit.MaxHp) < 0.45))
        {
            var ally = Alive(team).OrderBy(unit => unit.Hp / (double)unit.MaxHp).First();
            var heal = (int)Math.Round(hero.Stats.Mag * 1.55 + hero.Stats.Atk * 0.45);
            ally.Hp = Math.Min(ally.MaxHp, ally.Hp + heal);
            logs.Add(new CombatLogEntry { Text = $"{hero.Name} 施放小治疗术，为 {ally.Name} 恢复 {heal} 生命。", Type = "success", Amount = heal, TargetSide = "hero", TargetId = ally.Id });
            return;
        }

        if (hero.ClassId == "mage" && round % 3 == 0)
        {
            foreach (var enemy in Alive(enemies))
            {
                var damage = ApplyDamage(hero.Stats.Mag * 1.15 + hero.Stats.Atk, enemy.Stats.Def * 0.45);
                enemy.Hp -= damage;
                logs.Add(DamageLog(hero, enemy, damage, $"{hero.Name} 释放火星雨击中 {enemy.Name}，造成 {damage} 伤害。"));
                FinishIfDead(enemy, logs);
            }

            return;
        }

        var crit = _rng.NextDouble() * 100 < hero.Stats.Crit;
        var classBonus = hero.ClassId == "hunter" && round == 1 ? 1.45 : hero.ClassId == "hunter" ? 1.22 : hero.ClassId == "knight" ? 0.92 : 1;
        var damageBase = hero.Stats.Atk + hero.Stats.Mag * 0.65;
        var damageFinal = ApplyDamage(damageBase * classBonus * (crit ? 1.7 : 1), target.Stats.Def);
        target.Hp -= damageFinal;
        logs.Add(DamageLog(hero, target, damageFinal, $"{hero.Name}{(crit ? " 暴击" : "")} 攻击 {target.Name}，造成 {damageFinal} 伤害。"));
        FinishIfDead(target, logs);
    }

    private void MonsterAction(UnitSnapshot monster, List<UnitSnapshot> team, List<CombatLogEntry> logs)
    {
        var targets = Alive(team).ToList();
        var target = targets.FirstOrDefault(unit => unit.ClassId == "knight") ?? targets[_rng.Next(targets.Count)];
        var crit = _rng.NextDouble() * 100 < monster.Stats.Crit;
        var damage = ApplyDamage((monster.Stats.Atk + monster.Stats.Mag * 0.7) * (crit ? 1.55 : 1), target.Stats.Def);
        target.Hp -= damage;
        logs.Add(DamageLog(monster, target, damage, $"{monster.Name}{(crit ? " 凶狠一击" : "")} 命中 {target.Name}，造成 {damage} 伤害。", damage > 30 ? "danger" : ""));
        FinishIfDead(target, logs);
    }

    private CombatLogEntry DamageLog(UnitSnapshot actor, UnitSnapshot target, int damage, string text, string type = "")
    {
        return new CombatLogEntry { Text = text, Type = type, Amount = damage, TargetSide = target.Side, TargetId = target.Id };
    }

    private void FinishIfDead(UnitSnapshot unit, List<CombatLogEntry> logs)
    {
        if (unit.Hp > 0)
        {
            return;
        }

        unit.Hp = 0;
        logs.Add(new CombatLogEntry { Text = $"{unit.Name} 倒下。", Type = unit.Side == "hero" ? "danger" : "success" });
    }

    private List<LootItem> RollLoot(DungeonDefinition dungeon, int floor)
    {
        return _rng.NextDouble() < 0.35 + floor * 0.035
            ? [CreateLoot(dungeon.LootTable[_rng.Next(dungeon.LootTable.Count)], PickRarity(), floor)]
            : [];
    }

    private LootItem CreateLoot(string name, RarityDefinition rarity, int floor)
    {
        var template = _data.Items.ItemCatalog[name];
        var statBlock = new StatBlock();
        foreach (var stat in template.Stats)
        {
            var value = Math.Max(1, (int)Math.Round((2 + floor * 0.72) * rarity.Power));
            AddStat(statBlock, stat, stat == "hp" ? value * 5 : value);
        }

        return new LootItem
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Rarity = rarity.Name,
            Slot = template.Slot,
            SpriteIndex = template.SpriteIndex,
            Stats = statBlock
        };
    }

    private RarityDefinition PickRarity()
    {
        var roll = _rng.NextDouble();
        var cursor = 0.0;
        foreach (var rarity in _data.Items.Rarity)
        {
            cursor += rarity.Chance;
            if (roll <= cursor)
            {
                return rarity;
            }
        }

        return _data.Items.Rarity.Last();
    }

    private (int Min, int Max) FloorLevelBand(DungeonDefinition dungeon, int floor)
    {
        var min = dungeon.LevelRange.ElementAtOrDefault(0);
        var max = dungeon.LevelRange.ElementAtOrDefault(1);
        var t = dungeon.Floors <= 1 ? 0 : (floor - 1) / (double)(dungeon.Floors - 1);
        var center = min + (max - min) * t;
        return (Math.Max(1, (int)Math.Floor(center - 1)), Math.Max(1, (int)Math.Ceiling(center + 1)));
    }

    private StatBlock MonsterStats(string monsterId, int level)
    {
        var baseStats = _data.Monsters[monsterId].Base;
        var growth = 1 + level * 0.13;
        return new StatBlock
        {
            Hp = (int)Math.Round(baseStats.Hp * growth + level * 7),
            Atk = (int)Math.Round(baseStats.Atk * growth + level * 1.15),
            Mag = (int)Math.Round(baseStats.Mag * growth + level * 0.75),
            Def = (int)Math.Round(baseStats.Def * growth + level * 0.8),
            Spd = baseStats.Spd + level / 5,
            Crit = Math.Min(35, baseStats.Crit + level / 4)
        };
    }

    private int ApplyDamage(double power, double defense)
    {
        var variance = 0.88 + _rng.NextDouble() * 0.24;
        return Math.Max(1, (int)Math.Round(power * variance - defense * 0.58));
    }

    private string HeroName(string id) => _data.Classes.TryGetValue(id, out var hero) ? hero.Name : id;

    private string SlotName(string id) => _data.Items.SlotNames.TryGetValue(id, out var slot) ? slot : id;

    private static IEnumerable<UnitSnapshot> Alive(IEnumerable<UnitSnapshot> units) => units.Where(unit => unit.Hp > 0);

    private static StatBlock Scale(StatBlock block, int value)
    {
        return new StatBlock
        {
            Hp = block.Hp * value,
            Atk = block.Atk * value,
            Mag = block.Mag * value,
            Def = block.Def * value,
            Spd = block.Spd * value,
            Crit = block.Crit * value
        };
    }

    private static StatBlock Add(StatBlock left, StatBlock right)
    {
        return new StatBlock
        {
            Hp = left.Hp + right.Hp,
            Atk = left.Atk + right.Atk,
            Mag = left.Mag + right.Mag,
            Def = left.Def + right.Def,
            Spd = left.Spd + right.Spd,
            Crit = left.Crit + right.Crit
        };
    }

    private static void AddStat(StatBlock block, string stat, int value)
    {
        switch (stat)
        {
            case "hp": block.Hp += value; break;
            case "atk": block.Atk += value; break;
            case "mag": block.Mag += value; break;
            case "def": block.Def += value; break;
            case "spd": block.Spd += value; break;
            case "crit": block.Crit += value; break;
        }
    }

    private static UnitSnapshot CloneUnit(UnitSnapshot unit)
    {
        return new UnitSnapshot
        {
            Side = unit.Side,
            Id = unit.Id,
            ClassId = unit.ClassId,
            MonsterId = unit.MonsterId,
            Name = unit.Name,
            Level = unit.Level,
            SpriteIndex = unit.SpriteIndex,
            MaxHp = unit.MaxHp,
            Hp = unit.Hp,
            Stats = unit.Stats
        };
    }

    private static List<ExplorationSegment> NormalizeSegments(List<ExplorationSegment> segments)
    {
        var filtered = segments.Where(segment => segment.End > segment.Start).OrderBy(segment => segment.Start).ToList();
        for (var index = 0; index < filtered.Count; index += 1)
        {
            filtered[index].Start = index == 0 ? 0 : filtered[index - 1].End;
            filtered[index].End = index == filtered.Count - 1 ? 1 : Math.Min(filtered[index].End, filtered[index + 1].Start);
        }

        return filtered;
    }
}
