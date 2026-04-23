# DEPENDENCY_GRAPH.md

## Purpose
This document shows subsystem, phase, and work-package dependencies for RavenDB Test Runner MCP Server.

## Subsystem dependency graph

```text
RavenDB.TestRunner.McpServer.Core.Abstractions
  -> RavenDB.TestRunner.McpServer.Domain
  -> RavenDB.TestRunner.McpServer.Core

RavenDB.TestRunner.McpServer.Storage.RavenEmbedded
  -> RavenDB.TestRunner.McpServer.Domain
  -> RavenDB.TestRunner.McpServer.Core.Abstractions

RavenDB.TestRunner.McpServer.Semantics.Abstractions
  -> RavenDB.TestRunner.McpServer.Domain

RavenDB.TestRunner.McpServer.Semantics.Raven.V62 / V71 / V72
  -> RavenDB.TestRunner.McpServer.Semantics.Abstractions
  -> RavenDB.TestRunner.McpServer.Domain

RavenDB.TestRunner.McpServer.Build
  -> RavenDB.TestRunner.McpServer.Core.Abstractions
  -> RavenDB.TestRunner.McpServer.Domain
  -> RavenDB.TestRunner.McpServer.Storage.RavenEmbedded

RavenDB.TestRunner.McpServer.TestExecution
  -> RavenDB.TestRunner.McpServer.Build
  -> RavenDB.TestRunner.McpServer.Semantics.Abstractions
  -> RavenDB.TestRunner.McpServer.Storage.RavenEmbedded

RavenDB.TestRunner.McpServer.Results
  -> RavenDB.TestRunner.McpServer.Build
  -> RavenDB.TestRunner.McpServer.TestExecution

RavenDB.TestRunner.McpServer.Flaky
  -> RavenDB.TestRunner.McpServer.Results
  -> RavenDB.TestRunner.McpServer.TestExecution
  -> RavenDB.TestRunner.McpServer.Build

RavenDB.TestRunner.McpServer.Mcp.Host.Http / Stdio
  -> RavenDB.TestRunner.McpServer.Core / Build / TestExecution / Results / Flaky

RavenDB.TestRunner.McpServer.Web.Api
  -> RavenDB.TestRunner.McpServer.Core / Build / TestExecution / Results / Flaky

RavenDB.TestRunner.McpServer.Web.Ui
  -> RavenDB.TestRunner.McpServer.Web.Api contracts + live events
```

## Phase dependencies
- Phase 0 is mandatory before any code
- Phase 1 and Phase 2 may proceed in parallel after Phase 0
- Phase 3 depends on Phase 0 and the storage contract baseline; it should start as soon as Phase 1 has enough registry primitives
- Phase 4 depends on Build + Semantics + Storage
- Phase 5 depends on Build + Test Execution
- Phase 6 and 7 depend on the shared orchestration surfaces from prior phases
- Phase 8 depends on API + event contracts being stable
- Phase 9 depends on persisted attempts/results/builds
- Phase 10 depends on all prior phases

## Work package dependency summary
- `WP_A` gates everything
- `WP_B` and `WP_C` unblock `WP_D` and `WP_E`
- `WP_D` unblocks deterministic build-to-test orchestration
- `WP_E` and `WP_F` unblock `WP_G` and `WP_H`
- `WP_H` unblocks `WP_I`
- `WP_F` and `WP_E` unblock `WP_J`
- `WP_K` is cross-cutting but closes last
