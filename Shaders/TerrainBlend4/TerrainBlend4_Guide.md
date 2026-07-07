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
| `_MetallicSmoothness0..3` | Опциональная карта на каждый слой: R = metallic, A = smoothness. Дефолт — белая текстура (карта не меняет поведение, всё решают слайдеры ниже). Не тянется с террейна — чисто ручная настройка в материале, сэмплится той же UV (`uv_TextureN`), что и albedo/normal этого слоя — общий Tiling/Offset с albedo. |
| `_Metallic0..3` | Множитель на R-канал соответствующей карты. Без карты (белая) — это и есть итоговый metallic слоя. |
| `_Smoothness0..3` | Множитель на A-канал соответствующей карты. Без карты — итоговый smoothness слоя. |
| `_NormalStrength` | Множитель на XY нормали после блендинга (усиление/ослабление рельефа), общий на весь материал |

## Как блендится

```
w = mask / (mask.r + mask.g + mask.b + mask.a)   // нормализация весов
albedo = albedo0*w.r + albedo1*w.g + albedo2*w.b + albedo3*w.a
normal = normalize(n0*w.r + n1*w.g + n2*w.b + n3*w.a)
metallic   = (ms0.r*_Metallic0)*w.r + (ms1.r*_Metallic1)*w.g + (ms2.r*_Metallic2)*w.b + (ms3.r*_Metallic3)*w.a
smoothness = (ms0.a*_Smoothness0)*w.r + (ms1.a*_Smoothness1)*w.g + (ms2.a*_Smoothness2)*w.b + (ms3.a*_Smoothness3)*w.a
```

Metallic/smoothness каждого слоя считается независимо (карта × свой слайдер), а затем блендится теми же весами маски, что и albedo/normal — то есть на стыке слоёв металличность плавно переходит, как и цвет.

Веса нормализуются на случай, если маска не строго в сумме даёт 1 (баунд-эффекты сжатия PNG, если маска когда-то будет сжата с потерями — сейчас Terrain To Mesh пишет Uncompressed, так что это скорее защита на будущее).

## Ограничения

- Ровно 4 слоя, жёстко (не N). Больше слоёв на терраине — старшие обрежутся при запекании маски (см. GUIDE тула).
- `_NormalStrength` общий на весь материал (не per-layer) — остальное (albedo/normal/metallic/smoothness) уже per-layer.
- Не поддерживает Height-based blending (резкие переходы между слоями, без учёта высоты/наклона) — только чистый alpha-blend по маске, как есть в исходном сплате террейна.

## Файлы

```
TerrainBlend4/
├── TerrainBlend4.shader       — шейдер
└── TerrainBlend4_Guide.md     — этот файл
```

Companion tool: `scripts/editor/TerrainToMesh/`
