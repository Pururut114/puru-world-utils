using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace PuruWorldUtils.Editor
{
    public class SceneMaterialAnalyzer : EditorWindow
    {
        private class TextureInfo
        {
            public string propertyName;
            public Texture texture;
            public long memoryBytes;
        }

        private class MaterialInfo
        {
            public Material material;
            public List<Renderer> renderers = new List<Renderer>();
            public List<TextureInfo> textures = new List<TextureInfo>();
            public long totalTextureMemory;
            public bool foldout;
        }

        private readonly List<MaterialInfo> _materials = new List<MaterialInfo>();
        private Vector2 _scroll;
        private string _search = "";
        private SortMode _sortMode = SortMode.ByMemoryDesc;

        private enum SortMode { ByName, ByMemoryDesc }

        [MenuItem("Tools/Scene Materials/Analyzer")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneMaterialAnalyzer>("Scene Materials");
            window.minSize = new Vector2(520, 350);
        }

        private void OnGUI()
        {
            DrawToolbar();
            GUILayout.Space(10);

            if (_materials.Count == 0)
            {
                EditorGUILayout.HelpBox("Нажми «Сканировать сцену», чтобы собрать материалы.", MessageType.Info);
                return;
            }

            DrawSummary();
            GUILayout.Space(10);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var info in FilteredAndSorted())
                DrawMaterialEntry(info);
            EditorGUILayout.EndScrollView();
        }

        // ── Toolbar ───────────────────────────────────────────────────────────

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Сканировать сцену", EditorStyles.toolbarButton, GUILayout.Width(150)))
                ScanScene();

            GUILayout.Space(10);
            _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _search = "";
                GUI.FocusControl(null);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Сортировка:", GUILayout.Width(70));
            _sortMode = (SortMode)EditorGUILayout.EnumPopup(_sortMode, GUILayout.Width(140));

            EditorGUILayout.EndHorizontal();
        }

        // ── Summary ───────────────────────────────────────────────────────────

        private void DrawSummary()
        {
            long totalBytes = 0;
            foreach (var m in _materials) totalBytes += m.totalTextureMemory;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Материалов: {_materials.Count}");
            EditorGUILayout.LabelField($"Суммарный вес текстур: {FormatBytes(totalBytes)}");
            EditorGUILayout.EndVertical();
        }

        // ── Material entry ────────────────────────────────────────────────────

        private void DrawMaterialEntry(MaterialInfo info)
        {
            if (info.material == null) return;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            info.foldout = EditorGUILayout.Foldout(info.foldout, info.material.name, true);
            GUILayout.FlexibleSpace();
            GUILayout.Label(FormatBytes(info.totalTextureMemory), GUILayout.Width(90));
            if (GUILayout.Button("Выбрать объекты", GUILayout.Width(130))) SelectObjects(info);
            if (GUILayout.Button("Показать в Project", GUILayout.Width(140))) EditorGUIUtility.PingObject(info.material);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.ObjectField("Материал", info.material, typeof(Material), false);
            EditorGUILayout.LabelField("Шейдер: " + info.material.shader.name);
            EditorGUILayout.LabelField("Используется объектами: " + info.renderers.Count);

            if (info.foldout)
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField("Текстуры:", EditorStyles.boldLabel);

                if (info.textures.Count == 0)
                {
                    EditorGUILayout.LabelField("Нет текстур");
                }
                else
                {
                    foreach (var t in info.textures)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(t.texture, typeof(Texture), false,
                            GUILayout.Width(80), GUILayout.Height(80));
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField($"Слот: {t.propertyName}");
                        EditorGUILayout.LabelField($"Память: {FormatBytes(t.memoryBytes)}");
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        // ── Scan ─────────────────────────────────────────────────────────────

        private void ScanScene()
        {
            _materials.Clear();
            var lookup = new Dictionary<Material, MaterialInfo>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    ScanHierarchy(root.transform, lookup);
            }

            foreach (var kv in lookup)
            {
                CollectTextures(kv.Value);
                _materials.Add(kv.Value);
            }

            Repaint();
        }

        private void ScanHierarchy(Transform t, Dictionary<Material, MaterialInfo> lookup)
        {
            var r = t.GetComponent<Renderer>();
            if (r != null)
            {
                foreach (var m in r.sharedMaterials)
                {
                    if (m == null) continue;
                    if (!lookup.TryGetValue(m, out var info))
                    {
                        info = new MaterialInfo { material = m };
                        lookup[m] = info;
                    }
                    if (!info.renderers.Contains(r))
                        info.renderers.Add(r);
                }
            }

            for (int i = 0; i < t.childCount; i++)
                ScanHierarchy(t.GetChild(i), lookup);
        }

        private void CollectTextures(MaterialInfo info)
        {
            info.textures.Clear();
            info.totalTextureMemory = 0;
            if (info.material == null) return;

            var shader = info.material.shader;
            int count = shader.GetPropertyCount();

            for (int i = 0; i < count; i++)
            {
                if (shader.GetPropertyType(i) != ShaderPropertyType.Texture) continue;
                string prop = shader.GetPropertyName(i);
                Texture tex = info.material.GetTexture(prop);
                if (tex == null) continue;

                long bytes = 0;
                try { bytes = Profiler.GetRuntimeMemorySizeLong(tex); } catch { }

                info.textures.Add(new TextureInfo { propertyName = prop, texture = tex, memoryBytes = bytes });
                info.totalTextureMemory += bytes;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private IEnumerable<MaterialInfo> FilteredAndSorted()
        {
            var list = new List<MaterialInfo>();
            string s = string.IsNullOrWhiteSpace(_search) ? null : _search.ToLowerInvariant();

            foreach (var m in _materials)
            {
                if (s != null && !m.material.name.ToLowerInvariant().Contains(s)) continue;
                list.Add(m);
            }

            if (_sortMode == SortMode.ByName)
                list.Sort((a, b) => string.Compare(a.material.name, b.material.name, true));
            else
                list.Sort((a, b) => b.totalTextureMemory.CompareTo(a.totalTextureMemory));

            return list;
        }

        private void SelectObjects(MaterialInfo info)
        {
            var list = new List<Object>();
            foreach (var r in info.renderers)
                if (r != null) list.Add(r.gameObject);
            Selection.objects = list.ToArray();
            if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.FrameSelected();
        }

        private static string FormatBytes(long b)
        {
            if (b <= 0) return "0 B";
            const long KB = 1024, MB = KB * 1024, GB = MB * 1024;
            if (b >= GB) return $"{b / (float)GB:0.##} GB";
            if (b >= MB) return $"{b / (float)MB:0.##} MB";
            if (b >= KB) return $"{b / (float)KB:0.##} KB";
            return b + " B";
        }
    }
}
