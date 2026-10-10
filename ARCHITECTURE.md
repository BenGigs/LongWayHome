# Architecture

## Five-deck gameplay test

FiveDeckGameplay is a standalone movement test scene and the first Windows build scene. Bootstrap/ShipInterior remain available unchanged for the earlier regression suite. The Editor-generated test geometry uses an 8 m cylindrical room stack, 3.2 m deck spacing and a 2.1 m offset opening. Mesh decks and segmented orange grip colliders leave the shaft clear. There are no poles, doors, stairs or guardrails. Room fixtures are deliberately simple placeholders; no Blender mesh import is required to test the mechanics.

Each ring has a LedgeRing defining the deck plane and radial hang/landing targets. HandInteractor retains raycast occlusion and reach limits. Holding a grab button retries acquisition so a jump can bring a ring into reach; upper ledges cannot be caught while grounded. LedgeClimber, when present, is ticked by PlayerMovement: it uses bounded velocity corrections and gravity compensation through a held ring to hang, rise inside the shaft, then cross onto the floor while W is held. The Rigidbody remains dynamic and collidable throughout. Releasing W stops pull-up motion; releasing all ledge grips or switching to coasting cancels the assist. The grip remains held on landing. Existing non-ledge rails use the original locomotion path.

GameplayReset reloads only the active test scene on R while unpaused. There is no fall damage in this playtest. Scientific resource/navigation systems and polished art remain outside this test.

## Foundation

`Bootstrap` stays loaded and owns `SceneBootstrap`, which validates build scene availability, loads `ShipInterior` and `SpaceExterior` additively, then makes the interior active. No global singleton or DontDestroyOnLoad manager is needed. Start a normal session through Bootstrap. ShipInterior also works alone for quick movement inspection.

ShipInterior contains a 10 × 14 × 4 metre single-deck graybox. Habitat/greenhouse spans z=-7 to -1, workshop -1 to 3, and cockpit 3 to 7. Two open hatch frames provide a 3.2 m wide, 3.4 m high central passage. Rails at x=±1.25 m, y=1.45 m have 1.2 m segments with 5 cm gaps for handoffs; side rails provide access to fixtures. The cockpit window has a collider but no opaque pane mesh, preserving the pressure boundary while exposing the dark exterior. One berth and one pilot seat support the sole astronaut. +Y remains thrust-up, perpendicular to the deck. This is a reversible interior layout, not a commitment to the final rocket hull shape.

The accepted M2 room is preserved in GravityPrototype.unity, outside the build scene list, for direct Editor comparison. SpaceExterior is deliberately empty until M4. No launch sequence, functional consoles, robot behavior, gardening or animated doors are implemented here. Door panels are represented in their retracted/open positions.

`FoundationPlayer` retains its original asset identity but now owns only the private ShipControls clone, input dispatch, camera pitch, pause and prototype HUD. `PlayerMovement` owns FixedUpdate locomotion, yaw/gravity alignment, grounding and push-off. Capsule is 2 m tall, radius 0.35 m, mass 80 kg with continuous collision detection, interpolation and frozen physical rotation. Layer 2 excludes it from its ground/grab queries. Camera is a child at eye level. The IMGUI HUD is temporary.

`ShipGravityController` exposes a finite local acceleration vector and transforms its negative into world-space effective gravity. `ShipGravityBody` disables built-in gravity and applies acceleration in FixedUpdate, independent of mass. Each body has an explicit serialized source; no global discovery/singleton. The reusable player prefab receives its source through scene overrides. Coasting has zero linear/angular damping on loose tools. Disabling/missing the source yields zero acceleration; missing references are reported at startup.

`PlayerMovement` uses a sphere probe along gravity-up, a slope-normal threshold and bounded velocity corrections for grounded movement. Above 0.1 m/s² the body gradually aligns to gravity-up; below that threshold it retains orientation and cannot walk. In coasting, WASD only applies forces with a held rail. Space adds a configurable contact-normal impulse and preserves both grips. Powered floor jumps use a separate 4 m/s impulse; zero-g pushes retain 2.5 m/s. Powered airborne rail movement preserves the vertical jump arc rather than braking it. Mouse-button release still lets go; overstretch and invalid-target cleanup remain. Recorded static collision contact remains usable during sleep, with an 8 cm geometry tolerance and active-collider check before push-off. A nearby object without prior contact is insufficient for a push; loose objects never grant a free push impulse.

Each `HandInteractor` raycasts from the camera, so walls occlude targets. `Grabbable` permits shared rail grabs but exclusive loose-object ownership. A rail adds a limited-distance ConfigurableJoint on the player; locomotion supplies a bounded contact reaction. Tools use finite-force position spring drives to a kinematic hand target. Angular axes are locked relative to that target to preserve the pickup pose and follow hand rotation; rail angular axes remain free. Release unlocks tool rotation immediately before joint destruction. Tools temporarily ignore player collision; release restores it, frees the constraint immediately, destroys the joint and preserves velocities. Disabled/lost/out-of-reach targets are released. Hands remain simple noncolliding visual spheres. No IK or simulated finger grip.

Pause owns timeScale because there is one player and no other time systems; revisit ownership when simulation clocks arrive. Cloned actions are disabled and destroyed with the player. A held grab persists during pause; release state is reconciled on resume.

`FoundationSetup` is Editor-only. It creates scenes, materials, prefab and input using Unity APIs, preserves existing foundation scenes by refusing to overwrite them, configures visible meta files/text serialization, and writes the build scene list. It also exposes a Windows development build entry point. Generated assets are committed; routine setup does not rerun this tool.

`GravityPrototypeSetup` is a one-time upgrade adding source bindings, movement/hands, G input, rail and tools to the existing interior. It preserves existing room geometry and asset GUIDs. Creating a fresh foundation also invokes this upgrade; normal users simply open the committed Bootstrap scene.

`ShipBlockoutSetup` preserves the M2 scene, then replaces only known placeholder fixtures and the front wall. It adds grouped graybox areas, moves the existing tools to the workshop, and retains player/source bindings. The one-time command refuses to overwrite an existing blockout or comparison scene. `ShipBlockoutPreview.Capture` renders temporary inspection camera poses into ignored TestResults/BlockoutPreview PNGs without saving the camera changes.

## Future boundaries

- Ship remains near Unity origin. Astronomical position/time use double precision and explicit units; rendering uses scaled relative coordinates.
- ScriptableObjects store immutable planet, system, crop, module and configuration definitions. Mutable runtime/save state stores observations, mission, robot wear, resources, crops and clocks.
- UI reads observations, never hidden planetary truth. Deterministic survey and habitability logic stay independent of presentation.
- Prefer serialized dependencies, composition and small events/interfaces only as needed. Do not create the proposed manager roster preemptively.
- Runtime, Editor and PlayMode test assembly definitions separate shipping code and tooling. No large third-party framework.

Folders follow Assets/_Game/{Art,Audio,Data,Input,Prefabs,Scenes,Scripts,Tests,Editor}. Unity template rendering settings remain under Assets/Settings. Template sample assets are retained as supplied and excluded from the foundation build scene list.
