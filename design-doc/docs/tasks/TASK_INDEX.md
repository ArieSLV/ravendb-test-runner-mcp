# Task Index

## Purpose

Track the initial execution backlog. Status defaults to `Planned` until updated by the integrator.

## WP_A

- `WP_A_001` — Create the solution, project folders, Directory.Build props, and placeholder project references. (`Planned`) -> `docs/tasks/WP_A/WP_A_001_solution_scaffold.md`
- `WP_A_002` — Create the shared contracts package and initial DTO/type definitions aligned with frozen contracts. (`Planned`) -> `docs/tasks/WP_A/WP_A_002_contracts_package.md`
- `WP_A_003` — Implement document ID helpers, naming constants, and collection name constants. (`Planned`) -> `docs/tasks/WP_A/WP_A_003_document_and_id_conventions.md`
- `WP_A_004` — Create event envelope types, core event names, and state-machine enums. (`Planned`) -> `docs/tasks/WP_A/WP_A_004_event_contract_baseline.md`
- `WP_A_005` — Create contract test harness and document-validation checks used by later work packages. (`Planned`) -> `docs/tasks/WP_A/WP_A_005_phase0_validation_harness.md`

## WP_B

- `WP_B_001` — Implement RavenDB Embedded startup, license probe order, and database ensure flow. (`Planned`) -> `docs/tasks/WP_B/WP_B_001_embedded_bootstrap_and_database_init.md`
- `WP_B_002` — Create collection conventions, index bootstrap, and optimistic concurrency configuration. (`Planned`) -> `docs/tasks/WP_B/WP_B_002_collections_indexes_and_concurrency.md`
- `WP_B_003` — Implement RunArtifact persistence and artifact metadata registration service. (`Planned`) -> `docs/tasks/WP_B/WP_B_003_artifact_metadata_registration.md`
- `WP_B_004` — Implement canonical filesystem layout manager and path normalization. (`Planned`) -> `docs/tasks/WP_B/WP_B_004_filesystem_artifact_layout.md`
- `WP_B_005` — Implement startup recovery checks, orphan detection, and cleanup journal primitives. (`Planned`) -> `docs/tasks/WP_B/WP_B_005_restart_recovery_and_cleanup_journal.md`

## WP_C

- `WP_C_001` — Implement workspace fingerprinting and repo line detection for v6.2/v7.1/v7.2. (`Planned`) -> `docs/tasks/WP_C/WP_C_001_workspace_line_detection.md`
- `WP_C_002` — Define semantic plugin interfaces and router. (`Planned`) -> `docs/tasks/WP_C/WP_C_002_version_plugin_interfaces.md`
- `WP_C_003` — Implement `RavenV62Semantics` plugin. (`Planned`) -> `docs/tasks/WP_C/WP_C_003_v62_semantics_plugin.md`
- `WP_C_004` — Implement `RavenV71Semantics` plugin. (`Planned`) -> `docs/tasks/WP_C/WP_C_004_v71_semantics_plugin.md`
- `WP_C_005` — Implement `RavenV72Semantics`, category/requirement extraction, and catalog persistence. (`Planned`) -> `docs/tasks/WP_C/WP_C_005_v72_semantics_plugin_and_catalog_persistence.md`

## WP_D

- `WP_D_001` — Implement structured selector normalization and expert-filter wrapper behavior. (`Planned`) -> `docs/tasks/WP_D/WP_D_001_selector_normalization_engine.md`
- `WP_D_002` — Implement preflight readiness, capability checks, and deterministic skip prediction. (`Planned`) -> `docs/tasks/WP_D/WP_D_002_preflight_evaluator.md`
- `WP_D_003` — Implement project-step command synthesis, env overlay, and repro command rendering. (`Planned`) -> `docs/tasks/WP_D/WP_D_003_command_synthesizer.md`
- `WP_D_004` — Implement run queue, one-active-run-per-workspace policy, process supervision, cancellation, and timeout kill behavior. (`Planned`) -> `docs/tasks/WP_D/WP_D_004_scheduler_and_process_supervisor.md`
- `WP_D_005` — Integrate planning, scheduling, and execution into end-to-end run lifecycle. (`Planned`) -> `docs/tasks/WP_D/WP_D_005_planner_execution_integration.md`

## WP_E

- `WP_E_001` — Implement stdout/stderr/merged capture and artifact registration. (`Planned`) -> `docs/tasks/WP_E/WP_E_001_console_capture_pipeline.md`
- `WP_E_002` — Implement TRX ingestion and optional JUnit handling. (`Planned`) -> `docs/tasks/WP_E/WP_E_002_trx_junit_harvesting.md`
- `WP_E_003` — Map raw execution outcomes to normalized failure classifications. (`Planned`) -> `docs/tasks/WP_E/WP_E_003_failure_taxonomy_mapper.md`
- `WP_E_004` — Build canonical `RunResult` and `NormalizedTestResult` outputs. (`Planned`) -> `docs/tasks/WP_E/WP_E_004_normalized_result_builder.md`
- `WP_E_005` — Implement predicted-vs-actual reconciliation and diagnostics escalation metadata hooks. (`Planned`) -> `docs/tasks/WP_E/WP_E_005_predicted_vs_actual_and_diag_hooks.md`

