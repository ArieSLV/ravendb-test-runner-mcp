# IMPLEMENTATION_SPEC.md

**Product:** RavenDB Test Runner MCP Server
**Canonical short label:** RTRMS
**Status:** Architecture Freeze v2 / Implementation Specification
**Language:** English
**Audience:** AI coding agents, human integrator, maintainers
**Normative words:** MUST, MUST NOT, REQUIRED, SHOULD, SHOULD NOT, MAY

---

## 1. Purpose

This document is the constitutional specification for RavenDB Test Runner MCP Server. It defines the frozen architecture, constraints, naming, subsystem boundaries, and delivery assumptions for a production-grade, local-first, stand-alone application that manages RavenDB repository **build** and **test** workflows.

The product is intentionally more than a test wrapper. It is a long-lived local server and dashboard that owns deterministic build and test orchestration, exposes MCP surfaces to AI agents, and persists operational state in RavenDB Embedded.

This document is normative for architecture. The execution pack adds contracts, phases, work packages, and task cards that operationalize this document without redefining it.

---

## 2. Product identity and naming policy

### 2.1 Canonical product name

The canonical product name is **RavenDB Test Runner MCP Server**.

This name MUST be used in:
- architecture documents,
- phase briefs,
- work packages,
- ADRs,
- dashboards,
- operator-facing descriptions,
- packaging names,
- first-run UX,
- MCP server identity descriptions.

### 2.2 Retired legacy names

The following names are retired for current design purposes and MUST NOT be used as the primary product name:
- `RavenMcp Execution Pack`
- `RavenDB Test MCP Control Plane`
- `RavenMcpControlPlane`
- `RavenMcp.*`

They MAY appear only in historical migration notes or legacy-name mapping tables.

### 2.3 Internal technical naming

To avoid unwieldy namespace length while preserving product identity, the implementation MUST use the following naming convention:
- **Solution / repository-internal root namespace:** `RavenDB.TestRunner.McpServer`
- **Example project names:**
  - `RavenDB.TestRunner.McpServer.Core`
  - `RavenDB.TestRunner.McpServer.Core.Abstractions`
  - `RavenDB.TestRunner.McpServer.Domain`
  - `RavenDB.TestRunner.McpServer.Storage.RavenEmbedded`
  - `RavenDB.TestRunner.McpServer.Build`
  - `RavenDB.TestRunner.McpServer.TestExecution`
  - `RavenDB.TestRunner.McpServer.Results`
  - `RavenDB.TestRunner.McpServer.Flaky`
  - `RavenDB.TestRunner.McpServer.Mcp.Host.Http`
  - `RavenDB.TestRunner.McpServer.Mcp.Host.Stdio`
  - `RavenDB.TestRunner.McpServer.Web.Api`
  - `RavenDB.TestRunner.McpServer.Web.Ui`

The documentation MUST explicitly distinguish between:
- product name,
- solution/module root,
- historical retired names.

---

## 3. Product shape

### 3.1 What is being built

RavenDB Test Runner MCP Server is a **local stand-alone application** that:
1. runs as a long-lived process manually started by a developer,
2. exposes a full operator dashboard,
3. exposes MCP capabilities to AI agents,
4. persists metadata/state/history in RavenDB Embedded,
5. orchestrates explicit build workflows and explicit test workflows,
6. understands RavenDB repository semantics deeply enough to avoid delegating correctness to prompting quality.

### 3.2 What it is not

The product is not:
- a thin `dotnet test` wrapper,
- a child-process-only MCP tool,
- a polling-only browser app,
- a cloud-first multi-tenant service in v1,
- a system that lets random agent behavior determine when and how RavenDB is rebuilt.

---

## 4. Architecture freeze: already-decided constraints

### 4.1 Runtime and implementation language
- Default implementation target: `.NET 10 / C#`
- ASP.NET Core for browser-facing API/BFF host
- official MCP C# SDK for MCP surfaces

