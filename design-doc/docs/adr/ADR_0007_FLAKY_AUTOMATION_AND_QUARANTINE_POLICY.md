# ADR 0007 FLAKY AUTOMATION AND QUARANTINE POLICY

## Context

The product needs more than rerun-failed; it must support iterative reruns, analysis, and controlled automated actions.

## Decision

Implement first-class flaky analysis, iterative execution, diagnostics escalation, and explainable/journaled quarantine workflows.

## Alternatives considered

Manual reruns only; analysis without automation; quarantine out of scope.

## Consequences

Adds complexity but directly addresses unstable test workflows; requires strict explainability and auditability.

## Contract impact

Affects domain model, UI, MCP tools, history storage, and validation strategy.

## Migration / rollback note

Automation can be policy-disabled while retaining analysis infrastructure.
