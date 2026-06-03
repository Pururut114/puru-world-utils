using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PuruWorldUtils.Editor
{
    public class SceneMaterialInspector : EditorWindow
    {
        private Vector2 _scroll;
        private string _search = "";

        private Dictionary<string, List<Material>> _shaderGroups   = new Dictionary<string, List<Material>>();
        private Dictionary<string, bool>           _groupFoldouts  = new Dictionary<string, bool>();
        private Dictionary<Material, bool>         _checkStates    = new Dictionary<Material, bool>();
        private Dictionary<Material, Material>     _replacements   = new Dictionary<Material, Material>();
        private Dictionary<Material, int>          _rendererCounts = new Dictionary<Material, int>();

        [MenuItem("Tools/Scene Materials/Inspector")]
        public static void ShowWindow() => GetWindow<SceneMaterialInspector>("Scene Materials");

        private void OnEnable() => Refresh();

        private void OnGUI()
        {
            DrawToolbar();
            GUILayout.Space(6);

            if (_shaderGroups.Count == 0)
            {
                EditorGUILayout.HelpBox("Материалы в сцене не найдены.", MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var group in _shaderGroups)
            {
                if (!GroupMatchesSearch(group.Value)) continue;
                DrawShaderGroup(group.Key, group.Value);
            }
            EditorGUILayout.EndScrollView();
        }

        // ── Toolbar ──────────────────────────────────────────────────────────

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Обновить список", EditorStyles.toolbarButton, GUILayout.Width(120)))
                Refresh();

            GUILayout.Space(6);
            _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _search = "";
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();
        }

        // ── Shader group ──────────────────────────────────────────────────────

        private void DrawShaderGroup(string shaderName, List<Material> materials)
        {
            var filtered = FilteredMaterials(materials);
            if (filtered.Count == 0) return;

            if (!_groupFoldouts.ContainsKey(shaderName))
                _groupFoldouts[shaderName] = true;

            _groupFoldouts[shaderName] = EditorGUILayout.Foldout(
                _groupFoldouts[shaderName],
                $"{shaderName}  ({filtered.Count})",
                true, EditorStyles.foldoutHeader);

            if (!_groupFoldouts[shaderName]) return;

            EditorGUI.indentLevel++;
            foreach (var mat in filtered)
                DrawMaterial(mat);
            EditorGUI.indentLevel--;
            GUILayout.Space(4);
        }

        // ── Material row ─────────────────────────────────────────────────────

        private void DrawMaterial(Material mat)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            if (!_checkStates.ContainsKey(mat)) _checkStates[mat] = false;
            _checkStates[mat] = GUILayout.Toggle(_checkStates[mat], GUIContent.none, GUILayout.Width(18));

            EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.MinWidth(100));

            int count = _rendererCounts.TryGetValue(mat, out var c) ? c : 0;
            GUILayout.Label($"×{count}", GUILayout.Width(32));
            GUILayout.Label("→", GUILayout.Width(15));

            if (!_replacements.ContainsKey(mat)) _replacements[mat] = null;
            _replacements[mat] = (Material)EditorGUILayout.ObjectField(_replacements[mat], typeof(Material), false);

            bool hasReplacement = _replacements[mat] != null;
            var prevColor = GUI.backgroundColor;
            GUI.backgroundColor = hasReplacement ? new Color(0.3f, 0.8f, 1f) : Color.grey;
            EditorGUI.BeginDisabledGroup(!hasReplacement);
            if (GUILayout.Button("Заменить", GUILayout.Width(80)))
                ReplaceMaterial(mat, _replacements[mat]);
            EditorGUI.EndDisabledGroup();
            GUI.backgroundColor = prevColor;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(24);
            if (GUILayout.Button("Выделить", GUILayout.Width(70)))
            {
                Selection.activeObject = mat;
                EditorGUIUtility.PingObject(mat);
            }
            if (GUILayout.Button("Найти объекты", GUILayout.Width(110)))
                SelectObjects(mat);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.Space(2);
        }

        // ── Data ─────────────────────────────────────────────────────────────

        private void Refresh()
        {
            _shaderGroups.Clear();
            _rendererCounts.Clear();

            var matRenderers = new Dictionary<Material, HashSet<Renderer>>();

            foreach (var r in Object.FindObjectsOfType<Renderer>())
            {
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat == null) continue;
                    if (!matRenderers.ContainsKey(mat))
                        matRenderers[mat] = new HashSet<Renderer>();
                    matRenderers[mat].Add(r);
                }
            }

            foreach (var kv in matRenderers)
            {
                _rendererCounts[kv.Key] = kv.Value.Count;
                string shaderName = kv.Key.shader != null ? kv.Key.shader.name : "Unknown";
                if (!_shaderGroups.ContainsKey(shaderName))
                    _shaderGroups[shaderName] = new List<Material>();
                _shaderGroups[shaderName].Add(kv.Key);
            }

            foreach (var key in _shaderGroups.Keys.ToList())
                _shaderGroups[key] = _shaderGroups[key].OrderBy(m => m.name).ToList();

            var stale = _replacements.Keys.Where(m => !_rendererCounts.ContainsKey(m)).ToList();
            foreach (var m in stale) _replacements.Remove(m);
        }

        private void ReplaceMaterial(Material oldMat, Material newMat)
        {
            if (oldMat == null || newMat == null) return;

            var affected = Object.FindObjectsOfType<Renderer>()
                .Where(r => r.sharedMaterials.Contains(oldMat))
                .ToArray();

            if (affected.Length == 0) return;

            var undoTargets = new Object[affected.Length];
            for (int i = 0; i < affected.Length; i++) undoTargets[i] = affected[i];
            Undo.RecordObjects(undoTargets, "Replace Material");

            var dirtyScenes = new HashSet<UnityEngine.SceneManagement.Scene>();
            int replaceCount = 0;

            foreach (var r in affected)
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != oldMat) continue;
                    mats[i] = newMat;
                    changed = true;
                    replaceCount++;
                }
                if (!changed) continue;
                r.sharedMaterials = mats;
                EditorUtility.SetDirty(r);
                dirtyScenes.Add(r.gameObject.scene);
            }

            foreach (var scene in dirtyScenes)
                EditorSceneManager.MarkSceneDirty(scene);

            EditorUtility.DisplayDialog("Завершено",
                $"'{oldMat.name}' → '{newMat.name}'\n{replaceCount} замен на {affected.Length} объектах.", "Ок");

            Refresh();
        }

        private void SelectObjects(Material mat)
        {
            var objects = Object.FindObjectsOfType<Renderer>()
                .Where(r => r.sharedMaterials.Contains(mat))
                .Select(r => (Object)r.gameObject)
                .ToArray();

            if (objects.Length > 0)
                Selection.objects = objects;
            else
                EditorUtility.DisplayDialog("Ничего не найдено",
                    $"Ни один объект не использует: {mat.name}", "Ок");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private bool GroupMatchesSearch(List<Material> materials)
        {
            if (string.IsNullOrWhiteSpace(_search)) return true;
            string s = _search.ToLowerInvariant();
            return materials.Any(m => m.name.ToLowerInvariant().Contains(s));
        }

        private List<Material> FilteredMaterials(List<Material> materials)
        {
            if (string.IsNullOrWhiteSpace(_search)) return materials;
            string s = _search.ToLowerInvariant();
            return materials.Where(m => m.name.ToLowerInvariant().Contains(s)).ToList();
        }
    }
}
