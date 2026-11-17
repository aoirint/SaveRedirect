# Development operations

## Prerequisites

- Windows for the production path-separator and game-process contract;
- the SDK selected by `global.json`; and
- network access to the two mapped NuGet feeds for the initial locked restore.

## Agent skills

The repository pins its development skills from `aoirint/skills` in `apm.yml`.
Install APM CLI 0.35.0, then restore the selected skills and
verify its deployed-file hashes:

```powershell
apm install --frozen --target agent-skills
apm audit --ci
```

Do not hand-edit `.agents/skills` or `apm.lock.yaml`. Update the source pin,
skill selection, lockfile, and [third-party notice](../../THIRD_PARTY_NOTICES.md)
together using the update procedure in [AGENTS.md](../../AGENTS.md).

## Dependency updates

Use the SDK selected by `global.json`. Identify the project that declares
the package, its `packages.lock.json`, and the applicable `nuget.config`.
Use the checked-in package sources and source mappings.

1. Review the package identity, publisher, source, license, advisories, and
   publication date. Verify the configured seven-day minimum before accepting
   a version. Explicit version requests and ordinary restore do not enforce
   the minimum publish age.
2. Update the intended package reference to an exact version. Refresh the
   lockfile for the affected project with `--force-evaluate --use-lock-file`
   and the correct `--configfile`. Do not refresh unrelated projects.
3. Review the project and lockfile diffs. Check resolved versions, transitive
   changes, and content hashes. Investigate unexpected changes before
   keeping the update.
4. Run a locked restore, then the repository's format, build, and applicable
   tests. Keep the project and lockfile changes together in the PR and verify
   its CI before merging.

### SaveRedirect commands

Run from the repository root in PowerShell. For a plugin dependency update:

```powershell
dotnet restore SaveRedirect/SaveRedirect.csproj `
  --configfile nuget.config --force-evaluate --use-lock-file
dotnet restore SaveRedirect.slnx `
  --configfile nuget.config --locked-mode
```

For a test dependency, substitute
`SaveRedirect.Tests/SaveRedirect.Tests.csproj` in the first command.
The solution-level locked restore stays the same.

See [Build and test](../../README.md#build-and-test) for
the normal Release build and Windows test-harness commands.

## Verification

Run the commands in the root README in order. Restore must not change any
`packages.lock.json`. CI stages the built DLL and repository-owned package
files, normalizes their timestamps, and writes one ZIP under `artifacts/`,
matching the packaging ownership used by the related BepInEx repositories. The
test executable validates the path policy, archive paths, exact file set, DLL
count, resource bounds, and actual BepInEx custom attributes in both mutation
fixtures and the completed CI ZIP.

For Markdown changes, run the checked-in Markdown configuration. For workflow
changes, run `actionlint` followed by `pinact run --check --min-age 7`.
When a canonical repository template changes, apply and verify it through
`sync_templates.ps1`; do not edit a synchronized destination directly.

## Runtime validation

Use a disposable BepInEx profile and dedicated Windows account. Start with an
empty `SAVE_REDIRECT_ROOT`, wait for the ready marker, then create/load/delete a
vanilla slot. Repeat with LCBetterSaves 1.7.3 by creating, renaming, loading,
and deleting an extra slot. Verify all created files remain below the selected
root and the normal persistent-data directory is byte-for-byte unchanged.
