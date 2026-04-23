# ADR 0002 STANDALONE APP AND MCP SURFACES

## Context
The product must survive unstable child-process lifecycles in external AI hosts.

## Decision
Use a local stand-alone application with a primary Streamable HTTP MCP host and an optional stdio bridge host.

## Alternatives considered
- stdio-only MCP server
- remote-hosted service first

## Consequences
- Lifecycle is normalized by the server itself; stdio remains compatibility-only.
- Contracts and task cards must align with this decision.

## Contract impact
- update affected architecture and contract files when this ADR is introduced or revised
- ensure work package/task references remain synchronized

## Migration / rollback note
Rollback to stdio-only would reintroduce lifecycle coupling.
