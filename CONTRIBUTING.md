# Contributing

Keep changes focused and update `CHANGELOG.md` when behavior, compatibility, or
packaging changes. Do not commit game files, mod-manager profiles, build output,
credentials, or machine-local paths.

## Verification

| Changed surface | Required checks |
| --- | --- |
| C# or project files | locked restore, format verification, build, test executable |
| Save hook or path policy | v81 evidence review, boundary tests, live-game result or explicit gap |
| Package tool or contract | package tests, clean package creation, final archive validation |
| Markdown | repository Markdown lint |
| GitHub Actions | actionlint and `pinact run --check --min-age 7` |

The exact commands are maintained in
[development operations](docs/operations/development.md). Pull requests should
state which commands ran, which checks were skipped, and whether AI assistance
significantly shaped the contribution.
