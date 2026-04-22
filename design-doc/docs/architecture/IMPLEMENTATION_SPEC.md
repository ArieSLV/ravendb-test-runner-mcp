# IMPLEMENTATION_SPEC.md

**Project:** RavenDB Test MCP Control Plane  
**Status:** Architecture Freeze v1 / Implementation Specification  
**Language:** English  
**Audience:** AI coding agents, human integrator, maintainers  
**Normative words:** MUST, MUST NOT, REQUIRED, SHOULD, SHOULD NOT, MAY

---

## 1. Purpose

This document is the implementation specification for a production-grade, local-first, stand-alone application that manages RavenDB repository test execution through:

1. a **shared orchestration core**,
2. a **browser-facing operator dashboard**,
3. a **local MCP server surface** for AI agents,
4. a **RavenDB Embedded-backed metadata store**, and
5. a **flaky test management subsystem**.

This specification is intentionally delivery-oriented. It is written so that implementation can proceed in multiple phases and, after contract freeze, in parallel by multiple AI coding agents without architectural drift.

This document does **not** ask the implementation agent to redesign the system. It asks the implementation agent to **implement the system defined here**.

---

## 2. Product definition

### 2.1 What is being built

The product is a **local stand-alone control-plane application** for RavenDB test execution.

It MUST:

- run as a long-lived process manually started by a developer on their machine;
- expose a live dashboard for human operators;
- expose MCP capabilities to AI agents;
- remain usable even if a specific AI host has an unstable child-process lifecycle;
- understand the RavenDB repository deeply enough to plan, explain, execute, and analyze tests rather than behaving like a thin `dotnet test` wrapper.

### 2.2 What it is not

The product is **not**:

- a one-off script;
- a child-process-only MCP tool with no persistent state;
- a polling-only web UI;
- a generic test runner with no repository semantics;
- a multi-tenant cloud service in v1.

---

## 3. Architecture freeze: decisions already made

The following decisions are **frozen** for implementation v1.

### 3.1 Runtime and implementation language

- The default implementation target is **.NET 10 / C#**.
- The implementation MUST use the official **MCP C# SDK**.
- The browser-facing backend MUST use **ASP.NET Core**.

### 3.2 Deployment model

- v1 is a **local stand-alone application**.
- The application is launched manually by the developer.
- The application MUST provide a **full dashboard/operator UI**.
- The application MUST provide **MCP access for AI agents**.
- v1 is **single-user / local-first**.
- v1 is **not** a team-shared hosted service.

### 3.3 MCP transport strategy

The product MUST support **two MCP access paths**:

1. **Primary:** local **Streamable HTTP** MCP endpoint hosted by the stand-alone application.
2. **Secondary / compatibility:** local **stdio bridge host** for AI hosts that only support stdio child-process integration.

The stdio bridge MUST be a thin compatibility layer and MUST NOT become the system-of-record host.

### 3.4 Storage strategy

- **SQLite is forbidden.**
- The metadata store MUST be **RavenDB Embedded**.
- Large raw artifacts MUST NOT be stored as the primary canonical blob payload in RavenDB by default.
- The canonical raw artifact store MUST be the **filesystem**.
- RavenDB MUST store artifact metadata, summaries, fingerprints, paths, retention state, and selected compact attachments.

### 3.5 Supported repository lines

The system MUST support these repository lines as first-class targets:

- `v6.2`
- `v7.1`
- `v7.2`

The system MUST be designed so future support for `6.3`, `7.3`, `8.0`, and later versions is straightforward.

### 3.6 Versioning strategy

The system MUST use:

- a shared orchestration core,
- version-aware semantic plugins,
- a capability matrix,
- feature detection,
- branch/version routing.

The system MUST NOT rely on sprawling `if/else` condition chains as the long-term extensibility strategy.

### 3.7 AI compatibility rule

For implementation purposes, use the following compatibility model:

- `v6.2` → AI-specific test semantics are considered absent.
- `v7.0+` → AI-related semantics MAY exist.
- `v7.1+` → richer GenAI / AI Agent semantics MAY exist.
- `v7.2` → AI-related test semantics are a supported baseline.

This MUST be implemented as **capabilities**, not as one global boolean.

### 3.8 Frontend and visual direction

- The browser UI is REQUIRED.
- The UI MUST be real-time and MUST NOT depend on page refresh for normal operation.
- The UI SHOULD visually align with **RavenDB Studio 7.2**.
- The implementation MAY reuse style tokens, patterns, layout ideas, and interaction patterns from RavenDB Studio where feasible.
- The implementation MUST NOT become blocked by hard dependency on the Studio codebase.

### 3.9 Embedded licensing policy

- RavenDB Embedded licensing is REQUIRED.
- Missing or invalid embedded license is a startup-blocking condition.
- The application MUST attempt to discover a license automatically.
- If no valid license is found, the application MUST enter an explicit setup-required state and request one.

### 3.10 Flaky policy

- Flaky analysis is REQUIRED.
- Iterative reruns are REQUIRED.
- Automated diagnostics escalation is REQUIRED.
- Quarantine workflows are supported.
- Automated flaky/quarantine actions MUST be explainable, reversible, and journaled.

### 3.11 Multi-agent delivery discipline

- Multiple coding agents MAY work in parallel.
- One agent or human MUST act as integrator.
- Shared contracts MUST be frozen before parallel execution work proceeds.
- Architectural deviations MUST be documented as ADRs / Design Deltas.

---

## 4. Evidence baseline

This implementation spec is based on the following observed repository and official-platform facts.

### 4.1 Repository and platform observations

- `v6.2/global.json` pins `.NET SDK 8.0.420`; `v7.1/global.json` also pins `.NET SDK 8.0.420`; `v7.2/global.json` pins `.NET SDK 10.0.202`.  
- `v6.2` uses `Microsoft.NET.Test.Sdk 18.0.1`, `xunit 2.9.3`, `xunit.runner.visualstudio 2.8.2`, `xRetry 1.9.0`.  
- `v7.1` uses `Microsoft.NET.Test.Sdk 18.4.0`, `xunit 2.9.3`, `xunit.runner.visualstudio 2.8.2`, `xRetry 1.9.0`, and already includes AI-related packages and AI test helpers.  
- `v7.2` uses `Microsoft.NET.Test.Sdk 18.4.0`, `xunit.v3 3.2.2`, `xunit.runner.visualstudio 3.1.5`, and `xRetry.v3 1.0.0-rc3`.  
- `v6.2` and `v7.1` `FastTests` target `net8.0`; `v7.2` `FastTests` targets `net10.0` and explicitly sets `OutputType=Exe`.  
- `SlowTests.Issues` is present in `v7.1` and `v7.2` solution topology.  
- `test/xunit.runner.json` and `ParallelTestBase` enforce a dual-layer concurrency model.  
- `RavenFact` / `RavenTheory` / helpers encode significant runtime semantics outside `dotnet test` itself.

### 4.2 Official platform observations

