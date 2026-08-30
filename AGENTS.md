# Repository Instructions

## Commit Messages

When creating a commit, follow the commit message rules in [CONTRIBUTING.md](CONTRIBUTING.md#commit-messages).

## Documentation Synchronization

After changing code, configuration, or Markdown documentation, update affected Markdown documents within the same task. The root README, canonical `docs/*.md` architecture guides, and READMEs for runnable templates require English and Simplified Chinese versions. Scaffold-only READMEs and internal or governance documents may remain single-language. Whenever a localized counterpart exists, verify before completion that both versions remain semantically consistent.

## Documentation and Comments

Use idiomatic documentation comments for public or exported APIs whose contracts, constraints, side effects, failure modes, or architectural rationale are not evident from names and types; explain why instead of restating what the code does, and avoid boilerplate comments.

## Post-Edit Simplification Review

After completing code or configuration changes, assess and report unnecessary redundancy, complexity, and compatibility code against the actual runtime and deployment targets, provide clear and prudent simplification recommendations, and do not remove such content without authorization.
