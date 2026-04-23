# CHANGE_SUMMARY.md

## Purpose
This document summarizes the architectural deltas introduced by this execution-pack revision.

## Major changes
1. Consolidated product naming around **RavenDB Test Runner MCP Server**.
2. Retired the naming drift around `RavenMcp`, `RavenMcpControlPlane`, and `RavenDB Test MCP Control Plane`.
3. Added **Build Subsystem** as a first-class architectural concern.
4. Added separate build MCP tools, web APIs, live events, UI views, phases, work packages, tasks, and ADRs.
5. Updated run/test planning to consume explicit build readiness rather than hidden build behavior.
6. Added naming/module policy and build subsystem contract files.
