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

## Ship design — current five-deck direction

The user chose an upright cylindrical ship with five stacked rooms, an open sleeping nook, and thrust-generated gravity. Top to bottom: cockpit; navigation plus sleeping; workshop plus robot deployment; habitat/greenhouse plus water; fuel plus storage. Engines sit below the fuel/storage room. The proposed flat layout and earlier three-deck stair design are superseded.

There are no room doors, stairs, guardrails or raised hatch barriers in the current concept. Aligned open floor hatches allow the astronaut to fall or coast freely between decks. Orange grab rings sit at floor level around the openings. The user removed the climbing pole. The entire shaft stays clear. Under gravity the intended interaction is jump, catch the next floor-edge ring, then pull up onto that deck. In coasting it is grabbing/pulling and releasing to drift. FiveDeckGameplay now prototypes ledge catching, hanging and pulling onto the next floor with dynamic collisions. Jumping must continue to preserve grips.

The v05 Blender study uses an 8 m diameter cabin, 25 m overall height, five decks at 6.2 / 9.4 / 12.6 / 15.8 / 19 m, and aligned 2.1 m hatch openings offset from the cabin centre. Without stairs the cabin returns to the smaller diameter. These dimensions remain adjustable through review. Cutaway and exterior scenes share editable fixtures. Cutaway hides the front hull, cap and front floor sections to expose the rooms; it is not the actual open-sided hull. A 1.8 m human reference establishes scale.

Visual direction remains a practical ivory/graphite exterior, orange grab targets, warm interior lighting, muted green bedding, personal belongings in the open sleeping nook and living plants. The cockpit has upper wraparound windows. Navigation has its own table on the deck below. The workshop robot cradle is a space reservation; the reusable lander, pressure transfer and deployment engineering are unresolved. Fuel vessels are sealed fixtures within the service room. Drive and radiator shapes remain speculative placeholders.

Keep source .blend files outside Unity Assets. Review the geometry before importing selected meshes and simple collision shapes into Unity. The prior Unity movement testbed remains available. FiveDeckGameplay uses independent simple geometry for jump-to-ledge catches, deck pull-ups, ring interactions and falling through the shaft; these now have automated tests and await human playtesting. The v03 model remains as a historical design snapshot, not the current plan.