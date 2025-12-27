# Changelog

All notable changes to this project will be documented in this file.

## [0.8.0] - 2025-12-27
### Added
- **Terrain Physics**: "Falling Sand" simulation; unsupported terrain pixels now fall.
- **Physics Optimization**: Implemented Chunk-based Active Areas to optimize performance and ensuring hanging terrain collapses correctly.
- **Game Feel**: "Hit Stop" effect (0.1s freeze) when a tank dies for impactful feedback.
- **Laser Weapon**: Dynamic sound effects (pitch/speed ramp) and extended visual beam duration.
- **Debris Persistence**: Debris parts now stay on the battlefield indefinitely (smoke stops after 30s).
### Changed
- **Debris Logic**: Simplified debris from 3 parts to 2 (Body and Barrel) to fix visual issues.

## [0.7.0] - 2025-12-26
### Added
- Tank Debris system: tanks split into 3 parts (Body, Barrel, Turret) upon death.
- Debris physics with ballistic trajectories and terrain collision.
- "Smoking junk" visual effect for persist debris.

## [0.6.0] - 2025-12-25
### Added
- Comprehensive `documentation.html` for all project classes.
- Smoke effect for damaged tanks (health below 1/3) with wind influence.
- Custom `SmokeEffect.fx` shader for smoke visuals.

## [0.5.0] - 2025-12-18
### Added
- Burned border effect for craters.
### Fixed
- Terrain texture transparency in destroyed areas (fixed white-out bug).

## [0.4.0] - 2025-12-14
### Changed
- Refactored project namespaces (e.g., `Test25.Gameplay`, `Test25.UI.Screens`).
- Renamed `ShopManager` to `ShopScreen` and similar UI components.
- Updated screen resolution to 1280x720.

## [0.3.0] - 2025-12-13
### Added
- `SettingsManager` for persistent game options (Master, Music, SFX volumes).
- `settings.json` file for saves.
### Fixed
- Resolved `Random` namespace ambiguity in `ShopManager.cs` (switched to `Utilities.Rng`).

## [0.2.0] - 2025-12-10
### Added
- AI Tank personalities with varied accuracy and target preferences.
- `TextInput` GUI component for player name editing.

## [0.1.0] - 2025-12-05
### Added
- Laser weapon that penetrates terrain.
- Pixel-based terrain destruction system (refactored from heightmap).
- Intricate tank death mechanics (random explosion sizes, ammo cook-offs, debris).
- `TextureGenerator.CreateSolidColorTexture` utility.
