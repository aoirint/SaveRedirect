# Agent Instructions

Repository-local Agent Skills are deployed to `.agents/skills/` by
[APM](https://github.com/microsoft/apm). Do not edit that generated directory
directly.

## APM-managed Skills

- Keep this unpublished APM project at `version: 0.0.0` until its distribution
  and versioning design is explicitly decided.
- Use APM CLI 0.26.0 for lock operations. Its normal seven-day cooldown was
  explicitly waived because it fixes virtual-package `config-consistency`
  audit failures. The waiver covers only the CLI release time gate.
- A maintainer may explicitly waive the normal seven-day wait for a directly
  selected current `aoirint/skills` main commit. Record the waiver and exact
  full commit SHA in the pull request.
- That waiver applies only to the direct `aoirint/skills` commit selection. It
  does not cover dependencies of `aoirint/skills`; review those dependencies
  and enforce their cooldown independently.
- To update Skills, review source and license changes, update the full commit
  pin in `apm.yml`, remove only the validated project lock, regenerate it with
  APM 0.26.0, then run `apm install --frozen` and `apm audit --ci`. Commit the
  manifest, lockfile, notices, and generated `.agents/skills/` changes together.

## Markdown Checks

Use pnpm 11 or newer. Keep the exact package pin and all fail-closed cooldown
settings when reproducing the Markdown check locally:

```shell
pnpm \
  --config.minimumReleaseAge=10080 \
  --config.minimumReleaseAgeStrict=true \
  --config.minimumReleaseAgeIgnoreMissingTime=false \
  --config.minimumReleaseAgeExclude= \
  dlx markdownlint-cli2@0.22.0 \
  --config .markdownlint-cli2.yaml \
  '**/*.md'
```

Add `--fix` after the package version to apply supported automatic fixes, then
run the normal command again. Some rules, including prose line length, still
require a meaning-preserving manual edit.

## Pull Request Merges

- Merge pull requests with squash merge.
- Set the squash commit title to `<pull request title> (#<number>)`, including
  the pull request number.
