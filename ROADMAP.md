# Roadmap

## Milestone 1 — foundation

Implemented: Unity-generated URP project; Input System; project folders; three-scene additive boot; enclosed controllable graybox; Rigidbody camera/player prefab; foundation input map; pause; Unity-compatible Git tracking; persistent design and engineering docs; behavioral PlayMode tests and Windows build entry point.

Verification status is recorded in README.md. Human movement-feel and visual review remain part of acceptance even after automated checks pass.

## Milestone 2 — next, not implemented

1. Replace the temporary controller's global gravity dependency with ShipGravityController exposing a ship-local acceleration vector and ShipGravityBody applying -a in FixedUpdate with useGravity disabled. Test zero, intermediate, 1g, reversed and rotated vectors, independent of mass.
2. Implement Rigidbody PlayerMovement relative to gravity/contact normals; grounded acceleration and slope/contact tolerances. During free float retain momentum; WASD cannot add thrust. Make transition thresholds explicit and test both directions.
3. Add two visible simple hands, separate HandInteractor instances and camera-relative reach targets. Use stable configurable constraints, collision rules, reach limits and cleanup on release/disable. Distinguish static rail from movable held object; prevent contradictory double grabs.
4. Place one handrail and one grabbable wrench in the existing enclosed room. Implement contact-based pulling and Space push-off; no contact means no push impulse. Verify separate hands, two-object holding, rail transfer and object inertia.
5. Add a test-only 1g/coasting control driving the vector, not a gameplay gravity boolean. Test rapid transitions, rail grabs during acceleration, wall impacts, dropped wrench and repeated release/regrab. Tune stability before adding IK or visuals.
6. Record automated vector/inertia/constraint checks and manually playtest the complete walk → coast → rail pull → wrench handling → powered walking loop. Review movement enjoyment with the user before Milestone 3.

## Later milestones

3. Compact upright ship blockout: cockpit, workshop, greenhouse, connectors and traversable rails in both gravity states.
4. Configurable Aster/Nivalis/Eos system, navigation hologram, manual local flight, autopilot and basic scanning.
5. Named robot, modules, deployment/recovery, deterministic surveys, repairs and permanent loss.
6. One crop, food, enclosed irrigation, water reservoir/recycling/filter wear and consequences.
7. Ship/Earth clocks, relativistic travel, deadline, verified habitability, communication/fleet timeline, success/failure and save state.
8. Lighting/audio/UI polish, performance, balancing and external playtesting of the 15–20 minute slice.

Estimates in the original brief are preliminary. Full launch, extended galaxy, detailed orbital physics and broader survival remain post-slice features.
