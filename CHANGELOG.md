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

- Removed the standalone package executable and aligned host-neutral ZIP
  creation with the CI-owned staging used by related BepInEx repositories;
  final-archive and mutation validation remains in the test project.
- Kept the loader entry point focused by separating the Harmony adapter and
  framework-free path policy into cohesive source modules.
- Adopted the canonical contributor agreement, pull-request confirmation, and
  line-ending and nested-list Markdown policies with CI drift enforcement.
- Enforced full-SHA and allow-listed GitHub Actions, and required both source
  lint and test checks on up-to-date default-branch pull requests.

### Removed

- Removed the redundant `SECURITY.md`; contribution policy owns private
  vulnerability reporting, while the README, architecture, and operations
  documents remain the canonical owners of product boundaries and safe use.

### Fixed

- Aligned CI SDK verification with the patch roll-forward allowed by
  `global.json`, while retaining its stable feature-band boundary.

### Notes

- Static evidence covers Lethal Company v81 and LCBetterSaves 1.7.3.
- Live v81 validation remains required before stable publication.
