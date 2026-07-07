# TerrainBlend4 — Usage Guide

Простой surface shader: блендит 4 albedo+normal текстуры по RGBA-маске. Заточен под меши, которые генерирует **Terrain To Mesh** (`Puru World Tools → Terrain To Mesh`), но подойдёт под любой меш с той же UV-раскладкой.

Built-in RP, `#pragma surface ... Standard` — lightmap/GI/reflection probes работают из коробки (обычный Standard lighting model). Без Quest/mobile оптимизации.

## UV-контракт

Шейдер ждёт конкретную раскладку UV, не совпадающую с обычным мешем:

| Канал | Что там | Как читается в шейдере |
|-------|---------|------------------------|
| uv (channel 0 / TEXCOORD0) | Мировые координаты в метрах (не 0..1!) | `uv_Texture0..3` → `_Texture0..3` / `_Normal0..3` |
| uv2 (channel 1 / TEXCOORD1) | Не используется шейдером (зарезервирован под lightmap UV) | — |
| uv3 (channel 2 / TEXCOORD2) | Глобальные 0..1 координаты по всему объекту (не тайлятся) | вручную в `vert()` → `IN.maskUV` → `_MaskTex` |

Тайлинг текстур регулируется **Tiling** в инспекторе материала (обычный Unity Texture Scale/Offset) — не свойством шейдера, потому что сурфейс-шейдер сам подставляет `_TextureN_ST` в `uv_TextureN`. Меняешь тайлинг — не нужно перегенерировать меш.

**Почему маска не на uv (channel 0):** `Mesh.RecalculateTangents()` в Unity всегда берёт UV с channel 0 для расчёта tangent-базиса. Если туда положить 0..1 координату маски (а не ту UV, по которой реально сэмплится normal map), тангенты будут смотреть не туда — normal mapping перекашивается, визуально похоже на шумные тёмные пятна по всей поверхности. Поэтому channel 0 держит именно ту UV, что использует `_TextureN`/`_NormalN`, а маска вынесена на uv3 и читается вручную (без авто-конвенции `uv3_`, она не гарантирована генератором surface-шейдеров).

**Почему не uv2 (channel 1):** это канал `Unwrapping.GenerateSecondaryUVSet` (lightmap). Если положить туда что-то своё — при генерации lightmap UV оно будет затёрто.

Если генерируешь меш не через Terrain To Mesh — повтори эту раскладку: channel 0 = мировые координаты, channel 2 (uv3) = 0..1 маска, channel 1 (uv2) не занимать.

## Properties

| Property | Описание |
|----------|----------|
| `_MaskTex` | RGBA-маска весов слоёв (R=слой0, G=слой1, B=слой2, A=слой3). Генерируется Terrain To Mesh из `TerrainData.GetAlphamaps`. |
| `_Texture0..3` | Albedo каждого слоя |
| `_Normal0..3` | Normal map каждого слоя (tangent space, `bump` дефолт) |
| `_MetallicSmoothnessMap` | Опциональная общая (не per-layer) карта: R = metallic, A = smoothness. Дефолт — белая текстура, то есть по умолчанию карта не меняет поведение, всё решают слайдеры ниже. Не тянется с террейна — чисто ручная настройка в материале, сэмплится по тем же мировым координатам (channel 0), что и albedo, со своим Tiling/Offset. |
| `_Metallic` | Множитель на R-канал карты. Без карты (белая) — это и есть итоговый metallic. |
| `_Smoothness` | Множитель на A-канал карты. Без карты — итоговый smoothness, один слайдер на все 4 слоя (не per-layer). |
| `_NormalStrength` | Множитель на XY нормали после блендинга (усиление/ослабление рельефа) |

## Как блендится

```
w = mask / (mask.r + mask.g + mask.b + mask.a)   // нормализация весов
albedo = albedo0*w.r + albedo1*w.g + albedo2*w.b + albedo3*w.a
normal = normalize(n0*w.r + n1*w.g + n2*w.b + n3*w.a)
```

Веса нормализуются на случай, если маска не строго в сумме даёт 1 (баунд-эффекты сжатия PNG, если маска когда-то будет сжата с потерями — сейчас Terrain To Mesh пишет Uncompressed, так что это скорее защита на будущее).

## Ограничения

- Ровно 4 слоя, жёстко (не N). Больше слоёв на терраине — старшие обрежутся при запекании маски (см. GUIDE тула).
- `_MetallicSmoothnessMap`/`_Smoothness`/`_NormalStrength` общие на весь материал, не per-layer — осознанное упрощение ("простенький шейдерок"), если понадобится per-layer — расширять через доп. каналы маски или отдельные свойства.
- Не поддерживает Height-based blending (резкие переходы между слоями, без учёта высоты/наклона) — только чистый alpha-blend по маске, как есть в исходном сплате террейна.

## Файлы

```
TerrainBlend4/
├── TerrainBlend4.shader       — шейдер
└── TerrainBlend4_Guide.md     — этот файл
```

Companion tool: `scripts/editor/TerrainToMesh/`
