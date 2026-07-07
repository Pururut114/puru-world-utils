# PWU — Оперативные заметки

## GitHub

- **Репо:** https://github.com/Pururut114/puru-world-utils
- **Публичное,** MIT лицензия
- **VPM listing:** https://Pururut114.github.io/puru-world-utils/index.json
- **VCC install URL:** `vcc://vpm/add-repo?url=https://Pururut114.github.io/puru-world-utils/index.json`
- **Package ID:** `com.pururut.world-utils`

---

## Рабочий процесс — новый релиз

```powershell
# 0. Прогнать валидатор
python _validate_release.py

# 1. Обновить version в package.json ПЕРВЫМ (до коммита и тега!)
# 2. Обновить CHANGELOG.md
git add .
git commit -m "release: PWU v0.X.X"
git push origin main
git tag v0.X.X
git push origin v0.X.X
# → release.yml создаёт zip + Release
# → build-listing.yml тригерится автоматически (workflow_run)
# → index.json обновляется на GitHub Pages
```

**Критично:** обновить `package.json` до создания тега.

---

## GitHub Actions

### `release.yml`
Триггер: `git push origin v*` → создаёт `.zip` + GitHub Release.

### `build-listing.yml`
Триггер: `workflow_run` после успешного `release.yml`, также `workflow_dispatch`.
Генерирует `index.json` → пушит в ветку `gh-pages`.

**Если race condition** (новый релиз не попал в index — запустить вручную):
```powershell
$TOKEN = "..."
Invoke-RestMethod -Method POST "https://api.github.com/repos/Pururut114/puru-world-utils/actions/workflows/build-listing.yml/dispatches" `
  -Headers @{"Authorization"="token $TOKEN"; "Accept"="application/vnd.github+json"} `
  -Body '{"ref":"main"}'
```

---

## VPM пакет — критические правила

Пакет ДОЛЖЕН включать:
1. `.meta` файлы для всех папок и файлов (стабильные GUIDs)
2. `UdonSharpProgramAsset` (`.asset`) рядом с каждым UdonSharpBehaviour `.cs` (кроме ProTV/)
3. `UdonSharpAssemblyDefinition` рядом с каждым `.asmdef` содержащим UdonSharpBehaviour скрипты

**UdonSharpAssemblyDefinition файлы:**

| Файл | sourceAssembly GUID |
|------|---------------------|
| `Runtime/com.pururut.pwu.runtime.asset` | `30e81ebcf56f4ca684b0ca3f2dcb1563` |
| `ProTV/com.pururut.pwu.protv.asset` | `62914fafdd734bf9bbf05078965e3758` |

- `m_Script` GUID UdonSharpAssemblyDefinition.cs: `5136146375e9a0a498a72a0091b40cc1`
- fileID для AssemblyDefinitionAsset ссылок: `5897886265953266890`
- `UdonSharpProgramAsset` GUID (из VRChat пакета): `c333ccfdd0cbdbc4ca30cef2dd6e6b9b`

**Генераторы (gitignored):**
- `_gen_meta_assets.py` — запускать при добавлении новых UdonSharpBehaviour скриптов
- `_validate_release.py` — проверяет версию, changelog, program assets, meta файлы

---

## Conditional assemblies — правило

- `ProTV/` имеет `defineConstraints: ["PWU_PROTV_INSTALLED"]`
- `UdonSharpProgramAsset` файлы для ProTV скриптов в репо **НЕ включать**
- `PWU_AutoSetup.cs` автоматически добавляет/убирает `PWU_PROTV_INSTALLED` при domain reload

---

## Assembly Definition структура

| Assembly | Папка | Назначение |
|----------|-------|------------|
| `com.pururut.pwu.runtime` | `Runtime/` | Все standalone утилиты (Zones, FX, Teleport, Access, Select, Persistence, Economy) |
| `com.pururut.pwu.protv` | `ProTV/` | ProTV интеграция, `defineConstraints: ["PWU_PROTV_INSTALLED"]` |
| `com.pururut.pwu.editor` | `Editor/` | SpawnMenu, StandaloneUtilityEditor, SceneMaterial tools, AutoSetup |

---

## Shaders/ (не asmdef, просто ассеты)

- `Shaders/<Name>/<Name>.shader` — обычные .shader файлы, не входят ни в один asmdef, компилятору Unity это не важно.
- Первый: `Shaders/TerrainBlend4/` — companion для `Editor/Tools/TerrainToMesh/` (Terrain To Mesh тула).

## .meta без открытого Unity

Когда репо не открыто как проект (нет живого Editor, который сам создаёт `.meta` на импорт), можно писать `.meta` руками — формат стабилен между версиями Unity, GUID просто должен быть уникальным 32-символьным hex (`uuid4().hex` подходит):

```yaml
# папка
fileFormatVersion: 2
guid: <32 hex>
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
```
```yaml
# .cs
fileFormatVersion: 2
guid: <32 hex>
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
```
```yaml
# .shader
fileFormatVersion: 2
guid: <32 hex>
ShaderImporter:
  externalObjects: {}
  defaultTextures: []
  nonModifiableTextures: []
  userData:
  assetBundleName:
  assetBundleVariant:
```
```yaml
# .md / любой обычный текстовый ассет
fileFormatVersion: 2
guid: <32 hex>
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
```

`_validate_release.py` подтверждает — "All N Unity-tracked files have .meta" проходит на руками написанных метах так же, как на настоящих.

## Checklist нового компонента (только для UdonSharpBehaviour, не для чистых Editor-тулов/шейдеров)

- Файл в `Runtime/<Category>/PWU_<Name>.cs`
- Наследование от `UdonSharpBehaviour`, атрибут `[UdonBehaviourSyncMode]`
- `[AddComponentMenu("PWU/<Category>/PWU_<Name> [Utility]")]`
- `[MenuItem]` в `Editor/PWU_SpawnMenu.cs`
- Строка в `Docs/components.md`
- `_gen_meta_assets.py` — добавить путь в `BEHAVIOURS`, запустить
- `package.json` + `CHANGELOG.md` обновить
