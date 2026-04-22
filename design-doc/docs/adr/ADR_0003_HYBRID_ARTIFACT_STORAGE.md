# ADR 0003 HYBRID ARTIFACT STORAGE

## Context

Large raw artifacts are operationally awkward to store as primary large documents in the metadata store.

## Decision

Use hybrid artifact storage: RavenDB metadata + selected compact attachments; filesystem as canonical store for large raw artifacts.

## Alternatives considered

All artifacts in RavenDB; all artifacts in filesystem with no metadata; object storage from day one.

## Consequences

Balances searchability and operational practicality; requires threshold routing and orphan detection.

## Contract impact

Affects artifact contracts, cleanup, UI previews, and recovery workflows.

## Migration / rollback note

Thresholds can be tuned; attachment policy can be reduced if needed.
