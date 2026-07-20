# Changelog

All notable development changes are recorded here. This file follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## Unreleased

### Added

- Split the save redirection helper from ModDebugPilot into the independent
  SaveRedirect plugin.
- Added confined Easy Save 3 path resolution, LCBetterSaves-compatible path
  coverage, deterministic packaging, and assembly metadata validation.
- Added validation-only pull-request, merge-queue, and `main` workflows with
  immutable action pins and retained exact-commit development artifacts.

### Changed

- Moved host-neutral package creation into the plugin project and retained
  final-archive and mutation validation in the test project, removing the
  unnecessary standalone package executable.
- Kept the loader entry point focused by separating the Harmony adapter and
  framework-free path policy into cohesive source modules.
- Adopted the canonical contributor agreement, pull-request confirmation, and
  line-ending policy with CI drift enforcement.
- Enforced full-SHA and allow-listed GitHub Actions, and required both source
  lint and test checks on up-to-date default-branch pull requests.

### Fixed

- Aligned CI SDK verification with the patch roll-forward allowed by
  `global.json`, while retaining its stable feature-band boundary.

### Notes

- Static evidence covers Lethal Company v81 and LCBetterSaves 1.7.3.
- Live v81 validation remains required before stable publication.
