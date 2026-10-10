# The Long Way Home

Working title. A single-player, first-person scientific exploration and light survival game for Windows, built with Unity 6 and URP. Quiet, lonely, beautiful, occasionally intimidating; the compact spacecraft gradually becomes home. No combat, enemy factions, endless crafting, or arbitrary failure spam.

## Mission and constraints

Humanity remains on Earth, which has roughly 50,000 years of habitability left. One astronaut searches for a naturally human-compatible world aboard an experimental spacecraft capped at 99.999% of light speed. The astronaut never leaves the ship. Evidence, light-speed communication delays, Earth time, ship time, and the arrival of a colonization fleet determine success. Discovering native life or being inside a habitable zone is not sufficient.

## Core experience

- Walk under acceleration-generated apparent gravity; coast in weightlessness. Grab, pull, push, and hold separate objects with two hands. No WASD thrusters in free float. Start with simple hands and stable constraints, not full-body IK.
- An upright rocket has floors perpendicular to main thrust, handrails, and loose objects responding to effective gravity. Compact cockpit, workshop, greenhouse/living area; graybox first.
- Automated interstellar travel includes acceleration, relativistic cruise, and braking. Manual local piloting uses a simplified low-speed model. Deliberate time advancement supports shipboard activities.
- A navigation hologram eventually zooms galaxy → system → planet. Astronomical data stays separate from local rendering; never simulate billions of Unity stars.
- One configurable fictional system contains Aster (hazardous red rocky world), Nivalis (icy resources and equipment lessons), and Eos (promising blue-green candidate). Ground truth and observed knowledge are separate; confidence improves through scanning and robot surveys.
- A named, repairable, configurable robot uses a reusable lander for deployment and recovery. States: docked, preparing, deploying, surveying, returning, damaged, destroyed. Deterministic initial outcomes depend on conditions and equipment. Power, mass, slots, durability, and environmental protection matter. A rover cannot fly itself back into orbit.
- Modules may analyze atmosphere, temperature, radiation, soil, water and imagery, or supply drilling, shielding, batteries and mobility. Physical workshop repairs begin as simple interactions.
- One initial crop (tomatoes), enclosed hydroponic irrigation, harvest and consumption. Growth follows ship time. Water is consumed, partially reclaimed, and lost according to filter efficiency and degradation. Food and water need meaningful but infrequent maintenance. Oxygen, CO2 and detailed power come later.

## Outcomes

Verify atmosphere, pressure, water, temperature, radiation, gravity, stability and absence of disqualifying hazards. Transmit evidence and coordinates; estimated preparation and fleet travel must fit Earth's deadline. The slice needs one success and one explainable failure, such as loss of the only robot. Later failures include starvation, dehydration, life support, radiation, propulsion, exhausted resources, deadline expiry or incorrect habitability assessment. Support ordinary saves; permadeath is optional future work.

## Scope and open decisions

Target a polished 15–20 minute slice only after the mechanics work. Full interactive rocket launch, galaxy generation, detailed orbital dynamics, multiple systems, fabrication, expanded crops, historical Earth messages and cinematic endings are later work.

Do not settle the final title, Earth's catastrophe, ship appearance, astronaut lifespan/medical technology, total system count, propulsion/energy technology, Earth confirmation or final narrative without discussion. Milestone 1 makes no such commitments.

## Ship design discussion — 2026-10-10

The user selected an upright expedition ship with stacked decks, a practical exterior and a warm, lived-in cabin. This is the direction for the next Blender blockout; the existing Unity layout remains the movement testbed until the new circulation works.

Proposed, not yet approved in detail: three occupied decks in an approximately 8 m diameter, 24 m tall overall envelope. Upper deck: cockpit, one pilot station, navigation and scanning. Middle deck: enclosed greenhouse, galley, hygiene, one berth and stores. Lower deck: workshop, robot module storage, life support access and a sealed transfer lock for the reusable robot lander. Tanks and a service buffer separate the occupied cabin from the drive below. Reserve a protective cap, side observation windows with shutters, radiator panels and service access outside; these shapes do not establish a viable relativistic drive or shielding system.

Floors are perpendicular to main thrust. A central transfer shaft with handrails supports coasting; adjacent stairs must provide a traversable route under thrust. Start by testing this circulation with the existing 2 m player capsule. Target 2.8–3 m clear room height, 1.2 m clear doors and at least 1.4 m clear paths, then adjust through playtesting. Ladder climbing is not implemented and must not be assumed to work. Braking uses ship reorientation rather than requiring inverted cabin floors; a detailed flight model remains future work.

Visual direction: rounded pressure hull inside a faceted service shell; restrained off-white/graphite exterior, numbered access panels and a small mission-color accent. Inside: warm indirect lighting, muted green fabric, padded grips, labeled storage, personal objects near the berth, and plants as the strongest organic color. Keep observation views and machinery legible; avoid covering every surface with pipes. Exterior radiator/drive dimensions, lander volume, exact floor plans and final silhouette remain open.

Blender sequence after layout review: human-scale reference and deck volumes; stairs/shaft/rails; pressure hull and window openings; separate room fixtures; service shell/drive/lander placeholders; only then materials and surface detail. Keep source .blend files outside Unity Assets and export selected meshes with separate simple collision geometry. Import a scale reference into Unity before committing to detailed modeling.

The user authorized an editable 3D study to judge and change the design. The v03 Blender model uses a 10 m cabin diameter (wider than the first proposal to reserve circulation), 24 m total height, and deck elevations 10 / 13.4 / 16.8 m. Cutaway and complete exterior scenes share editable objects. Fixtures, a spiral stair, central zero-g shaft, radiators and a human scale reference are included. These are proposed dimensions, not accepted final geometry; Unity traversal, lander packaging, pressure doors and production collision still need design work.