- In .NET 10, `dotnet test` supports runner selection in `global.json`; `VSTest` remains the default when no MTP runner is specified.  
- VSTest remains the effective runner path for the supported RavenDB lines in scope.  
- MCP supports local stdio and Streamable HTTP transports; stdio servers MUST NOT write logs to `stdout`.  
- MCP supports long-running requests, progress, cancellation, tools, resources, and prompts.  
- ASP.NET Core 10 supports native Server-Sent Events via `TypedResults.ServerSentEvents`.  
- SignalR remains a strong real-time transport fit for interactive operator dashboards.  
- RavenDB Embedded supports licensing via configuration and environment variables.  
- RavenDB 6.2 changed TestDriver embedded license failure behavior so missing/invalid license may throw by default.

### 4.3 References

The implementation agent SHOULD keep the following references available during coding.

- .NET `dotnet test` and runner selection docs
- .NET VSTest-specific `dotnet test` docs
- .NET selective unit test filtering docs
- MCP specification, transports, cancellation, tools/resources docs
- MCP C# SDK docs
- ASP.NET Core SignalR docs
- ASP.NET Core 10 SSE docs
- RavenDB Embedded docs
- RavenDB 6.2 embedded/TestDriver breaking-change docs
- RavenDB repository branches `v6.2`, `v7.1`, `v7.2`

A reference appendix with source links is included at the end of this document.

---

## 5. Core goals

The system MUST provide the following end-user outcomes.

### 5.1 Repository intelligence

The system MUST:

- detect that a workspace is a RavenDB-like repository;
- determine the repository line (`v6.2`, `v7.1`, `v7.2`, or unsupported);
- build a branch-aware test topology;
- understand repository-specific test semantics;
- expose a structured, queryable test catalog.

### 5.2 Deterministic planning and execution

The system MUST:

- normalize selectors;
- explain what will run and why;
- perform preflight analysis;
- generate deterministic run plans;
- execute test runs safely;
- support cancellation, timeouts, and process tree cleanup;
- produce reproducible commands.

### 5.3 Observability and explainability

The system MUST:

- preserve raw and normalized run results;
- classify failures and skips;
- explain selectors, skips, and plan decisions;
- provide live output and partial status;
- record enough history to support flaky analysis.

### 5.4 Human and AI consumers

The system MUST support both:

- **AI-agent workflows** through MCP, and
- **human operator workflows** through a browser dashboard.

### 5.5 Long-term maintainability

The architecture MUST favor:

- version plugins over hard-coded branching,
- thin host surfaces over duplicated orchestration logic,
- contract-first parallel development,
- additive support for future RavenDB lines.

---

## 6. Non-goals for v1

The following are explicitly out of scope unless later promoted by ADR.

- Multi-tenant hosted SaaS deployment.
- Team-shared multi-user concurrency semantics.
- Distributed execution agents.
- Cloud object storage as mandatory baseline.
- Full MTP execution backend.
- In-proc test runner execution.
- Polling-only dashboard.
- Mandatory full code reuse from RavenDB Studio.

---

## 7. System context

### 7.1 High-level shape

The system consists of five major areas:

1. **Shared orchestration core**
2. **MCP-facing surfaces**
3. **Browser-facing surfaces**
4. **Storage and artifact subsystem**
5. **Flaky analytics subsystem**

### 7.2 High-level component diagram

```text
+-------------------------------------------------------------+
| Local Stand-alone Application                               |
|-------------------------------------------------------------|
| ASP.NET Core Host                                           |
|  - Dashboard UI assets / hosting                            |
|  - Browser APIs / BFF                                       |
|  - SignalR hub                                              |
|  - SSE endpoints                                            |
|  - Local Streamable HTTP MCP endpoint                       |
|-------------------------------------------------------------|
| Shared Orchestration Core                                   |
|  - Workspace Analyzer                                       |
|  - Version Router                                           |
|  - Semantic Plugins                                         |
|  - Test Catalog                                             |
|  - Preflight Evaluator                                      |
|  - Run Planner                                              |
|  - Command Synthesizer                                      |
|  - Scheduler                                                |
|  - Execution Engine                                         |
|  - Process Supervisor                                       |
|  - Artifact Manager                                         |
|  - Result Normalizer                                        |
|  - Flaky Engine                                             |
|  - Event Publisher                                          |
|-------------------------------------------------------------|
| Local Metadata + Artifacts                                  |
|  - RavenDB Embedded                                         |
|  - Filesystem Artifact Store                                |
+-------------------------------------------------------------+
              ^
              |
  +---------------------------+
  | Optional STDIO Bridge     |
  | - thin MCP stdio proxy    |
  | - forwards to local HTTP  |
  +---------------------------+
```

### 7.3 Host strategy

The implementation MUST provide:

- **Primary host:** the stand-alone ASP.NET Core application.
- **Optional secondary host:** a thin stdio bridge process for hosts that require stdio MCP.

The stdio bridge MUST NOT contain orchestration, storage, or domain logic. It MUST forward to the local stand-alone app.

---

## 8. Repository-line support model

### 8.1 Supported lines

The following lines are in scope for v1:

| Line | Status | Notes |
|---|---|---|
| `v6.2` | Required | LTS line; xUnit v2 / .NET 8 SDK baseline |
| `v7.1` | Required | Transition line; xUnit v2, AI semantics present |
| `v7.2` | Required | Newer line; xUnit v3 / .NET 10 SDK |

### 8.2 Capability matrix

The implementation MUST represent repository support through a capability matrix rather than only branch names.

Minimum required capability flags:

- `supportsXunitV2`
- `supportsXunitV3`
- `supportsSlowTestsIssues`
- `supportsAiEmbeddingsSemantics`
- `supportsAiGenAiSemantics`
- `supportsAiAgentsSemantics`
- `supportsAiSkipToggle`
- `supportsXunitV3SourceInfo`
- `projectOutputStyle` (`library-style`, `exe-style`)
- `runnerFamily` (`vstest`, future `mtp`)
- `supportsRunSettingsOverrides`
- `supportsDirectTestDriverCoupling`

### 8.3 Mandatory semantic plugins

The implementation MUST ship the following plugins:

- `RavenV62Semantics`
- `RavenV71Semantics`
- `RavenV72Semantics`

Each plugin MUST:

- identify supported repository line(s),
- contribute category/trait mapping,
- parse branch-specific test attributes and helpers,
- expose capability flags,
- provide branch-specific skip/retry/platform/service rules,
- surface branch-specific discovery/normalization hints.

### 8.4 Extensibility rule

New repository lines MUST be added primarily by:

1. adding a new plugin,
2. extending capability detection,
3. adding validation fixtures and compatibility tests.

The implementation SHOULD avoid modifying shared orchestration logic unless the new line introduces a truly shared concept.

---

## 9. Solution and project layout

The implementation MUST follow a boundary-preserving solution layout. Project names may vary slightly, but boundaries MUST remain intact.