### 4.2 Deployment model
- v1 is local, stand-alone, manually started, and single-user/local-first
- v1 includes a full dashboard/operator UI
- v1 includes MCP access
- v1 is not primarily a team-shared hosted service

### 4.3 MCP transport strategy
The product MUST support two MCP access paths:
1. **Primary:** local **Streamable HTTP** MCP endpoint hosted by the stand-alone application
2. **Secondary / compatibility:** local **stdio bridge host** for AI hosts that require subprocess MCP

The stdio bridge is a compatibility facade. It MUST NOT become the state-owning host.

### 4.4 Storage strategy
- RavenDB Embedded is mandatory for metadata/state/history
- SQLite is forbidden
- Large raw artifacts use the filesystem as canonical storage
- RavenDB stores metadata, references, compact attachments, and indexes

### 4.5 Supported repository lines
The product MUST support these first-class repository lines:
- `v6.2`
- `v7.1`
- `v7.2`

It MUST be straightforward to add future support for `6.3`, `7.3`, `8.0`, and later.

### 4.6 Versioning strategy
The product MUST use:
- shared orchestration core,
- version-aware semantic plugins,
- capability matrix,
- feature detection,
- branch/version routing.

Long-term extensibility MUST NOT rely on sprawling `if/else` chains.

### 4.7 AI capability baseline
Compatibility is capability-based:
- `v6.2` => AI-specific test semantics absent
- `v7.0+` => AI-related semantics may exist
- `v7.1+` => AI Agents / richer AI semantics may exist
- `v7.2` => AI-related semantics definitely supported as a modern baseline

### 4.8 UI direction
The browser UI MUST be visually aligned with **RavenDB Studio 7.2** where useful, by reusing design language and interaction patterns when practical. This MUST NOT force reuse of Studio internals if doing so harms clarity or maintainability.

### 4.9 Build subsystem rule
The product MUST treat build orchestration as a **first-class subsystem**, not as a hidden detail of test execution.

Build orchestration MUST have:
- its own domain model,
- its own lifecycle,
- its own persistence,
- its own event stream,
- its own MCP tools,
- its own browser-facing APIs,
- its own status/detail views,
- its own work package and phase.

---

## 5. Core problem statement

The RavenDB repository is large enough that build cost is a first-order architectural constraint. A careless implementation that rebuilds before every test run can amplify a 100 ms test into minutes of wall-clock time.

Therefore, RavenDB Test Runner MCP Server MUST guarantee the following at the architecture level:
- build policy is owned by the server, not by the agent prompt;
- tests never rebuild chaotically because an agent forgot or improvised;
- build reuse is explicit, inspectable, and governed by policy;
- build readiness is persisted and queryable;
- a test run either references an accepted build readiness token or invokes the build subsystem explicitly under server-owned rules.

This is a mandatory system property, not a quality-of-life improvement.

---

## 6. Top-level system decomposition

The product consists of the following top-level subsystems.

### 6.1 Shared orchestration core
- Workspace Analyzer
- Version Plugin Router
- Semantic Model Builder
- Test Catalog / Capability Matrix
- Build Graph Analyzer
- Build Planner
- Build Scheduler / Build Execution Engine
- Build Readiness Service
- Test Preflight Evaluator
- Test Run Planner
- Test Scheduler / Execution Engine
- Result Normalizer
- Flaky Analytics Engine
- Registry / State Service

### 6.2 MCP-facing surfaces
- Streamable HTTP MCP host (primary)
- stdio bridge MCP host (compatibility)
- MCP tools/resources/prompts surface
- MCP progress and cancellation mapping

### 6.3 Browser-facing surfaces
- ASP.NET Core API/BFF
- SignalR live hub
- SSE read-only/event/log streams
- browser operator UI

### 6.4 Persistence and artifacts
- RavenDB Embedded metadata database
- filesystem artifact root
- compact attachment policy for RavenDB
- retention / cleanup subsystem

