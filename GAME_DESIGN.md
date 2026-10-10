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
