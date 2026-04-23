# EXECUTION_PACK_TREE.md

```text
rtrms_execution_pack_v2_full
├── docs
│   ├── adr
│   │   ├── ADR_0001_PRODUCT_NAMING_AND_MODULE_POLICY.md
│   │   ├── ADR_0002_STANDALONE_APP_AND_MCP_SURFACES.md
│   │   ├── ADR_0003_RAVENDB_EMBEDDED_AS_METADATA_STORE.md
│   │   ├── ADR_0004_HYBRID_ARTIFACT_STORAGE.md
│   │   ├── ADR_0005_VERSION_PLUGIN_MODEL.md
│   │   ├── ADR_0006_BUILD_SUBSYSTEM_AS_FIRST_CLASS_CONCERN.md
│   │   ├── ADR_0007_SIGNALR_PRIMARY_BROWSER_TRANSPORT.md
│   │   ├── ADR_0008_SINGLE_USER_LOCAL_FIRST_V1.md
│   │   └── ADR_0009_FLAKY_AUTOMATION_AND_QUARANTINE_POLICY.md
│   ├── architecture
│   │   ├── CHANGE_SUMMARY.md
│   │   ├── DECISION_FREEZE.md
│   │   ├── DEPENDENCY_GRAPH.md
│   │   ├── EXECUTION_PACK_INDEX.md
│   │   ├── FIRST_10_TASKS_TO_EXECUTE.md
│   │   ├── HIGH_RISK_AREAS.md
│   │   ├── IMPLEMENTATION_ORDER_SUMMARY.md
│   │   ├── IMPLEMENTATION_SPEC.md
│   │   ├── MAIN_OPEN_QUESTIONS.md
│   │   ├── PARALLELIZATION_STRATEGY.md
│   │   └── REVISION_SUMMARY.md
│   ├── contracts
│   │   ├── ARTIFACTS_AND_RETENTION.md
│   │   ├── BUILD_SUBSYSTEM.md
│   │   ├── DOMAIN_MODEL.md
│   │   ├── ERROR_TAXONOMY.md
│   │   ├── EVENT_MODEL.md
│   │   ├── FRONTEND_VIEW_MODELS.md
│   │   ├── MCP_TOOLS.md
│   │   ├── NAMING_AND_MODULE_POLICY.md
│   │   ├── SECURITY_AND_REDACTION.md
│   │   ├── STATE_MACHINES.md
│   │   ├── STORAGE_MODEL.md
│   │   ├── VERSIONING_AND_CAPABILITIES.md
│   │   └── WEB_API.md
│   ├── phases
│   │   ├── PHASE_0_CONTRACT_FREEZE.md
│   │   ├── PHASE_10_VALIDATION_AND_PACKAGING.md
│   │   ├── PHASE_1_STORAGE_AND_REGISTRY.md
│   │   ├── PHASE_2_SEMANTICS_AND_CATALOG.md
│   │   ├── PHASE_3_BUILD_SUBSYSTEM.md
│   │   ├── PHASE_4_TEST_PLANNING_AND_EXECUTION.md
│   │   ├── PHASE_5_RESULTS_AND_DIAGNOSTICS.md
│   │   ├── PHASE_6_MCP_SURFACES.md
│   │   ├── PHASE_7_WEB_API_AND_LIVE_EVENTS.md
│   │   ├── PHASE_8_FRONTEND_UI.md
│   │   └── PHASE_9_FLAKY_AND_QUARANTINE.md
│   ├── runbooks
│   │   ├── DEVELOPER_SETUP.md
│   │   ├── OPERATOR_RUNBOOK.md
│   │   └── README.md
│   ├── tasks
│   │   ├── WP_A
│   │   │   ├── WP_A_001_solution_scaffold_and_name_freeze.md
│   │   │   ├── WP_A_002_shared_contracts_project_layout.md
│   │   │   ├── WP_A_003_document_id_and_collection_conventions.md
│   │   │   ├── WP_A_004_event_contract_baseline.md
│   │   │   ├── WP_A_005_state_machine_baseline.md
│   │   │   └── WP_A_006_phase0_validation_harness.md
│   │   ├── WP_B
│   │   │   ├── WP_B_001_embedded_bootstrap_and_database_init.md
│   │   │   ├── WP_B_002_collections_indexes_and_optimistic_concurrency.md
│   │   │   ├── WP_B_003_artifact_metadata_and_attachment_thresholds.md
│   │   │   ├── WP_B_004_deferred_bulky_diagnostics_and_spillover_guardrails.md
│   │   │   ├── WP_B_005_event_checkpoint_and_resume_persistence.md
│   │   │   └── WP_B_006_restart_recovery_cleanup_and_retention.md
│   │   ├── WP_C
│   │   │   ├── WP_C_001_workspace_and_repo_line_detection.md
│   │   │   ├── WP_C_002_semantic_plugin_contracts.md
│   │   │   ├── WP_C_003_v62_semantics_plugin.md
│   │   │   ├── WP_C_004_v71_semantics_plugin.md
│   │   │   ├── WP_C_005_v72_semantics_plugin.md
│   │   │   └── WP_C_006_catalog_persistence_and_capability_matrix.md
│   │   ├── WP_D
│   │   │   ├── WP_D_001_build_domain_contracts_and_policies.md
│   │   │   ├── WP_D_002_build_graph_analyzer.md
│   │   │   ├── WP_D_003_build_fingerprint_and_reuse_engine.md
│   │   │   ├── WP_D_004_build_scheduler_and_execution_engine.md
│   │   │   ├── WP_D_005_build_artifacts_status_and_binlog_capture.md
│   │   │   └── WP_D_006_build_readiness_integration.md
│   │   ├── WP_E
│   │   │   ├── WP_E_001_selector_normalization_engine.md
│   │   │   ├── WP_E_002_preflight_evaluator.md
│   │   │   ├── WP_E_003_test_run_planner.md
│   │   │   ├── WP_E_004_scheduler_and_process_supervisor.md
│   │   │   ├── WP_E_005_build_to_test_handoff.md
│   │   │   └── WP_E_006_repro_commands_and_execution_summaries.md
│   │   ├── WP_F
│   │   │   ├── WP_F_001_console_capture_pipeline.md
│   │   │   ├── WP_F_002_trx_junit_and_binlog_harvesting.md
│   │   │   ├── WP_F_003_failure_taxonomy_mapper.md
│   │   │   ├── WP_F_004_normalized_result_builder.md
│   │   │   ├── WP_F_005_diagnostic_hooks_and_blame_artifacts.md
│   │   │   └── WP_F_006_predicted_vs_actual_reconciliation.md
│   │   ├── WP_G
│   │   │   ├── WP_G_001_mcp_common_handler_layer.md
│   │   │   ├── WP_G_002_streamable_http_mcp_host.md
│   │   │   ├── WP_G_003_stdio_bridge_host.md
│   │   │   ├── WP_G_004_tests_toolset.md
│   │   │   ├── WP_G_005_build_toolset.md
│   │   │   └── WP_G_006_progress_cancellation_and_resume.md
│   │   ├── WP_H
│   │   │   ├── WP_H_001_query_api_surface.md
│   │   │   ├── WP_H_002_command_api_surface.md
│   │   │   ├── WP_H_003_signalr_hub_and_event_mapping.md
│   │   │   ├── WP_H_004_sse_and_log_cursor_endpoints.md
│   │   │   ├── WP_H_005_build_status_and_policy_endpoints.md
│   │   │   └── WP_H_006_localhost_security_posture.md
│   │   ├── WP_I
│   │   │   ├── WP_I_001_ui_app_shell_and_design_baseline.md
│   │   │   ├── WP_I_002_runs_and_builds_list_details.md
│   │   │   ├── WP_I_003_live_console_results_and_build_output.md
│   │   │   ├── WP_I_004_artifacts_diagnostics_and_plan_views.md
│   │   │   ├── WP_I_005_flaky_settings_and_policy_views.md
│   │   │   └── WP_I_006_accessibility_and_reconnect_behavior.md
│   │   ├── WP_J
│   │   │   ├── WP_J_001_iterative_run_planner.md
│   │   │   ├── WP_J_002_attempt_lifecycle_and_history_persistence.md
│   │   │   ├── WP_J_003_comparison_engine.md
│   │   │   ├── WP_J_004_stability_classification_and_scoring.md
│   │   │   ├── WP_J_005_quarantine_policy_and_audit_trail.md
│   │   │   └── WP_J_006_reporting_surfaces_and_notifications.md
│   │   ├── WP_K
│   │   │   ├── WP_K_001_unit_and_contract_test_matrix.md
│   │   │   ├── WP_K_002_cross_branch_integration_fixtures.md
│   │   │   ├── WP_K_003_ui_and_live_transport_validation.md
│   │   │   ├── WP_K_004_build_subsystem_validation.md
│   │   │   ├── WP_K_005_packaging_and_startup_smoke.md
│   │   │   └── WP_K_006_runbooks_and_operator_docs.md
│   │   ├── HANDOFF_TEMPLATE.md
│   │   ├── TASK_INDEX.md
│   │   └── TASK_TEMPLATE.md
│   └── work-packages
│       ├── WP_A_FOUNDATION_AND_CONTRACTS.md
│       ├── WP_B_STORAGE_AND_REGISTRY.md
│       ├── WP_C_SEMANTICS_V62_V71_V72.md
│       ├── WP_D_BUILD_SUBSYSTEM.md
│       ├── WP_E_TEST_PLANNING_AND_EXECUTION.md
│       ├── WP_F_RESULTS_AND_DIAGNOSTICS.md
│       ├── WP_G_MCP_SURFACE.md
│       ├── WP_H_WEB_API_AND_STREAMS.md
│       ├── WP_I_FRONTEND.md
│       ├── WP_J_FLAKY_AND_QUARANTINE.md
│       └── WP_K_VALIDATION_AND_PACKAGING.md
├── packaging
│   └── README.md
├── AGENTS.md
├── EXECUTION_PACK_TREE.md
└── README.md
```
