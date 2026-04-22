# EXECUTION_PACK_TREE

```text
├── AGENTS.md
├── EXECUTION_PACK_TREE.md
├── README.md
├── docs
│   ├── adr
│   │   ├── ADR_0001_STANDALONE_APP_AND_MCP_SURFACES.md
│   │   ├── ADR_0002_RAVENDB_EMBEDDED_AS_METADATA_STORE.md
│   │   ├── ADR_0003_HYBRID_ARTIFACT_STORAGE.md
│   │   ├── ADR_0004_VERSION_PLUGIN_MODEL.md
│   │   ├── ADR_0005_SIGNALR_PRIMARY_BROWSER_TRANSPORT.md
│   │   ├── ADR_0006_SINGLE_USER_LOCAL_FIRST_V1.md
│   │   └── ADR_0007_FLAKY_AUTOMATION_AND_QUARANTINE_POLICY.md
│   ├── architecture
│   │   ├── DECISION_FREEZE.md
│   │   ├── DEPENDENCY_GRAPH.md
│   │   ├── EXECUTION_PACK_INDEX.md
│   │   ├── FIRST_10_TASKS_TO_EXECUTE.md
│   │   ├── HIGH_RISK_AREAS.md
│   │   ├── IMPLEMENTATION_ORDER_SUMMARY.md
│   │   ├── IMPLEMENTATION_SPEC.md
│   │   ├── MAIN_OPEN_QUESTIONS.md
│   │   └── PARALLELIZATION_STRATEGY.md
│   ├── contracts
│   │   ├── ARTIFACTS_AND_RETENTION.md
│   │   ├── DOMAIN_MODEL.md
│   │   ├── ERROR_TAXONOMY.md
│   │   ├── EVENT_MODEL.md
│   │   ├── FRONTEND_VIEW_MODELS.md
│   │   ├── MCP_TOOLS.md
│   │   ├── SECURITY_AND_REDACTION.md
│   │   ├── STATE_MACHINES.md
│   │   ├── STORAGE_MODEL.md
│   │   ├── VERSIONING_AND_CAPABILITIES.md
│   │   └── WEB_API.md
│   ├── phases
│   │   ├── PHASE_0_CONTRACT_FREEZE.md
│   │   ├── PHASE_1_STORAGE_AND_REGISTRY.md
│   │   ├── PHASE_2_SEMANTICS_AND_CATALOG.md
│   │   ├── PHASE_3_PLANNING_AND_EXECUTION.md
│   │   ├── PHASE_4_RESULTS_AND_DIAGNOSTICS.md
│   │   ├── PHASE_5_MCP_SURFACE.md
│   │   ├── PHASE_6_WEB_API_AND_LIVE_EVENTS.md
│   │   ├── PHASE_7_FRONTEND_UI.md
│   │   ├── PHASE_8_FLAKY_SUBSYSTEM.md
│   │   └── PHASE_9_VALIDATION_AND_PACKAGING.md
│   ├── runbooks
│   │   ├── DEVELOPER_SETUP.md
│   │   ├── OPERATOR_RUNBOOK.md
│   │   └── README.md
│   ├── tasks
│   │   ├── HANDOFF_TEMPLATE.md
│   │   ├── TASK_INDEX.md
│   │   ├── TASK_TEMPLATE.md
│   │   ├── WP_A
│   │   │   ├── WP_A_001_solution_scaffold.md
│   │   │   ├── WP_A_002_contracts_package.md
│   │   │   ├── WP_A_003_document_and_id_conventions.md
│   │   │   ├── WP_A_004_event_contract_baseline.md
│   │   │   └── WP_A_005_phase0_validation_harness.md
│   │   ├── WP_B
│   │   │   ├── WP_B_001_embedded_bootstrap_and_database_init.md
│   │   │   ├── WP_B_002_collections_indexes_and_concurrency.md
│   │   │   ├── WP_B_003_artifact_metadata_registration.md
│   │   │   ├── WP_B_004_filesystem_artifact_layout.md
│   │   │   └── WP_B_005_restart_recovery_and_cleanup_journal.md
│   │   ├── WP_C
│   │   │   ├── WP_C_001_workspace_line_detection.md
│   │   │   ├── WP_C_002_version_plugin_interfaces.md
│   │   │   ├── WP_C_003_v62_semantics_plugin.md
│   │   │   ├── WP_C_004_v71_semantics_plugin.md
│   │   │   └── WP_C_005_v72_semantics_plugin_and_catalog_persistence.md
│   │   ├── WP_D
│   │   │   ├── WP_D_001_selector_normalization_engine.md
│   │   │   ├── WP_D_002_preflight_evaluator.md
│   │   │   ├── WP_D_003_command_synthesizer.md
│   │   │   ├── WP_D_004_scheduler_and_process_supervisor.md
│   │   │   └── WP_D_005_planner_execution_integration.md
│   │   ├── WP_E
│   │   │   ├── WP_E_001_console_capture_pipeline.md
│   │   │   ├── WP_E_002_trx_junit_harvesting.md
│   │   │   ├── WP_E_003_failure_taxonomy_mapper.md
│   │   │   ├── WP_E_004_normalized_result_builder.md
│   │   │   └── WP_E_005_predicted_vs_actual_and_diag_hooks.md
│   │   ├── WP_F
│   │   │   ├── WP_F_001_mcp_common_handler_layer.md
│   │   │   ├── WP_F_002_streamable_http_mcp_host.md
│   │   │   ├── WP_F_003_stdio_bridge_host.md
│   │   │   ├── WP_F_004_tool_set_core_a.md
│   │   │   └── WP_F_005_tool_set_core_b.md
│   │   ├── WP_G
│   │   │   ├── WP_G_001_query_api_surface.md
│   │   │   ├── WP_G_002_command_api_surface.md
│   │   │   ├── WP_G_003_signalr_hub_and_event_mapping.md
│   │   │   ├── WP_G_004_sse_and_log_cursor_endpoints.md
│   │   │   └── WP_G_005_localhost_posture_and_browser_security.md
│   │   ├── WP_H
│   │   │   ├── WP_H_001_ui_app_shell_and_routing.md
│   │   │   ├── WP_H_002_runs_list_and_details.md
│   │   │   ├── WP_H_003_live_console_and_results_explorer.md
│   │   │   ├── WP_H_004_artifacts_and_diagnostics_views.md
│   │   │   └── WP_H_005_flaky_and_settings_views.md
│   │   ├── WP_I
│   │   │   ├── WP_I_001_iterative_run_planner.md
│   │   │   ├── WP_I_002_attempt_lifecycle_persistence.md
│   │   │   ├── WP_I_003_comparison_engine.md
│   │   │   ├── WP_I_004_classification_and_quarantine.md
│   │   │   └── WP_I_005_history_and_reporting_surfaces.md
│   │   └── WP_J
│   │       ├── WP_J_001_unit_and_contract_test_matrix.md
│   │       ├── WP_J_002_integration_workspace_fixtures.md
│   │       ├── WP_J_003_ui_and_live_transport_validation.md
│   │       ├── WP_J_004_packaging_and_startup_smoke.md
│   │       └── WP_J_005_runbooks_and_operator_docs.md
│   └── work-packages
│       ├── WP_A_FOUNDATION_AND_CONTRACTS.md
│       ├── WP_B_STORAGE_AND_REGISTRY.md
│       ├── WP_C_SEMANTICS_V62_V71_V72.md
│       ├── WP_D_PLANNING_AND_EXECUTION.md
│       ├── WP_E_RESULTS_AND_DIAGNOSTICS.md
│       ├── WP_F_MCP_SURFACE.md
│       ├── WP_G_WEB_API_AND_STREAMS.md
│       ├── WP_H_FRONTEND.md
│       ├── WP_I_FLAKY_ANALYTICS.md
│       └── WP_J_VALIDATION_AND_PACKAGING.md
└── packaging
    └── README.md
```
