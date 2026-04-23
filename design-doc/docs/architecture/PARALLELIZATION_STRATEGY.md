# PARALLELIZATION_STRATEGY.md

## Rule
No parallel coding starts before Phase 0 contract freeze is approved.

## Safe parallel lanes after Phase 0
- Lane 1: `WP_B` Storage and Registry
- Lane 2: `WP_C` Semantics
- Lane 3: `WP_A` residual contract cleanups and validation harness

## Safe parallel lanes after storage + semantics stabilise
- Lane 4: `WP_D` Build Subsystem
- Lane 5: `WP_G` MCP host shell scaffolding (without final tool binding)
- Lane 6: `WP_H` API skeleton and event stream shells

## Later parallelization
- `WP_E` and `WP_F` can overlap once Build and Storage exist
- `WP_I` frontend shells can start once API/event contracts are frozen
- `WP_J` can start once result/attempt/build contracts are stable
- `WP_K` runs continuously but finalizes last
