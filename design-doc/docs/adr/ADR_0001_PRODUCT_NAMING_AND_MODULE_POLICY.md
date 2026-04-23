# ADR 0001 PRODUCT NAMING AND MODULE POLICY

## Context
Legacy names created ambiguity about whether the product is a control plane, execution pack, or RavenMcp-branded subsystem.

## Decision
Use **RavenDB Test Runner MCP Server** as the canonical product name and `{NS}` as the canonical internal namespace root.

## Alternatives considered
- keep mixed legacy names
- rename only marketing-facing docs
- keep RavenMcp as internal root

## Consequences
- All architecture docs, contracts, work packages, and tasks align around one product identity.
- Contracts and task cards must align with this decision.

## Contract impact
- update affected architecture and contract files when this ADR is introduced or revised
- ensure work package/task references remain synchronized

## Migration / rollback note
Rollback would require coordinated rename migration across the entire pack and any implementation scaffolding.
