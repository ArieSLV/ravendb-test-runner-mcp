# NAMING_AND_MODULE_POLICY.md

## Purpose
Define authoritative product naming, solution/module naming, and retired legacy name handling.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.


## Authoritative rules
1. Product name: **RavenDB Test Runner MCP Server**
2. Short label: **RTRMS**
3. Internal root namespace and project prefix: **RavenDB.TestRunner.McpServer**
4. Legacy names MUST NOT be used for current product/module naming except in historical references.

## Approved module naming pattern
- `RavenDB.TestRunner.McpServer.Core.Abstractions`
- `RavenDB.TestRunner.McpServer.Domain`
- `RavenDB.TestRunner.McpServer.Core`
- `RavenDB.TestRunner.McpServer.Storage.RavenEmbedded`
- `RavenDB.TestRunner.McpServer.Artifacts`
- `RavenDB.TestRunner.McpServer.Semantics.Abstractions`
- `RavenDB.TestRunner.McpServer.Semantics.Raven.V62`
- `RavenDB.TestRunner.McpServer.Semantics.Raven.V71`
- `RavenDB.TestRunner.McpServer.Semantics.Raven.V72`
- `RavenDB.TestRunner.McpServer.Build`
- `RavenDB.TestRunner.McpServer.TestExecution`
- `RavenDB.TestRunner.McpServer.Results`
- `RavenDB.TestRunner.McpServer.Flaky`
- `RavenDB.TestRunner.McpServer.Mcp.Host.Http`
- `RavenDB.TestRunner.McpServer.Mcp.Host.Stdio`
- `RavenDB.TestRunner.McpServer.Web.Api`
- `RavenDB.TestRunner.McpServer.Web.Ui`
- `RavenDB.TestRunner.McpServer.Shared.Contracts`

## Retired names
- `RavenMcp`
- `RavenMcp.*`
- `RavenMcpControlPlane`
- `RavenDB Test MCP Control Plane`
- `RavenMcp Execution Pack`

## UI and packaging names
- Browser title SHOULD use `RavenDB Test Runner MCP Server`
- Packaging SHOULD use a recognizable short form derived from `RTRMS` or the full product name
- MCP server identity strings MUST include the canonical product name

## Validation requirements
- Contract tests MUST verify that generated docs and solution-scaffold names use the approved prefix
- New ADRs and task cards MUST use the canonical product name