```text
/src
  /RavenMcp.Domain
  /RavenMcp.Core.Abstractions
  /RavenMcp.Core
  /RavenMcp.Semantics.Abstractions
  /RavenMcp.Semantics.Raven.V62
  /RavenMcp.Semantics.Raven.V71
  /RavenMcp.Semantics.Raven.V72
  /RavenMcp.Storage.RavenEmbedded
  /RavenMcp.Artifacts
  /RavenMcp.Execution
  /RavenMcp.Planning
  /RavenMcp.Results
  /RavenMcp.Flaky
  /RavenMcp.Contracts
  /RavenMcp.Web.Api
  /RavenMcp.Web.Ui
  /RavenMcp.Host.App
  /RavenMcp.Host.StdioBridge

/tests
  /RavenMcp.UnitTests
  /RavenMcp.ContractTests
  /RavenMcp.IntegrationTests
  /RavenMcp.CrossBranchTests
  /RavenMcp.FlakyFixtureTests
  /RavenMcp.UiTests

/docs
  /adr
  /contracts
  /delivery
  /spec-revisions
```

### 9.1 Dependency rules

- `Domain` MUST not depend on infrastructure.
- `Contracts` MUST contain public DTOs/events used across module boundaries.
- `Semantics.*` MUST depend on abstractions, not on host layers.
- `Host.App` MUST compose services but MUST NOT contain domain logic.
- `Host.StdioBridge` MUST be thin and MUST NOT duplicate orchestration logic.
- `Web.Ui` MUST consume browser-facing APIs and live streams, not call RavenDB Embedded directly.

---

## 10. Storage architecture

## 10.1 Metadata store

The system MUST use **RavenDB Embedded** as its metadata store.

The metadata database MUST be responsible for:

- workspace snapshots,
- semantic snapshots,
- compatibility matrices,
- test catalog entries,
- run plans,
- runs,
- attempts,
- normalized results,
- artifact metadata,
- event checkpoints,
- profiles/settings,
- flaky histories,
- quarantine records,
- UI preferences (if implemented).

## 10.2 Artifact store

The canonical raw artifact store MUST be the **filesystem**.

The implementation MUST maintain a deterministic artifact root under the local application data folder.

Default root strategy:

- Windows: `%LOCALAPPDATA%/RavenMcp/`
- Linux/macOS: equivalent `LocalApplicationData` path as resolved by .NET

Recommended layout:

```text
<appData>/
  ravendb/
    data/
    settings/
  artifacts/
    runs/
      <runId>/
        plan.json
        env.redacted.json
        attempts/
          000/
            01-SlowTests/
              command.json
              console.merged.log
              stdout.log
              stderr.log
              results.trx
              results.junit.xml
              diag.log
              normalized.json
          001/
            ...
  logs/
  cache/
```

## 10.3 RavenDB attachments policy

The implementation MUST support a **hybrid artifact storage policy**.

### 10.3.1 Default rule

- Small and medium artifacts MAY be stored as RavenDB attachments.
- Large artifacts MUST be stored on filesystem.

### 10.3.2 Default threshold

Until changed by ADR or configuration, use:

- `ArtifactAttachmentThresholdBytes = 5 * 1024 * 1024` (5 MiB)

### 10.3.3 Artifacts usually eligible for attachment storage

- compact JSON summaries
- normalized result bundles
- compact `trx`
- compact `junit`
- small diagnostic summaries
- plan / preflight / repro bundles

### 10.3.4 Artifacts usually filesystem-only

- very large `stdout`
- very large `stderr`
- merged console logs exceeding threshold
- large `diag` logs
- dumps / crash files / blame outputs
- bulky binary attachments

### 10.3.5 Metadata required for all artifacts

Every artifact record MUST include:

- `ArtifactId`
- `RunId`
- `AttemptIndex`
- `StepIndex`
- `Kind`
- `ContentType`
- `SizeBytes`
- `Sha256`
- `StorageKind` (`Attachment`, `FilePath`)
- `PathOrAttachmentName`
- `RetentionClass`
- `CreatedAtUtc`
- `Sensitive`
- `Redacted`

## 10.4 RavenDB collections and IDs

The following collections MUST exist.

| Collection | Purpose | ID pattern |
|---|---|---|
| `WorkspaceSnapshots` | workspace identity and branch/toolchain info | `workspaces/{hash}` |
| `SemanticSnapshots` | semantic model state | `semantics/{workspaceHash}/{snapshotHash}` |
| `CompatibilityMatrices` | capability matrix per workspace line | `compat/{workspaceHash}/{line}` |
| `TestCatalogEntries` | normalized test catalog | `tests/{workspaceHash}/{testHash}` |
| `RunPlans` | immutable run plans | `run-plans/{runId}` |
| `Runs` | runtime lifecycle document | `runs/{runId}` |
| `Attempts` | per-attempt document | `runs/{runId}/attempts/{n}` |
| `Artifacts` | artifact metadata | `artifacts/{runId}/{artifactHash}` |
| `FlakyHistories` | per-test stability history | `flaky/{workspaceHash}/{testHash}` |
| `Profiles` | execution profiles | `profiles/{profileName}` |
| `ServerSettings` | local app settings | `settings/{name}` |
| `EventCheckpoints` | UI/MCP replay checkpoints | `events/{streamKey}` |
| `QuarantineRecords` | quarantine decisions and evidence | `quarantine/{workspaceHash}/{testHash}` |

## 10.5 Required indexes

At minimum, implement indexes for:

- `WorkspaceSnapshots_ByRootAndSha`
- `Runs_ByStateAndStartedAt`
- `Attempts_ByRunAndIndex`
- `Artifacts_ByRunAndKind`
- `TestCatalog_ByFqnAndCategory`
- `FlakyHistories_ByTestAndRepoLine`
- `QuarantineRecords_ByStatus`

---

## 11. Licensing and secret bootstrap

## 11.1 Embedded license discovery

At startup, the application MUST attempt to discover the RavenDB Embedded license in the following order:

1. explicit persisted app configuration / secret reference;
2. environment variable `RAVEN_License`;
3. environment variable `RAVEN_LicensePath`;
4. compatibility alias `RAVEN_License_Path`;
5. interactive first-run setup flow.

## 11.2 Startup gating behavior

If no valid license is found:

- the application MUST NOT start normal run orchestration;
- the application MUST enter `SetupRequired` state;
- the browser UI MUST provide a setup flow;
- MCP operations that require the metadata store MUST return a clear setup-required error.

## 11.3 Test-run license propagation

The application MUST support a separate test-run license overlay for repository tests.

Default behavior:

- if the application has a validated RavenDB license JSON available,
- and no explicit test-run override is configured,
- the application SHOULD make the same license available to test processes as `RAVEN_LICENSE`.

This behavior MUST be visible and explainable in the environment/profile view.

## 11.4 Secret handling

Plaintext licenses MUST NOT be written to:

- application logs,
- MCP responses,
- browser API responses,
- filesystem debug dumps,
- event streams.

The system MAY store encrypted local secret references or protected configuration values, but MUST expose only:

- source of license,
- detected/not-detected state,
- validation result,
- non-sensitive fingerprint information.

---

## 12. Domain model

The implementation MUST create stable DTOs / entities for the following concepts.

## 12.1 Required core entities

### `WorkspaceSnapshot`

Fields:

- `WorkspaceId`
- `RootPath`
- `RepoLine`
- `BranchName`
- `GitSha`
- `SdkVersion`
- `RunnerFamily`
- `FrameworkFamily`
- `AdapterVersion`
- `ProjectOutputStyle`
- `SupportsSlowTestsIssues`
- `SupportsAiEmbeddingsSemantics`
- `SupportsAiGenAiSemantics`
- `SupportsAiAgentsSemantics`
- `SupportsAiSkipToggle`
- `SupportsXunitV3SourceInfo`
- `SupportsDirectTestDriverCoupling`
- `SemanticPluginId`
- `CreatedAtUtc`

### `SemanticSnapshot`

Fields:

- `SemanticSnapshotId`
- `WorkspaceId`
- `SemanticPluginId`
- `CategoryCatalogVersion`
- `AttributeRegistryVersion`
- `CapabilitySnapshot`
- `CompileSymbolHooks`
- `SourceFingerprint`
- `CreatedAtUtc`

### `CompatibilityMatrix`

Fields:

- `WorkspaceId`
- `RepoLine`
- `RunnerFamily`
- `FrameworkFamily`
- `Capabilities`
- `VersionSensitivePoints`
- `Warnings`
- `CreatedAtUtc`

### `TestProject`

Fields:

- `ProjectId`
- `Name`
- `Path`
- `Role`
- `TargetFrameworks`
- `OutputType`
- `AssemblyName`
- `References`

### `TestIdentity`

Fields:

- `TestId`
- `ProjectId`
- `AssemblyName`
- `TargetFramework`
- `FullyQualifiedName`
- `ClassFqn`
- `MethodName`
- `DisplayName`
- `FrameworkFamily`
- `XunitUniqueId`
- `SourceFilePath`
- `SourceLineNumber`
- `SelectorStabilityLevel`

### `TestCategory`

Fields:

- `Key`
- `TraitKey`
- `TraitValue`
- `Aliases`
- `BitValue`
- `Implies`
- `RepoLineSupport`

### `TestRequirement`

Fields:

- `Kind`
- `DeclaredBy`
- `DeclaredValue`
- `EnvironmentKeys`
- `RuntimeOnly`
- `Confidence`

### `EnvironmentProfile`

Fields:

- `ProfileName`
- `Configuration`
- `InheritMode`
- `Set`
- `Unset`
- `RedactionRules`
- `RepoSpecificSemantics`
- `ProfileNotes`

### `RunRequest`

Fields:

- `WorkspaceId`
- `Selector`
- `ExecutionProfile`
- `ClientRequestId`
- `DiagnosticsMode`
- `TimeoutPolicy`

### `RunPlan`

Fields:

- `RunId`
- `WorkspaceId`
- `RepoLine`
- `SemanticPluginId`
- `RunnerFamily`
- `FrameworkFamily`
- `NormalizedSelector`
- `PredictedSelection`
- `PredictedSkips`
- `CompatibilityWarnings`
- `CommandSteps`
- `ArtifactRoot`
- `CreatedAtUtc`

### `RunExecution`

Fields:

- `RunId`
- `State`
- `Phase`
- `CurrentStepIndex`
- `StepCount`
- `CanCancel`
- `StartedAtUtc`
- `FinishedAtUtc`
- `FailureClassification`

### `RunResult`

Fields:

- `RunId`
- `RepoLine`
- `FrameworkFamily`
- `Status`
- `Summary`
- `FailureClassification`
- `PredictedVsActual`
- `Artifacts`
- `Tests`
- `Attempts`
- `NormalizationPrecision`

### `RunArtifact`

Fields:

- `ArtifactId`
- `RunId`
- `AttemptIndex`
- `StepIndex`
- `Kind`
- `StorageKind`
- `PathOrAttachmentName`
- `ContentType`
- `SizeBytes`
- `Sha256`
- `Sensitive`
- `Redacted`

### `SkipPrediction`

Fields:

- `TestId`
- `ReasonCode`
- `Message`
- `Confidence`
- `RequiresRuntimeValidation`
- `ClassificationHint`

### `FailureClassification`

Fields:

- `Kind`
- `Scope`
- `Phase`
- `Retriable`
- `SuggestedAction`

## 12.2 Required flaky entities

### `FlakyPolicy`

Fields:

- `Mode`
- `MaxAttempts`
- `OverallBudgetMs`
- `PerAttemptTimeoutMs`
- `PassThreshold`
- `FailureThreshold`
- `StopOnFirstFailure`
- `EscalateDiagnosticsAfterFailures`
- `FallbackToSequentialAfterInconsistency`
- `FreezeEnvironment`

### `IterativeRunRequest`

Fields:

- `BaseRunRequest`
- `FlakyPolicy`
- `CompareMode`

### `AttemptPlan`

Fields:

- `RunId`
- `AttemptIndex`
- `DerivedProfile`
- `ReasonForEscalation`
- `CommandSteps`

### `AttemptResult`

Fields:

- `RunId`
- `AttemptIndex`
- `Status`
- `Summary`
- `FailureClassification`
- `SignatureHashes`
- `DurationMs`
- `Artifacts`

### `StabilitySignal`

Fields:

- `Kind`
- `Confidence`
- `EvidenceRefs`
- `Notes`

### `FlakyClassification`

Fields:

- `Kind`
- `Score`
- `ReasonCodes`
- `EvidenceRefs`
- `Confidence`

### `HistoricalOutcomeRollup`

Fields:

- `TestId`
- `Window`
- `PassRate`
- `FailureRate`
- `SkipRate`
- `DistinctFailureSignatures`
- `ProfilesSeen`

### `QuarantineRecord`

Fields:

- `TestId`
- `RepoLine`
- `Status`
- `ProposedAtUtc`
- `AppliedAtUtc`
- `ReasonCodes`
- `EvidenceRefs`
- `PolicySnapshot`
- `ReversalInfo`

---

## 13. Shared contract freeze requirements

Before parallel implementation begins, the following MUST be frozen and published under `/docs/contracts` and corresponding shared code packages.

### 13.1 DTO contracts

All entities listed in section 12.

### 13.2 Artifact path contract

Artifact root structure, naming, redaction file names, attempt numbering, step numbering.

### 13.3 Event contract

All live events consumed by browser UI and optionally exposed through MCP-linked status flows.

### 13.4 State machines

- Run lifecycle state machine
- Attempt lifecycle state machine
- Cancellation transition model
- Timeout transition model

### 13.5 Error model

Stable error codes for:

- setup-required
- unsupported-repo-line
- workspace-invalid
- toolchain-unavailable
- no-tests-matched
- build-error
- restore-error
- discovery-error
- adapter-error
- host-crashed
- timed-out
- cancelled
- artifact-unavailable
- license-missing
- branch-capability-mismatch

---

## 14. Branch-aware semantics architecture

## 14.1 Shared abstractions

The implementation MUST define plugin abstractions for:

- repository line detection
- capability declaration
- category extraction
- custom attribute extraction
- skip requirement extraction
- retry extraction
- preflight evaluation hooks
- normalization hints
- branch-specific UI labels / warnings

## 14.2 Plugin contract

Each plugin MUST implement:

