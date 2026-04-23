# Packaging Notes

Packaging for RavenDB Test Runner MCP Server is deferred until Phase 10, but the package MUST preserve:
- standalone local app startup,
- browser UI hosting,
- local Streamable HTTP MCP host,
- optional stdio bridge host,
- RavenDB Embedded storage bootstrap,
- artifact-root configuration.
