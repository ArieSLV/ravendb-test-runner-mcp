# TASK_INDEX.md

## Purpose
This index lists the starter task backlog for RavenDB Test Runner MCP Server.

## Usage rules
- pick tasks through the relevant work package
- do not start tasks that violate phase prerequisites
- treat this file as the static backlog, not the mutable status tracker
- update task status in `../../IMPLEMENTATION_PROGRESS.md`

## WP_A
- `WP_A_001_solution_scaffold_and_name_freeze` — Create the solution scaffold and rename the implementation surface to the canonical product/module names.
- `WP_A_002_shared_contracts_project_layout` — Create the shared contracts/package layout and map each contract document to a target project/module.
- `WP_A_003_document_id_and_collection_conventions` — Freeze document ID patterns, collection names, and module ownership tables.
- `WP_A_004_event_contract_baseline` — Freeze the event envelope, ordering rules, cursors, and replay semantics across build and test subsystems.
- `WP_A_005_state_machine_baseline` — Freeze build/run/attempt lifecycle state machines and optimistic concurrency expectations.
- `WP_A_006_phase0_validation_harness` — Create validation checklists and contract approval gates required before any production implementation starts.

## WP_B
- `WP_B_001_embedded_bootstrap_and_database_init` — Bootstrap RavenDB Embedded, database initialization, and mandatory licensed startup checks.
- `WP_B_002_collections_indexes_and_optimistic_concurrency` — Implement collection creation, indexes, revisions policy decisions, and optimistic concurrency baseline.
- `WP_B_003_artifact_metadata_and_attachment_thresholds` — Implement artifact metadata documents and attachment-backed persistence for in-scope v1 artifacts.
- `WP_B_004_deferred_bulky_diagnostics_and_spillover_guardrails` — Define the deferred bulky-diagnostics extension point and explicit out-of-v1-scope spillover guardrails.
- `WP_B_005_event_checkpoint_and_resume_persistence` — Persist event checkpoints and stream resume cursors for build and run streams.
- `WP_B_006_restart_recovery_cleanup_and_retention` — Implement restart recovery, retention metadata, and cleanup job journal design.

## WP_C
- `WP_C_001_workspace_and_repo_line_detection` — Implement workspace detection, branch line routing, and capability discovery for v6.2, v7.1, and v7.2.
- `WP_C_002_semantic_plugin_contracts` — Create semantic plugin interfaces and shared capability routing abstractions.
- `WP_C_003_v62_semantics_plugin` — Implement the v6.2 plugin, including xUnit v2 assumptions and no-AI capability baseline.
- `WP_C_004_v71_semantics_plugin` — Implement the v7.1 plugin, including transitional AI capabilities and xUnit v2-era behavior.
- `WP_C_005_v72_semantics_plugin` — Implement the v7.2 plugin, including xUnit v3-era capabilities and modern test topology.
- `WP_C_006_catalog_persistence_and_capability_matrix` — Persist semantic snapshots, category catalogs, and compatibility matrices in RavenDB Embedded.

## WP_D
- `WP_D_001_build_domain_contracts_and_policies` — Implement build domain contracts, build policy enums, and the explicit build ownership model.
- `WP_D_002_build_graph_analyzer` — Implement build graph analysis for solution/project scopes and deterministic build target enumeration.
- `WP_D_003_build_fingerprint_and_reuse_engine` — Implement build fingerprints, reuse decisions, readiness tokens, and stale-build invalidation logic.
- `WP_D_004_build_scheduler_and_execution_engine` — Implement build scheduler, restore/build/clean/rebuild orchestration, and process supervision.
- `WP_D_005_build_artifacts_status_and_binlog_capture` — Implement binlog/text output capture, build artifacts, live build status, and build result documents.
- `WP_D_006_build_readiness_integration` — Expose build readiness and reuse decisions to the test planning subsystem and browser/MCP surfaces.

## WP_E
- `WP_E_001_selector_normalization_engine` — Implement structured selector normalization and expert-mode raw filter isolation.
- `WP_E_002_preflight_evaluator` — Implement preflight evaluation, deterministic skip prediction, and runtime unknown reporting.
- `WP_E_003_test_run_planner` — Implement run planning with explicit build dependency resolution and artifact path generation.
- `WP_E_004_scheduler_and_process_supervisor` — Implement run scheduling, single-workspace process discipline, cancellation, and timeout handling.
- `WP_E_005_build_to_test_handoff` — Implement explicit build-to-test handoff so test execution never performs chaotic hidden rebuilds.
- `WP_E_006_repro_commands_and_execution_summaries` — Implement exact repro commands and execution summaries for builds and runs.