### 6.5 Flaky / quarantine subsystem
- iterative planning
- attempt lifecycle
- stability signal generation
- classification/scoring
- quarantine recommendations/actions
- reporting/history

---

## 7. Versioned semantic architecture

### 7.1 Shared orchestration core vs semantic plugins
The orchestration core is branch-agnostic. Repo-specific semantics are supplied by plugins.

Required plugins:
- `RavenV62Semantics`
- `RavenV71Semantics`
- `RavenV72Semantics`

Future plugin examples:
- `RavenV63Semantics`
- `RavenV73Semantics`
- `RavenV80Semantics`

### 7.2 Plugin responsibilities
Each plugin MUST define:
- project topology assumptions,
- category and trait mapping behavior,
- custom attribute interpretation,
- capability flags,
- branch-specific preflight rules,
- result normalization hints,
- theory/data-row caveats,
- AI capability flags.

### 7.3 Capability matrix
The capability matrix MUST expose at least:
- `supportsSlowTestsIssuesProject`
- `supportsAiEmbeddingsSemantics`
- `supportsAiConnectionStrings`
- `supportsAiAgentsSemantics`
- `supportsAiTestAttributes`
- `frameworkFamily`
- `runnerFamily`
- `adapterFamily`

---

## 8. Build subsystem as a first-class concern

### 8.1 Why build is separate
Build is intentionally separated because:
- RavenDB-scale build cost is high;
- implicit rebuilds are expensive and easy to trigger accidentally;
- prompts are not a reliable build policy mechanism;
- deterministic reuse requires explicit build identities and cache rules;
- humans and agents both need visibility into build state and reuse decisions.

### 8.2 Build subsystem responsibilities
The build subsystem MUST own:
- build graph analysis,
- build target selection,
- restore/build/clean/rebuild orchestration,
- build fingerprints,
- build reuse decisions,
- build readiness tokens,
- build execution state,
- build artifacts (including `binlog`),
- build status APIs and MCP surfaces,
- build history and summaries.

### 8.3 Build modes
The design MUST support:
- `restore-only`
- `build`
- `rebuild`
- `clean`
- `clean + build`
- `verify-existing-outputs`

### 8.4 Build policy
Server-owned build policy MUST support explicit modes such as:
- `require_existing_ready_build`
- `build_if_missing_or_stale`
- `force_incremental_build`
- `force_rebuild`
- `expert_skip_build`

The default MUST be safe and deterministic. It MUST NOT assume that test execution can decide ad hoc how to build.

### 8.5 Build fingerprinting and readiness
The subsystem MUST compute a build fingerprint from inputs such as:
- repo line / git SHA / dirty fingerprint,
- selected solution or projects,
- configuration,
- target frameworks/runtime identifiers as applicable,
- global SDK/package constraints,
- relevant MSBuild properties,
- relevant environment inputs,
- artifact output manifest.

The subsystem MUST persist a build readiness token that test planning can reference.

### 8.6 Build artifacts
The build subsystem MUST capture:
- stdout/stderr/merged output,
- build summary,
- manifest of output artifacts,
- `binlog` for detailed diagnostics,
- command line and effective environment fingerprint.

### 8.7 Build transports and status surfaces
Build MUST have first-class surfaces:
- MCP tools: `build.*`
- Browser APIs: `/api/builds/*`
- Live events: `build.*` event stream family
- UI pages: Builds list, Build details, Build graph/plan inspector, Build policy/cache views

---

## 9. Build-to-test handshake

Test execution MUST NOT own build orchestration implicitly.

Instead, test planning MUST use one of these explicit paths:
1. consume an acceptable existing build readiness token;
2. trigger a build plan owned by the build subsystem and wait for readiness;
3. fail early because build policy forbids implicit build creation.

Every test run MUST record:
- build policy used,
- linked build ID or readiness token,
- whether build reuse occurred,
- why reuse was accepted or rejected.

