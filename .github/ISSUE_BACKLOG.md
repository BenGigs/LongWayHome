# Prepared issues

No remote has been chosen. These descriptions are ready to copy into GitHub; no issues or PRs have been created.

## M1 — Establish Unity project foundation

Create Unity 6 URP project, input actions, additive Bootstrap/ShipInterior/SpaceExterior scenes, enclosed graybox and basic controllable Rigidbody player. Add Unity-compatible tracking and persistent design/architecture/science documentation. Verify compilation, scene composition, movement/collision and pause. Human acceptance: open Bootstrap, walk/look, confirm room visuals and no Console errors. Record automated verification and remaining manual checks in README.

Branch: `chore/unity-setup`. Suggested commit: `chore: establish Unity 6 URP project foundation`.

## M2 — Prove ship gravity and two-handed locomotion

Implement configurable acceleration vector, per-body apparent gravity, grounded and contact-driven weightless movement, two simple hands, a handrail and grabbable wrench. Use the existing enclosed room. Add test-only 1g/coasting toggle. No free-flight WASD propulsion. Test arbitrary/intermediate acceleration, inertia, constraint cleanup and transitions. Acceptance: walk, coast, pull along rail, manipulate wrench, push off contact, return to grounded movement without instability. See ROADMAP for implementation order.

Suggested branch: `feat/ship-gravity` for the first bounded portion, followed by movement and hand-interaction branches as needed.

## Publish after choosing the repository

Replace OWNER/REPOSITORY with the intended destination, and ensure its existing history is reconciled before pushing. Do not create or overwrite a remote main branch automatically. Save each issue body and the PR description to a UTF-8 file, then use:

```powershell
git remote add origin https://github.com/OWNER/REPOSITORY.git
git fetch origin
gh issue create --repo OWNER/REPOSITORY --title 'M1: Establish Unity project foundation' --body-file PATH_TO_ISSUE_BODY
git push -u origin chore/unity-setup
gh pr create --repo OWNER/REPOSITORY --base main --head chore/unity-setup --title 'Establish Unity 6 URP project foundation' --body-file PATH_TO_PR_BODY
```

A pull request needs an existing base branch with compatible history; an empty remote requires a deliberate initial-base setup first. Do not push directly to main or merge without the user's permission. Link the issue number and actual verification results in the PR description.
