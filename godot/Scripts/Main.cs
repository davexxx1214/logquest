using System;
using System.Linq;
using Godot;
using LogQuest.Data;
using LogQuest.Runtime;

public partial class Main : Control
{
    private const int HeroColumns = 6;
    private const int HeroRows = 4;
    private const int MonsterColumns = 6;
    private const int MonsterRows = 8;
    private const int ItemColumns = 8;
    private const int ItemRows = 3;

    private readonly Color _ink = new("#f7dc8a");
    private readonly Color _muted = new("#c9a966");
    private readonly Color _panel = new("#24182c");
    private readonly Color _panelAlt = new("#3a2c42");
    private readonly Color _panelDark = new("#120d16");
    private readonly Color _border = new("#8b6346");
    private readonly Color _accent = new("#f0b74a");
    private readonly Color _success = new("#63d08e");
    private readonly Color _danger = new("#ff7766");

    private GameData _data = new();
    private GameLogic _logic = null!;
    private GameState _state = new();
    private Texture2D? _heroSheet;
    private Texture2D? _monsterSheet;
    private Texture2D? _itemSheet;
    private string _selectedRegionId = "";
    private string _selectedDungeonId = "";
    private int _selectedFloor = 1;
    private string _status = "Godot 迁移版已就绪";
    private double _tickAccumulator;

    public override void _Ready()
    {
        _data = GameDatabase.Load();
        _logic = new GameLogic(_data);
        _state = SaveGame.Load() ?? _logic.CreateInitialState();
        _logic.NormalizeState(_state);

        var firstRegion = _data.Regions.First();
        _selectedRegionId = _state.UnlockedRegions.FirstOrDefault() ?? firstRegion.Id;
        var selectedRegion = Region();
        _selectedDungeonId = selectedRegion.Dungeons.First().Id;
        _selectedFloor = Math.Max(1, Math.Min(1, FloorCap(Dungeon())));

        _heroSheet = LoadPng("res://Assets/heroes-16bit-v2.png");
        _monsterSheet = LoadPng("res://Assets/monsters-16bit-v2.png");
        _itemSheet = LoadPng("res://Assets/items-16bit.png");

        _logic.FinishReadyRuns(_state);
        SaveGame.Save(_state);
        BuildInterface();
    }

    public override void _Process(double delta)
    {
        _tickAccumulator += delta;
        if (_tickAccumulator < 1)
        {
            return;
        }

        _tickAccumulator = 0;
        var finished = _logic.FinishReadyRuns(_state);
        if (finished.Count > 0)
        {
            _status = $"{finished.Count} 支小队完成探索";
            SaveGame.Save(_state);
        }

        if (_state.ActiveRuns.Count > 0 || finished.Count > 0)
        {
            BuildInterface();
        }
    }

