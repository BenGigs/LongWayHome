# Roadmap

## Milestone 1 — foundation

Implemented: Unity-generated URP project; Input System; project folders; three-scene additive boot; enclosed controllable graybox; Rigidbody camera/player prefab; foundation input map; pause; Unity-compatible Git tracking; persistent design and engineering docs; behavioral PlayMode tests and Windows build entry point.

Verification status is recorded in README.md. Human movement-feel and visual review remain part of acceptance even after automated checks pass.

## Milestone 2 — movement baseline accepted

The user approved the rail grabbing, then confirmed that the corrected tool grip feels good. This establishes the movement baseline for M3. The fix passed 12 PlayMode tests and a Windows build. Further tuning can follow layout playtests; this is not a claim of final production polish.

The movement baseline includes a configurable acceleration source, per-body gravity, gravity-relative grounded movement, coasting inertia, two hand markers, rail constraints/pulling, a wrench and sample canister, contact push-off and G gravity toggle. Input, locomotion, hands and gravity are separate components. The implementation sequence is retained below; automated verification is recorded in README.

1. Replace the temporary controller's global gravity dependency with ShipGravityController exposing a ship-local acceleration vector and ShipGravityBody applying -a in FixedUpdate with useGravity disabled. Test zero, intermediate, 1g, reversed and rotated vectors, independent of mass.
2. Implement Rigidbody PlayerMovement relative to gravity/contact normals; grounded acceleration and slope/contact tolerances. During free float retain momentum; WASD cannot add thrust. Make transition thresholds explicit and test both directions.
3. Add two visible simple hands, separate HandInteractor instances and camera-relative reach targets. Use stable configurable constraints, collision rules, reach limits and cleanup on release/disable. Distinguish static rail from movable held object; prevent contradictory double grabs.
4. Place one handrail and one grabbable wrench in the existing enclosed room. Implement contact-based pulling and Space push-off; no contact means no push impulse. Verify separate hands, two-object holding, rail transfer and object inertia.
5. Add a test-only 1g/coasting control driving the vector, not a gameplay gravity boolean. Test rapid transitions, rail grabs during acceleration, wall impacts, dropped wrench and repeated release/regrab. Tune stability before adding IK or visuals.
6. Record automated vector/inertia/constraint checks and manually playtest the complete walk → coast → rail pull → wrench handling → powered walking loop. Review movement enjoyment with the user before Milestone 3.

## Milestone 3 — spacecraft blockout, ready for layout playtest

Three areas share a compact 10 × 14 m deck with +Y thrust normal to the floor: habitat/greenhouse, workshop and cockpit. Open hatch frames and rail segments connect the central passage. Fixtures reserve space for crops/water, food preparation, one sleeping berth, robot docking, workbench/tool storage, one pilot seat and consoles. A clear collision window forms the cockpit pressure boundary. All systems beyond movement remain placeholders.

The previous M2 room is preserved as GravityPrototype.unity for comparison. Existing player/hand code remains the accepted baseline. Acceptance: walk and rail-pull through all areas, in both directions, inspect windows and work surfaces, and review the layout before M4. Verification details are recorded in README.

## Later milestones

4. Configurable Aster/Nivalis/Eos system, navigation hologram, manual local flight, autopilot and basic scanning.
5. Named robot, modules, deployment/recovery, deterministic surveys, repairs and permanent loss.
6. One crop, food, enclosed irrigation, water reservoir/recycling/filter wear and consequences.
7. Ship/Earth clocks, relativistic travel, deadline, verified habitability, communication/fleet timeline, success/failure and save state.
8. Lighting/audio/UI polish, performance, balancing and external playtesting of the 15–20 minute slice.

Estimates in the original brief are preliminary. Full launch, extended galaxy, detailed orbital physics and broader survival remain post-slice features.

M3 follow-up: user requested grip-preserving, stronger jumps and selected an upright expedition ship with stacked decks, a practical exterior and a warm cabin. The Blender design proposal is recorded in GAME_DESIGN.md. Review deck circulation and silhouette before replacing the current Unity blockout; M4 remains later work.

M3 Blender review model: ArtSource/Ship/LongWayHome_Expedition_v03.blend contains editable cutaway and exterior scenes, three furnished decks, spiral stairs/shaft, a human reference and propulsion/radiator silhouettes. v01/v02 are retained locally as earlier review snapshots. Rendered previews accompany the source. This is a design study awaiting user changes, not a replacement for the Unity scene or a validated production mesh.

Current M3 design supersedes the three-deck stair model: v04 has five cylindrical stacked rooms, no doors/stairs/guardrails, open aligned hatches, floor-level grab rings and a side-mounted climbing pole. Navigation includes an open sleeping nook. Blender review first; powered pole climbing and the new layout's Unity integration remain pending.

M3 v05 supersedes the pole: remove it entirely and preserve floor-edge grab rings. Planned powered traversal is jump, catch the next deck and pull up. The shaft stays clear for falling/coasting. Unity ledge catching and pull-ups remain pending; the existing rail motor does not implement them.

Gameplay testing now takes priority over art: feat/ledge-traversal adds FiveDeckGameplay, catches/hanging/pull-ups, free drops, coasting and reset. Preserve earlier scenes for regression tests. This is still the M3 movement/layout playtest, not M4 navigation implementation.

Human feedback approved the initial traversal. Current M3 tuning eases active pull-ups and adds input-gated, occlusion-checked ring aim tolerance to help select a deck during descent. Review the new feel before adding further braking or movement mechanics.

User accepted the smoother climbing and catch tolerance. Follow-up adds A/D body shifts and S lowering while gripping, and removes the on-screen HUD/instructions/crosshair. This remains M3 traversal tuning.

Traversal follow-up replaces the recessed segmented ring with a continuous flush rim. Ring and deck share their top plane, thickness and boundary so the player can cross the orange edge without encountering a step. The original opening diameter and grab behavior are retained.

User accepted traversal and the flush rims. The next approved M3 interaction test is battery delivery: carry a pack from fuel/storage through two hatches with one free hand, latch it into the workshop to power a lamp/fan, remove it, and repeat while coasting. No navigation, robot deployment or resource simulation is included. Work is on feat/battery-delivery, stacked on feat/ledge-traversal.

Battery carry feedback: climbing works, but spring lag, visible jitter and rim snags caused unwanted drops. The follow-up replaces the loose spring grip with a firm joint, drives it from physics-space poses, tucks the pack during climbs, retracts near obstacles, and animates visible reaching/retrieval. Intentional release remains immediate; no HUD is added.
