# Parallelization Strategy

## Objective

Enable multiple AI coding agents to work concurrently without destabilizing the architecture.

## Rule set

- Phase 0 is serialized.
- Shared contracts are frozen before broad parallel implementation.
- Each work package has an owner.
- One integrator controls merges into shared contracts and shared abstractions.

## Recommended parallel lanes

### Lane 1
- WP-B Storage and Registry

### Lane 2
- WP-C Semantics and Catalog

### Lane 3
- WP-F MCP Surface skeleton

After event/API contracts stabilize:

### Lane 4
- WP-G Web API and Streams

### Lane 5
- WP-H Frontend

After run/result/attempt models stabilize:

### Lane 6
- WP-I Flaky Analytics

## Forbidden parallel changes without integrator review

- document ID patterns
- collection names
- event names or payload fields
- run state enum changes
- MCP request/response shape changes
- browser API shape changes
- capability names
