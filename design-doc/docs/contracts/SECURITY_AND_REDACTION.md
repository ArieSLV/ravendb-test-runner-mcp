# Security and Redaction Contract

## Purpose

Define local-security assumptions, sensitive-data handling, and redaction rules.

## v1 trust model

- local-first
- single-user
- localhost-bound services by default
- trusted-local browser operator by default

This does NOT remove the need for secret handling, path validation, or protocol hygiene.

## Sensitive inputs

Sensitive inputs include:
- embedded license values
- repository test license values
- tokens, secrets, keys, passwords
- connection strings that embed credentials
- selected environment variables

## Redaction rules

### Exact keys to redact
- `RAVEN_License`
- `RAVEN_LicensePath` values when they reveal sensitive location details
- `RAVEN_License_Path`
- `RAVEN_LICENSE`
- any configured secret aliases

### Pattern-based redaction
- `*TOKEN*`
- `*SECRET*`
- `*KEY*`
- `*PASSWORD*`
- `*CREDENTIAL*`

### Connection-string redaction
Credential-bearing segments MUST be redacted in previews, logs, and summaries.

## Logging rules

### MCP stdio host
- MUST NOT write logs to stdout
- protocol output only on stdout
- logs go to stderr or file/structured log sink

### Browser/API host
- logs may be structured and file/console based
- sensitive fields must be redacted before emission

## Path safety

- artifact and workspace paths must be normalized
- no path traversal outside permitted roots
- downloads must resolve only to indexed and authorized artifacts

## Browser exposure

- localhost binding by default
- explicit override required for non-loopback binding
- v1 should warn loudly if exposed beyond localhost

## Quarantine and automation auditability

Any automated quarantine or mitigation action MUST record:
- actor
- policy
- reason codes
- timestamp
- rollback path

## Validation requirements

- redaction tests
- stdout purity tests for stdio host
- path traversal tests
- localhost-only default binding tests
- audit record tests for automated actions
