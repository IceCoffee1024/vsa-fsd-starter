# Contributing

## Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) using this format:

```text
<type>[optional scope][!]: <description>
```

- Keep each commit focused on a single concern.
- Write a concise, imperative description without a trailing period.
- Use `feat` for new behavior and `fix` for bug fixes; use an appropriate type such as `docs`, `test`, `refactor`, or `chore` for other changes.
- Add `!` before the colon and describe the impact in a `BREAKING CHANGE:` footer when a commit introduces a breaking change.

Examples:

```text
feat(orders): add batch creation endpoint
docs: add contribution guidelines
fix(host)!: change startup configuration contract

BREAKING CHANGE: host startup now requires the new configuration shape
```
