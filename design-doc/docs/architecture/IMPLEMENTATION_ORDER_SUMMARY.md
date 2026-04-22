# Implementation Order Summary

## Recommended implementation order

1. Approve `DECISION_FREEZE.md`
2. Freeze contract files in Phase 0
3. Create solution scaffold and shared contracts package
4. Implement RavenDB Embedded bootstrap and persistence basics
5. Implement workspace detection and semantic plugin routing
6. Implement planning, preflight, scheduler, and execution backend
7. Implement result normalization and artifact harvesting
8. Implement MCP surfaces
9. Implement browser-facing API and live event delivery
10. Implement operator UI
11. Implement flaky iterative subsystem
12. Harden, validate, package, and document

## First merge checkpoints

- Checkpoint A: solution and contracts frozen
- Checkpoint B: storage and registry usable after restart
- Checkpoint C: one project/category/method run end-to-end
- Checkpoint D: live browser UI for a run
- Checkpoint E: iterative rerun and flaky analysis
- Checkpoint F: validation and packaging complete
