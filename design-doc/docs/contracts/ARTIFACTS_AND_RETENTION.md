# ARTIFACTS_AND_RETENTION.md

## Purpose
Define artifact kinds, retention classes, storage thresholds, and cleanup expectations for build and test workflows.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.


## Artifact classes
### Build artifacts
- `build.command`
- `build.stdout`
- `build.stderr`
- `build.merged`
- `build.binlog`
- `build.output_manifest`
- `build.diagnostics`
- `build.summary`

### Test/run artifacts
- `run.command`
- `run.stdout`
- `run.stderr`
- `run.merged`
- `run.trx`
- `run.junit`
- `run.diag`
- `run.blame`
- `run.summary`

### Attempt/flaky artifacts
- `attempt.summary`
- `attempt.diff`
- `flaky.analysis`
- `quarantine.audit`

## Retention classes
- `ephemeral`
- `standard`
- `diagnostic`
- `compliance`
- `manual-hold`

## Threshold policy
The attachment threshold MUST be configurable. The default policy SHOULD be conservative: compact artifacts may be stored as RavenDB attachments; large transcripts and binary diagnostics go to the filesystem.

## Cleanup rules
- Never delete artifacts referenced by active builds/runs.
- Prefer tombstoning metadata before deleting bulky filesystem artifacts.
- Cleanup MUST emit audit events.

## Preview policy
The system SHOULD store preview metadata for large artifacts to allow UI rendering without loading the whole file.

## Validation requirements
- Retention cleanup tests MUST cover build and run artifacts.
- Manual-hold artifacts MUST survive automated cleanup.
