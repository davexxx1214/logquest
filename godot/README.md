# LogQuest Godot Port

Godot 4.6.2 .NET desktop port of the local LogQuest prototype.

## Run

Open `project.godot` with the Godot 4.6.2 .NET editor, or build from PowerShell:

```powershell
dotnet build .\LogQuest.csproj
```

Headless smoke test:

```powershell
& "C:\Users\davex\Downloads\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe" --headless --path . --quit-after 2
```

## Current Scope

- C# project scaffold
- JSON data loaded from `Data/`
- Pixel art tile sheets loaded from `Assets/`
- Main desktop UI generated from `Scripts/Main.cs`
- Runtime save data in `user://logquest-save.json`
- Hero selection, expedition slots, exploration timeline, auto battle, completion rewards, class/region/floor unlocks

## Next Step

Split the large generated UI into reusable scenes, then add attack animation timing, HP interpolation, floating damage numbers, and equipment auto-equip.
