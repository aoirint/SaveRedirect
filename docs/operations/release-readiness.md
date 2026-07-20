# Release readiness

## Current state

GitHub Actions is enabled for source and package validation. APM, Thunderstore,
and GitHub Release publication are intentionally disabled. No bundled workflow
template matches this host-neutral, validation-only contract, so workflows are
repository-owned.

GitHub API inspection on 2026-07-20 confirmed read-only default workflow
permissions, mandatory SHA pins, a narrow Actions allow-list, required `lint`
and `test` checks on the default branch, private vulnerability reporting, and
immutable releases. These settings remain external mutable state and must be
rechecked for a stable release.

## Stable release gate

Do not publish a stable release until all of these are recorded:

- clean-profile runtime results for Lethal Company v81 and BepInEx 5.4.21;
- the LCBetterSaves 1.7.3 extra-slot scenario from development operations;
- a reviewed decision for GitHub Releases and any external package host.

The current CI artifact is a retained edge build, not a compatibility claim or
stable package publication.
