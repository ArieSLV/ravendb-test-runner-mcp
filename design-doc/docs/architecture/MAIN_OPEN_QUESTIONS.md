# MAIN_OPEN_QUESTIONS.md

## Version-sensitive questions
1. Whether any branch-specific build graph quirks require additional plugin hooks beyond semantic plugins.
2. How much xUnit v2 vs v3 row identity metadata can be surfaced uniformly.
3. Whether any future RavenDB lines require build capability plugins in addition to semantic plugins.

## Operational questions
1. Exact attachment-size threshold for RavenDB vs filesystem artifacts.
2. Whether build binlogs should always be captured or only under specific profiles.
3. Whether the stdio bridge should be packaged as a separate executable or an alternate startup mode.
4. Exact first-run UX for embedded license provisioning.

## UI questions
1. Which Studio design tokens can be reused directly without coupling to Studio internals.
2. Whether build and test pages should be separate top-level navigation entries or merged with tabs.
