# RavenDB Test Runner MCP Server — Execution Pack

This directory contains the full execution pack for **RavenDB Test Runner MCP Server**.

Highlights of this revision:
- canonical naming consolidated around **RavenDB Test Runner MCP Server**
- first-class **Build Subsystem** added across architecture, contracts, phases, work packages, ADRs, and tasks
- explicit build-to-test handshake so test execution cannot trigger chaotic rebuilds
- branch-aware support model preserved for `v6.2`, `v7.1`, and `v7.2`

Start with:
1. `AGENTS.md`
2. `docs/architecture/DECISION_FREEZE.md`
3. `docs/contracts/NAMING_AND_MODULE_POLICY.md`
4. `docs/phases/PHASE_0_CONTRACT_FREEZE.md`
