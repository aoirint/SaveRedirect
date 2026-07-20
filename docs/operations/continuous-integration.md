# Continuous integration

## Contract

The `Pull Request` workflow validates pull requests and merge-queue groups.
Superseded pull-request runs are canceled, while merge-queue runs complete.
The `Main` workflow repeats validation on each exact `main` commit, creates and
validates the release-shaped development ZIP in its build job, and retains it
for 14 days. This matches the event and packaging ownership used by the related
BepInEx repositories. Neither workflow publishes a release or exposes a
manual-dispatch operation.

All jobs have read-only repository access and no secrets. Linux workflow lint
uses `ubuntu-24.04`. Plugin compilation and archive validation use
`windows-2025` because the supported host is Windows and the build consumes
Windows-targeted Lethal Company references.

The workflows verify:

- full-SHA action pins with a seven-day minimum age;
- GitHub Actions syntax and expressions;
- exact canonical contributor, pull-request, and line-ending templates;
- repository Markdown;
- locked NuGet restore and the stable SDK feature band selected by
  `global.json`;
- formatting and warnings-as-errors Release compilation;
- path-confinement and archive mutation tests on proposed and integrated source;
- plugin assembly identity and BepInEx attributes; and
- deterministic package creation and final-archive validation on the exact
  integrated commit.

The retained artifact is named `SaveRedirect-<source-commit>` and contains the
validated package plus `SHA256SUMS`. It is a development artifact, not a stable
release.

## Local workflow checks

```powershell
actionlint -color -pyflakes=
pinact run --check --min-age 7
```

Also extract the Bash run blocks from Composite Actions and pass each one to
`shellcheck -s bash -` over standard input; ShellCheck does not parse YAML.

GitHub API inspection on 2026-07-20 confirmed read-only default workflow
permissions, mandatory full-SHA action pins, and an allow-list limited to
GitHub-owned actions plus the pinned Markdownlint action. The active default
branch ruleset requires an up-to-date pull request with both `lint` and `test`,
and prevents deletion and non-fast-forward updates. Private vulnerability
reporting and immutable releases are enabled. Recheck these external settings
before stable publication because repository source cannot enforce them.

Update this document when workflow events, permissions, runners, action pins,
quality gates, artifact retention, or publication responsibility changes.
