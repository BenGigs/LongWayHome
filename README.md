# The Long Way Home

A quiet, first-person scientific space exploration and survival game. One astronaut remains aboard a compact spacecraft, searching for humanity's next home. **Current playtest: jump, catch a ring and pull up through five open decks. Ship art remains a Blender design study.**

## Open and run

1. In Unity Hub, choose **Add → Add project from disk**, then select `D:\DistantStars`.
2. Open with **Unity 6000.3.4f1** (the installed Unity 6.3 Editor used to generate this project). Allow the package import to finish.
3. Open `Assets/_Game/Scenes/FiveDeckGameplay.unity` for the current gameplay test. The earlier `Bootstrap.unity` scene remains available for comparison.
4. Press Play and click the Game view. Walk with **WASD**, look with the **mouse**, and pause/resume with **Escape**. Exit Play to release the cursor.
5. Follow the five-deck playtest below. Check movement feel and mouse-look comfort manually; automated tests do not establish either.

Scenes, input references, player prefab and URP settings are already generated and serialized. Do **not** rerun the one-time `The Long Way Home → Create Foundation (once)` or `Upgrade to Gravity Prototype (once)` or `Build Ship Blockout (once)` commands; the committed scenes already contain these upgrades. Build Profiles start with FiveDeckGameplay, followed by Bootstrap, ShipInterior and SpaceExterior, all enabled. Do not rerun Create Five Deck Gameplay (once); the scene and mesh are committed.

## Five-deck gameplay playtest

Latest cockpit/drop correction: ceiling contacts cannot trigger an upward tabletop lift. Held presentation stays close to the physical body, and release reconciles only small collision-free visual offsets. **32/32 PlayMode tests passed** (`TestResults/drop-playmode.xml`), including a cockpit ceiling jump followed by a drop that stays on the cockpit floor, plus table clearance, walking stability, climbing and socket regressions. Human confirmation of the reported glitch remains pending.

Latest table-approach refinement: actual held-collider overlap checks replace chest-directed sphere retraction. The pack lifts just enough to clear nearby tabletops, with surface-normal sliding for taller obstacles. Automatic climb tucking remains removed. **31/31 PlayMode tests passed** (`TestResults/surface-playmode.xml`), including forward/visible/nonpenetrating carry during table approach while looking ahead and down, plus the existing camera-relative jitter, climbing, pickup and socket regressions. Human feel review remains pending.

Latest carry presentation: automatic climb tucking is removed; the object stays in the normal forward hold, with retraction only for obstructions. Held mesh rendering now follows the camera frame instead of independently interpolated physics bodies. **30/30 PlayMode tests passed** (`TestResults/steady-playmode.xml`), including a LateUpdate check requiring under 2 mm camera-relative visible grip error during walking, original-renderer restoration after release, and the previous movement/carry/socket tests. This supersedes the tucking behavior described in the earlier verification record below. Human feel review remains pending.

The steady-carry Windows build succeeded with Editor exit code 0 (`TestResults/steady-build.log`) and was launched from `Builds/WindowsSteady/TheLongWayHome.exe`.

Carry refinement: tools use a firm grip with a visible 0.18 s reach and 0.28 s retrieval. They tuck closer during climbs and retract near obstructions, retaining world collision. Carried objects no longer detach from the rail distance rule. **29/29 PlayMode tests passed** (`TestResults/carry-final.xml`), including grip-error bounds while walking and looking around during two one-handed climbs, visible reach timing, release/inertia, and battery installation/removal. Human review of pickup and carrying feel remains pending.

The grip-refinement Windows build succeeded with Editor exit code 0 (`TestResults/carry-build.log`) and was launched from `Builds/WindowsGrip/TheLongWayHome.exe`.

Battery delivery playtest: pick up the orange pack with a blue stripe from its pedestal in fuel/storage. Carry it up two decks to the workshop using your free hand on the rings. Bring it close to the blue socket between orange guides on the workbench, aim at the socket, and press **E** to latch it. The green lamp and spinning fan indicate power. Aim at the installed battery and press E again to release it; catch it with a mouse button. G lets you repeat in coasting, and R restores the initial battery position. No HUD prompts are shown. This is a reusable interaction test, not a battery-charge or ship-power simulation.

Battery verification: **28/28 PlayMode tests passed** (`TestResults/battery-playmode.xml`), including two actual one-handed ascents while retaining the pack, E installation/removal, lamp/fan feedback, latch stability across gravity changes, ejection/drift, reset, distance/obstruction rejection, and the prior movement regressions. Supply and workshop renders were inspected. Human review of the delivery loop remains pending.

