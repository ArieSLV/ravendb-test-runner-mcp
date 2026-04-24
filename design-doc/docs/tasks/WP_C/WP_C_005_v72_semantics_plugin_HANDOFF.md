# WP_C_005 Handoff

## Completed task
`WP_C_005_v72_semantics_plugin`

## What changed
- Hardened `RavenV72Semantics` around the modern v7.2 baseline:
  - framework family remains `xunit.v3`,
  - runner family remains `xunit.v3`,
  - adapter family remains `xunit.v3`,
  - xUnit v3 source-info support remains enabled,
  - AI capability fields are locked on for the v7.2 modern baseline instead of depending on marker-only discovery.
- Added a v7.2 workspace fixture without AI path markers to prove v7.2 capability routing remains modern/AI-capable from repo-line and xUnit v3 evidence.
- Strengthened result-normalization validation for xUnit v2 versus xUnit v3 identity/source-info vocabularies.
- Updated the v7.2 capability matrix snapshot.
- Preserved v6.2 and v7.1 behavior.

## Touched contracts
- No design contract documents were changed.
- Implementation remains aligned with `VERSIONING_AND_CAPABILITIES.md`, which defines v7.2 as the modern fully expected AI-capable baseline.
- `DOMAIN_MODEL.md` capability matrix shape remains unchanged.

## Touched modules/files
- `src/RavenDB.TestRunner.McpServer.Semantics.Raven.V72/`
- `tests/RavenDB.TestRunner.McpServer.Semantics.Tests/`

## Validation performed
- Worker validation passed:
  `$env:DOTNET_CLI_HOME=(Join-Path (Resolve-Path '.').Path '.tmp-dotnet-home'); $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:MSBuildSDKsPath='C:\Program Files\dotnet\sdk\10.0.203\Sdks'; dotnet run --no-build --project .\tests\RavenDB.TestRunner.McpServer.Semantics.Tests\RavenDB.TestRunner.McpServer.Semantics.Tests.csproj -v minimal`
- Worker result: 8 workspace detection and capability checks passed.
- Final integrator validation is recorded in `IMPLEMENTATION_PROGRESS.md`.

## Progress ledger update
- Mark `WP_C_005_v72_semantics_plugin` as `Done`.
- Record worker as `worker/Ampere`.
- Keep `ENV-001` open.
- Keep `WP_C_006` not started.

## Risks / follow-ups
- Future repo lines must be added as peer plugins rather than mutating v7.2 assumptions.
- `WP_C_006` still owns catalog persistence and capability matrix persistence integration.

## ADR or design delta notes
- No ADR or design delta required.

## External docs / version assumptions
- No external docs were used.
- Worker used `model=gpt-5.5` with `reasoning_effort=xhigh`.
