# DECISION_FREEZE.md

## Purpose
This document freezes non-negotiable implementation decisions for **RavenDB Test Runner MCP Server**. It is shorter than `IMPLEMENTATION_SPEC.md` and is intended for rapid reference by coding agents.

## 1. Product naming
- Canonical product name: **RavenDB Test Runner MCP Server**
- Canonical short label: **RTRMS**
- Canonical internal root namespace / module root: **RavenDB.TestRunner.McpServer**
- Retired legacy names: `RavenMcp`, `RavenMcpControlPlane`, `RavenDB Test MCP Control Plane`, `RavenMcp Execution Pack`
- Legacy names MAY appear only in migration notes and historical review artifacts

## 2. Product shape
- Stand-alone local application
- Manually started by the developer
- Long-lived process
- Full operator dashboard required
- MCP access required
- Single-user local-first for v1

## 3. MCP surfaces
- Primary MCP surface: local Streamable HTTP
- Secondary compatibility surface: stdio bridge
- stdio bridge is not the system-of-record host
- MCP and browser UI are separate surfaces over the same orchestration core

## 4. Storage
- RavenDB Embedded is mandatory
- SQLite is forbidden
- **v1 artifact storage is attachments-first**
- RavenDB Embedded is the authoritative metadata store **and** the authoritative v1 artifact store for build/test artifacts that are in scope for v1
- Build and test artifacts in scope for v1 MUST default to RavenDB attachments
- Filesystem artifact ownership is **not** the default v1 strategy
- Bulky binary diagnostics outside the practical v1 attachment policy (for example dumps, bulky blame bundles, and oversized diagnostic exports) are deferred to later milestones unless a later ADR explicitly introduces them
- The design MAY reserve a future extension point for filesystem-backed bulky diagnostics, but that future extension point is non-normative for v1

## 5. Embedded licensing
License probe order:
1. explicit app config / first-run setup record
2. `RAVEN_License`
3. `RAVEN_LicensePath`
4. `RAVEN_License_Path`
5. interactive setup flow

Plaintext license material MUST NOT be logged.

## 6. Supported repo lines
- `v6.2`
- `v7.1`
- `v7.2`
Future lines MUST be added via the version plugin / capability model.

## 7. AI capability baseline
- `v6.2`: no AI-specific test semantics
- `v7.0+`: AI-related semantics may exist
- `v7.1+`: AI Agents / richer AI semantics may exist
- `v7.2`: AI-capable baseline confirmed

## 8. Build subsystem
- Build orchestration is first-class
- Build MUST have its own domain, persistence, APIs, events, tasks, pages, and tools
- Test execution MUST NOT hide build decisions
- Repeated meaningless rebuilds are an architectural failure
- Build policy MUST be server-owned, deterministic, persisted, and explainable
- Build and test workflows MUST link through explicit build readiness contracts, not ad hoc implicit rebuild behavior

## 9. Browser UI
- Visual baseline: RavenDB Studio 7.2
- Design-language reuse encouraged
- Stack freedom preserved
- SignalR primary live transport
- SSE supplementary transport

## 10. Delivery model
- Shared contracts freeze before parallel coding
- Multiple coding agents are expected
- One integrating agent is expected
- ADR discipline is mandatory
