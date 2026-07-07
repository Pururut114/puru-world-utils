using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PuruWorldUtils.Editor
{
    public class TerrainToMeshWindow : EditorWindow
    {
        private Terrain _terrain;
        private int _chunksX = 4;
        private int _chunksZ = 4;
        private int _downsample = 2;
        private bool _addMeshCollider = true;
        private bool _generateLightmapUV = true;
        private bool _bakeSplatMask = true;
        private bool _createMaterial = true;
        private string _outputFolder = "Assets/Generated/TerrainMesh";

        private Texture2D _lastBakedMask;
        private Material _lastMaterial;
        private readonly List<GameObject> _lastChunks = new List<GameObject>();

        [MenuItem("Tools/World Utils/Terrain To Mesh")]
        public static void Open()
        {
            GetWindow<TerrainToMeshWindow>("Terrain To Mesh");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);
            _terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", _terrain, typeof(Terrain), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mesh Settings", EditorStyles.boldLabel);
            _downsample = Mathf.Max(1, EditorGUILayout.IntField(new GUIContent("Downsample Step", "1 = full heightmap resolution, 2 = каждая 2-я точка, и т.д."), _downsample));
            _chunksX = Mathf.Max(1, EditorGUILayout.IntField("Chunks X", _chunksX));
            _chunksZ = Mathf.Max(1, EditorGUILayout.IntField("Chunks Z", _chunksZ));
            _addMeshCollider = EditorGUILayout.Toggle("Add Mesh Collider", _addMeshCollider);
            _generateLightmapUV = EditorGUILayout.Toggle(new GUIContent("Generate Lightmap UV (UV2)", "Unwrapping.GenerateSecondaryUVSet, нужно для baked lightmapping"), _generateLightmapUV);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Splat / Material", EditorStyles.boldLabel);
            _bakeSplatMask = EditorGUILayout.Toggle(new GUIContent("Bake Splat Mask (up to 4 layers)", "Читает TerrainData.GetAlphamaps, пишет RGBA PNG (R/G/B/A = вес слоя 0..3)"), _bakeSplatMask);
            using (new EditorGUI.DisabledScope(!_bakeSplatMask))
            {
                _createMaterial = EditorGUILayout.Toggle("Auto-create Blend Material", _createMaterial);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            _outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);
            if (GUILayout.Button("...", GUILayout.Width(28)))
            {
                string picked = EditorUtility.SaveFolderPanel("Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(picked))
                {
                    _outputFolder = ToRelativeAssetPath(picked);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_terrain == null))
            {
                if (GUILayout.Button("Convert Terrain To Mesh", GUILayout.Height(32)))
                {
                    Convert();
                }

                using (new EditorGUI.DisabledScope(_lastChunks.Count == 0))
                {
                    if (GUILayout.Button("Export Last Result To OBJ"))
                    {
                        ExportChunksToObj(_lastChunks, _outputFolder + "/OBJ_Export");
                    }
                }
            }

            if (_terrain != null)
            {
                EditorGUILayout.HelpBox(EstimateInfo(), MessageType.Info);
            }
        }

        private string EstimateInfo()
        {
            TerrainData data = _terrain.terrainData;
            int res = data.heightmapResolution;
            int totalSamples = ((res - 1) / _downsample) + 1;
            int approxVertsPerChunkX = Mathf.CeilToInt((float)totalSamples / _chunksX) + 1;
            int approxVertsPerChunkZ = Mathf.CeilToInt((float)totalSamples / _chunksZ) + 1;
            return $"Heightmap res: {res}x{res}. После даунсемплинга: ~{totalSamples}x{totalSamples} точек.\n" +
                   $"~{approxVertsPerChunkX}x{approxVertsPerChunkZ} вершин на чанк ({_chunksX * _chunksZ} чанков всего).";
        }

        private static string ToRelativeAssetPath(string absolutePath)
        {
            absolutePath = absolutePath.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');
            if (absolutePath.StartsWith(dataPath))
            {
                return "Assets" + absolutePath.Substring(dataPath.Length);
            }
            Debug.LogWarning("Terrain To Mesh: выбранная папка не внутри Assets, оставляю дефолтный путь.");
            return "Assets/Generated/TerrainMesh";
        }

        private void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private void Convert()
        {
            if (_terrain == null || _terrain.terrainData == null)
            {
                Debug.LogError("Terrain To Mesh: терраин не назначен.");
                return;
            }

            EnsureFolder(_outputFolder);

            _lastChunks.Clear();
            _lastBakedMask = null;
            _lastMaterial = null;

            TerrainData data = _terrain.terrainData;

            if (_bakeSplatMask)
            {
                _lastBakedMask = BakeSplatMask(data, _terrain.name, _outputFolder);
            }

            if (_bakeSplatMask && _createMaterial)
            {
                _lastMaterial = CreateBlendMaterial(data, _terrain.name, _outputFolder, _lastBakedMask);
            }

            GameObject root = new GameObject($"TerrainMesh_{_terrain.name}");
            Undo.RegisterCreatedObjectUndo(root, "Terrain To Mesh");

            int[] xIdx = BuildSampleIndices(data.heightmapResolution, _downsample);
            int[] zIdx = BuildSampleIndices(data.heightmapResolution, _downsample);

            int[] xSegStarts = SplitSegments(xIdx.Length, _chunksX);
            int[] zSegStarts = SplitSegments(zIdx.Length, _chunksZ);

            Vector3 terrainPos = _terrain.transform.position;
            Vector3 size = data.size;

            float[,] heights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);

            int chunkCount = 0;
            for (int cz = 0; cz < _chunksZ; cz++)
            {
                int zStart = zSegStarts[cz];
                int zEnd = zSegStarts[cz + 1];
                if (zEnd <= zStart) continue;

                for (int cx = 0; cx < _chunksX; cx++)
                {
                    int xStart = xSegStarts[cx];
                    int xEnd = xSegStarts[cx + 1];
                    if (xEnd <= xStart) continue;

                    GameObject chunk = BuildChunk(
                        data, terrainPos, size, heights,
                        xIdx, zIdx, xStart, xEnd, zStart, zEnd,
                        $"Chunk_{cx}_{cz}", _outputFolder, _terrain.name,
                        _lastMaterial);

                    chunk.transform.SetParent(root.transform, true);
                    _lastChunks.Add(chunk);
                    chunkCount++;
                }
            }

            Debug.Log($"Terrain To Mesh: готово, {chunkCount} чанков в '{root.name}'.");
            Selection.activeGameObject = root;
        }

        private static int[] BuildSampleIndices(int heightmapResolution, int step)
        {
            int last = heightmapResolution - 1;
            List<int> list = new List<int>();
            for (int i = 0; i <= last; i += step)
            {
                list.Add(i);
            }
            if (list[list.Count - 1] != last)
            {
                list.Add(last);
            }
            return list.ToArray();
        }

        // Делит [0, sampleCount-1] индексов на chunkCount сегментов, границы разделяются
        // между соседними чанками (общая вершина = бесшовный стык).
        private static int[] SplitSegments(int sampleCount, int chunkCount)
        {
            int segments = sampleCount - 1;
            chunkCount = Mathf.Clamp(chunkCount, 1, Mathf.Max(1, segments));
            int[] starts = new int[chunkCount + 1];
            for (int c = 0; c <= chunkCount; c++)
            {
                starts[c] = Mathf.RoundToInt((float)c * segments / chunkCount);
            }
            return starts;
        }

        private GameObject BuildChunk(
            TerrainData data, Vector3 terrainPos, Vector3 size, float[,] heights,
            int[] xIdx, int[] zIdx, int xStartSeg, int xEndSeg, int zStartSeg, int zEndSeg,
            string chunkName, string outputFolder, string terrainName,
            Material material)
        {
            int xCount = xEndSeg - xStartSeg + 1;
            int zCount = zEndSeg - zStartSeg + 1;
            int vertCount = xCount * zCount;

            Vector3[] vertices = new Vector3[vertCount];
            Vector2[] worldUV = new Vector2[vertCount]; // мировые координаты в метрах, канал uv (channel 0) — тайлинг текстур + источник для RecalculateTangents
            Vector2[] maskUV = new Vector2[vertCount];  // глобальные 0..1 координаты, канал uv3 (channel 2) — сплат-маска. uv2 (channel 1) не трогаем, туда пишет GenerateSecondaryUVSet (lightmap)

            int res = data.heightmapResolution;
            float invResX = 1f / (res - 1);
            float invResZ = 1f / (res - 1);

            Vector3 origin = default;
            bool originSet = false;

            for (int rz = 0; rz < zCount; rz++)
            {
                int zSample = zIdx[zStartSeg + rz];
                float worldZ = terrainPos.z + zSample * invResZ * size.z;

                for (int rx = 0; rx < xCount; rx++)
                {
                    int xSample = xIdx[xStartSeg + rx];
                    float worldX = terrainPos.x + xSample * invResX * size.x;
                    float worldY = terrainPos.y + heights[zSample, xSample] * size.y;

                    if (!originSet)
                    {
                        origin = new Vector3(worldX, worldY, worldZ);
                        originSet = true;
                    }

                    int vi = rz * xCount + rx;
                    vertices[vi] = new Vector3(worldX - origin.x, worldY - origin.y, worldZ - origin.z);
                    worldUV[vi] = new Vector2(worldX, worldZ);
                    maskUV[vi] = new Vector2((worldX - terrainPos.x) / size.x, (worldZ - terrainPos.z) / size.z);
                }
            }

            int quadCountX = xCount - 1;
            int quadCountZ = zCount - 1;
            int[] triangles = new int[quadCountX * quadCountZ * 6];
            int ti = 0;
            for (int rz = 0; rz < quadCountZ; rz++)
            {
                for (int rx = 0; rx < quadCountX; rx++)
                {
                    int i = rz * xCount + rx;
                    triangles[ti++] = i;
                    triangles[ti++] = i + xCount;
                    triangles[ti++] = i + 1;

                    triangles[ti++] = i + 1;
                    triangles[ti++] = i + xCount;
                    triangles[ti++] = i + xCount + 1;
                }
            }

            Mesh mesh = new Mesh { name = $"{terrainName}_{chunkName}" };
            mesh.indexFormat = vertCount > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, worldUV);
            mesh.SetUVs(2, maskUV);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();

            // Самопроверка направления нормалей: терраин должен смотреть вверх.
            Vector3[] normalsCheck = mesh.normals;
            float upDot = 0f;
            for (int i = 0; i < normalsCheck.Length; i += Mathf.Max(1, normalsCheck.Length / 8))
            {
                upDot += Vector3.Dot(normalsCheck[i], Vector3.up);
            }
            if (upDot < 0f)
            {
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int tmp = triangles[i + 1];
                    triangles[i + 1] = triangles[i + 2];
                    triangles[i + 2] = tmp;
                }
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            if (_generateLightmapUV)
            {
                Unwrapping.GenerateSecondaryUVSet(mesh);
            }

            string meshPath = AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/{terrainName}_{chunkName}.asset");
            AssetDatabase.CreateAsset(mesh, meshPath);

            GameObject go = new GameObject(chunkName);
            go.transform.position = origin;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            if (material != null)
            {
                mr.sharedMaterial = material;
            }
            if (_addMeshCollider)
            {
                MeshCollider mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh;
            }

            return go;
        }

        private Texture2D BakeSplatMask(TerrainData data, string terrainName, string outputFolder)
        {
            int layerCount = data.terrainLayers != null ? data.terrainLayers.Length : 0;
            if (layerCount == 0)
            {
                Debug.LogWarning("Terrain To Mesh: у террейна нет TerrainLayers, маска не запечена.");
                return null;
            }
            if (layerCount > 4)
            {
                Debug.LogWarning($"Terrain To Mesh: у террейна {layerCount} слоёв, шейдер Puru/TerrainBlend4 поддерживает только первые 4.");
            }

            int alphaRes = data.alphamapResolution;
            float[,,] alphamaps = data.GetAlphamaps(0, 0, alphaRes, alphaRes);
            int usedLayers = Mathf.Min(4, layerCount);

            Texture2D mask = new Texture2D(alphaRes, alphaRes, TextureFormat.RGBA32, true);
            Color[] pixels = new Color[alphaRes * alphaRes];
            for (int y = 0; y < alphaRes; y++)
            {
                for (int x = 0; x < alphaRes; x++)
                {
                    float r = usedLayers > 0 ? alphamaps[y, x, 0] : 0f;
                    float g = usedLayers > 1 ? alphamaps[y, x, 1] : 0f;
                    float b = usedLayers > 2 ? alphamaps[y, x, 2] : 0f;
                    float a = usedLayers > 3 ? alphamaps[y, x, 3] : 0f;
                    pixels[y * alphaRes + x] = new Color(r, g, b, a);
                }
            }
            mask.SetPixels(pixels);
            mask.Apply();

            byte[] png = mask.EncodeToPNG();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/{terrainName}_SplatMask.png");
            string fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllBytes(fullPath, png);
            Object.DestroyImmediate(mask);

            AssetDatabase.ImportAsset(assetPath);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.sRGBTexture = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        private Material CreateBlendMaterial(TerrainData data, string terrainName, string outputFolder, Texture2D mask)
        {
            Shader shader = Shader.Find("Puru/TerrainBlend4");
            if (shader == null)
            {
                Debug.LogError("Terrain To Mesh: шейдер Puru/TerrainBlend4 не найден в проекте.");
                return null;
            }

            Material mat = new Material(shader) { name = $"{terrainName}_TerrainBlend" };
            mat.SetTexture("_MaskTex", mask);

            TerrainLayer[] layers = data.terrainLayers;
            int usedLayers = Mathf.Min(4, layers != null ? layers.Length : 0);
            for (int i = 0; i < usedLayers; i++)
            {
                TerrainLayer layer = layers[i];
                string albedoProp = "_Texture" + i;
                string normalProp = "_Normal" + i;

                mat.SetTexture(albedoProp, layer.diffuseTexture);
                mat.SetTexture(normalProp, layer.normalMapTexture);

                // Шейдер сэмплит normal map той же трансформированной UV, что и albedo
                // (uv_Texture0..3) — свой _ST у _NormalN не читается, тайлинг задавать
                // только через albedo-свойство.
                Vector2 tileSize = layer.tileSize;
                Vector2 scale = new Vector2(
                    tileSize.x > 0.0001f ? 1f / tileSize.x : 1f,
                    tileSize.y > 0.0001f ? 1f / tileSize.y : 1f);
                mat.SetTextureScale(albedoProp, scale);
            }

            string matPath = AssetDatabase.GenerateUniqueAssetPath($"{outputFolder}/{terrainName}_TerrainBlend.mat");
            AssetDatabase.CreateAsset(mat, matPath);
            return mat;
        }

        private static void ExportChunksToObj(List<GameObject> chunks, string outputFolder)
        {
            string absoluteFolder = Path.Combine(Application.dataPath, outputFolder.Substring("Assets/".Length));
            Directory.CreateDirectory(absoluteFolder);

            foreach (GameObject chunk in chunks)
            {
                MeshFilter mf = chunk.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;

                string path = Path.Combine(absoluteFolder, chunk.name + ".obj");
                WriteObj(mf.sharedMesh, path);
            }

            AssetDatabase.Refresh();
            Debug.Log($"Terrain To Mesh: экспортировано {chunks.Count} OBJ в {outputFolder}");
        }

        // Unity — левая система координат, OBJ трактуется как правая: зеркалим X
        // и разворачиваем winding, чтобы геометрия не была вывернута при реимпорте.
        private static void WriteObj(Mesh mesh, string path)
        {
            Vector3[] verts = mesh.vertices;
            Vector3[] normals = mesh.normals;
            List<Vector2> uvList = new List<Vector2>();
            mesh.GetUVs(2, uvList); // маска-UV (0..1), нормальный референс для Blender — не сырые мировые метры из channel 0
            Vector2[] uvs = uvList.Count == mesh.vertexCount ? uvList.ToArray() : mesh.uv;
            int[] tris = mesh.triangles;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Puru Terrain To Mesh export");
            sb.AppendLine("o " + mesh.name);

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 v = verts[i];
                sb.AppendLine($"v {(-v.x).ToString("F6")} {v.y.ToString("F6")} {v.z.ToString("F6")}");
            }
            for (int i = 0; i < uvs.Length; i++)
            {
                Vector2 uv = uvs[i];
                sb.AppendLine($"vt {uv.x.ToString("F6")} {uv.y.ToString("F6")}");
            }
            bool hasNormals = normals != null && normals.Length == verts.Length;
            if (hasNormals)
            {
                for (int i = 0; i < normals.Length; i++)
                {
                    Vector3 n = normals[i];
                    sb.AppendLine($"vn {(-n.x).ToString("F6")} {n.y.ToString("F6")} {n.z.ToString("F6")}");
                }
            }

            bool hasUv = uvs != null && uvs.Length == verts.Length;
            for (int i = 0; i < tris.Length; i += 3)
            {
                int a = tris[i] + 1;
                int b = tris[i + 1] + 1;
                int c = tris[i + 2] + 1;
                // reversed winding (b<->c) компенсирует зеркалирование X
                sb.AppendLine("f " + FaceVertex(a, hasUv, hasNormals) + " " + FaceVertex(c, hasUv, hasNormals) + " " + FaceVertex(b, hasUv, hasNormals));
            }

            File.WriteAllText(path, sb.ToString());
        }

        private static string FaceVertex(int index, bool hasUv, bool hasNormal)
        {
            if (hasUv && hasNormal) return $"{index}/{index}/{index}";
            if (hasUv) return $"{index}/{index}";
            if (hasNormal) return $"{index}//{index}";
            return index.ToString();
        }
    }
}
