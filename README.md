# Puru World Utils

Standalone UdonSharp utilities for VRChat worlds. Drop-in components — no framework required.

## Install via VCC

Add repository:
```
vcc://vpm/add-repo?url=https://Pururut114.github.io/puru-world-utils/index.json
```

Or manually add the listing URL in VCC → Settings → Packages → Add Repository:
```
https://Pururut114.github.io/puru-world-utils/index.json
```

## Utilities

Spawn all utilities from `Tools > World Utils > Spawn`.

| Category | Components |
|----------|-----------|
| Zones | ZoneEnableWhileInside, FallZoneBlackoutTeleport, ZoneReparentSnap, ZoneHeadToggle |
| Persistence | PositionPersistence |
| Teleport | InteractTeleport, PickupPortal |
| FX | FadeOnJoin, CameraLayerIsolation, CameraLayerIsolationZone, MaterialCycler, WebImageFrame |
| Select | MultiSelectController, MultiSelectButton |
| Access | AdminVisibility, AdminVisibilityFull, InstanceOwnerVisibility, MasterVisibility, ZoneAdminVisibility, ZoneAdminVisibilityFull |
| Economy | ProductToggle, ProductToggleFull, OpenWorldStore, OpenGroupStore, OpenListing |
| ProTV *(optional)* | ProTVAmbientFade, ProTVAccessGate |

ProTV utilities require [ProTV](https://protv.dev) to be installed. They are automatically enabled when detected.

## Requirements

- Unity 2022.3 LTS+
- VRChat SDK (Worlds) 3.x
- UdonSharp 1.x
- Post Processing (com.unity.postprocessing 3.2.2)

## License

MIT
