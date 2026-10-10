# The Long Way Home

A quiet, first-person scientific space exploration and survival game. One astronaut remains aboard a compact spacecraft, searching for humanity's next home. **Milestone 1 is implemented; Milestone 2's first gravity and two-hand prototype is ready for validation.**

## Open and run

1. In Unity Hub, choose **Add → Add project from disk**, then select `D:\DistantStars`.
2. Open with **Unity 6000.3.4f1** (the installed Unity 6.3 Editor used to generate this project). Allow the package import to finish.
3. Open `Assets/_Game/Scenes/Bootstrap.unity`.
4. Press Play and click the Game view. Walk with **WASD**, look with the **mouse**, and pause/resume with **Escape**. Exit Play to release the cursor.
5. Verify the enclosed room renders, walls stop movement, the camera remains inside the capsule, and the Console has no errors. Check both movement and mouse look manually; batch tests do not evaluate visual quality or comfort.

Scenes, input references, player prefab and URP settings are already generated and serialized. Do **not** rerun the one-time `The Long Way Home → Create Foundation (once)` or `Upgrade to Gravity Prototype (once)` commands; the committed scenes already contain both. Build Profiles should include Bootstrap, ShipInterior and SpaceExterior in that order, all enabled.

## Try the gravity prototype

- **G**: toggle 1g acceleration / zero-g coasting. The `Ship acceleration` scene object's Inspector exposes the actual acceleration vector for intermediate and other directions.
- **Hold left/right mouse** while aiming the crosshair at a tool or rail within 1.8 m: grab with the corresponding hand. Release the button to let go. Blue is left; amber is right.
- **WASD**: walk while grounded. While holding a rail, pull in the camera's forward/right directions, including vertically when looking up/down. In free float, WASD adds no thrust.
- **Space**: jump from the floor, or push away from a contacted static surface / held rail in zero-g. Pushing releases both hands. Space in open space does nothing.
- The rail is to the left of the starting position. The wrench and sample canister sit on a shelf to the right. Aim at each in turn to hold two different objects.
- **Escape** pauses physics and releases the cursor. Resuming reconciles any grab buttons released while paused. **E** remains reserved for future consoles.

Manual acceptance loop: walk and grab the rail; press G; look slightly up and hold W to pull; press Space to release/push away; observe drift without input thrust. Return to 1g, pick up the wrench and canister, coast, release a moving tool and observe inertia, then restore 1g and check that the objects settle. Repeat near walls and while holding both hands. Record jitter, clipping, excessive reach or uncomfortable camera motion before M3.

The prototype uses simple hand markers, rail reach constraints and spring-driven tool grabs. Tools preserve their pickup orientation relative to the hand, follow camera turns, and regain free rotation on release. No full-body IK, production repair interaction, robot, navigation, survival or science clocks yet.

## Dependencies and project organization

Unity's installed Universal 3D template generated this project. Direct packages include URP **17.3.0**, Input System **1.17.0**, Test Framework **1.6.0** and Unity UI **2.0.0**. The complete authoritative lists are `Packages/manifest.json` and `Packages/packages-lock.json`. Bundled template utilities are retained; none is a gameplay dependency unless referenced by our assembly definitions. Input System is the active backend. Text serialization and visible meta files are enabled.

- `Assets/_Game/Scenes`: Bootstrap, ShipInterior, SpaceExterior.
- `Assets/_Game/Scripts`: bootstrap; player input/view, movement and hand components; ship gravity source and bodies.
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

M2 tool-grip fix on 2026-10-10:

- User playtest feedback: either hand feels good on the rail; loose tools spin instead of feeling held.
- Tool grips now preserve the pickup orientation relative to the hand and follow turns. Pickup anchors and joint frames use the current physics pose, avoiding stale interpolated poses. Release immediately restores free rotation and preserves momentum. Rail constraints are unchanged.
- **12/12 PlayMode tests passed**, including the actual compound wrench in both hands under 1g and coasting, arresting initial spin, following view rotation, keeping the grasp point near the hand, and restoring angular/linear inertia on release. The existing rail tests also pass. Results: `TestResults/grip-playmode.xml` and `TestResults/grip-playmode.log`.
- **Windows x64 development build succeeded**, with Unity exit code 0 in `TestResults/grip-build.log`. Retest wrench handling interactively; no scene regeneration is needed.

Initial M2 verification on 2026-10-10:

- **10/10 PlayMode tests passed** after the final physics-loop input changes. Coverage includes additive boot, walking/wall collision, pause, rotated/intermediate/reversed gravity and mass independence, free-flight inertia with WASD/Space input, repeated G transitions, two-hand object ownership/release, rail pulling and push-off, sleeping surface push-off, wall occlusion, and joint/collision cleanup on hand disable.
- Results: `TestResults/gravity-playmode.xml` and `TestResults/gravity-playmode.log`. Grab tests wait for physics steps because grab requests are now consumed in FixedUpdate.
- No missing asset metadata or duplicate GUIDs; Git diff whitespace checks passed.
- **Windows x64 development build succeeded**, with `FOUNDATION_WINDOWS_BUILD_OK` and Unity exit code 0 in `TestResults/gravity-build.log`. Output: `Builds/Windows/TheLongWayHome.exe`. Human visual/feel playtesting remains outstanding; passing tests do not establish comfortable movement or production-ready joint stability.

Verified on 2026-10-10 with Unity 6000.3.4f1:

- Unity generated/imported the URP project and compiled the runtime, Editor and test assemblies successfully.
- One-time Editor setup generated all three scenes, the player prefab, input asset and materials successfully.
- **3/3 PlayMode tests passed**: additive composition with exactly one player/camera/listener; synthetic W input moves the Rigidbody and stops at the forward wall with floor support; Escape pauses and resumes physics time. Results: `TestResults/playmode.xml` and `TestResults/playmode.log`.
- Initial tests caught an unassigned input reference in the generated prefab; it was repaired through Unity's prefab API, and setup now forces synchronous input import and rejects a missing asset. Headless input tests explicitly route input without a focused Game view and restore prior settings afterward.
- Asset metadata audit found no missing `.meta` files or duplicate asset GUIDs. Package and input JSON parsed successfully. Staged Git whitespace checks passed; Unity-generated YAML retains its native empty-value spacing through `.gitattributes`.
- **Windows x64 development build succeeded**, and Unity exited with code 0. Build report marker: `FOUNDATION_WINDOWS_BUILD_OK` in `TestResults/build.log`. Executable: `Builds/Windows/TheLongWayHome.exe`. The standalone executable has not been interactively playtested.
- **Not performed:** interactive visual review, mouse-look comfort assessment, or human playtesting. Complete the Open and run steps above for final M1 acceptance.

## Git and next work

Remote: [BenGigs/LongWayHome](https://github.com/BenGigs/LongWayHome). The foundation is published on `chore/unity-setup`; the prototype is developed on `feat/ship-gravity`. The initially empty remote uses the foundation branch as its first review base. No direct push to main or PR merge is authorized. Track `.meta` files, Packages and ProjectSettings; caches and builds are ignored. No large custom binary assets exist yet, so LFS filters are not required.

Foundation acceptance: [issue #1](https://github.com/BenGigs/LongWayHome/issues/1). Gravity prototype: [issue #2](https://github.com/BenGigs/LongWayHome/issues/2).

Next task: validate and tune the M2 movement loop with human playtesting before moving to ship blockout. See ROADMAP.md.
