# ADR 0005 SIGNALR PRIMARY BROWSER TRANSPORT

## Context

The browser UI needs rich live updates, reconnect behavior, and low-friction local integration.

## Decision

Use SignalR as the primary browser live transport, with SSE as a supplementary read-only/fallback channel.

## Alternatives considered

Polling only; raw WebSocket only; SSE only.

## Consequences

Improves live UX and transport flexibility; adds hub contract maintenance.

## Contract impact

Affects event model, web API surface, and UI implementation.

## Migration / rollback note

SSE-only fallback is possible for reduced scope, but would weaken rich live UX.
