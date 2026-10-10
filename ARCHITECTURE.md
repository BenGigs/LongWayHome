# Architecture

## Foundation

`Bootstrap` stays loaded and owns `SceneBootstrap`, which validates build scene availability, loads `ShipInterior` and `SpaceExterior` additively, then makes the interior active. No global singleton or DontDestroyOnLoad manager is needed. Start a normal session through Bootstrap. ShipInterior also works alone for quick movement inspection.

ShipInterior contains an enclosed 10 × 14 × 4 metre test room, collision geometry, placeholder benches, lighting and a reusable player prefab. SpaceExterior is deliberately empty except for a marker object until Milestone 4. No launch simulation or spacecraft production layout yet.

`FoundationPlayer` reads a private runtime clone of ShipControls. Update samples input and mouse look; FixedUpdate rotates and moves a continuous-collision Rigidbody. A ground probe gates horizontal movement; no steering in free fall. Capsule is 2 m tall, radius 0.35 m, mass 80 kg, with frozen physics rotation. Layer 2 excludes the capsule from its own default-layer ground probe. Camera is a child at eye level. A debug IMGUI panel shows controls and pause state; production UI is deferred.

This M1 controller uses global downward gravity solely for the test room. M2 replaces it with independently applied ship acceleration and separate movement, hands and gravity scripts. It does not yet support arbitrary floor directions, zero-g locomotion or jumping. Pause currently owns timeScale because there is one player and no other time systems; replace that ownership when simulation clocks arrive. Cloned actions are disabled and destroyed with the player.

`FoundationSetup` is Editor-only. It creates scenes, materials, prefab and input using Unity APIs, preserves existing foundation scenes by refusing to overwrite them, configures visible meta files/text serialization, and writes the build scene list. It also exposes a Windows development build entry point. Generated assets are committed; routine setup does not rerun this tool.

## Future boundaries

- Ship remains near Unity origin. Astronomical position/time use double precision and explicit units; rendering uses scaled relative coordinates.
- ScriptableObjects store immutable planet, system, crop, module and configuration definitions. Mutable runtime/save state stores observations, mission, robot wear, resources, crops and clocks.
- UI reads observations, never hidden planetary truth. Deterministic survey and habitability logic stay independent of presentation.
- Prefer serialized dependencies, composition and small events/interfaces only as needed. Do not create the proposed manager roster preemptively.
- Runtime, Editor and PlayMode test assembly definitions separate shipping code and tooling. No large third-party framework.

Folders follow Assets/_Game/{Art,Audio,Data,Input,Prefabs,Scenes,Scripts,Tests,Editor}. Unity template rendering settings remain under Assets/Settings. Template sample assets are retained as supplied and excluded from the foundation build scene list.
