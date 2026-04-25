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

## Prompt Engineering for GPT-5.5 xHigh

When preparing implementation or corrective prompts for a GPT-5.5 xHigh coding
agent, prefer compact outcome-first prompts instead of legacy process-heavy prompt
stacks. Source guidance: https://developers.openai.com/api/docs/guides/prompt-guidance

- Start from the desired outcome, success criteria, constraints, available context,
  and required final report. Do not prescribe every internal reasoning step unless
  the step is a real invariant or dependency.
- Use absolute words such as `MUST`, `NEVER`, and `ONLY` for true invariants:
  scope boundaries, forbidden side effects, safety rules, required output fields,
  version requirements, and validation gates. Use decision rules for judgment calls.
- Keep personality and collaboration guidance short. For this project, prompts
  should reinforce pragmatic, direct engineering behavior without replacing concrete
  task goals, scope, validation, or stop rules.
- Include a retrieval/context budget. Name the exact design docs, handoffs, source
  files, and external versioned docs that matter for the task. Tell the agent not to
  load the entire design pack unless the task genuinely requires it.
- Make dependency checks explicit. Before implementing, the agent must inspect the
  current ledger, relevant handoffs, and changed code surfaces that the task depends
  on. Do not skip prerequisite discovery because the final task appears obvious.
- Decide parallelism deliberately. Use a single integrating agent for shared write
  sets, cross-cutting contracts, ledger ownership, or ambiguous dependencies. Use a
  worker wave only for independent tasks with non-overlapping write sets; each worker
  must use `model=gpt-5.5` and `reasoning_effort=xhigh`.
- Define completion criteria and stop rules. Treat the task as incomplete until all
  requested deliverables are implemented, validated, documented in the handoff, and
  recorded in `design-doc/IMPLEMENTATION_PROGRESS.md`, or explicitly marked blocked
  with the blocking reason.
- Require concrete validation commands and test counts. Prefer targeted tests for
  changed behavior, affected-project build checks, relevant regression harnesses,
  `git diff --check`, and explicit TRX count inspection when tests are run.
- Add a lightweight verification loop before finalizing: check scope, invariants,
  grounding in the design docs, validation results, ledger updates, and accidental
  changes to `TASK_INDEX.md` or unrelated files.
- Keep final reports concise and stable: changed files, behavior implemented or
  fixed, validation results with counts, commit hashes, working tree status, and
  whether anything was pushed.
