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

### Fixed

- Aligned CI SDK verification with the patch roll-forward allowed by
  `global.json`, while retaining its stable feature-band boundary.

### Notes

- Static evidence covers Lethal Company v81 and LCBetterSaves 1.7.3.
- Live v81 validation remains required before stable publication.
