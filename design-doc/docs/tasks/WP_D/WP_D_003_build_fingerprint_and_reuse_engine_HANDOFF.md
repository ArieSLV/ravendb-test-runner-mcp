# WP_D_003 Handoff

## Completed task
`WP_D_003_build_fingerprint_and_reuse_engine`

## What changed
- Added deterministic build fingerprint creation under `RavenDB.TestRunner.McpServer.Build`.
- Build fingerprints consume `WP_D_002` `BuildGraphAnalysisResult` output instead of reimplementing graph analysis.
- Added stable hashes for:
  - build properties,
  - relevant environment inputs,
  - dependency input identifiers,
  - current graph/scope identity,
  - optional output manifest hash.
- Added explicit build reuse evaluation for:
  - ready matching fingerprints,
  - stale fingerprint mismatch,
  - missing outputs,
  - expired readiness,
  - forced rebuild / forced incremental policies,
  - expert skip-build policy only when the build ownership boundary has already accepted expert mode.
- Added readiness token issuance with deterministic `build-readiness/<workspace-hash>/<fingerprint>` document IDs.
- Added readiness invalidation records that keep `BuildReadinessToken.status` separate from `BuildExecution.state` and `BuildResult.status`.
- Added explicit superseded readiness invalidation for newer incompatible fingerprints.
- Added focused build tests for fingerprint determinism, reuse acceptance, stale rebuilds, readiness-token fingerprint mismatch, missing output invalidation, forced rebuild invalidation, ready token issuance, expired readiness invalidation, superseded readiness status, and expert-skip ownership proof.

## Corrective passes
- Review finding fixed: `expert_skip_build` can no longer be accepted by `BuildReuseEngine` from policy mode alone.
- `BuildReuseEvaluationRequest` now carries an optional `BuildDependencyResolution` proof from `BuildOwnershipModel`.
- `expert_skip_build` is accepted only when `OwnershipResolution.Kind == expert_skip_build_accepted`.
- Missing or rejected ownership proof returns `rejected_existing` with `expert_mode_required`, without requiring a new build and without pretending expert skip was accepted.
- The accepted skip path still emits `skipped_by_policy` with both `expert_skip_build` and `expert_skip_build_accepted` reason vocabulary.
- Second review finding fixed: rejected expert-skip decisions no longer reuse arbitrary ownership proof reason codes.
- Non-expert ownership proofs such as readiness-token or linked-build acceptance are rejected with canonical `expert_mode_required` and do not leak token IDs, build IDs, policy mode strings, or unrelated ownership reason values into the reuse decision.

## Touched contracts
- Implementation remains aligned with:
  - `docs/contracts/BUILD_SUBSYSTEM.md`
  - `docs/contracts/DOMAIN_MODEL.md`
  - `docs/contracts/STORAGE_MODEL.md`
  - `docs/contracts/VERSIONING_AND_CAPABILITIES.md`
- No design contract documents were changed.
- No RavenDB storage runtime files were changed; this pass creates deterministic domain records and stable document IDs for later persistence integration.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Build/BuildFingerprintReuseEngine.cs`
- `tests/RavenDB.TestRunner.McpServer.Build.Tests/BuildFingerprintReuseEngineTests.cs`

## Validation performed
- `git diff --check` passed.
- `rg -n "Process|Start\\(|dotnet|MSBuild|Microsoft.Build|Exec|BuildManager" src\RavenDB.TestRunner.McpServer.Build tests\RavenDB.TestRunner.McpServer.Build.Tests` was reviewed; matches are existing build/test execution vocabulary such as `BuildExecution`, not process spawning, `dotnet`, MSBuild invocation, or `Microsoft.Build` usage in product code.
- `dotnet build .\RavenDB.TestRunner.McpServer.sln -m:1 -v minimal` succeeded with the `AGENTS.md` isolated .NET environment.
- `dotnet test .\tests\RavenDB.TestRunner.McpServer.Build.Tests\RavenDB.TestRunner.McpServer.Build.Tests.csproj -m:1 -v minimal --logger "trx;LogFileName=wp-d-003-expert-skip-reason-codes.trx"` succeeded.
- Build test result: 33 tests discovered/executed, 33 passed.
- Storage tests were not run because no `Storage.RavenEmbedded` files changed.
- Build artifact/binlog smoke execution was not run because actual build execution/process spawning belongs to later `WP_D` tasks.
- Generated `.tmp-dotnet-home` and TRX/TestResults artifacts were removed after validation.

## Progress ledger update
- Mark `WP_D_003_build_fingerprint_and_reuse_engine` as `Done`.
- Record implementation commit after it is created.
- Keep `ENV-001` open.
- Keep `WP_B_003`, `WP_D_004+`, `WP_C_005+`, `WP_E_*`, `WP_F_*`, `WP_G_*`, MCP host/tools, Web API, UI, and test execution subsystem work not started.

## Risks / follow-ups
- Actual persistence writes for readiness/reuse records remain for later storage integration surfaces.
- Output existence checks are represented as explicit inputs to reuse evaluation; later scheduler/execution work must provide those facts from real output manifests.
- Runtime build execution, binlog capture, and scheduler behavior remain later `WP_D` responsibilities.

## ADR or design delta notes
- No ADR or design delta required.
