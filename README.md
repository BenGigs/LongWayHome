# The Long Way Home

A quiet, first-person scientific space exploration and survival game. One astronaut remains aboard a compact spacecraft, searching for humanity's next home. **The movement baseline is accepted; Milestone 3 adds a cockpit, workshop and habitat blockout for layout playtesting.**

## Open and run

1. In Unity Hub, choose **Add → Add project from disk**, then select `D:\DistantStars`.
2. Open with **Unity 6000.3.4f1** (the installed Unity 6.3 Editor used to generate this project). Allow the package import to finish.
3. Open `Assets/_Game/Scenes/Bootstrap.unity`.
4. Press Play and click the Game view. Walk with **WASD**, look with the **mouse**, and pause/resume with **Escape**. Exit Play to release the cursor.
5. Verify all three areas render, walls stop movement, the camera remains inside the capsule, and the Console has no errors. Check both movement and mouse look manually; batch tests do not evaluate visual quality or comfort.

Scenes, input references, player prefab and URP settings are already generated and serialized. Do **not** rerun the one-time `The Long Way Home → Create Foundation (once)` or `Upgrade to Gravity Prototype (once)` or `Build Ship Blockout (once)` commands; the committed scenes already contain these upgrades. Build Profiles should include Bootstrap, ShipInterior and SpaceExterior in that order, all enabled.

## Try the gravity prototype

- **G**: toggle 1g acceleration / zero-g coasting. The `Ship acceleration` scene object's Inspector exposes the actual acceleration vector for intermediate and other directions.
- **Hold left/right mouse** while aiming the crosshair at a tool or rail within 1.8 m: grab with the corresponding hand. Release the button to let go. Blue is left; amber is right.
- **WASD**: walk while grounded. While holding a rail, pull in the camera's forward/right directions, including vertically when looking up/down. In free float, WASD adds no thrust.
- **Space**: jump from the floor, or push away from a contacted static surface / held rail in zero-g. Jumping and pushing keep both grips; release each mouse button to let go. Floor jumps use a separate 4 m/s impulse (about 0.8 m rise at 1g when unobstructed). Space in open space does nothing.
- The rail is to the left of the starting position. The wrench and sample canister sit on the right-hand shelf in the workshop, through the first hatch. Aim at each in turn to hold two different objects.
- **Escape** pauses physics and releases the cursor. Resuming reconciles any grab buttons released while paused. **E** remains reserved for future consoles.

Manual acceptance loop: walk and grab the rail; press G; look slightly up and hold W to pull; press Space to push while keeping your grip; release the mouse buttons to let go and observe drift without input thrust. Return to 1g, pick up the wrench and canister, coast, release a moving tool and observe inertia, then restore 1g and check that the objects settle. Repeat near walls and while holding both hands. Record jitter, clipping, excessive reach or uncomfortable camera motion during the layout playtest.

The prototype uses simple hand markers, rail reach constraints and spring-driven tool grabs. Tools preserve their pickup orientation relative to the hand, follow camera turns, and regain free rotation on release. No full-body IK, production repair interaction, robot, navigation, survival or science clocks yet.

## Dependencies and project organization

### Blender ship design study

Open `ArtSource/Ship/LongWayHome_Expedition_v03.blend` with Blender 5.0. The default scene is **01 CUTAWAY — interior review**. Switch the scene selector to **02 EXTERIOR — complete hull** for the full ship. Middle mouse orbits, the wheel zooms, Shift + middle mouse pans, and Numpad 0 shows the review camera. The Outliner groups hull sections, each deck, stairs, fixtures and lighting separately. Select a fixture and use G/R/S to move, rotate or scale it. Save design revisions under a new filename.

This editable concept has a 10 m cabin diameter, 24 m overall height, three decks and a 1.8 m human reference. The cabin was widened from the initial 8 m proposal to reserve a 1.2 m stair around a 2 m zero-g shaft. These dimensions and fixture positions are for review; circulation and collision have not been imported or playtested in Unity. Blender +Z is ship-up, while the Unity testbed uses +Y. The drive and radiators are speculative shape placeholders. Source files stay outside Unity Assets.

`ArtSource/Ship/build_ship.py` reproducibly creates the model and preview renders using Blender's Python API. It refuses to overwrite the generated .blend unless explicitly passed `--replace-generated`; do not regenerate over manual edits. The original Unity ship blockout remains unchanged. The .blend includes a **START HERE** text block with review instructions.

