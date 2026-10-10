# The Long Way Home

A quiet, first-person scientific space exploration and survival game. One astronaut remains aboard a compact spacecraft, searching for humanity's next home. This repository currently implements **Milestone 1: Project Foundation**, not the full game.

## Open and run

1. In Unity Hub, choose **Add → Add project from disk**, then select `D:\DistantStars`.
2. Open with **Unity 6000.3.4f1** (the installed Unity 6.3 Editor used to generate this project). Allow the package import to finish.
3. Open `Assets/_Game/Scenes/Bootstrap.unity`.
4. Press Play and click the Game view. Walk with **WASD**, look with the **mouse**, and pause/resume with **Escape**. The orange benches and striped floor provide orientation. Exit Play to release the cursor.
5. Verify the enclosed room renders, walls stop movement, the camera remains inside the capsule, and the Console has no errors. Check both movement and mouse look manually; batch tests do not evaluate visual quality or comfort.

Scenes, input references, player prefab and URP settings are already generated and serialized. Do **not** rerun the one-time `The Long Way Home → Create Foundation (once)` command; it refuses to overwrite existing scenes. Build Profiles should include Bootstrap, ShipInterior and SpaceExterior in that order, all enabled.

The Input System asset reserves left/right mouse grabs, Space jump/push and E interaction for future milestones; these actions have **no gameplay behavior yet**. Acceleration gravity, hands, robot, navigation, survival and science clocks are not implemented. M1 uses a basic grounded Rigidbody controller and Unity's default downward gravity.

## Dependencies and project organization

Unity's installed Universal 3D template generated this project. Direct packages include URP **17.3.0**, Input System **1.17.0**, Test Framework **1.6.0** and Unity UI **2.0.0**. The complete authoritative lists are `Packages/manifest.json` and `Packages/packages-lock.json`. Bundled template utilities are retained; none is a gameplay dependency unless referenced by our assembly definitions. Input System is the active backend. Text serialization and visible meta files are enabled.

- `Assets/_Game/Scenes`: Bootstrap, ShipInterior, SpaceExterior.
- `Assets/_Game/Scripts`: runtime bootstrap and temporary foundation player.
- `Assets/_Game/Input`, `Prefabs/Player`, `Art/Materials`: input map, player and graybox materials.
- `Assets/_Game/Editor`: one-time scene/asset generation and Windows build entry point.
- `Assets/_Game/Tests/PlayMode`: behavior tests.
- `Assets/Settings`: template-generated URP assets. Original template sample content is retained outside `_Game`.

See [game design](GAME_DESIGN.md), [architecture](ARCHITECTURE.md), [roadmap and M2 plan](ROADMAP.md), [scientific assumptions](SCIENTIFIC_ASSUMPTIONS.md), and [agent conventions](AGENTS.md).

## Verification commands

Close any Editor using this project first. Run in PowerShell from the repository. Start-Process waits here so its exit code belongs to Unity, not the launcher. Runtime logs and outputs are ignored by Git.

```powershell
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.3.4f1\Editor\Unity.exe'
New-Item -ItemType Directory -Force TestResults | Out-Null
$testRun = Start-Process -FilePath $unityEditor -ArgumentList '-batchmode -nographics -projectPath D:\DistantStars -runTests -testPlatform PlayMode -testResults D:\DistantStars\TestResults\playmode.xml -logFile D:\DistantStars\TestResults\playmode.log' -WindowStyle Hidden -PassThru -Wait
$testRun.ExitCode

$buildRun = Start-Process -FilePath $unityEditor -ArgumentList '-batchmode -quit -projectPath D:\DistantStars -executeMethod LongWayHome.Editor.FoundationSetup.BuildWindows -logFile D:\DistantStars\TestResults\build.log' -WindowStyle Hidden -PassThru -Wait
$buildRun.ExitCode

git diff --check
```

Test runs intentionally omit `-quit`: the test runner exits when complete. Read the XML and logs in addition to the exit code. In the Editor, use **Window → General → Test Runner → PlayMode → Run All**. Build output is `Builds/Windows/TheLongWayHome.exe`; the Windows build support module is required.

## Verification record

Verified on 2026-10-10 with Unity 6000.3.4f1:

- Unity generated/imported the URP project and compiled the runtime, Editor and test assemblies successfully.
- One-time Editor setup generated all three scenes, the player prefab, input asset and materials successfully.
- **3/3 PlayMode tests passed**: additive composition with exactly one player/camera/listener; synthetic W input moves the Rigidbody and stops at the forward wall with floor support; Escape pauses and resumes physics time. Results: `TestResults/playmode.xml` and `TestResults/playmode.log`.
- Initial tests caught an unassigned input reference in the generated prefab; it was repaired through Unity's prefab API, and setup now forces synchronous input import and rejects a missing asset. Headless input tests explicitly route input without a focused Game view and restore prior settings afterward.
- Asset metadata audit found no missing `.meta` files or duplicate asset GUIDs. Package and input JSON parsed successfully. Staged Git whitespace checks passed; Unity-generated YAML retains its native empty-value spacing through `.gitattributes`.
- **Windows x64 development build succeeded**, and Unity exited with code 0. Build report marker: `FOUNDATION_WINDOWS_BUILD_OK` in `TestResults/build.log`. Executable: `Builds/Windows/TheLongWayHome.exe`. The standalone executable has not been interactively playtested.
- **Not performed:** interactive visual review, mouse-look comfort assessment, or human playtesting. Complete the Open and run steps above for final M1 acceptance.

## Git and next work

The initially empty folder had no Unity project, Git history or remote. Local branch: `chore/unity-setup`. No GitHub repository destination has been selected; nothing has been published. Do not push main or merge without approval. Track `.meta` files, Packages and ProjectSettings; caches and builds are ignored. No large custom binary assets exist yet, so LFS filters are not required. Add LFS deliberately before introducing large art/audio files.

Suggested commit: `chore: establish Unity 6 URP project foundation`.

Prepared issue descriptions and publication commands: [.github/ISSUE_BACKLOG.md](.github/ISSUE_BACKLOG.md). Next task: Milestone 2 acceleration-based gravity and two-hand movement prototype, following ROADMAP.md.