## WP_F
- `WP_F_001_console_capture_pipeline` — Implement stdout/stderr/merged transcript capture for build and test processes.
- `WP_F_002_trx_junit_and_binlog_harvesting` — Implement TRX/JUnit harvesting for tests and binlog harvesting for builds.
- `WP_F_003_failure_taxonomy_mapper` — Implement canonical failure classifications for build and test execution outcomes.
- `WP_F_004_normalized_result_builder` — Implement normalized build/run result builders and persistence.
- `WP_F_005_diagnostic_hooks_and_blame_artifacts` — Implement diagnostic hooks, blame-style capture, and artifact indexing.
- `WP_F_006_predicted_vs_actual_reconciliation` — Implement reconciliation between predicted preflight outcomes and actual execution outcomes.

## WP_G
- `WP_G_001_mcp_common_handler_layer` — Implement the shared MCP handler layer over the orchestration core.
- `WP_G_002_streamable_http_mcp_host` — Implement the primary local Streamable HTTP MCP host with local-only posture.
- `WP_G_003_stdio_bridge_host` — Implement the optional stdio bridge host with stdout protocol purity.
- `WP_G_004_tests_toolset` — Implement the tests.* MCP tools over the shared core.
- `WP_G_005_build_toolset` — Implement the build.* MCP tools as a first-class sibling surface.
- `WP_G_006_progress_cancellation_and_resume` — Implement MCP progress, cancellation, and resumability-friendly behavior.

## WP_H
- `WP_H_001_query_api_surface` — Implement query APIs for builds, runs, catalogs, capabilities, and settings.
- `WP_H_002_command_api_surface` — Implement command APIs for planning, launching, cancelling, and cleaning builds/runs.
- `WP_H_003_signalr_hub_and_event_mapping` — Implement SignalR hubs and event mapping for build/run/attempt streams.
- `WP_H_004_sse_and_log_cursor_endpoints` — Implement SSE endpoints and cursor-based log playback endpoints.
- `WP_H_005_build_status_and_policy_endpoints` — Implement dedicated build status, build history, and build policy endpoints.
- `WP_H_006_localhost_security_posture` — Implement localhost binding, origin validation, and local browser safety rules.

## WP_I
- `WP_I_001_ui_app_shell_and_design_baseline` — Implement the UI shell and RavenDB Studio-aligned design baseline without hard-coupling to Studio internals.
- `WP_I_002_runs_and_builds_list_details` — Implement combined runs/builds list and detail views with live state.
- `WP_I_003_live_console_results_and_build_output` — Implement live console/output panes, results explorer, and build output inspectors.
- `WP_I_004_artifacts_diagnostics_and_plan_views` — Implement artifact explorer, diagnostics views, and plan inspectors for builds and runs.
- `WP_I_005_flaky_settings_and_policy_views` — Implement flaky analysis views, settings, and build/test policy screens.
- `WP_I_006_accessibility_and_reconnect_behavior` — Implement keyboard navigation, reconnect handling, and degraded-mode UX.

## WP_J
- `WP_J_001_iterative_run_planner` — Implement iterative run planning modes and attempt sequencing.
- `WP_J_002_attempt_lifecycle_and_history_persistence` — Implement attempt lifecycle persistence and historical rollups.
- `WP_J_003_comparison_engine` — Implement attempt/build/run comparison engine and signature drift detection.
- `WP_J_004_stability_classification_and_scoring` — Implement stability signals, classification, and explainable scoring.
- `WP_J_005_quarantine_policy_and_audit_trail` — Implement quarantine actions/proposals, reversibility, and audit trail requirements.
- `WP_J_006_reporting_surfaces_and_notifications` — Implement flaky reporting surfaces for MCP, web API, and browser UI.

## WP_K
- `WP_K_001_unit_and_contract_test_matrix` — Define and implement unit and contract test matrix for all subsystems.
- `WP_K_002_cross_branch_integration_fixtures` — Implement real workspace fixtures for v6.2, v7.1, and v7.2.
- `WP_K_003_ui_and_live_transport_validation` — Implement UI, SignalR, SSE, and reconnect validation suites.
- `WP_K_004_build_subsystem_validation` — Implement build determinism, reuse, and no-chaotic-rebuild validation suites.
- `WP_K_005_packaging_and_startup_smoke` — Implement packaging, startup smoke, and first-run embedded license flow validation.
- `WP_K_006_runbooks_and_operator_docs` — Finalize runbooks, developer setup docs, and operational recovery guidance.