The battery Windows development build succeeded with Editor exit code 0 (`TestResults/battery-build.log`) and was launched from `Builds/WindowsBattery/TheLongWayHome.exe`. Do not rerun `BatteryDeliverySetup.Create`; the fixtures and references are committed in FiveDeckGameplay.

Latest tuning build: `Builds/WindowsSteady/TheLongWayHome.exe` (separate from the prior playtest). Rebuild with the batch Editor entry point `LongWayHome.Editor.FoundationSetup.BuildSteadyPlaytest`.

Flush-rim verification: the full run passed the previous 24 tests; the new crossing check needed a longer traversal interval (`TestResults/flush-playmode.xml`). With the corrected interval and final mesh shading, all 10 ledge tests passed (`TestResults/flush-final.xml`), including same-height ring/deck ray checks and a capsule crossing the rim into a fall. The rendered seam was inspected. Unity generated the replacement continuous rim meshes and scene changes, preserving the deck GUID; metadata/whitespace audits passed.

The flush-rim Windows build succeeded with Editor exit code 0 (`TestResults/flush-build.log`) and was launched for playtesting.

Directional control verification: all 24 PlayMode tests passed (`TestResults/directional-playmode.xml`). After tightening sideways-only movement during a paused mantle, all 9 ledge tests passed again (`TestResults/directional-final.xml`). The new test checks left/right body movement, fixed hand anchors, lowering, pausing and resuming ascent. The user accepted the preceding smoothing/aim-assistance pass; directional movement awaits feel review.

The directional Windows build succeeded with Editor exit code 0 (`TestResults/directional-build.log`) and was launched for playtesting. Runtime HUD rendering was removed, including the crosshair and pause text.

Tuning verification: 22/23 checks passed in `TestResults/smooth-playmode.xml`; the new ease-in test's initial timing bound was corrected for coroutine scheduling, then passed in `TestResults/smooth-ramp.xml`. Runtime code was unchanged between runs. These cover all 23 behaviors, including near-miss descent acquisition, blocked/out-of-reach rejection, four-deck ascent and free falls. Windows development build succeeded with Editor exit code 0 (`TestResults/smooth-build.log`). Human review of the new feel remains pending.

Start in fuel/storage on the bottom deck. The orange ring above the open hatch is the next grab target. No poles, stairs, doors, guardrails or fall damage are included. On-screen instructions, status boxes and the crosshair are removed; Escape still pauses/resumes without a menu.

1. Aim up at the orange ring, hold either mouse button, then press Space to jump. A held grab button retries while the target comes within reach. Floor collision still blocks grabs through solid deck surfaces.
2. Keep the grab button held to hang. Hold **W** to pull into the opening, rise above the floor, and move onto its edge. Use **A/D** to shift your body left/right beneath the fixed grip and **S** to lower yourself. Release movement keys to pause; releasing your grip lets you fall. Sideways travel is limited to 35 cm from the catch path, and lowering extends 40 cm below the normal hang.
3. On landing, release W before walking again. Your grip stays held until you release its mouse button. Let go before jumping to the next ring. Repeat through habitat/water, workshop/robot, navigation/open sleeping nook, and cockpit.
   For descent, look toward the orange ring of the floor you want and hold a mouse button as it comes into reach. Ring targeting tolerates a near miss of up to 28 cm around the aim ray, with the same 1.8 m reach and solid-surface occlusion. Release both buttons to fall freely. Pull-ups now ease into motion and around the lip; release W to pause as before.
4. Press **G** to coast. While holding a ring, look where you want to move and use WASD to pull. Release to drift; WASD in open space adds no thrust. Space pushes only from contact or a held ring.
5. **R** resets the test scene. **Escape** pauses/resumes and releases the cursor. Try missed catches, letting go mid-pull, falling several decks, and toggling gravity while attached. Report clipping, uncomfortable camera motion or catches that feel unreliable.

Pull-ups keep the Rigidbody dynamic and collidable. The orange rims share the floor height and thickness, with a continuous mesh collider and no recessed step. The 2.1 m opening remains clear; the inner face and underside are grabbable. This test uses simple Unity-generated geometry matching the five-deck dimensions; it does not yet import the Blender ship. The existing tool/gravity prototype below remains available through Bootstrap.
## Try the gravity prototype

