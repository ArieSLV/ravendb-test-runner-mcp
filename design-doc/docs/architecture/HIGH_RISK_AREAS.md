# High-Risk Areas

## 1. Contract drift

Risk:
- multiple agents change shared DTOs, event payloads, or document schemas concurrently

Mitigation:
- Phase 0 freeze
- integrator review
- ADR discipline
- contract tests

## 2. Version plugin leakage

Risk:
- branch-specific semantics leak into shared orchestration core

Mitigation:
- explicit semantic plugin interfaces
- capability-based branching
- cross-branch tests

## 3. Embedded startup and licensing

Risk:
- application startup blocked by missing or invalid embedded license
- poor user experience during first-run

Mitigation:
- explicit setup-required state
- deterministic probe order
- non-plaintext logging
- setup flow validation

## 4. Artifact growth and retention

Risk:
- unbounded large artifacts on developer machines
- retention inconsistency between RavenDB metadata and filesystem payloads

Mitigation:
- retention classes
- cleanup job
- size thresholds
- restart recovery tests

## 5. Live event consistency

Risk:
- browser UI shows stale or inconsistent state after reconnect
- MCP progress and browser events diverge

Mitigation:
- authoritative run registry
- replayable event log
- cursor-based log access
- state rehydrate endpoints

## 6. Flaky false positives

Risk:
- deterministic skip/failure conditions incorrectly classified as flaky

Mitigation:
- explicit deterministic skip taxonomy
- environment fingerprint freeze
- score explanation rules
- reviewable quarantine workflow
