# Scientific assumptions

These are intended models, not a claim that all systems are implemented.

The five-deck gameplay prototype models climbing as a bounded contact reaction through a held ring: it compensates local gravity and steers the dynamic capsule toward a hang point, then above the floor and onto a landing when the player holds W. It is a gameplay approximation, not simulated arms, stamina or biomechanical strength. Release removes the assist; no ledge grip means no climbing force. Pull-up is enabled only when effective gravity aligns with the deck's up direction. Collisions remain active and falls currently cause no damage. Coasting retains the earlier contact-only movement model.

| Quantity/model | Convention |
| --- | --- |
| Local physics | SI: metres, seconds, kilograms; +Y up in the initial test room |
| Apparent gravity (M2) | g_effective = -a_ship in the ship frame; configurable Vector3, 9.81 m/s² initial powered case, zero in coasting |
| Initial frame approximation | Stationary rendered interior; omit rotational centrifugal/Coriolis forces and external gravity gradients initially; document this before adding rotating flight |
| Current M2 physics | Each participating Rigidbody disables Unity gravity and receives effective acceleration explicitly; zero-g preserves momentum absent contact/constraints |
| Speed | c = 299,792,458 m/s; beta <= 0.99999; propulsion and shielding are speculative |
| Constant-speed travel | gamma = 1 / sqrt(1 - beta²); ship proper duration = Earth-frame coordinate duration / gamma |
| Accelerating travel | Select and document a proper-acceleration flight profile and integration/analytic method before implementing; never apply one cruise gamma to the whole route |
| Communication | At c in the selected reference frame; no instantaneous Earth communication |
| Deadline | Approximate initial 50,000 Earth years; specify year conversion and fleet preparation/speed before balancing |
| Planet suitability | Evidence-based human survival criteria and confidence; no arbitrary random percentage or life-equals-habitable shortcut |
| Robot | Finite battery, mass, slots, wear; explicit reusable lander and return capability |
| Water | Recovered fraction of used water returns to reservoir; losses remain real; enclosed zero-g irrigation |

The Earth clock is initially an idealized shared inertial reference clock, ignoring gravitational time dilation, cosmological effects and stellar motion. Future travel models must expose their assumptions and precision. No travel, resource, habitability or clock simulation ships in the movement prototype.

M2 tolerances: locomotion treats acceleration below 0.1 m/s² as weightless, while bodies still receive the exact small acceleration. Ground probe tolerance is approximately 8 cm and slope normal dot-up must exceed 0.65. Gravity-alignment is capped at 120 degrees/s. Walking/pulling corrections are bounded to 12 m/s²; zero-g contact push adds 2.5 m/s, while powered floor jumps add 4 m/s (approximately 0.8 m ballistic rise at 1g without obstruction). Jump/push preserves both grips; rail constraints still limit travel. Rail reach is 1.8 m, with automatic release beyond 2.6 m to recover overstretched constraints. Static rail anchors and the fixed room represent the ship as an effectively infinite mass. Tool targets have capped 350 N spring drives, a gameplay approximation of muscle forces; tools do not feed reaction forces back through a fully simulated astronaut arm. Held tools use an idealized rigid orientation constraint relative to the hand, rather than a simulated finger grip or finite grip torque. On release they regain free rotation and keep their current linear/angular momentum; collisions can change momentum. These are prototype tolerances for stability and feel, not claims of a complete biomechanical simulation.