## WP_F

- `WP_F_001` — Implement shared MCP handler orchestration layer independent of transport. (`Planned`) -> `docs/tasks/WP_F/WP_F_001_mcp_common_handler_layer.md`
- `WP_F_002` — Implement the primary local Streamable HTTP MCP host. (`Planned`) -> `docs/tasks/WP_F/WP_F_002_streamable_http_mcp_host.md`
- `WP_F_003` — Implement the stdio compatibility host with strict stdout hygiene. (`Planned`) -> `docs/tasks/WP_F/WP_F_003_stdio_bridge_host.md`
- `WP_F_004` — Implement projects/categories/capabilities/discover/preflight/plan tools. (`Planned`) -> `docs/tasks/WP_F/WP_F_004_tool_set_core_a.md`
- `WP_F_005` — Implement run/status/output/results/cancel/rerun/explain/repro/iterative/flaky tools. (`Planned`) -> `docs/tasks/WP_F/WP_F_005_tool_set_core_b.md`

## WP_G

- `WP_G_001` — Implement browser query endpoints for workspaces, runs, results, artifacts, and capabilities. (`Planned`) -> `docs/tasks/WP_G/WP_G_001_query_api_surface.md`
- `WP_G_002` — Implement write endpoints for planning, running, cancellation, reruns, and quarantine actions. (`Planned`) -> `docs/tasks/WP_G/WP_G_002_command_api_surface.md`
- `WP_G_003` — Implement SignalR hub and mapping from internal events to browser events. (`Planned`) -> `docs/tasks/WP_G/WP_G_003_signalr_hub_and_event_mapping.md`
- `WP_G_004` — Implement SSE endpoints and cursor-based log access. (`Planned`) -> `docs/tasks/WP_G/WP_G_004_sse_and_log_cursor_endpoints.md`
- `WP_G_005` — Implement localhost-only defaults, trusted-local auth baseline, and browser security headers as applicable. (`Planned`) -> `docs/tasks/WP_G/WP_G_005_localhost_posture_and_browser_security.md`

## WP_H

- `WP_H_001` — Implement SPA shell, routing, layout baseline, and global stores. (`Planned`) -> `docs/tasks/WP_H/WP_H_001_ui_app_shell_and_routing.md`
- `WP_H_002` — Implement runs list and run details pages. (`Planned`) -> `docs/tasks/WP_H/WP_H_002_runs_list_and_details.md`
- `WP_H_003` — Implement live console, cursor consumption, and results explorer. (`Planned`) -> `docs/tasks/WP_H/WP_H_003_live_console_and_results_explorer.md`
- `WP_H_004` — Implement artifact explorer and diagnostics pages. (`Planned`) -> `docs/tasks/WP_H/WP_H_004_artifacts_and_diagnostics_views.md`
- `WP_H_005` — Implement flaky analysis page, settings/profiles page, and Studio-aligned styling baseline. (`Planned`) -> `docs/tasks/WP_H/WP_H_005_flaky_and_settings_views.md`

## WP_I

- `WP_I_001` — Implement iterative run request handling and attempt plan generation. (`Planned`) -> `docs/tasks/WP_I/WP_I_001_iterative_run_planner.md`
- `WP_I_002` — Implement attempt persistence, state transitions, and attempt event publication. (`Planned`) -> `docs/tasks/WP_I/WP_I_002_attempt_lifecycle_persistence.md`
- `WP_I_003` — Implement attempt comparison, signature diffing, and duration/env diffing. (`Planned`) -> `docs/tasks/WP_I/WP_I_003_comparison_engine.md`
- `WP_I_004` — Implement stability signals, scoring, and quarantine proposal/accept/revoke workflow. (`Planned`) -> `docs/tasks/WP_I/WP_I_004_classification_and_quarantine.md`
- `WP_I_005` — Implement flaky history, stability report, MCP surfaces, and UI model integration. (`Planned`) -> `docs/tasks/WP_I/WP_I_005_history_and_reporting_surfaces.md`

## WP_J

- `WP_J_001` — Create unit and contract test matrix across shared packages. (`Planned`) -> `docs/tasks/WP_J/WP_J_001_unit_and_contract_test_matrix.md`
- `WP_J_002` — Implement integration fixtures for v6.2, v7.1, and v7.2 workspaces. (`Planned`) -> `docs/tasks/WP_J/WP_J_002_integration_workspace_fixtures.md`
- `WP_J_003` — Implement browser UI smoke tests, reconnect tests, and log-stream tests. (`Planned`) -> `docs/tasks/WP_J/WP_J_003_ui_and_live_transport_validation.md`
- `WP_J_004` — Implement packaging scripts and cold-start smoke validation for the stand-alone app. (`Planned`) -> `docs/tasks/WP_J/WP_J_004_packaging_and_startup_smoke.md`
- `WP_J_005` — Write developer/operator runbooks, troubleshooting, and upgrade notes. (`Planned`) -> `docs/tasks/WP_J/WP_J_005_runbooks_and_operator_docs.md`
