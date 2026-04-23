# IMPLEMENTATION_PROGRESS.md

## Purpose
Track implementation progress for RavenDB Test Runner MCP Server.

`TASK_INDEX.md` is the static backlog. This file is the mutable implementation ledger.

## Status Values
- `Not Started` - no implementation work has begun.
- `In Progress` - work is actively assigned or being edited.
- `Partial` - some work landed, but the task is not definition-of-done complete.
- `Done` - task is complete and validated or validation is explicitly recorded.
- `Blocked` - work cannot continue without a decision, dependency, or external input.
- `Deferred` - task is intentionally postponed by integrator decision or ADR.

## Update Rules
- Before starting a task, set its status to `In Progress` and fill `Owner/Agent`.
- If work lands but does not meet the task definition of done, set status to `Partial`.
- When a task is complete, set status to `Done`, record validation, and include the last relevant commit.
- If blocked, set status to `Blocked` and explain the blocker in `Notes`.
- Every worker handoff must be reflected here before the integrator considers the handoff accepted.
- This file is the source of truth for "what is done, partial, blocked, or not started".

## Active Workstreams
| Workstream | Owner/Agent | Status | Scope | Notes |
|---|---|---|---|---|
| WP_A shared contracts layout | integrating-agent | Done | WP_A_002 shared contracts project layout | Phase 0 only; WP_B/WP_C remain gated until WP_A_001-WP_A_006 are done or explicitly deferred |

## Open Risks / Blockers
| ID | Status | Owner/Agent | Description | Next Action |
|---|---|---|---|---|
| ENV-001 | Open | integrating-agent | Current shell has `MSBuildSDKsPath` pinned to .NET SDK 8.0.403, causing plain `dotnet build` of the `net10.0` scaffold to fail even though SDK 10.0.203 is installed. Do not mutate the global/user environment while other active work may depend on it. | Use per-command SDK environment override for current validation; address deterministic build environment sanitization in Phase 0 validation/build subsystem planning. |

## Recent Handoffs
| Date | Task ID | Owner/Agent | Result | Handoff Notes |
|---|---|---|---|---|
| None |  |  |  |  |

