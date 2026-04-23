# ARTIFACTS_AND_RETENTION.md

## Purpose
Define artifact classes, retention classes, attachment-backed v1 storage expectations, and deferred handling for oversized diagnostics.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.

## Authoritative v1 policy
For artifact classes that are in scope for v1, the expected storage model is **RavenDB attachment-backed persistence**.

The current pack does not require a default filesystem spillover path for v1 build/test artifacts. Bulky binary diagnostics and oversized diagnostic bundles are deferred unless explicitly introduced later.

## Artifact classes
### Build artifacts
- `build.command`
- `build.stdout`
- `build.stderr`
- `build.merged`
- `build.binlog`
- `build.output_manifest`
- `build.summary`
- `build.diagnostics.compact`

### Test/run artifacts
- `run.command`
- `run.stdout`
- `run.stderr`
- `run.merged`
- `run.trx`
- `run.junit`
- `run.summary`
- `run.normalized_result`
- `run.diagnostics.compact`

### Attempt/flaky artifacts
- `attempt.summary`
- `attempt.diff`
- `flaky.analysis`
- `quarantine.audit`

### Deferred bulky diagnostics (not mandatory in v1)
- `build.dump`
- `build.diagnostics.oversized`
- `run.dump`
- `run.blame_bundle`
- `run.diagnostics.oversized`

## Retention classes
- `ephemeral`
- `standard`
- `diagnostic`
- `compliance`
- `manual-hold`

## Storage expectation per artifact class
| Artifact class family | Default v1 storage | Notes |
|---|---|---|
| build/test commands and summaries | `raven_attachment` | authoritative in v1 |
| stdout/stderr/merged logs | `raven_attachment` | subject to practical attachment guardrails |
| `trx` / `junit` / `binlog` | `raven_attachment` | expected in v1 when enabled |
| compact diagnostics | `raven_attachment` | expected in v1 |
| dumps / bulky blame bundles / oversized diagnostics | deferred | not mandatory in v1 |

## Practical attachment guardrail
The implementation MAY enforce a configurable practical attachment guardrail for v1. When an artifact exceeds that guardrail:
- it MUST NOT silently become a filesystem-owned default v1 artifact,
- it MUST either remain unsupported / deferred,
- or be handled only if a later ADR explicitly adds a non-attachment extension path.

## Cleanup rules
- Never delete artifacts referenced by active builds/runs/attempts.
- Cleanup MUST work against attachment-backed payloads and `ArtifactRef` metadata.
- Cleanup MUST emit audit events.
- Deferred artifact classes MUST NOT appear in v1 cleanup assumptions unless the corresponding extension is formally enabled.

## Preview policy
The system SHOULD store preview metadata for attachment-backed artifacts to allow UI rendering without loading the whole payload when not necessary.

## Validation requirements
- Retention cleanup tests MUST cover build and run attachments.
- Manual-hold artifacts MUST survive automated cleanup.
- Deferred bulky diagnostics MUST be classified as deferred or unsupported rather than silently materialized outside the normative v1 policy.
