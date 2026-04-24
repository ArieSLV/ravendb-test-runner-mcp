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
| WP_B collections indexes and optimistic concurrency | integrating-agent | Done | WP_B_002 collections, indexes, revisions-policy decisions, and optimistic concurrency baseline | Storage schema baseline, required indexes, revision decisions, and optimistic concurrency baseline landed; corrective lowerCamel field-casing/static-index semantic validation applied; WP_B_001 lifecycle control and attachments-first policy preserved |
| WP_C v6.2 semantics plugin | worker/James | Done | WP_C_003 v6.2 semantics plugin baseline | Handoff accepted; worker used model `gpt-5.5` with `reasoning_effort=xhigh`; semantics harness passed 6 checks; no external docs used |
| WP_D build graph analyzer | integrating-agent | Done | WP_D_002 deterministic build graph analysis and target enumeration | Deterministic solution/project/directory scope analysis and target enumeration landed; build tests passed 11/11; no actual build execution, process spawning, MCP host, Web API, UI, or test execution subsystem work |

## Open Risks / Blockers
| ID | Status | Owner/Agent | Description | Next Action |
|---|---|---|---|---|
| ENV-001 | Open | integrating-agent | Current shell has `MSBuildSDKsPath` pinned to .NET SDK 8.0.403, causing plain `dotnet build` of the `net10.0` scaffold to fail even though SDK 10.0.203 is installed. Do not mutate the global/user environment while other active work may depend on it. | Use per-command SDK environment override for current validation; address deterministic build environment sanitization in Phase 0 validation/build subsystem planning. |

## Recent Handoffs
| Date | Task ID | Owner/Agent | Result | Handoff Notes |
|---|---|---|---|---|
| 2026-04-23 | WP_C_001_workspace_and_repo_line_detection | worker/Epicurus | Accepted | `design-doc/docs/tasks/WP_C/WP_C_001_workspace_and_repo_line_detection_HANDOFF.md` |
| 2026-04-23 | WP_C_002_semantic_plugin_contracts | worker/Ramanujan | Accepted | `design-doc/docs/tasks/WP_C/WP_C_002_semantic_plugin_contracts_HANDOFF.md` |
| 2026-04-24 | WP_C_003_v62_semantics_plugin | worker/James | Accepted | `design-doc/docs/tasks/WP_C/WP_C_003_v62_semantics_plugin_HANDOFF.md` |

