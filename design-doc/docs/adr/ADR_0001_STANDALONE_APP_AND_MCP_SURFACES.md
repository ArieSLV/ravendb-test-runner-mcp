# ADR 0001 STANDALONE APP AND MCP SURFACES

## Context

The product must outlive unstable child-process lifecycles from AI hosts while still supporting MCP access.

## Decision

Implement a stand-alone local application with a primary local Streamable HTTP MCP surface and an optional stdio bridge host.

## Alternatives considered

Only stdio child-process MCP; no browser UI; remote hosted service first.

## Consequences

Enables stable lifecycle and dashboard support; requires dual host surfaces and localhost API design.

## Contract impact

Affects MCP contracts, startup model, logging discipline, and packaging.

## Migration / rollback note

A stdio-only fallback remains possible, but would reduce lifecycle normalization and dashboard utility.
