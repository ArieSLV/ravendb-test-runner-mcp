# Project Agent Notes

## .NET Validation Commands

Do not run sandbox-first `dotnet test` or `dotnet run` validation in this repository.
The sandbox user profile cannot write the .NET first-use sentinel, which causes failures
under `C:\Users\CodexSandboxOffline\.dotnet\*.sentinel` before project code runs.

Use per-command environment isolation instead:

```powershell
$env:DOTNET_CLI_HOME = (Join-Path (Resolve-Path '.').Path '.tmp-dotnet-home')
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:MSBuildSDKsPath = 'C:\Program Files\dotnet\sdk\10.0.203\Sdks'
```

Remove `.tmp-dotnet-home`, `.tmp-review-results`, and generated `TestResults`
directories after validation unless a task explicitly asks to keep them.

Keep `ENV-001` open until the project implements deterministic build environment
sanitization. Do not mutate global or user-level environment variables to work around it.

## Implementation Review Workflow

When reviewing a completed implementation-agent iteration for this project, always
finish the review with an actionable prompt for the implementation session.

- If findings exist at any severity, provide a prompt-engineered fix directive for
  the implementation session immediately after the findings.
- If findings are empty, provide a prompt-engineered directive for the next
  implementation step immediately after the accepted review summary.
- Design each next-step prompt from the current repository state and task ledger at
  review time. Do not blindly reuse old wave plans.
- Explicitly decide whether the next step should use one integrating agent or a
  parallel worker wave. If using workers, define non-overlapping write sets and use
  `model=gpt-5.5` with `reasoning_effort=xhigh`.
