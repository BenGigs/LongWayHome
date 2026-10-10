# Scientific assumptions

These are intended models, not a claim that all systems are implemented.

| Quantity/model | Convention |
| --- | --- |
| Local physics | SI: metres, seconds, kilograms; +Y up in the initial test room |
| Apparent gravity (M2) | g_effective = -a_ship in the ship frame; configurable Vector3, 9.81 m/s² initial powered case, zero in coasting |
| Initial frame approximation | Stationary rendered interior; omit rotational centrifugal/Coriolis forces and external gravity gradients initially; document this before adding rotating flight |
| M1 physics | Unity default gravity (0, -9.81, 0) only; not yet an acceleration model |
| Speed | c = 299,792,458 m/s; beta <= 0.99999; propulsion and shielding are speculative |
| Constant-speed travel | gamma = 1 / sqrt(1 - beta²); ship proper duration = Earth-frame coordinate duration / gamma |
| Accelerating travel | Select and document a proper-acceleration flight profile and integration/analytic method before implementing; never apply one cruise gamma to the whole route |
| Communication | At c in the selected reference frame; no instantaneous Earth communication |
| Deadline | Approximate initial 50,000 Earth years; specify year conversion and fleet preparation/speed before balancing |
| Planet suitability | Evidence-based human survival criteria and confidence; no arbitrary random percentage or life-equals-habitable shortcut |
| Robot | Finite battery, mass, slots, wear; explicit reusable lander and return capability |
| Water | Recovered fraction of used water returns to reservoir; losses remain real; enclosed zero-g irrigation |

The Earth clock is initially an idealized shared inertial reference clock, ignoring gravitational time dilation, cosmological effects and stellar motion. Future travel models must expose their assumptions and precision. Gameplay tolerances in grabbing and locomotion should be recorded alongside the implementation. No scientific travel, resource, habitability or clock calculations ship in M1.
