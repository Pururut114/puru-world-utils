# PWU — Work In Progress

Этот файл удалить после завершения работы.

---

## Что сделано

**Фаза 0 — ГОТОВО.** Scaffold нового репо создан в `x:\Сlaude\Unity\Puru_World_Utils\`:
- `package.json` — com.pururut.world-utils, v0.1.0
- `source.json`, `.gitignore`, `LICENSE`, `CHANGELOG.md`, `README.md`, `NOTES.md`
- `.github/workflows/release.yml` + `build-listing.yml`
- `Runtime/com.pururut.pwu.runtime.asmdef`
- `ProTV/com.pururut.pwu.protv.asmdef` (defineConstraints: PWU_PROTV_INSTALLED)
- `Editor/com.pururut.pwu.editor.asmdef` (versionDefines: dev.architech.protv → PWU_PROTV_INSTALLED)
- `_gen_meta_assets.py` — адаптирован под PWU_*, BEHAVIOURS уже заполнен всеми 25 путями
- `_validate_release.py` — адаптирован под PWU_*

**Фазы 1+2 — ГОТОВО.** Все 33 скрипта скопированы, переименованы и очищены от PSS-зависимостей:
- `Runtime/PWU_NoteAttribute.cs` (новый — замена PSS_NoteAttribute)
- 25 Runtime скриптов (Zones/Persistence/Teleport/FX/Select/Access/Economy)
- 2 ProTV скрипта (условная сборка, PWU_PROTV_INSTALLED)
- 5 Editor скриптов (PWU_AutoSetup, PWU_SpawnMenu, PWU_StandaloneUtilityEditor, SceneMaterialAnalyzer, SceneMaterialInspector)
- PSS-поля (PSS_ChannelLocal) удалены прямо при написании, отдельной фазы 2 не потребовалось
- keyPrefix в PWU_PositionPersistence изменён с "PSS" на "PWU"

---

## Что нужно сделать

### ~~Фаза 1 — Перенести скрипты из PSS в PWU (переименовать PSS_ → PWU_)~~ ГОТОВО

### ~~Фаза 2 — Убрать PSS-поля из 8 классов (ПОСЛЕ копирования в PWU)~~ ГОТОВО (сделано сразу при написании)

Источник: `x:\Сlaude\Unity\Puru_Signals_System\`

#### Runtime/ (из Modules/Standalone Utilities/)

**Zones/** ← `Modules/Standalone Utilities/Zones/`
- `PSS_ZoneEnableWhileInside.cs` → `PWU_ZoneEnableWhileInside.cs`
- `PSS_FallZoneBlackoutTeleport.cs` → `PWU_FallZoneBlackoutTeleport.cs`
- `PSS_ZoneReparentSnap.cs` → `PWU_ZoneReparentSnap.cs`
- `PSS_ZoneHeadToggle.cs` → `PWU_ZoneHeadToggle.cs`

**Persistence/** ← `Modules/Standalone Utilities/Persistence/`
- `PSS_PositionPersistence.cs` → `PWU_PositionPersistence.cs`

**Teleport/** ← `Modules/Standalone Utilities/Teleport/`
- `PSS_InteractTeleport.cs` → `PWU_InteractTeleport.cs`
- `PSS_PickupPortal.cs` → `PWU_PickupPortal.cs`

**FX/** ← `Modules/Standalone Utilities/FX/`
- `PSS_FadeOnJoin.cs` → `PWU_FadeOnJoin.cs`
- `PSS_CameraLayerIsolation.cs` → `PWU_CameraLayerIsolation.cs`
- `PSS_CameraLayerIsolationZone.cs` → `PWU_CameraLayerIsolationZone.cs`
- `PSS_MaterialCycler.cs` → `PWU_MaterialCycler.cs`
- `PSS_WebImageFrame.cs` → `PWU_WebImageFrame.cs`

**Select/** ← `Modules/Standalone Utilities/Select/`
- `PSS_MultiSelectController.cs` → `PWU_MultiSelectController.cs`
- `PSS_MultiSelectButton.cs` → `PWU_MultiSelectButton.cs`

**Access/** ← `Modules/Standalone Utilities/Access/`
- `PSS_AdminVisibility.cs` → `PWU_AdminVisibility.cs`
- `PSS_AdminVisibilityFull.cs` → `PWU_AdminVisibilityFull.cs`
- `PSS_InstanceOwnerVisibility.cs` → `PWU_InstanceOwnerVisibility.cs`
- `PSS_MasterVisibility.cs` → `PWU_MasterVisibility.cs`
- `PSS_ZoneAdminVisibility.cs` → `PWU_ZoneAdminVisibility.cs`
- `PSS_ZoneAdminVisibilityFull.cs` → `PWU_ZoneAdminVisibilityFull.cs`

**Economy/** ← `Modules/Standalone Utilities/Economy/`
- `PSS_ProductToggle.cs` → `PWU_ProductToggle.cs`
- `PSS_ProductToggleFull.cs` → `PWU_ProductToggleFull.cs`
- `PSS_OpenWorldStore.cs` → `PWU_OpenWorldStore.cs`
- `PSS_OpenGroupStore.cs` → `PWU_OpenGroupStore.cs`
- `PSS_OpenListing.cs` → `PWU_OpenListing.cs`

#### ProTV/ (из Modules/ProTV/)
- `PSS_ProTVAmbientFade.cs` → `PWU_ProTVAmbientFade.cs`
- `PSS_ProTVAccessGate.cs` → `PWU_ProTVAccessGate.cs`

#### Editor/ (из Editor/ PSS)
- `PSS_SpawnMenu.cs` → `PWU_SpawnMenu.cs` (меню: Tools > World Utils > Spawn > ...)
- `PSS_StandaloneUtilityEditor.cs` → `PWU_StandaloneUtilityEditor.cs`
- `PSS_AutoSetup.cs` → `PWU_AutoSetup.cs` (только ProTV detection, без LTCGI)
- `Editor/Tools/SceneMaterialAnalyzer.cs` → `Editor/Tools/SceneMaterialAnalyzer.cs` (без переименования класса)
- `Editor/Tools/SceneMaterialInspector.cs` → `Editor/Tools/SceneMaterialInspector.cs` (без переименования класса)

#### Трансформации при копировании (применять ко всем файлам):
1. `using` и namespace: `PuruSignals` → `PuruWorldUtils`
2. Имена классов: `PSS_` → `PWU_` (везде в файле)
3. `[AddComponentMenu("PSS/...")]` → `[AddComponentMenu("PWU/...")]`
4. `Tools > PSS > Spawn` → `Tools > World Utils > Spawn` (в SpawnMenu)
5. Строки вида `"PSS_..."` (nameof, string literals) → `"PWU_..."`

---

### Фаза 2 — Убрать PSS-поля из 8 классов (ПОСЛЕ копирования в PWU)

Поля для удаления из уже переименованных файлов в PWU:

| Файл в PWU | Поля для удаления |
|---|---|
| `Runtime/Persistence/PWU_PositionPersistence.cs` | `public PSS_ChannelLocal onRestoredChannel` |
| `Runtime/Access/PWU_AdminVisibility.cs` | `public PSS_ChannelLocal onAdminChannel` |
| `Runtime/Access/PWU_AdminVisibilityFull.cs` | `public PSS_ChannelLocal onAdminChannel`, `public PSS_ChannelLocal onNonAdminChannel` |
| `Runtime/Access/PWU_InstanceOwnerVisibility.cs` | `public PSS_ChannelLocal onOwnerChannel`, `public PSS_ChannelLocal onNonOwnerChannel` |
| `Runtime/Access/PWU_MasterVisibility.cs` | `public PSS_ChannelLocal onMasterChannel`, `public PSS_ChannelLocal onNonMasterChannel` |
| `Runtime/Access/PWU_ZoneAdminVisibility.cs` | `public PSS_ChannelLocal onAdminChannel` |
| `Runtime/Access/PWU_ZoneAdminVisibilityFull.cs` | `public PSS_ChannelLocal onAdminChannel`, `public PSS_ChannelLocal onNonAdminChannel` |
| `Runtime/Select/PWU_MultiSelectController.cs` | `public PSS_ChannelLocal[] onSelectChannels` + вся логика вызова в `Apply()` |

Также убрать все `using PuruSignals;` если остались после чистки полей.

---

### ~~Фаза 3 — Почистить PSS~~ ГОТОВО (PSS v0.2.0)

Удалить из `x:\Сlaude\Unity\Puru_Signals_System\`:
- `Modules/Standalone Utilities/` — весь каталог
- `Modules/ProTV/` — весь каталог
- `Editor/PSS_SpawnMenu.cs` + `.meta`
- `Editor/PSS_StandaloneUtilityEditor.cs` + `.meta`
- `Editor/Generated/` — все `*Editor.cs` для standalone (23 файла) + папка если пустая
- `Editor/Tools/SceneMaterialAnalyzer.cs` + `.cs.meta`
- `Editor/Tools/SceneMaterialInspector.cs` + `.cs.meta`
- `Editor/Tools/` папку + `.meta` если пустая после удаления

Обновить в PSS:
- `Editor/PSS_AutoSetup.cs` — убрать ProTV detection (`PSS_PROTV_INSTALLED`), оставить только LTCGI
- `_gen_meta_assets.py` — убрать все строки `Modules/Standalone Utilities/...` и `Modules/ProTV/...`
- `_validate_release.py` — убрать `"Modules/ProTV"` из `CONDITIONAL_DIRS`, убрать `"Modules/Standalone Utilities/"` из SKIP_ASSET если там было
- `Docs/modules.md` — удалить секции Standalone Utilities и ProTV
- `Docs/STANDALONE_UTILITIES.md` — удалить файл (переехал в PWU)
- `package.json` — обновить description (убрать "with Standalone Utilities"), bump version
- `CHANGELOG.md` — добавить запись о выделении standalone в PWU

---

### После всех фаз

1. Инициализировать git в `Puru_World_Utils/`, создать репо на GitHub `puru-world-utils`
2. Запустить `_gen_meta_assets.py` из `Puru_World_Utils/` — создаст .meta и .asset файлы
3. Открыть в Unity, создать UdonSharpAssemblyDefinition для runtime и protv asmdef
4. Записать GUIDs в `NOTES.md`
5. Первый релиз: v0.1.0
6. Удалить этот файл (`WORK_IN_PROGRESS.md`)