---

## 10. Test execution subsystem

The test subsystem remains first-class, but now sits beside the build subsystem rather than hiding build inside itself.

It MUST own:
- selector normalization,
- preflight and skip prediction,
- run planning,
- scheduling and execution,
- run artifacts,
- normalized test results,
- repro commands,
- explainability.

Build and test are separate but linked lifecycles.

---

## 11. Persistence model

### 11.1 RavenDB Embedded stores
RavenDB Embedded MUST store:
- workspace snapshots,
- semantic snapshots,
- capability matrices,
- build graph snapshots,
- build plans,
- build executions/results,
- build readiness records,
- test catalogs,
- run plans,
- runs,
- attempts,
- normalized results,
- artifact metadata,
- flaky histories,
- settings and policies,
- event checkpoints.

### 11.2 Filesystem stores
The filesystem artifact root MUST store:
- large transcripts,
- large `trx`/`junit` files,
- large `binlog` files if above threshold,
- dumps/blame artifacts,
- bulky diagnostics,
- large export bundles.

### 11.3 Compact attachments
RavenDB attachments MAY be used for compact build/test artifacts under the configured threshold.

---

## 12. Browser-facing system

The browser-facing system MUST provide:
- build list and build details,
- run list and run details,
- live console/output panes,
- plan inspectors,
- artifact explorer,
- diagnostics pages,
- flaky analysis pages,
- settings/policy pages,
- explicit build reuse explanations.

SignalR is the primary live transport. SSE is supplementary and especially useful for read-only or cursor-driven streams.

---

## 13. MCP-facing system

The MCP layer MUST expose build and test surfaces separately, while sharing the same registry and orchestration core.

Required MCP families:
- `build.*`
- `tests.*`
- optional `workspace.*` / `catalog.*` read helpers if helpful

The Streamable HTTP host is the primary MCP surface. The stdio host is a compatibility bridge.

---

## 14. Flaky subsystem

The flaky subsystem remains first-class and MUST now understand the build subsystem too.

It MUST distinguish between:
- deterministic build failure,
- deterministic test failure,
- environment-sensitive failure,
- build reuse mismatch,
- concurrency-sensitive instability,
- infrastructure instability,
- selector instability,
- likely flaky behavior.

Flaky analysis MUST NOT misclassify deterministic build or license/platform failures as flakiness.

---

## 15. Security and local-host posture

The product is local-first, but local does not mean unsafe.

The product MUST:
- bind local HTTP surfaces to localhost by default,
- validate browser-facing origins appropriately,
- keep MCP stdio `stdout` protocol-clean,
- keep secrets and license material redacted,
- never log plaintext embedded license data,
- distinguish trusted-local v1 mode from future multi-user modes.

---

## 16. Delivery model

The delivery model is explicitly multi-agent:
- one coordinating/integrating agent,
- multiple bounded coding agents,
- frozen contracts before parallel implementation,
- ADR discipline for deviations.

The execution pack generated around this constitution is the operational delivery layer.

---

## 17. Acceptance baseline

The design is only considered realized when:
1. the product runs as a standalone local application,
2. the product exposes build and test surfaces separately,
3. build orchestration is deterministic and server-owned,
4. repeated meaningless rebuilds are prevented by design and persistence,
5. build and test states are queryable via MCP and browser surfaces,
6. v6.2, v7.1, and v7.2 are supported via plugins/capabilities,
7. the browser UI exposes live visibility without full refresh,
8. the flaky subsystem understands build/test interactions,
9. the execution pack supports multi-agent implementation without contract drift.

---

## 18. Relationship to the rest of the execution pack

The rest of the execution pack refines this document into:
- frozen decisions,
- detailed contracts,
- phase briefs,
- work packages,
- task cards,
- ADRs,
- validation strategy.

If a later document appears to contradict this one, this document wins unless an ADR explicitly supersedes the rule.