## Task Progress Ledger
| Task ID | Status | Owner/Agent | Last Commit | Validation | Notes |
|---|---|---|---|---|---|
| WP_A_001_solution_scaffold_and_name_freeze | Done | integrating-agent | `42203b3` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with `MSBuildSDKsPath` set to .NET SDK 10.0.203; naming review, cross-link validation, and WP_A_001-scoped contract completeness review passed | Handoff: `docs/tasks/WP_A/WP_A_001_solution_scaffold_and_name_freeze_HANDOFF.md`; no WP_A_002 contract mapping |
| WP_A_002_shared_contracts_project_layout | Done | integrating-agent | `024e8c6` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; contract-document mapping review covered 13/13 contract docs; no runtime dependencies introduced | Handoff: `docs/tasks/WP_A/WP_A_002_shared_contracts_project_layout_HANDOFF.md`; ENV-001 remains open; WP_B/WP_C remain gated |
| WP_A_003_document_id_and_collection_conventions | Done | integrating-agent | `4eff359` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; 20/20 collection names and 20/20 document ID patterns covered; no runtime dependencies introduced; `git diff --check` passed | Handoff: `docs/tasks/WP_A/WP_A_003_document_id_and_collection_conventions_HANDOFF.md`; ENV-001 remains open; WP_B/WP_C remain gated |
| WP_A_004_event_contract_baseline | Done | integrating-agent | `0719fa1` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; event envelope 7/7 fields, 5/5 stream patterns, and 39/39 event types covered; ordering/cursor/replay semantics represented; no runtime dependencies introduced; `git diff --check` passed | Handoff: `docs/tasks/WP_A/WP_A_004_event_contract_baseline_HANDOFF.md`; ENV-001 remains open; WP_B/WP_C remain gated |
| WP_A_005_state_machine_baseline | Done | integrating-agent | `07e1519` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; build lifecycle states 15/15, build result statuses 6/6, build readiness statuses 4/4, run lifecycle states 13/13, attempt lifecycle states 8/8, and quarantine lifecycle states 5/5 covered; build terminal mappings 5/5 represented; build vocabulary separation and optimistic concurrency expectations preserved; no runtime dependencies introduced; `git diff --check` passed | Handoff: `docs/tasks/WP_A/WP_A_005_state_machine_baseline_HANDOFF.md`; ENV-001 remains open; WP_B/WP_C remain gated; carry forward to WP_A_006 review whether `STORAGE_MODEL.md` example ID patterns should mirror all 20 implementation-facing patterns from WP_A_003 |
| WP_A_006_phase0_validation_harness | Done | integrating-agent | `1544a6d` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; Phase 0 validation harness now covers WP_A_001 through WP_A_005 with 5/5 contract-complete checks, 1/1 known non-blocking finding, and 0 blocking drift findings; Phase 0 contract freeze gate is satisfied and explicitly records WP_B/WP_C start criteria; no runtime dependencies introduced; `git diff --check` passed | Handoff: `docs/tasks/WP_A/WP_A_006_phase0_validation_harness_HANDOFF.md`; ENV-001 remains open as non-blocking; STORAGE_MODEL now mirrors the full 20-pattern ID baseline; Phase 0 is complete and WP_B/WP_C may start in later tasks |
| WP_B_001_embedded_bootstrap_and_database_init | Done | integrating-agent | `9653b96` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; `dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj -m:1 -v minimal` succeeded with the same override, including repeated matching bootstrap and conflicting bootstrap rejection regression coverage; `git diff --check` passed | Corrective pass accepted: process-wide `EmbeddedServer.Instance` now has explicit configuration fingerprinting and mismatch rejection; attachments-first v1 storage rule remains intact; handoff updated at `docs/tasks/WP_B/WP_B_001_embedded_bootstrap_and_database_init_HANDOFF.md` |
| WP_B_002_collections_indexes_and_optimistic_concurrency | Done | integrating-agent | `158ea2e` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; `dotnet test .\tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests\RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests.csproj -m:1 -v minimal` succeeded with 7 tests discovered/executed; validation covered embedded startup, required index deployment, lowerCamel storage conventions, static index semantic queries against stored PascalCase probe DTOs for all 8 static indexes, revisions-policy decisions, optimistic concurrency convention and conflict behavior, artifact routing, and WP_B_001 lifecycle preservation; `git diff --check` passed | Corrective field-casing/index-semantics pass accepted; handoff: `docs/tasks/WP_B/WP_B_002_collections_indexes_and_optimistic_concurrency_HANDOFF.md`; no build/test execution, MCP host, Web API, or UI scope introduced; official RavenDB 7.2 index/revisions/optimistic-concurrency docs, official RavenDB 7.x conventions docs, and installed RavenDB 7.2.1 package behavior were used |
| WP_B_003_artifact_metadata_and_attachment_thresholds | Not Started |  |  |  |  |
| WP_B_004_deferred_bulky_diagnostics_and_spillover_guardrails | Not Started |  |  |  |  |
| WP_B_005_event_checkpoint_and_resume_persistence | Not Started |  |  |  |  |
| WP_B_006_restart_recovery_cleanup_and_retention | Not Started |  |  |  |  |
| WP_C_001_workspace_and_repo_line_detection | Done | integrating-agent | `9653b96` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; `dotnet run --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj --no-build` succeeded with the same override and validated 5/5 checks including deterministic truncation and truncated close-score ambiguity; `git diff --check` passed | Corrective pass accepted: bounded scan traversal order is normalized before evidence caps, and truncated scans require stronger score separation before decisive selection; handoff updated at `docs/tasks/WP_C/WP_C_001_workspace_and_repo_line_detection_HANDOFF.md` |
| WP_C_002_semantic_plugin_contracts | Done | worker/Ramanujan | `d5cd0b6` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; `dotnet run --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj --no-build` succeeded with the same override and validated 6/6 checks including result-normalization contract alignment with capability routing; `git diff --check` passed | Handoff accepted: `docs/tasks/WP_C/WP_C_002_semantic_plugin_contracts_HANDOFF.md`; worker used model `gpt-5.5` with `reasoning_effort=xhigh`; no external docs used; no ledger, WP_B, MCP host, Web API, UI, build subsystem, or test execution subsystem files were delegated |
| WP_C_003_v62_semantics_plugin | Done | worker/James | `662f519` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; `dotnet run --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal` succeeded with 6 workspace detection and capability checks; `git diff --check` passed | Handoff accepted: `docs/tasks/WP_C/WP_C_003_v62_semantics_plugin_HANDOFF.md`; worker used model `gpt-5.5` with `reasoning_effort=xhigh`; no external docs used; no ledger, storage/build/MCP/Web/UI, or test execution subsystem files were delegated |
| WP_C_004_v71_semantics_plugin | Not Started |  |  |  |  |
| WP_C_005_v72_semantics_plugin | Not Started |  |  |  |  |
| WP_C_006_catalog_persistence_and_capability_matrix | Not Started |  |  |  |  |
| WP_D_001_build_domain_contracts_and_policies | Done | integrating-agent | `31b4336` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj -m:1 -v minimal --logger "trx;LogFileName=wp-d-001-deferred-artifacts.trx"` succeeded with 8 tests discovered/executed; `git diff --check` passed | Corrective deferred artifact routing coverage accepted; handoff: `docs/tasks/WP_D/WP_D_001_build_domain_contracts_and_policies_HANDOFF.md`; pure build domain/policy baseline only; runtime reuse/fingerprint integration and build artifact/binlog smoke execution remain for later WP_D tasks; no workers used; generated TRX/TestResults artifacts removed |
| WP_D_002_build_graph_analyzer | Done | integrating-agent | `7b62a76` | `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with per-command `MSBuildSDKsPath` set to .NET SDK 10.0.203; `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj -m:1 -v minimal --logger "trx;LogFileName=wp-d-002-build-graph.trx"` succeeded with 11 tests discovered/executed; `git diff --check` passed | Handoff: `docs/tasks/WP_D/WP_D_002_build_graph_analyzer_HANDOFF.md`; deterministic build graph analysis and target enumeration only; reuse/fingerprint persistence remains for WP_D_003; actual build execution/process spawning/binlog smoke remains later WP_D work; generated TRX/TestResults artifacts removed |
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
