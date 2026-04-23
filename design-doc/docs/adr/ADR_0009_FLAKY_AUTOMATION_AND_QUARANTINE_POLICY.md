# ADR 0009 FLAKY AUTOMATION AND QUARANTINE POLICY

## Context
The product must go beyond passive flaky reporting and support policy-driven automation.

## Decision
Support explainable, reversible, journaled quarantine proposals/actions and iterative reruns.

## Alternatives considered
- analysis-only
- unbounded opaque automation

## Consequences
- Automation exists but remains reviewable and auditable.
- Contracts and task cards must align with this decision.

## Contract impact
- update affected architecture and contract files when this ADR is introduced or revised
- ensure work package/task references remain synchronized

## Migration / rollback note
Rollback would reduce the product to passive reporting and break related task/work-package assumptions.
