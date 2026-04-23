# WEB_API.md

## Purpose
Define browser-facing HTTP APIs, SignalR hubs, and SSE/read-only stream endpoints for RavenDB Test Runner MCP Server.

## Scope
This file is normative for browser-facing surfaces. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Surface split
Browser-facing APIs are distinct from MCP APIs.

- MCP is for AI-agent integration.
- Browser-facing APIs are for the operator UI.
- Both surfaces project the same underlying authoritative registry and event model.

## Query endpoints
### Workspace and catalog
- `GET /api/workspaces`
- `GET /api/workspaces/{workspaceId}/capabilities`
- `GET /api/workspaces/{workspaceId}/projects`
- `GET /api/workspaces/{workspaceId}/categories`

### Build queries
- `GET /api/builds`
- `GET /api/builds/{buildId}`
- `GET /api/builds/{buildId}/results`
- `GET /api/builds/{buildId}/artifacts`
- `GET /api/builds/{buildId}/logs/{stream}?cursor=...`
- `GET /api/builds/{buildId}/readiness`
- `GET /api/builds/{buildId}/plan`

### Run queries
- `GET /api/runs`
- `GET /api/runs/{runId}`
- `GET /api/runs/{runId}/results`
- `GET /api/runs/{runId}/attempts`
- `GET /api/runs/{runId}/artifacts`
- `GET /api/runs/{runId}/logs/{stream}?cursor=...`
- `GET /api/runs/{runId}/plan`

### Flaky and settings
- `GET /api/flaky/{testId}/history`
- `GET /api/quarantine/actions/{actionId}`
- `GET /api/settings`

## Command endpoints
### Build commands
- `POST /api/builds/plan`
- `POST /api/builds`
- `POST /api/builds/{buildId}/cancel`
- `POST /api/builds/{buildId}/clean`
- `POST /api/builds/{buildId}/invalidate-readiness`

### Run commands
- `POST /api/runs/plan`
- `POST /api/runs`
- `POST /api/runs/{runId}/cancel`
- `POST /api/runs/{runId}/rerun-failed`
- `POST /api/runs/iterative`

### Flaky and settings commands
- `POST /api/flaky/analyze`
- `POST /api/quarantine/actions`
- `POST /api/settings/profiles`

## Live transports
### SignalR hubs
- `/hubs/builds`
- `/hubs/runs`
- `/hubs/flaky`

SignalR is the primary browser live transport.

### SSE endpoints
- `GET /api/builds/{buildId}/events`
- `GET /api/runs/{runId}/events`
- `GET /api/builds/{buildId}/logs/{stream}/events`
- `GET /api/runs/{runId}/logs/{stream}/events`

SSE is supplementary and useful for read-only streams and cursor replay.

## Localhost security
- bind to localhost by default
- validate browser origin for local UI host
- distinguish browser session/auth from MCP session/auth behavior

## Validation requirements
- endpoint contract tests
- SignalR/SSE parity tests for event shapes
- log cursor tests
- localhost/origin validation tests
- build and run surface consistency tests
