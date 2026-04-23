# ADR 0007 SIGNALR PRIMARY BROWSER TRANSPORT

## Context
The browser UI needs rich live updates, reconnect behavior, and multiple event families.

## Decision
Use SignalR as the primary browser live transport and SSE as a supplementary read-only transport.

## Alternatives considered
- SSE-only
- raw WebSocket only

## Consequences
- The UI gets a richer and more ergonomic live model while preserving SSE for simple streams.
- Contracts and task cards must align with this decision.

## Contract impact
- update affected architecture and contract files when this ADR is introduced or revised
- ensure work package/task references remain synchronized

## Migration / rollback note
Rollback to SSE-only would reduce bidirectional/eventing flexibility.