```text
CanHandle(workspace) -> bool
GetRepoLine() -> string
BuildCapabilities(workspace) -> CapabilitySet
BuildCategoryCatalog(workspace) -> CategoryCatalog
BuildAttributeRegistry(workspace) -> AttributeRegistry
AnalyzeProjects(workspace) -> ProjectTopology
AnalyzeTests(workspace) -> TestCatalogEntries
PredictSkips(request, env, machine) -> SkipPredictions
NormalizeResults(rawArtifacts) -> NormalizationHints
```

## 14.3 Branch specifics

### `RavenV62Semantics`

Must cover:

- `.NET SDK 8.0.420` line behavior
- xUnit v2 stack
- `xRetry`
- `RavenFact` / `RavenTheory` / `RavenRetryFact`
- license/nightly/service/platform semantics
- no AI semantics by default
- direct TestDriver coupling awareness

### `RavenV71Semantics`

Must cover:

- `.NET SDK 8.0.420` line behavior
- xUnit v2 stack with newer test SDK
- AI-related helpers, env flags, and test attributes
- `SlowTests.Issues` topology
- still VSTest path

### `RavenV72Semantics`

Must cover:

- `.NET SDK 10.0.202` line behavior
- xUnit v3 stack
- `xRetry.v3`
- AI-related semantics
- `SlowTests.Issues`
- xUnit v3-specific metadata/source-info opportunities

---

## 15. Planning, preflight, and execution

## 15.1 Selector model

The internal canonical selector model MUST be structured.

Supported selector dimensions:

- projects
- assemblies
- categories
- class FQN
- method FQN
- exact FQN list
- contains-FQN list
- previous-run failed set

Raw `--filter` strings MAY be supported only in **expert mode** and MUST NOT become the canonical internal representation.

## 15.2 Category normalization rule

Category matching MUST use the repository semantic catalog and the canonical xUnit trait values.

The implementation MUST NOT assume that enum names are identical to filter values.

## 15.3 Preflight requirements

Preflight MUST evaluate:

- workspace validity
- repository line support
- SDK presence
- capability matrix
- predicted project selection
- predicted license-dependent skips
- nightly gating
- platform/architecture/intrinsics gating
- integration toggle gating
- AI toggle gating where supported
- service requirement uncertainty
- environment profile validity

Preflight output MUST distinguish:

- deterministic skip
- runtime-unknown skip potential
- unsupported capability
- setup/license failure
- possible flaky-related risk

## 15.4 Environment profile model

The system MUST support named profiles, including at minimum:

- `repo-default`
- `ci-workflow-parity`
- `sequential`
- `diagnostic`
- `iterative-sequential`

Profiles MUST be persisted in RavenDB.

Profiles MUST support:

- configuration (`Release` / `Debug`)
- env set/unset overlay
- diagnostics knobs
- concurrency knobs
- timeouts
- optional xUnit override options where valid

## 15.5 Scheduler rules

Default scheduler policy:

- 1 active run per workspace
- 1 active `dotnet test` process per workspace
- 1 active iterative run per workspace

Overrides MAY exist, but MUST be explicit and policy-controlled.

## 15.6 Command synthesis rules

The canonical execution target MUST be the project path.

The command synthesizer MUST generate deterministic `argv[]` and MUST NOT execute shell strings.

Required support:

- run project
- run category
- run class
- run method
- run category + class/method
- rerun failed
- iterative run

The synthesizer MUST support:

- `--configuration`
- `--results-directory`
- `--filter`
- `--logger`
- `--diag`
- `--blame-*`
- `--no-restore`
- `--no-build`

only when justified by the plan and supported by the effective runner family.

## 15.7 Cancellation and timeout

The execution engine MUST support:

- explicit cancellation request
- overall run timeout
- per-attempt timeout
- per-step timeout
- process tree cleanup
- timeout classification

---

## 16. Result normalization and artifact handling

## 16.1 Canonical result policy

The system MUST store both:

- **raw results** (console, TRX, diagnostics, etc.)
- **normalized results**

The canonical programmatic source of truth is the normalized result model.

## 16.2 Minimum artifact set per executed step

Each executed step MUST attempt to produce:

- `command.json`
- `console.merged.log`
- `stdout.log`
- `stderr.log`
- `results.trx`
- `normalized.json`

Optional artifacts depending on mode/capability:

- `results.junit.xml`
- `diag.log`
- `blame/*`
- `plan.json`
- `repro-command.<shell>.txt`

## 16.3 Failure taxonomy

At minimum, the normalized model MUST support:

- `passed`
- `failed`
- `skipped`
- `not-found`
- `workspace-invalid`
- `unsupported-repo-line`
- `toolchain-unavailable`
- `restore-error`
- `build-error`
- `discovery-error`
- `adapter-error`
- `no-tests-matched`
- `all-selected-tests-skipped`
- `host-crashed`
- `hung`
- `timed-out`
- `cancelled`
- `artifact-parse-failed`
- `inconsistent-result-set`

## 16.4 Predicted vs actual

The normalizer MUST compare:

- predicted skips vs actual skips,
- predicted selection vs actual discovered/executed set,
- preflight warnings vs observed outcome.

This comparison MUST be persisted and surfaced in both UI and MCP results when relevant.

---

## 17. MCP implementation requirements

## 17.1 Primary MCP surface

The primary MCP endpoint MUST be the **local Streamable HTTP** endpoint exposed by the stand-alone application.

Recommended path:

- `/mcp`

## 17.2 STDIO bridge

A secondary stdio bridge MUST be available for compatibility.

The stdio bridge MUST:

- be stateless or minimally stateful,
- connect to the running local MCP HTTP endpoint,
- forward requests/responses/progress/cancellation,
- fail clearly if the stand-alone application is not running,
- never become the authoritative runtime owner.

## 17.3 Logging rule

For stdio mode:

- the bridge MUST NOT write logs to `stdout`;
- logging MUST go to `stderr` or files.

## 17.4 MCP features

v1 MUST support:

- **Tools**
- **Resources** (recommended for run summaries, plans, results)

**Prompts** are optional and MAY be deferred.

## 17.5 Required tools

The MCP server MUST provide at least:

- `tests.projects.list`
- `tests.categories.list`
- `tests.discover`
- `tests.preflight`
- `tests.plan`
- `tests.run`
- `tests.run_status`
- `tests.run_output_tail`
- `tests.run_results`
- `tests.cancel`
- `tests.rerun_failed`
- `tests.explain_filter`
- `tests.explain_skip`
- `tests.repro_command`
- `tests.capabilities`
- `tests.iterative_run`
- `tests.flaky_analyze`
- `tests.flaky_history`
- `tests.compare_attempts`
- `tests.stability_report`
- `tests.quarantine_candidates`

## 17.6 Tool contract rules

Every tool response SHOULD include when relevant:

- `RepoLine`
- `FrameworkFamily`
- `SemanticPluginId`
- `CapabilitiesUsed`
- `Warnings`
- `VersionSensitiveNotes`

Long-running tools MUST support:

- progress notifications,
- cancellation,
- resumable status lookup through run IDs.

---

## 18. Browser-facing backend requirements

## 18.1 Browser-facing surfaces

The stand-alone app MUST expose:

- REST/JSON query endpoints
- REST/JSON write endpoints
- SignalR hub
- optional SSE endpoints
- static UI hosting or dev-proxy integration

## 18.2 Live transport

The primary browser live transport MUST be **SignalR**.

The backend SHOULD expose supplemental SSE endpoints for simple stream cases and operational debugging.

## 18.3 Required browser endpoints

Minimum query endpoints:

- `GET /api/workspaces`
- `GET /api/workspaces/{id}/projects`
- `GET /api/workspaces/{id}/categories`
- `GET /api/runs`
- `GET /api/runs/{runId}`
- `GET /api/runs/{runId}/results`
- `GET /api/runs/{runId}/artifacts`
- `GET /api/runs/{runId}/attempts`
- `GET /api/runs/{runId}/logs/{stream}?cursor=...`
- `GET /api/flaky/{testId}/history`

Minimum write endpoints:

- `POST /api/runs/plan`
- `POST /api/runs`
- `POST /api/runs/{runId}/cancel`
- `POST /api/runs/{runId}/rerun-failed`
- `POST /api/runs/iterative`
- `POST /api/flaky/analyze`

Live endpoints:

- `GET /hubs/runs`
- optional `GET /api/runs/{runId}/events`
- optional `GET /api/runs/{runId}/logs/{stream}/events`

## 18.4 Local access policy

By default, the browser-facing backend MUST bind to localhost only.

Authentication is optional in v1 local mode, but the architecture SHOULD allow later addition of local auth or team-mode auth without major rewrite.

---

## 19. Frontend implementation requirements

## 19.1 UI stack

The frontend MUST be implemented in **React + TypeScript**.

The implementation SHOULD favor a modern SPA architecture with:

- React
- TypeScript
- client routing
- query caching
- SignalR client integration
- virtualized log/result views

Specific library choices MAY be finalized during bootstrap, but MUST preserve the architecture.

## 19.2 Visual baseline

The UI SHOULD be visually aligned with **RavenDB Studio 7.2**.

The frontend MAY reuse:

- style tokens,
- color palette,
- layout conventions,
- interaction patterns,
- iconography style,
- table/panel conventions,

when feasible.

Hard dependency on the Studio source tree is NOT required.

## 19.3 Required pages

The UI MUST contain at least:

1. Runs list page
2. Run details page
3. Live console page/panel
4. Test results explorer
5. Run plan inspector
6. Artifact explorer
7. Diagnostics page
8. Flaky analysis page
9. Settings / profiles page

## 19.4 Operator UX requirements

The UI MUST support:

- live phase updates
- live stdout/stderr/merged output
- partial result visibility
- attempt-aware rendering
- explainability panels
- repro command visibility
- cancellation controls
- artifact browsing
- flaky attempt comparison

## 19.5 No-refresh rule

The UI MUST NOT rely on full-page refresh as the normal update mechanism.

Snapshot reloads are acceptable only for:

- reconnect recovery,
- manual refresh fallback,
- exceptional error recovery.

---

## 20. Live event model

The implementation MUST define typed live events shared across browser UI and internal runtime.

## 20.1 Required events

- `run.created`
- `run.queued`
- `run.started`
- `run.phase_changed`
- `run.progress`
- `step.started`
- `step.output`
- `step.summary_updated`
- `test.result_observed`
- `artifact.available`
- `run.cancellation_requested`
- `run.cancelled`
- `run.completed`
- `run.failed`
- `run.timed_out`
- `attempt.started`
- `attempt.completed`
- `flaky.analysis_completed`

## 20.2 Event rules

- Events MUST be ordered per run stream.
- `step.output` MUST support cursors.
- The browser MUST be able to resume from a snapshot + cursor.
- Event schemas MUST be versioned if broken.
- Event payloads MUST remain stable once published under contract freeze.

---

## 21. Execution engine requirements

## 21.1 General rule

The execution engine MUST execute plans step-by-step against project-level commands.

## 21.2 Process supervision

The process supervisor MUST:

- start processes via `argv[]`
- capture stdout and stderr separately
- create merged transcript
- enforce cancellation and timeouts
- kill the full process tree when required
- classify abnormal exits

## 21.3 Concurrency rule

The engine MUST respect the repository’s dual-layer concurrency semantics and MUST NOT launch multiple `dotnet test` processes for the same workspace by default.

## 21.4 CI parity rule

Named execution profiles MUST support at least:

- repo-default runs
- CI-like minimal-parallel runs
- Codebase-convention subset runs
- Debug/Release differences when required by observed workflows

---

## 22. Flaky subsystem requirements

## 22.1 Flaky taxonomy

The implementation MUST distinguish at minimum:

- deterministic failure
- deterministic skip
- environment-sensitive failure
- infrastructure-induced failure
- suspected flaky failure
- likely flaky failure
- confirmed flaky failure
- concurrency-sensitive instability
- external dependency instability
- selector instability (especially theory/data-row level)

## 22.2 Not-flaky rule

The implementation MUST NOT classify the following as flaky by default:

- missing license
- nightly window unmet
- deterministic platform mismatch
- deterministic architecture mismatch
- integration tests explicitly disabled
- AI tests explicitly disabled
- unsupported capability on the current branch

## 22.3 Iterative execution modes

The system MUST support:

- fixed-count reruns
- until-first-failure
- until-first-pass
- until-threshold
- budget-limited
- time-limited
- diagnostics escalation after N failures
- sequential fallback after inconsistency detection

## 22.4 Attempt model

Each attempt MUST have:

- stable attempt index
- derived execution profile
- full artifact subfolder
- normalized attempt result
- signature/hash data for comparison

## 22.5 Compare-attempts

The flaky subsystem MUST support side-by-side comparison of attempts including:

- outcome per attempt
- duration deltas
- failure message diff
- stack trace diff
- skip reason diff
- profile diff
- env fingerprint diff

## 22.6 Automated actions

The system MAY automate:

- diagnostics escalation
- profile tightening
- quarantine proposal
- quarantine application

but only under explicit policy and with:

- reason codes
- confidence
- evidence
- journal/audit record
- reversal path

---

## 23. Security, privacy, and local trust model

## 23.1 Local trust model

v1 is single-user / local-first.

Default assumptions:

- services bind to localhost by default;
- no public exposure is assumed;
- local operators are trusted;
- however, secret redaction and safe command execution are still mandatory.

## 23.2 Command safety

The backend MUST NEVER execute interpolated shell command strings.

The backend MUST ALWAYS:

- construct argument arrays,
- validate paths,
- normalize selectors,
- limit expert-mode raw filter use.

## 23.3 Redaction

The system MUST redact at minimum:

- `RAVEN_LICENSE`
- `RAVEN_License`
- `RAVEN_LicensePath`
- `RAVEN_License_Path`
- secrets matching `*TOKEN*`, `*KEY*`, `*PASSWORD*`, `*SECRET*`

## 23.4 Output safety

The UI and MCP responses MUST truncate or page very large payloads.

The system MUST preserve full raw artifacts in storage when allowed, but MUST NOT indiscriminately inline them into live responses.

---

## 24. Observability requirements

