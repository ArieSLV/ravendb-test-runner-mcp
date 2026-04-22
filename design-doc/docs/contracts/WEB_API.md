# Web API Contract

## Purpose

Define the browser-facing API and live communication model.

## Architectural boundary

The browser-facing API is NOT the MCP API.
Both are peers over the shared orchestration core.

## Auth posture for v1

- single-user trusted-local mode
- localhost-only binding by default
- no mandatory enterprise auth in v1
- dangerous operations still require explicit user action in UI

## API surface categories

### Query endpoints

- `GET /api/workspaces`
- `GET /api/workspaces/{workspaceId}`
- `GET /api/workspaces/{workspaceId}/projects`
- `GET /api/workspaces/{workspaceId}/categories`
- `GET /api/workspaces/{workspaceId}/capabilities`
- `GET /api/runs`
- `GET /api/runs/{runId}`
- `GET /api/runs/{runId}/results`
- `GET /api/runs/{runId}/artifacts`
- `GET /api/runs/{runId}/attempts`
- `GET /api/runs/{runId}/logs/{stream}`
- `GET /api/flaky/{testId}/history`
- `GET /api/settings`
- `GET /api/profiles`

### Command endpoints

- `POST /api/runs/plan`
- `POST /api/runs`
- `POST /api/runs/{runId}/cancel`
- `POST /api/runs/{runId}/rerun-failed`
- `POST /api/runs/iterative`
- `POST /api/flaky/analyze`
- `POST /api/quarantine/{testId}/propose`
- `POST /api/quarantine/{testId}/accept`
- `POST /api/quarantine/{testId}/revoke`
- `POST /api/settings`
- `POST /api/profiles`

## Live delivery

### SignalR hub

Primary hub:
- `/hubs/runs`

Hub responsibilities:
- run lifecycle updates
- step output
- result observation
- artifact availability
- attempt updates
- flaky analysis completion

### SSE endpoints

Optional supplementary endpoints:
- `GET /api/runs/{runId}/events`
- `GET /api/runs/{runId}/logs/{stream}/events`

## Log cursor contract

Log requests MUST support:
- stream selection: `stdout|stderr|merged`
- cursor-based continuation
- bounded page size
- reverse navigation if implemented
- truncation indicator

Response shape:
- `cursor`
- `lines[]`
- `hasMore`
- `truncated`

## Browser-facing request/response rules

- browser endpoints may aggregate multiple domain entities into view models
- browser responses MUST remain compatible with `FRONTEND_VIEW_MODELS.md`
- browser APIs may expose richer summaries than MCP tools
- browser APIs must not bypass the shared authorization, redaction, or retention logic

## Localhost and exposure rules

The web host MUST:
- bind to localhost by default
- not expose broad LAN access by accident
- emit the effective binding in startup diagnostics
- allow explicit override only via configuration

## Validation requirements

- API contract tests
- localhost binding tests
- SignalR reconnect tests
- SSE stream tests
- cursor/log paging tests
- browser/model mapping tests
