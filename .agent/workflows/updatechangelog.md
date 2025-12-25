---
description: Updates the changelog file with the latest changes
---

To update the `CHANGELOG.md` file correctly, follow these steps:

1.  **Identify Changes**: Review the latest commits or task completions.
2.  **Determine Version**:
    *   **Patch** (e.g., 0.1.x): For bug fixes.
    *   **Minor** (e.g., 0.x.0): For new features (backwards compatible).
    *   **Major** (e.g., x.0.0): For breaking changes.
3.  **Update [Unreleased] Section**:
    *   Collect all changes under the `## [Unreleased]` header.
    *   Group changes into sub-headers: `### Added`, `### Changed`, `### Fixed`, `### Removed`.
4.  **Create New Version Entry**:
    *   When ready to "release", move items from `[Unreleased]` to a new version header.
    *   Format: `## [Version] - YYYY-MM-DD`.
5.  **Maintain Links**: (Optional) Update the comparison links at the bottom of the file if used.

Example entry:
```markdown
## [0.2.0] - 2025-12-25

### Added
- Smoke effect for damaged tanks.
- Persistant settings for volume.
```
