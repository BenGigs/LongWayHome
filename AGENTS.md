# Development instructions

- Read README.md, GAME_DESIGN.md, ARCHITECTURE.md, ROADMAP.md and SCIENTIFIC_ASSUMPTIONS.md before architectural changes. Work one milestone at a time; explain its status before advancing.
- Unity 6000.3.4f1, URP, Input System, Windows PC, C#. Respect the master vision recorded in GAME_DESIGN.md; astronaut never leaves the ship. No free-floating WASD thrust.
- Inspect git status and existing work first. Feature branches only. Do not push main or merge a PR without explicit user approval. No remote is configured initially; do not assume GitHub account/repository destination.
- Track Assets, Packages and ProjectSettings plus all corresponding Unity .meta files. Never commit Library, Temp, Logs, UserSettings, builds or test results. Preserve GUIDs. Unity must generate serialized scenes/prefabs/settings and meta files; use Editor APIs, not invented YAML.
- Use Assets/_Game and LongWayHome namespaces; serialized private fields, small methods, explicit dependencies, FixedUpdate physics. No speculative managers, global singleton collection or paid frameworks.
- Keep immutable definitions separate from mutable state, astronomical coordinates separate from rendering, and scientific truth separate from player evidence.
- Follow README verification commands. Do not run two Editors against this project. Do not claim compilation, tests, rendering, or manual playtesting passed without evidence. Unity batch process can detach in PowerShell; launch via Start-Process -PassThru -WindowStyle Hidden and inspect process exit code and log/result files.
- Tests should verify meaningful behavior and pure calculation rules. Maintain tests for additive boot, input-driven collision, pause, and later gravity/inertia. Do not broaden testing repeatedly after unchanged successful results.
- Update roadmap and scientific assumptions when behavior changes. Preserve unresolved narrative/technology decisions for discussion.
- All scene creation in FoundationSetup is one-time and refuses to overwrite existing foundation scenes. Normal startup opens Bootstrap; do not rerun generation to reset user changes.