The implementation MUST use structured logging and SHOULD use OpenTelemetry.

## 24.1 Minimum telemetry

- workspace analysis duration
- semantic snapshot build duration
- preflight duration
- build duration
- execution duration
- normalization duration
- tests selected / run / skipped / failed
- predicted skip mismatch count
- cancellation count
- timeout count
- flaky classification count
- quarantine action count
- UI reconnect count

## 24.2 Logging policy

- App host logging MAY use normal structured logging.
- HTTP host logging MAY use stdout or configured sinks.
- stdio bridge logging MUST avoid stdout and MUST use stderr or files.

---

## 25. Multi-agent implementation model

## 25.1 Required delivery strategy

The implementation MUST proceed in phases.

Parallel work MAY begin only after contract freeze for the relevant surfaces.

## 25.2 Mandatory work packages

- `WP-A`: Foundation & Contracts
- `WP-B`: Storage & Registry
- `WP-C`: Semantics & Catalog
- `WP-D`: Planning & Execution
- `WP-E`: Results & Diagnostics
- `WP-F`: MCP Surface
- `WP-G`: Web API / Live Streams
- `WP-H`: Frontend
- `WP-I`: Flaky Analytics
- `WP-J`: Validation & Packaging

## 25.3 Integrator role

One integrator agent or human MUST own:

- contract freeze
- dependency graph
- ADR review
- merge order
- regression verification

---

## 26. Implementation phases

## Phase 0 — Contract Freeze / Bootstrap

### Goal

Freeze the system’s shared contracts and create the solution scaffold.

### Scope

- repository scaffold
- project skeletons
- DTO contracts
- event contracts
- state machines
- artifact path conventions
- RavenDB collection naming and ID policy
- ADR folder and template
- bootstrap README

### Deliverables

- initial solution structure
- shared contracts project
- `/docs/contracts/` package
- `/docs/adr/ADR-0001-*.md` baseline decisions

### Definition of Done

- downstream work packages can implement against frozen contracts
- IDs and collection names are stable
- event names are frozen
- run/attempt states are frozen

## Phase 1 — Storage & Registry Foundation

### Goal

Bring up RavenDB Embedded and artifact indexing.

### Scope

- embedded bootstrap
- database initialization
- collection and index creation
- run registry
- artifact metadata persistence
- local paths manager
- retention metadata

### Definition of Done

- app starts with valid embedded license
- creates metadata database
- persists runs and updates state
- indexes artifacts
- survives restart

## Phase 2 — Workspace / Branch / Semantics

### Goal

Implement branch-aware repository understanding.

### Scope

- workspace detection
- branch detection
- capability matrix builder
- `RavenV62Semantics`
- `RavenV71Semantics`
- `RavenV72Semantics`
- category catalog
- test topology discovery
- test catalog persistence

### Definition of Done

- the app distinguishes supported lines correctly
- supports capability-based routing
- persists semantic snapshots and catalog entries

## Phase 3 — Planning / Preflight / Execution

### Goal

Implement deterministic planning and real execution.

### Scope

- selector normalization
- preflight
- environment profiles
- run planner
- command synthesis
- scheduler
- process supervisor
- cancellation and timeout handling

### Definition of Done

- project, category, class, and method runs work
- plans are reproducible
- cancellation and timeout are enforced correctly

## Phase 4 — Results / Normalization / Artifacts

### Goal

Implement canonical result handling.

### Scope

- raw capture
- TRX collection
- optional JUnit collection
- normalized result model
- failure taxonomy
- predicted vs actual comparison

### Definition of Done

- result classification is stable and queryable
- raw and normalized artifacts are both available

## Phase 5 — MCP Surface

### Goal

Expose the shared core through MCP.

### Scope

- local Streamable HTTP MCP host
- stdio bridge
- tools
- resources
- progress
- cancellation support

### Definition of Done

- AI agents can use the system through MCP
- stdio bridge does not own lifecycle
- stdio logging is safe

## Phase 6 — Browser API / Live Streams

### Goal

Expose browser-facing APIs and live streams.

### Scope

- REST APIs
- SignalR hub
- optional SSE endpoints
- log tailing
- artifact endpoints
- settings/profile endpoints

### Definition of Done

- browser can observe active runs without refresh
- logs and partial results stream correctly

## Phase 7 — Frontend / Operator UI

### Goal

Ship the operator dashboard.

### Scope

- runs list
- run details
- live console
- results explorer
- plan inspector
- artifact explorer
- diagnostics page
- flaky analysis page
- settings page
- RavenDB Studio-aligned visuals

### Definition of Done

- operator can launch, observe, cancel, inspect, and analyze runs through the UI

## Phase 8 — Flaky Subsystem

### Goal

Implement the full flaky management workflow.

### Scope

- iterative plans
- attempts
- compare attempts
- scoring and classification
- automated escalation
- quarantine workflow

### Definition of Done

- iterative execution and flaky analysis are end-to-end operational
- deterministic skips are excluded from flaky classification by default

## Phase 9 — Validation / Hardening / Packaging

### Goal

Make the system robust enough for regular internal developer usage.

### Scope

- contract tests
- integration tests
- cross-branch tests
- flaky fixture tests
- UI tests
- packaging
- setup/runbook docs

### Definition of Done

- the system is repeatably testable and installable
- restart persistence, redaction, and concurrency safeguards are validated

---

## 27. Work package dependencies

After Phase 0:

- `WP-B`, `WP-C`, `WP-F` MAY proceed in parallel.
- `WP-D` depends on frozen domain/contracts and enough of `WP-B` + `WP-C`.
- `WP-E` depends on `WP-D` producing executable runs.
- `WP-G` depends on event and API contracts.
- `WP-H` depends on frozen browser API/event/view-model contracts.
- `WP-I` depends on stabilized run/result/attempt contracts.
- `WP-J` is cross-cutting but finishes last.

---

## 28. Required implementation behavior for AI coding agents

Before starting a large phase or work package, the agent MUST provide:

1. scope summary
2. touched modules/projects
3. contracts touched
4. migration risks
5. acceptance criteria
6. file/module plan

After completing a phase or work package, the agent MUST provide:

1. what was implemented
2. what remains
3. contract changes
4. ADRs created or updated
5. risks left open
6. validation performed

---

## 29. ADR / Design Delta format

Any architectural deviation MUST be documented using this format:

- **Context**
- **Decision**
- **Alternatives considered**
- **Consequences**
- **Impact on existing contracts**
- **Migration / rollback note**

Examples of changes that REQUIRE ADRs:

- changing storage policy
- replacing RavenDB Embedded
- changing MCP transport ownership
- changing UI transport strategy
- changing artifact retention defaults
- changing core event schemas
- changing state-machine semantics

---

## 30. Testing requirements for the product itself

## 30.1 Mandatory test categories

The implementation MUST include:

- DTO / schema contract tests
- workspace detection tests
- version plugin tests
- category normalization tests
- command synthesis golden tests
- preflight tests
- execution integration tests
- cancellation tests
- timeout tests
- artifact indexing tests
- MCP tool contract tests
- live event ordering tests
- UI live update tests
- flaky fixture tests
- redaction tests
- restart persistence tests

