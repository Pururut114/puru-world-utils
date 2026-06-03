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
| `Runtime/com.pururut.pwu.runtime.asset` | *(заполнить после первого Unity import)* |
| `ProTV/com.pururut.pwu.protv.asset` | *(заполнить после первого Unity import)* |

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

## Checklist нового компонента

- Файл в `Runtime/<Category>/PWU_<Name>.cs`
- Наследование от `UdonSharpBehaviour`, атрибут `[UdonBehaviourSyncMode]`
- `[AddComponentMenu("PWU/<Category>/PWU_<Name> [Utility]")]`
- `[MenuItem]` в `Editor/PWU_SpawnMenu.cs`
- Строка в `Docs/components.md`
- `_gen_meta_assets.py` — добавить путь в `BEHAVIOURS`, запустить
- `package.json` + `CHANGELOG.md` обновить
