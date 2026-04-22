# ADR 0002 RAVENDB EMBEDDED AS METADATA STORE

## Context

The system needs rich metadata/state/history storage and repository-aligned local persistence.

## Decision

Use RavenDB Embedded as the mandatory metadata/state store.

## Alternatives considered

SQLite; flat files only; external shared database.

## Consequences

Rich document model and query capabilities; requires embedded licensing and startup bootstrap.

## Contract impact

Affects storage contracts, packaging, first-run setup, and restart behavior.

## Migration / rollback note

A future pluggable alternate store is possible, but not in v1 baseline.
