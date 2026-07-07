# Changelog

## [0.2.3] — 2026-07-07

### Changed
- `Puru/TerrainBlend4`: metallic/smoothness теперь **per-layer**, а не общие на материал — заменил `_MetallicSmoothnessMap`/`_Metallic`/`_Smoothness` (0.2.2) на `_MetallicSmoothness0..3`/`_Metallic0..3`/`_Smoothness0..3`, каждый на своей UV (та же, что у albedo/normal слоя), блендится тем же весом маски. `_NormalStrength` остаётся общим.

## [0.2.2] — 2026-07-07

### Added
- `Puru/TerrainBlend4`: опциональная общая metallic/smoothness-карта (`_MetallicSmoothnessMap`, R=metallic/A=smoothness) + слайдер `_Metallic`. Не тянется с террейна, чисто ручная опция в материале, дефолт (белая текстура) не меняет прежнее поведение.

## [0.2.1] — 2026-07-07

### Fixed
- Terrain To Mesh: чанки теперь помечаются `StaticEditorFlags.ContributeGI` при включённом "Generate Lightmap UV" — без этого флага ни встроенный Progressive Lightmapper, ни Bakery не бейкали объект (Bakery, в отличие от встроенного, не делает auto-unwrap и просто пропускает объекты без валидного UV2 + флага)

## [0.2.0] — 2026-07-07

### Added
- Editor: Terrain To Mesh (`Tools > World Utils > Terrain To Mesh`) — Terrain → chunked mesh (даунсемплинг, бесшовные стыки, bake сплатмапы в RGBA-маску, auto-material, OBJ экспорт под Blender)
- Shader: `Puru/TerrainBlend4` — 4-слоевой сплат-блендинг (albedo+normal по маске), companion для Terrain To Mesh

## [0.1.0] — 2026-06-04

### Added
- Initial release — standalone UdonSharp utilities extracted from Puru Signals System
- Zones: ZoneEnableWhileInside, FallZoneBlackoutTeleport, ZoneReparentSnap, ZoneHeadToggle
- Persistence: PositionPersistence
- Teleport: InteractTeleport, PickupPortal
- FX: FadeOnJoin, CameraLayerIsolation, CameraLayerIsolationZone, MaterialCycler, WebImageFrame
- Select: MultiSelectController, MultiSelectButton
- Access: AdminVisibility, AdminVisibilityFull, InstanceOwnerVisibility, MasterVisibility, ZoneAdminVisibility, ZoneAdminVisibilityFull
- Economy: ProductToggle, ProductToggleFull, OpenWorldStore, OpenGroupStore, OpenListing
- ProTV: ProTVAmbientFade, ProTVAccessGate (conditional — requires ProTV)
- Editor: SpawnMenu (`Tools > World Utils > Spawn`), SceneMaterialAnalyzer
