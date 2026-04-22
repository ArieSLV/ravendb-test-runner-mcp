# Error Taxonomy Contract

## Purpose

Provide a stable classification system for failures, warnings, and operator-facing error summaries.

## Design rules

- classify at the highest meaningful layer
- avoid collapsing distinct operational states into generic failure
- separate deterministic conditions from runtime uncertainty
- preserve original error details while exposing normalized categories

## Top-level error classes

### Workspace / configuration
- `workspace_invalid`
- `unsupported_repo_shape`
- `repo_line_unsupported`
- `embedded_license_missing`
- `embedded_license_invalid`
- `artifact_root_unavailable`
- `settings_invalid`

### Toolchain
- `toolchain_unavailable`
- `sdk_mismatch`
- `dotnet_not_found`
- `runner_mode_conflict`

### Build pipeline
- `restore_error`
- `build_error`
- `discovery_error`
- `adapter_error`

### Selection / execution
- `no_tests_matched`
- `all_selected_tests_skipped`
- `test_failures`
- `host_crashed`
- `hung`
- `cancelled`
- `timed_out`

### Data / storage / artifacts
- `artifact_parse_failed`
- `artifact_missing`
- `artifact_orphaned`
- `inconsistent_result_set`
- `event_replay_gap`
- `storage_conflict`

### Flaky workflow
- `iterative_policy_invalid`
- `attempt_escalation_failed`
- `quarantine_apply_failed`

## Deterministic skip reason codes

- `license_missing`
- `nightly_window_unmet`
- `integration_tests_disabled`
- `ai_integration_tests_disabled`
- `platform_mismatch`
- `architecture_mismatch`
- `intrinsics_missing`

These reason codes MUST NOT be treated as flaky by default.

## Severity mapping

Severity enum:
- `info`
- `warning`
- `error`
- `critical`

## Scope mapping

Scope enum:
- `startup`
- `workspace`
- `run`
- `step`
- `attempt`
- `test`
- `artifact`
- `ui`
- `mcp`
- `web-api`

## User-facing rule

Every normalized error should include:
- stable code
- short human summary
- raw-detail availability
- retry guidance if applicable

## Validation requirements

- taxonomy mapping tests
- deterministic skip separation tests
- severity rendering tests
- browser and MCP summary consistency tests
