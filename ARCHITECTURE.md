# Architecture

## Foundation

`Bootstrap` stays loaded and owns `SceneBootstrap`, which validates build scene availability, loads `ShipInterior` and `SpaceExterior` additively, then makes the interior active. No global singleton or DontDestroyOnLoad manager is needed. Start a normal session through Bootstrap. ShipInterior also works alone for quick movement inspection.

ShipInterior contains an enclosed 10 × 14 × 4 metre test room, collision geometry, placeholder benches, lighting and a reusable player prefab. SpaceExterior is deliberately empty except for a marker object until Milestone 4. No launch simulation or spacecraft production layout yet.

`FoundationPlayer` retains its original asset identity but now owns only the private ShipControls clone, input dispatch, camera pitch, pause and prototype HUD. `PlayerMovement` owns FixedUpdate locomotion, yaw/gravity alignment, grounding and push-off. Capsule is 2 m tall, radius 0.35 m, mass 80 kg with continuous collision detection, interpolation and frozen physical rotation. Layer 2 excludes it from its ground/grab queries. Camera is a child at eye level. The IMGUI HUD is temporary.

`ShipGravityController` exposes a finite local acceleration vector and transforms its negative into world-space effective gravity. `ShipGravityBody` disables built-in gravity and applies acceleration in FixedUpdate, independent of mass. Each body has an explicit serialized source; no global discovery/singleton. The reusable player prefab receives its source through scene overrides. Coasting has zero linear/angular damping on loose tools. Disabling/missing the source yields zero acceleration; missing references are reported at startup.

`PlayerMovement` uses a sphere probe along gravity-up, a slope-normal threshold and bounded velocity corrections for grounded movement. Above 0.1 m/s² the body gradually aligns to gravity-up; below that threshold it retains orientation and cannot walk. In coasting, WASD only applies forces with a held rail. Space adds a configurable contact-normal impulse and releases both hands. Recorded static collision contact remains usable during sleep, with an 8 cm geometry tolerance and active-collider check before push-off. A nearby object without prior contact is insufficient for a push; loose objects never grant a free push impulse.

Each `HandInteractor` raycasts from the camera, so walls occlude targets. `Grabbable` permits shared rail grabs but exclusive loose-object ownership. A rail adds a limited-distance ConfigurableJoint on the player; locomotion supplies a bounded contact reaction. Tools use finite-force position spring drives to a kinematic hand target. Angular axes are locked relative to that target to preserve the pickup pose and follow hand rotation; rail angular axes remain free. Release unlocks tool rotation immediately before joint destruction. Tools temporarily ignore player collision; release restores it, frees the constraint immediately, destroys the joint and preserves velocities. Disabled/lost/out-of-reach targets are released. Hands remain simple noncolliding visual spheres. No IK or simulated finger grip.

Pause owns timeScale because there is one player and no other time systems; revisit ownership when simulation clocks arrive. Cloned actions are disabled and destroyed with the player. A held grab persists during pause; release state is reconciled on resume.

`FoundationSetup` is Editor-only. It creates scenes, materials, prefab and input using Unity APIs, preserves existing foundation scenes by refusing to overwrite them, configures visible meta files/text serialization, and writes the build scene list. It also exposes a Windows development build entry point. Generated assets are committed; routine setup does not rerun this tool.

`GravityPrototypeSetup` is a one-time upgrade adding source bindings, movement/hands, G input, rail and tools to the existing interior. It preserves existing room geometry and asset GUIDs. Creating a fresh foundation also invokes this upgrade; normal users simply open the committed Bootstrap scene.

## Future boundaries

- Ship remains near Unity origin. Astronomical position/time use double precision and explicit units; rendering uses scaled relative coordinates.
- ScriptableObjects store immutable planet, system, crop, module and configuration definitions. Mutable runtime/save state stores observations, mission, robot wear, resources, crops and clocks.
- UI reads observations, never hidden planetary truth. Deterministic survey and habitability logic stay independent of presentation.
- Prefer serialized dependencies, composition and small events/interfaces only as needed. Do not create the proposed manager roster preemptively.
- Runtime, Editor and PlayMode test assembly definitions separate shipping code and tooling. No large third-party framework.

Folders follow Assets/_Game/{Art,Audio,Data,Input,Prefabs,Scenes,Scripts,Tests,Editor}. Unity template rendering settings remain under Assets/Settings. Template sample assets are retained as supplied and excluded from the foundation build scene list.
