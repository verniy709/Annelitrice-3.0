# Annelitrice DDS cleaner for RimWorld 1.7

This helper asks players before removing loose DDS texture files that can override bundled mod textures. It is a separate 1.7 assembly, so the existing Annelitrice code and DLL do not need to be rebuilt or ported.

## What players see

At the first usable main menu, the helper runs a read-only check in the background. If DDS files exist, a dialog explains the reason, lists each affected mod copy, its location and file count, and offers a file-path preview.

- **Not now**, Escape, or the close button: keep every file. Ask again next launch if DDS files remain.
- **Delete DDS files**: delete only the previewed candidates after validating them again. Files changed since the preview are skipped.
- A result dialog reports deleted, already absent, skipped and failed files. **Restart RimWorld now** uses the game's own restart function; **Restart later** keeps the game open. Enter never approves deletion or restarting.

Cleanup and restart are available only at the main menu. This avoids abandoning an unsaved colony. A manual check is available under Options → Mod settings → **Annelitrice texture cleanup**. English and Simplified Chinese text are included; other languages use the game's English fallback.

An empty automatic check displays nothing and changes nothing. Each deletion result and affected path is written to the game log. The preview shows up to 200 paths; the result shows up to 40 operations and directs players to the full log.

## Scope

Exact, case-insensitive package IDs:

```
Verniy709.Annelitrice3.0
Verniy709.Annelitrice3.0.Ahmislan
Verniy709.Annelitrice3.0.Gakuen
Verniy709.Annelitrice3.0.Jurnizia
Verniy709.Annelitrice3.0.NeoYokohama
Verniy709.Annelitrice3.0.Oubliee
Verniy709.Annelitrice3.0.YoRHa
Verniy709.FacialAnimationForAnnelitrice3.0Remake
```

All installed copies are checked, including disabled copies and same-origin duplicates that RimWorld omits from its registry. Discovery merges registered locations with immediate child folders of the local Mods directory and registered mod containers, including Workshop containers. It does not search arbitrary drives or recursively treat backup folders as installed copies.

Within an approved mod copy, the cleaner discovers physical `Textures` directories, including version and DLC folders. It deletes individual files whose final extension is `.dds`, case-insensitively. PNGs, `Assets.zip`, bundles, non-texture DDS files and other package IDs are preserved. Source, About, Assemblies, AssetBundles, version-control folders, nested mod roots, symbolic links and junctions are excluded.

The exact package ID in physical `About/About.xml`, canonical mod/texture boundaries, link status, file length and modification time are checked again before deletion. There is no recursive directory deletion and no clearing of read-only flags. Files that cannot be safely checked or deleted are reported. Permissions are never remembered as automatic consent.

This deliberately removes DDS files even when a matching source PNG is absent; the player is warned about permanent deletion, including personal DDS edits. Ship the intended texture sources or bundled replacement assets with the mod. If a converter regenerates DDS files, players should exclude these mods from that converter.

## Build and ship

The source targets the .NET 10 runtime bundled with RimWorld 1.7. Harmony is already a dependency of Annelitrice.

From this source folder:

```powershell
.\build.ps1
```

For another game/Harmony location:

```powershell
.\build.ps1 -RimWorldDirectory 'D:\Steam\steamapps\common\RimWorld' -HarmonyAssembly 'D:\Steam\steamapps\workshop\content\294100\2009463077\1.7\Assemblies\0Harmony.dll'
```

The script uses an installed .NET SDK's Roslyn compiler and explicit references to the installed 1.7 assemblies. It needs no NuGet packages and works with the installed SDK 9 even without SDK 10. The SDK project is also included for development with SDK 10.

Copy only `bin/Release/Annelitrice.DdsCleaner.dll` to the main mod's `1.7/Assemblies`. Copy the contents of this project's `Languages` folder into the main mod's `1.7/Languages`. Do not distribute game assemblies or another copy of Harmony. The helper is loaded once by the main mod and handles all eight package IDs; do not duplicate the DLL into every add-on.

## Tests and verification

```powershell
.\Tests\run-tests.ps1
```

The engine suite covers package matching, all/disabled copies, omitted duplicates, physical texture boundaries, metadata changes, links/junctions, changed/missing/locked/read-only files, nested mod roots, preservation of other assets, and nonmutating scans. The UI suite runs the real controller/window source against narrow fake game APIs to exercise consent, cancellation, once-per-session checks, play-state restrictions and separate restart approval. Only test fixtures are changed.

At implementation time: 26 engine/discovery cases and 9 UI/lifecycle cases passed. Two file-symlink checks could not run because Windows denied creation privileges; directory-junction tests passed. Compilation against the installed RimWorld 1.7 and Harmony assemblies completed with zero warnings. A read-only preview found 15 matching local/Workshop copies and no loose DDS files.

Native window rendering, Harmony dispatch inside the running game, the real asynchronous scheduler and a real process restart still require an in-game check. To exercise the dialog, use a throwaway test copy with a dummy DDS inside `Textures`; review and cancel first. No real mod textures were removed during implementation.
