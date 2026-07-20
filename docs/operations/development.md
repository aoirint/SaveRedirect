# Development operations

## Prerequisites

- Windows for the production path-separator and game-process contract;
- the SDK selected by `global.json`; and
- network access to the two mapped NuGet feeds for the initial locked restore.

## Agent skills

The repository pins its development skills from `aoirint/skills` in `apm.yml`.
After installing the reviewed APM CLI, restore the exact selected subset and
verify its deployed-file hashes:

```powershell
apm install --frozen --target agent-skills
apm audit --ci
```

Do not hand-edit `.agents/skills` or `apm.lock.yaml`. Update the source pin,
skill selection, lockfile, and [third-party notice](../../THIRD_PARTY_NOTICES.md)
together through an approved APM update.

## Verification

Run the commands in the root README in order. Restore must not change any
`packages.lock.json`. The test executable validates the path policy and built
assembly identity. The package command produces exactly one ZIP under
`artifacts/`; the validation command checks its paths, files, DLL count, and
BepInEx custom attributes.

For Markdown changes, run the checked-in Markdown configuration. For workflow
changes, run `actionlint` followed by `pinact run --check --min-age 7`.

## Runtime validation

Use a disposable BepInEx profile and dedicated Windows account. Start with an
empty `SAVE_REDIRECT_ROOT`, wait for the ready marker, then create/load/delete a
vanilla slot. Repeat with LCBetterSaves 1.7.3 by creating, renaming, loading,
and deleting an extra slot. Verify all created files remain below the selected
root and the normal persistent-data directory is byte-for-byte unchanged.
