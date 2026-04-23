# SECURITY_AND_REDACTION.md

## Purpose
Define local security posture, secret handling, redaction, and host-specific logging rules.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.


## Local trust model
v1 is single-user local-first, but local surfaces are still required to avoid casual abuse and accidental leakage.

## Redacted keys
The implementation MUST redact at minimum:
- `RAVEN_License`
- `RAVEN_LicensePath`
- `RAVEN_License_Path`
- `RAVEN_LICENSE`
- `*TOKEN*`
- `*SECRET*`
- `*KEY*`
- `*PASSWORD*`

## Host rules
### Streamable HTTP host
- bind to localhost by default
- validate origin headers for browser-facing interactions
- keep session IDs and headers out of normal logs where unnecessary

### stdio bridge host
- MUST NOT write non-protocol data to stdout
- MAY write logs to stderr

## Artifact security
- sensitive artifacts MUST be marked as such in metadata
- access in the browser SHOULD be explicit and auditable
- plaintext license material MUST NOT be persisted as a normal log artifact

## Validation requirements
- stdout purity tests for stdio host
- redaction tests for logs, API payloads, and persisted metadata previews
- localhost/origin validation tests
