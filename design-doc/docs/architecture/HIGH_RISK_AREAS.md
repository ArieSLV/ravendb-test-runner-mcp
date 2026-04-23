# HIGH_RISK_AREAS.md

1. **Build determinism and reuse correctness** — false cache hits are worse than extra builds.
2. **Build/test handoff** — hidden rebuilds must be impossible by design.
3. **Cross-branch semantics** — v6.2, v7.1, and v7.2 divergence must stay explicit.
4. **SignalR/SSE/live cursors** — reconnection and cursor replay must remain coherent.
5. **RavenDB Embedded licensing and startup** — startup UX must be safe and deterministic.
6. **stdio bridge purity** — no accidental logging to stdout.
7. **Quarantine automation** — must stay explainable and reversible.