## Task Progress Ledger
| Task ID | Status | Owner/Agent | Last Commit | Validation | Notes |
|---|---|---|---|---|---|
| WP_A_001_solution_scaffold_and_name_freeze | Done | integrating-agent | `42203b3` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with `MSBuildSDKsPath` set to .NET SDK 10.0.203; naming review, cross-link validation, and WP_A_001-scoped contract completeness review passed | Handoff: `docs/tasks/WP_A/WP_A_001_solution_scaffold_and_name_freeze_HANDOFF.md`; no WP_A_002 contract mapping |
| WP_A_002_shared_contracts_project_layout | Done | integrating-agent | N/A (pending commit) | `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; contract-document mapping review covered 13/13 contract docs; no runtime dependencies introduced | Handoff: `docs/tasks/WP_A/WP_A_002_shared_contracts_project_layout_HANDOFF.md`; ENV-001 remains open; WP_B/WP_C remain gated |
| WP_A_003_document_id_and_collection_conventions | Not Started |  |  |  |  |
| WP_A_004_event_contract_baseline | Not Started |  |  |  |  |
| WP_A_005_state_machine_baseline | Not Started |  |  |  |  |
| WP_A_006_phase0_validation_harness | Not Started |  |  |  |  |
| WP_B_001_embedded_bootstrap_and_database_init | Not Started |  |  |  |  |
| WP_B_002_collections_indexes_and_optimistic_concurrency | Not Started |  |  |  |  |
| WP_B_003_artifact_metadata_and_attachment_thresholds | Not Started |  |  |  |  |
| WP_B_004_deferred_bulky_diagnostics_and_spillover_guardrails | Not Started |  |  |  |  |
| WP_B_005_event_checkpoint_and_resume_persistence | Not Started |  |  |  |  |
| WP_B_006_restart_recovery_cleanup_and_retention | Not Started |  |  |  |  |
| WP_C_001_workspace_and_repo_line_detection | Not Started |  |  |  |  |
| WP_C_002_semantic_plugin_contracts | Not Started |  |  |  |  |
| WP_C_003_v62_semantics_plugin | Not Started |  |  |  |  |
| WP_C_004_v71_semantics_plugin | Not Started |  |  |  |  |
| WP_C_005_v72_semantics_plugin | Not Started |  |  |  |  |
| WP_C_006_catalog_persistence_and_capability_matrix | Not Started |  |  |  |  |
| WP_D_001_build_domain_contracts_and_policies | Not Started |  |  |  |  |
| WP_D_002_build_graph_analyzer | Not Started |  |  |  |  |
| WP_D_003_build_fingerprint_and_reuse_engine | Not Started |  |  |  |  |
| WP_D_004_build_scheduler_and_execution_engine | Not Started |  |  |  |  |
| WP_D_005_build_artifacts_status_and_binlog_capture | Not Started |  |  |  |  |
| WP_D_006_build_readiness_integration | Not Started |  |  |  |  |
| WP_E_001_selector_normalization_engine | Not Started |  |  |  |  |
| WP_E_002_preflight_evaluator | Not Started |  |  |  |  |
| WP_E_003_test_run_planner | Not Started |  |  |  |  |
| WP_E_004_scheduler_and_process_supervisor | Not Started |  |  |  |  |
| WP_E_005_build_to_test_handoff | Not Started |  |  |  |  |
| WP_E_006_repro_commands_and_execution_summaries | Not Started |  |  |  |  |
| WP_F_001_console_capture_pipeline | Not Started |  |  |  |  |
| WP_F_002_trx_junit_and_binlog_harvesting | Not Started |  |  |  |  |
| WP_F_003_failure_taxonomy_mapper | Not Started |  |  |  |  |
| WP_F_004_normalized_result_builder | Not Started |  |  |  |  |
| WP_F_005_diagnostic_hooks_and_blame_artifacts | Not Started |  |  |  |  |
| WP_F_006_predicted_vs_actual_reconciliation | Not Started |  |  |  |  |
| WP_G_001_mcp_common_handler_layer | Not Started |  |  |  |  |
| WP_G_002_streamable_http_mcp_host | Not Started |  |  |  |  |
| WP_G_003_stdio_bridge_host | Not Started |  |  |  |  |
| WP_G_004_tests_toolset | Not Started |  |  |  |  |
| WP_G_005_build_toolset | Not Started |  |  |  |  |
| WP_G_006_progress_cancellation_and_resume | Not Started |  |  |  |  |
| WP_H_001_query_api_surface | Not Started |  |  |  |  |
| WP_H_002_command_api_surface | Not Started |  |  |  |  |
| WP_H_003_signalr_hub_and_event_mapping | Not Started |  |  |  |  |
| WP_H_004_sse_and_log_cursor_endpoints | Not Started |  |  |  |  |
| WP_H_005_build_status_and_policy_endpoints | Not Started |  |  |  |  |
| WP_H_006_localhost_security_posture | Not Started |  |  |  |  |
| WP_I_001_ui_app_shell_and_design_baseline | Not Started |  |  |  |  |
| WP_I_002_runs_and_builds_list_details | Not Started |  |  |  |  |
| WP_I_003_live_console_results_and_build_output | Not Started |  |  |  |  |
| WP_I_004_artifacts_diagnostics_and_plan_views | Not Started |  |  |  |  |
| WP_I_005_flaky_settings_and_policy_views | Not Started |  |  |  |  |
| WP_I_006_accessibility_and_reconnect_behavior | Not Started |  |  |  |  |
| WP_J_001_iterative_run_planner | Not Started |  |  |  |  |
| WP_J_002_attempt_lifecycle_and_history_persistence | Not Started |  |  |  |  |
| WP_J_003_comparison_engine | Not Started |  |  |  |  |
| WP_J_004_stability_classification_and_scoring | Not Started |  |  |  |  |
| WP_J_005_quarantine_policy_and_audit_trail | Not Started |  |  |  |  |
| WP_J_006_reporting_surfaces_and_notifications | Not Started |  |  |  |  |
| WP_K_001_unit_and_contract_test_matrix | Not Started |  |  |  |  |
| WP_K_002_cross_branch_integration_fixtures | Not Started |  |  |  |  |
| WP_K_003_ui_and_live_transport_validation | Not Started |  |  |  |  |
| WP_K_004_build_subsystem_validation | Not Started |  |  |  |  |
| WP_K_005_packaging_and_startup_smoke | Not Started |  |  |  |  |
| WP_K_006_runbooks_and_operator_docs | Not Started |  |  |  |  |
