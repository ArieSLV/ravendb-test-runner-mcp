# REVISION_SUMMARY.md

## Purpose
Summarize the integrated changes introduced in this execution-pack revision.

## Core conceptual changes
1. Product naming consolidated around **RavenDB Test Runner MCP Server**.
2. Internal namespace/module policy consolidated around `RavenDB.TestRunner.McpServer`.
3. Build orchestration promoted to a first-class subsystem with explicit persistence, tools, APIs, events, UI pages, phases, work packages, ADRs, and tasks.
4. Test execution changed from potentially hidden-build behavior to explicit build-readiness consumption.
5. Contracts updated to make build/test handshake deterministic and visible.

## New or materially updated contract areas
- `NAMING_AND_MODULE_POLICY.md`
- `BUILD_SUBSYSTEM.md`
- `DOMAIN_MODEL.md`
- `STORAGE_MODEL.md`
- `EVENT_MODEL.md`
- `STATE_MACHINES.md`
- `MCP_TOOLS.md`
- `WEB_API.md`
- `FRONTEND_VIEW_MODELS.md`
- `ERROR_TAXONOMY.md`
- `SECURITY_AND_REDACTION.md`

## New or materially updated phase/work package areas
- dedicated build phase added
- dedicated build work package added
- later phases renumbered/reframed around separate build and test surfaces
- task backlog expanded to include explicit build subsystem implementation

## Integration note
This revision was produced as a whole-pack integration update rather than a local patch so that architecture docs, contracts, phase briefs, work packages, ADRs, and task cards stay aligned.
