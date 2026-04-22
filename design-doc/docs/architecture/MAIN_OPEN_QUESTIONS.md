# Main Open Questions

## Q1: Exact large-artifact size threshold

A default threshold is required for deciding:
- RavenDB attachment storage
- filesystem-only storage

Validation requirement:
- benchmark at least small, medium, and large artifact sizes on developer machines.

## Q2: Exact local auth posture for browser UI

Trusted-local auth is acceptable in v1, but implementation still needs:
- loopback binding policy,
- CSRF strategy if applicable,
- browser session policy,
- local-only exposure defaults.

## Q3: Scope of optional MCP resources/prompts in v1

Tools are mandatory.
Resources and prompts are optional.
Need explicit v1 scope decision after Phase 5 skeleton is validated.

## Q4: Exact extent of RavenDB Studio reuse

The UI design baseline is clear, but implementation needs a practical decision about:
- token reuse only,
- CSS/pattern reuse,
- selected component reuse.

## Q5: Attachment streaming strategy

Need final decision on:
- whether compact attachments are served through RavenDB directly,
- or normalized through the same artifact API abstraction as filesystem artifacts.

## Q6: Future remote/team mode migration path

Out of scope for v1, but packaging and contracts should avoid blocking a future shared service mode.
