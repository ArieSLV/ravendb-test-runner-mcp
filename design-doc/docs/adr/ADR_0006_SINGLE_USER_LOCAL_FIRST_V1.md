# ADR 0006 SINGLE USER LOCAL FIRST V1

## Context

The first delivery targets individual developer machines rather than team-hosted service deployment.

## Decision

Optimize v1 for single-user local-first operation with localhost binding and minimal trusted-local auth.

## Alternatives considered

Build multi-user/team auth from day one; cloud-first service model.

## Consequences

Simplifies v1 delivery; defers team-hosting concerns; still requires secret handling and loopback safety.

## Contract impact

Affects browser auth posture, packaging, and local security assumptions.

## Migration / rollback note

Future team-mode evolution remains possible through additional auth layers.
