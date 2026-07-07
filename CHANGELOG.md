# Changelog

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