    private void BuildInterface()
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }

        var background = new ColorRect { Color = new Color("#15111b") };
        background.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(background);

        var root = new MarginContainer();
        root.SetAnchorsPreset(LayoutPreset.FullRect);
        root.AddThemeConstantOverride("margin_left", 22);
        root.AddThemeConstantOverride("margin_right", 22);
        root.AddThemeConstantOverride("margin_top", 18);
        root.AddThemeConstantOverride("margin_bottom", 18);
        AddChild(root);

        var stack = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        stack.AddThemeConstantOverride("separation", 16);
        root.AddChild(stack);

        stack.AddChild(BuildHeader());

        var columns = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        columns.AddThemeConstantOverride("separation", 16);
        stack.AddChild(columns);

        var left = BuildColumn();
        left.AddChild(BuildMapPanel());
        left.AddChild(BuildCodexPanel());
        columns.AddChild(WrapColumn(left, 0.29f));

        var center = BuildColumn();
        center.AddChild(BuildMissionPanel());
        center.AddChild(BuildExplorationPanel());
        center.AddChild(BuildLogPanel());
        columns.AddChild(WrapColumn(center, 0.41f));

        var right = BuildColumn();
        right.AddChild(BuildRosterPanel());
        right.AddChild(BuildItemPanel());
        columns.AddChild(WrapColumn(right, 0.3f));
    }

    private Control BuildHeader()
    {
        var panel = BuildPanel();
        panel.CustomMinimumSize = new Vector2(0, 96);
        var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        row.AddThemeConstantOverride("separation", 20);
        panel.AddChild(Padded(row, 18, 14));

        var titleBox = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        titleBox.AddChild(Text("LOCAL IDLE STRATEGY - GODOT .NET", 15, _muted));
        titleBox.AddChild(Text("LogQuest 冒险日志", 38, _ink));
        row.AddChild(titleBox);

        var status = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ShrinkEnd };
        status.AddChild(Text(_status, 18, _accent, HorizontalAlignment.Right));
        status.AddChild(Text($"{_state.ActiveRuns.Count} / {GameLogic.MaxActiveRuns} 小队探索中 · 已解锁 {_state.UnlockedClasses.Count} 职业", 16, _success, HorizontalAlignment.Right));
        var reset = new Button { Text = "重置存档", CustomMinimumSize = new Vector2(140, 34), SizeFlagsHorizontal = SizeFlags.ShrinkEnd };
        StyleButton(reset, _panelAlt, _ink, false);
        reset.Pressed += () =>
        {
            SaveGame.Reset();
            _state = _logic.CreateInitialState();
            _selectedRegionId = _data.Regions.First().Id;
            _selectedDungeonId = _data.Regions.First().Dungeons.First().Id;
            _selectedFloor = 1;
            _status = "存档已重置";
            SaveGame.Save(_state);
            BuildInterface();
        };
        status.AddChild(reset);
        row.AddChild(status);
        return panel;
    }

    private VBoxContainer BuildColumn()
    {
        var column = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ShrinkBegin
        };
        column.AddThemeConstantOverride("separation", 14);
        return column;
    }

    private ScrollContainer WrapColumn(Control content, float ratio)
    {
        var scroll = new ScrollContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = ratio,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
        };
        scroll.AddChild(content);
        return scroll;
    }

    private Control BuildMapPanel()
    {
        var panel = TitledPanel("大地图", $"{_state.UnlockedRegions.Count} / {_data.Regions.Count}", out var content);
        var list = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        list.AddThemeConstantOverride("separation", 8);
        content.AddChild(list);

        foreach (var region in _data.Regions)
        {
            var unlocked = _state.UnlockedRegions.Contains(region.Id);
            var selected = region.Id == _selectedRegionId;
            var button = new Button
            {
                Text = $"{region.Name} / {region.Dungeons[0].Name}\n{(unlocked ? region.Tone : "未解锁")} · {region.Dungeons[0].Floors} 层 · Lv.{region.Dungeons[0].LevelRange[0]}-{region.Dungeons[0].LevelRange[1]}",
                Disabled = !unlocked,
                CustomMinimumSize = new Vector2(0, 68),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                Alignment = HorizontalAlignment.Left
            };
            StyleButton(button, selected ? _accent : _panelAlt, selected ? new Color("#111017") : _ink, selected);
            var captured = region;
            button.Pressed += () =>
            {
                _selectedRegionId = captured.Id;
                _selectedDungeonId = captured.Dungeons[0].Id;
                _selectedFloor = Math.Max(1, Math.Min(_selectedFloor, FloorCap(captured.Dungeons[0])));
                _status = $"{captured.Name} 已选中";
                BuildInterface();
            };
            list.AddChild(button);
        }

        return panel;
    }

    private Control BuildMissionPanel()
    {
        var region = Region();
        var dungeon = Dungeon();
        var cap = FloorCap(dungeon);
        if (cap > 0)
        {
            _selectedFloor = Math.Clamp(_selectedFloor, 1, cap);
        }

        var canStart = _logic.CanStart(_state, region.Id, dungeon.Id, _selectedFloor);
        var panel = TitledPanel("出击计划", cap > 0 ? $"可挑战至 {cap} 层" : "未解锁", out var content);
        content.AddChild(Text($"{region.Name} / {dungeon.Name} 第 {_selectedFloor} 层", 24, _ink));
        content.AddChild(Wrapped(region.Description, 15, new Color("#ead59a")));

        var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        row.AddThemeConstantOverride("separation", 10);
        var floorSelect = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(0, 44) };
        floorSelect.AddThemeFontSizeOverride("font_size", 17);
        for (var floor = 1; floor <= Math.Max(1, cap); floor += 1)
        {
            floorSelect.AddItem($"第 {floor} 层", floor);
            if (floor == _selectedFloor)
            {
                floorSelect.Select(floor - 1);
            }
        }
        floorSelect.Disabled = cap <= 0;
        floorSelect.ItemSelected += index =>
        {
            _selectedFloor = (int)floorSelect.GetItemId((int)index);
            _status = $"目标层数调整为第 {_selectedFloor} 层";
            BuildInterface();
        };
        row.AddChild(floorSelect);

        var buttonText = _state.ActiveRuns.Count >= GameLogic.MaxActiveRuns ? "远征栏位已满" : _state.SelectedHeroIds.Count == 0 ? "选择英雄后出击" : "派遣队伍";
        var start = new Button { Text = buttonText, Disabled = !canStart, CustomMinimumSize = new Vector2(0, 44), SizeFlagsHorizontal = SizeFlags.ExpandFill };
        StyleButton(start, _accent, new Color("#111017"), true);
        start.Pressed += () =>
        {
            var run = _logic.StartRun(_state, region.Id, dungeon.Id, _selectedFloor);
            _status = $"小队出发：{string.Join(" / ", run.HeroIds.Select(HeroName))}";
            SaveGame.Save(_state);
            BuildInterface();
        };
        row.AddChild(start);
        content.AddChild(row);

        var active = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        active.AddThemeConstantOverride("separation", 6);
        active.AddChild(Text("EXPEDITION SLOTS", 15, _muted));
        if (_state.ActiveRuns.Count == 0)
        {
            active.AddChild(Text("暂无正在探索的小队", 16, _muted));
        }
        else
        {
            foreach (var run in _state.ActiveRuns)
            {
                active.AddChild(Wrapped($"{Region(run.RegionId).Name} 第 {run.Floor} 层 · {string.Join(" / ", run.HeroIds.Select(HeroName))} · {FormatTime(run.EndAt - GameLogic.NowMs())}", 16, _success));
            }
        }
        content.AddChild(active);
        return panel;
    }

    private Control BuildExplorationPanel()
    {
        var panel = TitledPanel("探索画面", $"{_state.ActiveRuns.Count} / {GameLogic.MaxActiveRuns}", out var content);
        if (_state.ActiveRuns.Count == 0)
        {
            content.AddChild(BuildEmptyScene());
            return panel;
        }

        foreach (var run in _state.ActiveRuns)
        {
            content.AddChild(BuildRunScene(run));
        }

        return panel;
    }

    private Control BuildRunScene(ExpeditionRun run)
    {
        var region = Region(run.RegionId);
        var phase = CurrentPhase(run);
        var scene = new PanelContainer { CustomMinimumSize = new Vector2(0, 230), SizeFlagsHorizontal = SizeFlags.ExpandFill };
        scene.AddThemeStyleboxOverride("panel", SceneStyle(region));

        var lane = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        lane.AddThemeConstantOverride("separation", 12);
        scene.AddChild(Padded(lane, 16, 14));

        var banner = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        banner.AddChild(Text(phase.Kind == "battle" ? "AUTO BATTLE" : "EXPLORING", 21, new Color("#101018")));
        banner.AddChild(Spacer());
        banner.AddChild(Text(phase.Title, 21, new Color("#101018"), HorizontalAlignment.Right));
        var bannerWrap = new PanelContainer();
        bannerWrap.AddThemeStyleboxOverride("panel", Flat(new Color("#f2d98c"), new Color("#111017"), 3));
        bannerWrap.AddChild(Padded(banner, 14, 8));
        lane.AddChild(bannerWrap);

        var actors = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        actors.AddThemeConstantOverride("separation", 20);
        foreach (var heroId in run.HeroIds)
        {
            var hero = _data.Classes[heroId];
            actors.AddChild(SpriteCard(_heroSheet, HeroTile(hero.SpriteIndex, phase.Kind == "battle" ? 2 : 0), hero.Name, _success, 70));
        }
        actors.AddChild(Spacer());
        if (phase.Kind == "battle")
        {
            foreach (var enemy in run.Simulation.Encounter.Take(3))
            {
                actors.AddChild(SpriteCard(_monsterSheet, MonsterTile(enemy.SpriteIndex, false), enemy.Name, _danger, 70));
            }
        }
        lane.AddChild(actors);

        lane.AddChild(BuildProgress(RunProgress(run), _success, _danger));
        lane.AddChild(Wrapped($"{phase.Title} · {phase.Text}", 15, _muted));
        return scene;
    }

    private Control BuildEmptyScene()
    {
        var region = Region();
        var scene = new PanelContainer { CustomMinimumSize = new Vector2(0, 190), SizeFlagsHorizontal = SizeFlags.ExpandFill };
        scene.AddThemeStyleboxOverride("panel", SceneStyle(region));
        var stack = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        stack.Alignment = BoxContainer.AlignmentMode.Center;
        stack.AddChild(Text("等待出击", 25, _ink, HorizontalAlignment.Center));
        stack.AddChild(Text("选择最多 3 名英雄后派遣队伍", 16, _muted, HorizontalAlignment.Center));
        scene.AddChild(Padded(stack, 16, 16));
        return scene;
    }

    private Control BuildRosterPanel()
    {
        var panel = TitledPanel("英雄队伍", $"{_state.SelectedHeroIds.Count} / {GameLogic.TeamLimit}", out var content);
        var grid = new GridContainer { Columns = 2, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        grid.AddThemeConstantOverride("h_separation", 10);
        grid.AddThemeConstantOverride("v_separation", 10);
        content.AddChild(grid);

        var busy = _logic.BusyHeroIds(_state);
        foreach (var entry in _data.Classes.OrderBy(item => item.Value.SpriteIndex))
        {
            var hero = _state.Heroes.First(item => item.Id == entry.Key);
            grid.AddChild(BuildHeroCard(hero, entry.Value, busy.Contains(hero.Id), _state.SelectedHeroIds.Contains(hero.Id)));
        }

        return panel;
    }

    private Control BuildHeroCard(HeroState hero, ClassDefinition data, bool busy, bool selected)
    {
        var button = new Button
        {
            Disabled = !hero.Unlocked || busy,
            CustomMinimumSize = new Vector2(0, 96),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            Text = "",
            FocusMode = FocusModeEnum.None
        };
        StyleButton(button, selected ? new Color("#5a3d4f") : hero.Unlocked ? _panelAlt : new Color("#211b24"), selected ? _accent : _border, selected);
        button.Pressed += () => ToggleHero(hero.Id);

        var row = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill };
        row.AddThemeConstantOverride("separation", 8);
        row.AddChild(SpriteImage(_heroSheet, HeroTile(data.SpriteIndex, 2), new Vector2(58, 58)));

        var stats = _logic.HeroStats(hero);
        var texts = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        texts.AddChild(Text($"{data.Name} Lv.{hero.Level}", 17, hero.Unlocked ? _ink : new Color("#776b75")));
        texts.AddChild(Wrapped(busy ? "探索中" : hero.Unlocked ? data.Role : $"未解锁 · {data.Unlock}", 12, busy ? _success : _muted));
        texts.AddChild(Text($"HP{stats.Hp} ATK{stats.Atk} MAG{stats.Mag}", 11, new Color("#e1c77a")));
        texts.AddChild(Text($"DEF{stats.Def} SPD{stats.Spd} CRT{stats.Crit}", 11, new Color("#e1c77a")));
        row.AddChild(texts);
        button.AddChild(Padded(row, 8, 7));
        return button;
    }

    private Control BuildCodexPanel()
    {
        var dungeon = Dungeon();
        var panel = TitledPanel("怪物图鉴", $"Lv.{dungeon.LevelRange[0]}-{dungeon.LevelRange[1]}", out var content);
        var grid = new GridContainer { Columns = 4, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        grid.AddThemeConstantOverride("h_separation", 8);
        grid.AddThemeConstantOverride("v_separation", 8);
        content.AddChild(grid);

        foreach (var monsterId in dungeon.MonsterIds)
        {
            var monster = _data.Monsters[monsterId];
            grid.AddChild(SpriteCard(_monsterSheet, MonsterTile(monster.SpriteIndex, false), monster.Name, _muted, 86));
        }

        return panel;
    }

    private Control BuildLogPanel()
    {
        var sourceRun = _state.ActiveRuns.FirstOrDefault();
        var logs = sourceRun != null ? VisibleLogs(sourceRun) : _state.LastLogs;
        var panel = TitledPanel("战斗日志", sourceRun != null ? "进行中" : "最近结算", out var content);
        var scroll = new ScrollContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, CustomMinimumSize = new Vector2(0, 190), HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        var list = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        list.AddThemeConstantOverride("separation", 4);
        scroll.AddChild(list);
        foreach (var log in logs.TakeLast(24))
        {
            list.AddChild(Wrapped(log.Text, 15, LogColor(log)));
        }
        if (!logs.Any())
        {
            list.AddChild(Text("暂无日志", 18, _muted));
        }
        content.AddChild(scroll);
        return panel;
    }

    private Control BuildItemPanel()
    {
        var panel = TitledPanel("装备与结算", $"{_state.Inventory.Count} 件", out var content);
        var grid = new GridContainer { Columns = 4, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        grid.AddThemeConstantOverride("h_separation", 8);
        grid.AddThemeConstantOverride("v_separation", 8);
        content.AddChild(grid);

        var items = _state.Inventory.Count > 0
            ? _state.Inventory.TakeLast(12).Reverse()
            : _data.Items.ItemCatalog.OrderBy(entry => entry.Value.SpriteIndex).Take(12).Select(entry => new LootItem { Name = entry.Key, SpriteIndex = entry.Value.SpriteIndex, Slot = entry.Value.Slot, Rarity = "图鉴" });
        foreach (var item in items)
        {
            grid.AddChild(SpriteCard(_itemSheet, ItemTile(item.SpriteIndex), $"{item.Rarity}\n{item.Name}", _accent, 86));
        }

        return panel;
    }

    private void ToggleHero(string heroId)
    {
        if (_state.SelectedHeroIds.Contains(heroId))
        {
            _state.SelectedHeroIds.Remove(heroId);
        }
        else if (_state.SelectedHeroIds.Count < GameLogic.TeamLimit)
        {
            _state.SelectedHeroIds.Add(heroId);
        }

        _status = _state.SelectedHeroIds.Count == 0 ? "请选择出击英雄" : $"已选择 {string.Join(" / ", _state.SelectedHeroIds.Select(HeroName))}";
        SaveGame.Save(_state);
        BuildInterface();
    }

    private VBoxContainer TitledPanel(string title, string chip, out VBoxContainer content)
    {
        var panel = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ShrinkBegin };
        panel.AddThemeConstantOverride("separation", 8);

        var head = new HBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        head.AddChild(Text(title, 26, _ink));
        head.AddChild(Spacer());
        head.AddChild(Chip(chip));
        panel.AddChild(head);

        var body = BuildPanel();
        body.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        body.SizeFlagsVertical = SizeFlags.ShrinkBegin;
        content = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ShrinkBegin };
        content.AddThemeConstantOverride("separation", 10);
        body.AddChild(Padded(content, 12, 12));
        panel.AddChild(body);
        return panel;
    }

    private PanelContainer BuildPanel() => BuildPanel(_panel, 3);

    private PanelContainer BuildPanel(Color color, int borderWidth)
    {
        var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        panel.AddThemeStyleboxOverride("panel", Flat(color, _border, borderWidth));
        return panel;
    }

    private void StyleButton(Button button, Color background, Color textOrBorder, bool primary)
    {
        button.AddThemeFontSizeOverride("font_size", 18);
        button.AddThemeColorOverride("font_color", primary ? new Color("#111017") : textOrBorder);
        button.AddThemeColorOverride("font_disabled_color", new Color("#7c6f73"));
        button.AddThemeStyleboxOverride("normal", Flat(background, primary ? new Color("#111017") : _border, 3));
        button.AddThemeStyleboxOverride("hover", Flat(background.Lightened(0.08f), _accent, 3));
        button.AddThemeStyleboxOverride("pressed", Flat(background.Darkened(0.08f), _accent, 3));
        button.AddThemeStyleboxOverride("disabled", Flat(background.Darkened(0.25f), new Color("#302632"), 3));
    }

    private Label Text(string text, int size, Color color, HorizontalAlignment align = HorizontalAlignment.Left)
    {
        var label = new Label
        {
            Text = text,
            HorizontalAlignment = align,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    private Label Wrapped(string text, int size, Color color)
    {
        var label = Text(text, size, color);
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        return label;
    }

    private Control Chip(string text)
    {
        var chip = new PanelContainer { CustomMinimumSize = new Vector2(96, 36) };
        chip.AddThemeStyleboxOverride("panel", Flat(new Color("#f2d98c"), new Color("#111017"), 3));
        chip.AddChild(Padded(Text(text, 15, new Color("#111017"), HorizontalAlignment.Center), 8, 5));
        return chip;
    }

    private Control BuildProgress(double progress, Color from, Color to)
    {
        var bar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = 100,
            Value = Math.Round(progress * 100),
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 18),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        bar.AddThemeStyleboxOverride("background", Flat(_panelDark, new Color("#050407"), 3));
        bar.AddThemeStyleboxOverride("fill", Flat(progress < 0.68 ? from : to, new Color("#050407"), 0));
        return bar;
    }

    private Control Spacer() => new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill };

    private MarginContainer Padded(Control child, int horizontal, int vertical)
    {
        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", horizontal);
        margin.AddThemeConstantOverride("margin_right", horizontal);
        margin.AddThemeConstantOverride("margin_top", vertical);
        margin.AddThemeConstantOverride("margin_bottom", vertical);
        margin.AddChild(child);
        return margin;
    }

    private Control SpriteCard(Texture2D? sheet, Rect2 region, string label, Color color, int size = 102)
    {
        var box = new VBoxContainer { CustomMinimumSize = new Vector2(size, 0), SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
        box.AddThemeConstantOverride("separation", 4);
        box.AddChild(SpriteImage(sheet, region, new Vector2(size, size)));
        var text = Text(label, size > 90 ? 13 : 11, color, HorizontalAlignment.Center);
        text.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        box.AddChild(text);
        return box;
    }

    private TextureRect SpriteImage(Texture2D? sheet, Rect2 region, Vector2 size)
    {
        var texture = new AtlasTexture();
        if (sheet != null)
        {
            texture.Atlas = sheet;
            texture.Region = region;
        }

        return new TextureRect
        {
            Texture = texture,
            CustomMinimumSize = size,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };
    }

    private RegionDefinition Region() => Region(_selectedRegionId);

    private RegionDefinition Region(string id) => _data.Regions.FirstOrDefault(item => item.Id == id) ?? _data.Regions.First();

    private DungeonDefinition Dungeon() => Region().Dungeons.FirstOrDefault(item => item.Id == _selectedDungeonId) ?? Region().Dungeons.First();

    private int FloorCap(DungeonDefinition dungeon) => _state.FloorCaps.GetValueOrDefault(dungeon.Id);

    private string HeroName(string id) => _data.Classes.TryGetValue(id, out var hero) ? hero.Name : id;

    private double RunProgress(ExpeditionRun run)
    {
        return Math.Clamp((GameLogic.NowMs() - run.StartedAt) / (double)run.DurationMs, 0, 1);
    }

    private ExplorationSegment CurrentPhase(ExpeditionRun run)
    {
        var progress = RunProgress(run);
        return run.Exploration.Segments.FirstOrDefault(segment => progress >= segment.Start && progress <= segment.End)
            ?? run.Exploration.Segments.LastOrDefault()
            ?? new ExplorationSegment { Kind = "travel", Title = "行军推进", Text = "队伍正在前进。" };
    }

    private List<CombatLogEntry> VisibleLogs(ExpeditionRun run)
    {
        var progress = RunProgress(run);
        return run.Simulation.Logs.Where(log => log.RevealAt <= progress).ToList();
    }

    private Color LogColor(CombatLogEntry entry)
    {
        return entry.Type switch
        {
            "danger" => _danger,
            "success" => _success,
            "loot" => _accent,
            _ => _ink
        };
    }

    private string FormatTime(long ms)
    {
        var seconds = Math.Max(0, (int)Math.Ceiling(ms / 1000.0));
        return $"{seconds / 60:00}:{seconds % 60:00}";
    }

    private Rect2 HeroTile(int column, int row)
    {
        return _heroSheet == null ? new Rect2() : Tile(_heroSheet, HeroColumns, HeroRows, column, row);
    }

    private Rect2 MonsterTile(int spriteIndex, bool attacking)
    {
        if (_monsterSheet == null)
        {
            return new Rect2();
        }

        var group = spriteIndex / MonsterColumns;
        var column = spriteIndex % MonsterColumns;
        var row = group * 2 + (attacking ? 1 : 0);
        return Tile(_monsterSheet, MonsterColumns, MonsterRows, column, row);
    }

    private Rect2 ItemTile(int spriteIndex)
    {
        return _itemSheet == null ? new Rect2() : Tile(_itemSheet, ItemColumns, ItemRows, spriteIndex % ItemColumns, spriteIndex / ItemColumns);
    }

    private static Rect2 Tile(Texture2D texture, int columns, int rows, int column, int row)
    {
        var width = texture.GetWidth() / (float)columns;
        var height = texture.GetHeight() / (float)rows;
        return new Rect2(column * width, row * height, width, height);
    }

    private static Texture2D? LoadPng(string path)
    {
        var imported = ResourceLoader.Load<Texture2D>(path);
        if (imported != null)
        {
            return imported;
        }

        var bytes = Godot.FileAccess.GetFileAsBytes(path);
        if (bytes.Length == 0)
        {
            return null;
        }

        var image = new Image();
        return image.LoadPngFromBuffer(bytes) == Error.Ok ? ImageTexture.CreateFromImage(image) : null;
    }

    private StyleBoxFlat SceneStyle(RegionDefinition region)
    {
        return Flat(ParseColor(region.Scene.ElementAtOrDefault(1), new Color("#243447")), new Color("#111017"), 4);
    }

    private StyleBoxFlat Flat(Color background, Color border, int borderWidth)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthLeft = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthTop = borderWidth,
            BorderWidthBottom = borderWidth,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0
        };
    }

    private static Color ParseColor(string? hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return fallback;
        }

        try
        {
            return new Color(hex);
        }
        catch (Exception)
        {
            return fallback;
        }
    }
}