Verified with Blender 5.0.1: reviewed exterior, cutaway and cabin-detail renders; reopened the saved v03 model in a fresh process and confirmed both scene visibility states, metric units, deck collections, 40 stair treads, and 24 m overall mesh height. The model contains 443 editable objects. This validation does not establish gameplay clearance or production mesh suitability.

Unity's installed Universal 3D template generated this project. Direct packages include URP **17.3.0**, Input System **1.17.0**, Test Framework **1.6.0** and Unity UI **2.0.0**. The complete authoritative lists are `Packages/manifest.json` and `Packages/packages-lock.json`. Bundled template utilities are retained; none is a gameplay dependency unless referenced by our assembly definitions. Input System is the active backend. Text serialization and visible meta files are enabled.

- `Assets/_Game/Scenes`: Bootstrap, ShipInterior, SpaceExterior; GravityPrototype is the preserved M2 room and is excluded from builds.
- `Assets/_Game/Scripts`: bootstrap; player input/view, movement and hand components; ship gravity source and bodies.
- `Assets/_Game/Input`, `Prefabs/Player`, `Art/Materials`: input map, player and graybox materials.
- `Assets/_Game/Editor`: one-time scene/asset generation and Windows build entry point.
- `Assets/_Game/Tests/PlayMode`: behavior tests.
- `Assets/Settings`: template-generated URP assets. Original template sample content is retained outside `_Game`.

See [game design](GAME_DESIGN.md), [architecture](ARCHITECTURE.md), [roadmap](ROADMAP.md), [scientific assumptions](SCIENTIFIC_ASSUMPTIONS.md), and [agent conventions](AGENTS.md).

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

Jump follow-up on 2026-10-10:

- Space preserves rail and tool grips; mouse buttons control intentional release. Powered floor jumps now add 4 m/s separately from the unchanged 2.5 m/s zero-g push. Airborne rail movement no longer brakes the powered jump's vertical arc.
- **15/15 PlayMode tests passed**, including a powered jump with a rail and tool held simultaneously, measured 0.65–0.95 m peak rise and return to deck, and zero-g rail push retaining both hands until mouse release. Results: `TestResults/jump-playmode.xml` and `TestResults/jump-playmode.log`.
- **Windows x64 development build succeeded** with `FOUNDATION_WINDOWS_BUILD_OK` and Unity exit code 0 in `TestResults/jump-build.log`.
- User selected an upright expedition ship with stacked decks and a warm cabin. The proposed Blender layout is recorded in GAME_DESIGN.md; no replacement ship model has been created yet.
M3 blockout verification on 2026-10-10:

- User accepted the corrected M2 movement feel before this layout work.
- **14/14 PlayMode tests passed**: all 12 movement regressions plus capsule passage/window collision and real rail handoffs from habitat to cockpit and back without free-flight thrust. Results: `TestResults/blockout-playmode.xml` and `TestResults/blockout-playmode.log`.
- Four rendered Editor views inspected in `TestResults/BlockoutPreview`. Inspection caught signs drawing through bulkheads; a depth-tested cabin font shader corrected this, and the views were recaptured and reviewed (`TestResults/blockout-signs.log`).
- **Windows x64 development build succeeded** after the sign correction, with `FOUNDATION_WINDOWS_BUILD_OK` and exit code 0 in `TestResults/blockout-build.log`. Output: `Builds/Windows/TheLongWayHome.exe`.
- The graybox has no exterior scenery or functional consoles, crops or robot. Human layout/comfort acceptance remains pending.
- The original movement room is preserved in `Assets/_Game/Scenes/GravityPrototype.unity`, outside the build list. Runtime movement code is unchanged.
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

Remote: [BenGigs/LongWayHome](https://github.com/BenGigs/LongWayHome). The foundation is published on `chore/unity-setup`; the movement prototype is on `feat/ship-gravity`, and M3 is on `feat/ship-interior` based on that branch. The initially empty remote uses the foundation branch as its first review base. No direct push to main or PR merge is authorized. Track `.meta` files, Packages and ProjectSettings; caches and builds are ignored. No large custom binary assets exist yet, so LFS filters are not required.

Foundation acceptance: [issue #1](https://github.com/BenGigs/LongWayHome/issues/1). Gravity prototype: [issue #2](https://github.com/BenGigs/LongWayHome/issues/2). Ship blockout: [issue #4](https://github.com/BenGigs/LongWayHome/issues/4).

Next task: playtest the M3 layout before starting M4 navigation and scanning. Walk from the habitat through the workshop to the cockpit, then press G and use the passage rails to travel both ways. Check hatch clearance, tool access and the single pilot seat/berth arrangement. Consoles, crops and the robot bay are space reservations only. See ROADMAP.md.
