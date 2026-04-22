# Decision Freeze

## Status

This document is normative and frozen for implementation baseline v1.
Implementation agents MUST treat it as authoritative unless superseded by a later ADR and explicit contract update.

## DF-001 Product shape

- The product is a stand-alone local application.
- The product is manually started by a developer on the developer workstation.
- The product MUST remain useful even if an external AI host has unstable child-process lifecycle behavior.
- The product MUST expose a full dashboard/operator UI.

## DF-002 MCP surfaces

- The primary MCP surface is a local Streamable HTTP endpoint hosted by the stand-alone application.
- An optional stdio bridge exists for tools that only support subprocess-style MCP.
- The stdio bridge is compatibility-only and MUST remain thin.
- MCP and browser-facing APIs are separate surfaces over the same shared orchestration core.

## DF-003 Implementation target

- Default language/runtime: `.NET / C#`
- Runtime baseline: `.NET 10`
- Browser-facing backend: `ASP.NET Core`
- Browser UI: `React + TypeScript`
- Browser real-time transport: `SignalR` primary, `SSE` supplementary

## DF-004 Storage

- SQLite is forbidden.
- RavenDB Embedded is mandatory for metadata/state.
- Large raw artifacts MUST use filesystem canonical storage by default.
- RavenDB stores metadata, summaries, fingerprints, state, history, and selected compact attachments.

## DF-005 Embedded licensing

- RavenDB Embedded licensing is required.
- License discovery order:
  1. explicit application config / first-run setup record
  2. `RAVEN_License`
  3. `RAVEN_LicensePath`
  4. `RAVEN_License_Path` compatibility alias
  5. interactive setup
- License plaintext MUST NOT be logged.

## DF-006 Supported repository lines

First-class supported lines:
- `v6.2`
- `v7.1`
- `v7.2`

The architecture MUST remain extensible for:
- `6.3`
- `7.3`
- `8.0`
- later lines

## DF-007 Version strategy

- Shared orchestration core
- Version-specific semantic plugins
- Capability-based behavior
- No sprawling ad-hoc `if/else` version branching as the long-term model

## DF-008 AI compatibility

Use capability-based AI modeling.

Baseline assumptions:
- `v6.2` => no AI-specific test semantics
- `v7.0+` => AI-related semantics may exist
- `v7.1+` => richer AI Agents / GenAI semantics may exist
- `v7.2` => AI semantics are a supported modern baseline

Capabilities MUST include, at minimum:
- `supportsAiEmbeddingsSemantics`
- `supportsAiConnectionStrings`
- `supportsAiAgentsSemantics`
- `supportsAiTestAttributes`

## DF-009 UI / design direction

- The UI SHOULD visually align with RavenDB Studio 7.2.
- Style tokens, layout conventions, and interaction patterns MAY be reused.
- Studio reuse MUST NOT constrain implementation stack freedom.

## DF-010 User mode for v1

- Single-user local-first
- No mandatory multi-user/team mode
- Minimal trusted-local browser auth is acceptable in v1

## DF-011 Filter policy

- Structured selectors are canonical.
- Raw `dotnet test --filter` expressions are expert-mode only.
- Raw filters MUST NOT become the internal source of truth.

## DF-012 Flaky and quarantine policy

- Flaky analysis is first-class.
- Iterative reruns are first-class.
- Automated diagnostics escalation is supported.
- Quarantine workflows are supported.
- Automated actions MUST be explainable, reversible, journaled, and visible in UI and MCP payloads.

## DF-013 Delivery model

- Parallel implementation by multiple AI coding agents is expected.
- One coordinating / integrating agent is assumed.
- Shared contracts must be frozen before parallel coding.
- ADR discipline is mandatory.

## DF-014 Concurrency default

- One active run per workspace by default
- One active `dotnet test` process per workspace by default
- More aggressive concurrency requires explicit policy and explanation

## DF-015 Artifact threshold policy

- Compact artifacts MAY be stored as RavenDB attachments.
- Large logs, dumps, blame artifacts, and bulky diagnostics MUST remain filesystem-backed.
- Threshold values are contract-controlled and configurable.

## DF-016 Authority order

When sources conflict, use:
1. this Decision Freeze
2. `docs/architecture/IMPLEMENTATION_SPEC.md`
3. repository code/projects/workflows/config
4. official documentation
5. markdown guidance files