## 30.2 Cross-branch validation

There MUST be validation against real repository workspaces or durable fixtures for:

- `v6.2`
- `v7.1`
- `v7.2`

Validation MUST verify at least:

- repo-line detection
- capability matrix
- project topology
- category catalog
- command synthesis
- preflight behavior
- AI capability gating
- `SlowTests.Issues` topology where applicable

## 30.3 Flaky fixture suite

The system MUST include dedicated fixture cases for:

- deterministic failure
- deterministic skip
- random failure
- timeout
- order dependency
- parallelism sensitivity
- service availability sensitivity
- theory-row instability
- diagnostics escalation
- quarantine decision path

---

## 31. Definition of done for the overall project

The project is considered implementation-complete for its first acceptable internal milestone only when:

1. the app runs as a local stand-alone process;
2. RavenDB Embedded is used as the metadata store;
3. raw artifacts are stored through the hybrid RavenDB+filesystem strategy;
4. `v6.2`, `v7.1`, and `v7.2` are supported through version plugins;
5. a local Streamable HTTP MCP endpoint is operational;
6. the stdio bridge is operational;
7. the operator dashboard is operational and live;
8. run history and flaky history persist across restarts;
9. iterative/flaky workflows are operational;
10. cross-branch validation passes;
11. secrets are redacted correctly;
12. the system can continue evolving without architectural chaos.

---

## 32. Immediate next action list for the implementation agent

The implementation agent MUST start with the following sequence.

### Step 1

Create the solution scaffold and ADR baseline.

### Step 2

Freeze contracts for:

- IDs
- collections
- DTOs
- events
- state machines
- artifact paths

### Step 3

Implement RavenDB Embedded bootstrap and setup-required license flow.

### Step 4

Implement repository-line detection and semantic plugins.

### Step 5

Implement run planning and execution.

### Step 6

Implement normalization and storage.

### Step 7

Implement local Streamable HTTP MCP endpoint.

### Step 8

Implement browser APIs and live event hub.

### Step 9

Implement the UI.

### Step 10

Implement iterative/flaky subsystem.

No other start order is acceptable without an ADR.

---

## 33. Reference appendix

### 33.1 Repository references

- RavenDB `v6.2` `global.json`  
  https://raw.githubusercontent.com/ravendb/ravendb/v6.2/global.json
- RavenDB `v7.1` `global.json`  
  https://raw.githubusercontent.com/ravendb/ravendb/v7.1/global.json
- RavenDB `v7.2` `global.json`  
  https://raw.githubusercontent.com/ravendb/ravendb/v7.2/global.json
- RavenDB `v6.2` `Directory.Packages.props`  
  https://github.com/ravendb/ravendb/blob/v6.2/Directory.Packages.props
- RavenDB `v7.1` `Directory.Packages.props`  
  https://github.com/ravendb/ravendb/blob/v7.1/Directory.Packages.props
- RavenDB `v7.2` `Directory.Packages.props`  
  https://github.com/ravendb/ravendb/blob/v7.2/Directory.Packages.props
- RavenDB `v6.2` `FastTests.csproj`  
  https://github.com/ravendb/ravendb/blob/v6.2/test/FastTests/FastTests.csproj
- RavenDB `v7.1` `FastTests.csproj`  
  https://github.com/ravendb/ravendb/blob/v7.1/test/FastTests/FastTests.csproj
- RavenDB `v7.2` `FastTests.csproj`  
  https://github.com/ravendb/ravendb/blob/v7.2/test/FastTests/FastTests.csproj
- RavenDB `v7.1` `RavenDB.sln` (`SlowTests.Issues`)  
  https://github.com/ravendb/ravendb/blob/v7.1/RavenDB.sln
- RavenDB `v7.2` `RavenDB.sln` (`SlowTests.Issues`)  
  https://github.com/ravendb/ravendb/blob/v7.2/RavenDB.sln
- RavenDB `v7.1` `RavenTestHelper.cs`  
  https://raw.githubusercontent.com/ravendb/ravendb/v7.1/test/Tests.Infrastructure/RavenTestHelper.cs
- RavenDB `v7.2` `RavenTestHelper.cs`  
  https://raw.githubusercontent.com/ravendb/ravendb/v7.2/test/Tests.Infrastructure/RavenTestHelper.cs
- RavenDB `v7.1` `RavenAiIntegrationAttribute.cs`  
  https://raw.githubusercontent.com/ravendb/ravendb/v7.1/test/Tests.Infrastructure/RavenAiIntegrationAttribute.cs
- RavenDB `v7.2` `CLAUDE.md`  
  https://github.com/ravendb/ravendb/blob/v7.2/CLAUDE.md

### 33.2 Official .NET references

- `dotnet test` command  
  https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test
- `dotnet test` with VSTest  
  https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test-vstest
- selective unit tests / filtering  
  https://learn.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests
- `global.json` overview  
  https://learn.microsoft.com/en-us/dotnet/core/tools/global-json

### 33.3 Official MCP references

- MCP spec overview  
  https://modelcontextprotocol.io/specification/2025-06-18
- MCP transports  
  https://modelcontextprotocol.io/specification/2025-06-18/basic/transports
- MCP cancellation  
  https://modelcontextprotocol.io/specification/2025-06-18/basic/utilities/cancellation
- MCP SDK overview  
  https://modelcontextprotocol.io/docs/sdk
- official MCP C# SDK overview  
  https://csharp.sdk.modelcontextprotocol.io/
- MCP server build guidance / stdio logging caution  
  https://modelcontextprotocol.io/docs/develop/build-server

### 33.4 ASP.NET / UI references

- ASP.NET Core SignalR overview  
  https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-10.0
- ASP.NET Core 10 SSE support  
  https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0
- `TypedResults.ServerSentEvents` API  
  https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.typedresults.serversentevents?view=aspnetcore-10.0
- React official docs  
  https://react.dev/

### 33.5 RavenDB Embedded and AI references

- RavenDB Embedded docs  
  https://docs.ravendb.net/7.2/server/embedded
- RavenDB 6.2 embedded/TestDriver breaking changes  
  https://docs.ravendb.net/6.2/migration/embedded/testdriver-breaking-changes/
- RavenDB 7.1 AI agents configuration  
  https://docs.ravendb.net/7.1/ai-integration/ai-agents/ai-agents_configuration
- RavenDB Cloud feature availability summary  
  https://docs.ravendb.net/cloud/cloud-features
- RavenDB 7.2 embeddings generation task docs  
  https://docs.ravendb.net/7.2/ai-integration/generating-embeddings/embeddings-generation-task/

---

## 34. Final instruction to the implementation agent

Do not answer this specification with high-level prose.

For each implementation phase and each work package, provide:

- concrete file/module plan,
- contract changes,
- storage decisions,
- event changes,
- acceptance criteria,
- test plan,
- dependency notes,
- risks.

Do not silently change architecture.  
Do not replace RavenDB Embedded.  
Do not introduce SQLite.  
Do not make the stdio bridge the primary runtime.  
Do not ship a polling-only UI.  
Do not classify deterministic skips as flaky.

Implement the system described here.