- **G**: toggle 1g acceleration / zero-g coasting. The `Ship acceleration` scene object's Inspector exposes the actual acceleration vector for intermediate and other directions.
- **Hold left/right mouse** while aiming the centre of the view at a tool or rail within 1.8 m: grab with the corresponding hand. Release the button to let go. Blue is left; amber is right.
- **WASD**: walk while grounded. While holding a rail, pull in the camera's forward/right directions, including vertically when looking up/down. In free float, WASD adds no thrust.
- **Space**: jump from the floor, or push away from a contacted static surface / held rail in zero-g. Jumping and pushing keep both grips; release each mouse button to let go. Floor jumps use a separate 4 m/s impulse (about 0.8 m rise at 1g when unobstructed). Space in open space does nothing.
- The rail is to the left of the starting position. The wrench and sample canister sit on the right-hand shelf in the workshop, through the first hatch. Aim at each in turn to hold two different objects.
- **Escape** pauses physics and releases the cursor. Resuming reconciles any grab buttons released while paused. **E** operates the workshop battery latch in FiveDeckGameplay.

Manual acceptance loop: walk and grab the rail; press G; look slightly up and hold W to pull; press Space to push while keeping your grip; release the mouse buttons to let go and observe drift without input thrust. Return to 1g, pick up the wrench and canister, coast, release a moving tool and observe inertia, then restore 1g and check that the objects settle. Repeat near walls and while holding both hands. Record jitter, clipping, excessive reach or uncomfortable camera motion during the layout playtest.

The prototype uses simple hand markers, rail reach constraints and firm tool grips with a visible reach/retrieve motion. Tools preserve their pickup orientation relative to the hand, follow camera turns, and regain free rotation on release. No full-body IK, production repair interaction, robot, navigation, survival or science clocks yet.

## Dependencies and project organization

### Blender ship design study

Open `ArtSource/Ship/LongWayHome_Expedition_v05.blend` with Blender 5.0. The default scene is **01 CUTAWAY — five open decks**. Switch to **02 EXTERIOR — five deck cylinder** for the full hull. Middle mouse orbits, wheel zooms, Shift + middle mouse pans, and Numpad 0 shows the review camera. Named Outliner collections contain each room, hull, rings and fixtures; G/R/S move, rotate and scale selected objects. Save revisions under a new filename.

Top to bottom: cockpit; navigation/open sleeping nook; workshop/robot deployment; habitat/water; fuel/storage. The 8 m diameter, 25 m tall design has no stairs, doors or guardrails. Five deck-level orange grab rings surround aligned 2.1 m openings. The pole is removed, leaving the shaft fully clear for falls and zero-g movement. Under gravity the proposed movement is jump → catch the next floor-edge ring → pull up onto the deck. Front floor sections are hidden only in cutaway view. The 1.8 m human is a scale reference.

`ArtSource/Ship/build_five_deck_ship.py` creates v05 and the renders in `ArtSource/Ship/Previews/v05`. It refuses to overwrite an existing model unless explicitly passed `--replace-generated`; do not regenerate over manual edits. Blender +Z is ship-up; Unity uses +Y. The older v03 model and generator are retained as design history.

Verified in a fresh Blender 5.0.1 process: both saved scene states, five correctly ordered decks, five grab rings, no pole/stair/door/guardrail objects, 25 m mesh height, and an unobstructed ray down the shaft to the bottom room. This is a geometry check, not a player-capsule traversal test.

This remains editable Blender design geometry. FiveDeckGameplay separately prototypes ledge catches and pull-ups using Unity-generated colliders; the Blender mesh itself has not been imported. Drive/radiator/robot-deployment shapes are placeholders. The .blend includes a START HERE text block.

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

Five-deck gameplay verification:

- **21/21 PlayMode tests passed** in `TestResults/ledge-playmode.xml`: earlier movement/tool regressions plus all four powered deck transitions, hanging, continuous-input catches, two-hand support, pausing a pull-up, release/fall, gravity transition, collision blocking, zero-g pull/release inertia, and reset without duplicate players.
- Tests caught and resolved a primitive floor collider mismatch and grab targets hidden by the deck lip. The final floor is a static mesh collider; the underside of each ring projects slightly below the lip. The ascent test uses actual jumps/grabs/motor forces without teleporting between decks. The independent full-height drop check starts a stationary capsule over the shaft.
- **Windows x64 development build succeeded** (`FOUNDATION_WINDOWS_BUILD_OK`, Unity exit code 0, `TestResults/ledge-build.log`). Spawn/hatch and cockpit renders were inspected. The standalone playtest was launched; human movement-feel acceptance remains pending.
- Metadata audit found no missing .meta files or duplicate asset GUIDs; Git whitespace checks passed.


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

Table-approach Windows rebuild succeeded with Editor exit code 0 (`TestResults/surface-build.log`), replacing `Builds/WindowsSteady/TheLongWayHome.exe`.

Cockpit/drop Windows rebuild succeeded with Editor exit code 0 (`TestResults/drop-build.log`) at `Builds/WindowsSteady/TheLongWayHome.exe`.
