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
