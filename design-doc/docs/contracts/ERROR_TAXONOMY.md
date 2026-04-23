# ERROR_TAXONOMY.md

## Purpose
Define canonical failure classifications for build, test, artifact, and policy workflows.

## Scope
This file is normative for the bounded area described below. If implementation notes elsewhere conflict with this file, this file wins unless an ADR explicitly supersedes it.


## Build classifications
- `build_graph_error`
- `restore_error`
- `build_error`
- `build_outputs_missing`
- `build_reuse_rejected`
- `build_cancelled`
- `build_timed_out`
- `build_artifact_parse_error`

## Test/run classifications
- `workspace_invalid`
- `unsupported_repo_shape`
- `toolchain_unavailable`
- `no_tests_matched`
- `all_selected_tests_skipped`
- `discovery_error`
- `adapter_error`
- `test_failures`
- `host_crashed`
- `run_cancelled`
- `run_timed_out`
- `inconsistent_result_set`

## Flaky / quarantine classifications
- `deterministic_failure`
- `deterministic_skip`
- `likely_environment_issue`
- `likely_infra_issue`
- `selector_instability`
- `likely_flaky`
- `confirmed_flaky`
- `quarantine_policy_blocked`

## Rule
Deterministic build/test/license/platform failures MUST NOT be reported as flaky.

## Validation requirements
- mapping tests from raw outcomes to canonical taxonomy
- false-positive suppression tests for deterministic skips/failures
