#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UdonSharp;

namespace PuruWorldUtils.Editor
{
    // Runs on every domain reload. Auto-manages PWU_PROTV_INSTALLED scripting define
    // based on whether ProTV is loaded, then silently creates any missing UdonSharpProgramAsset files.
    [InitializeOnLoad]
    public static class PWU_AutoSetup
    {
        static PWU_AutoSetup()
        {
            EditorApplication.delayCall += Run;
        }

        static void Run()
        {
            if (SyncDefines())
                return; // recompile triggered — repair runs on next domain reload

            RepairSilent();
        }

        // ── Define sync ───────────────────────────────────────────────────────────

        static bool SyncDefines()
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
#pragma warning disable CS0618
            var raw = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
#pragma warning restore CS0618
            var defines = new HashSet<string>(
                raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

            bool changed = false;
            changed |= SyncDefine(defines, "PWU_PROTV_INSTALLED", IsAssemblyLoaded("ArchiTech.ProTV.Runtime"));

            if (!changed) return false;

#pragma warning disable CS0618
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", defines));
#pragma warning restore CS0618
            return true;
        }

        static bool SyncDefine(HashSet<string> defines, string symbol, bool shouldExist)
        {
            if (shouldExist && defines.Add(symbol))
            {
                Debug.Log($"[PWU AutoSetup] Added define: {symbol}");
                return true;
            }
            if (!shouldExist && defines.Remove(symbol))
            {
                Debug.Log($"[PWU AutoSetup] Removed define: {symbol}");
                return true;
            }
            return false;
        }

        static bool IsAssemblyLoaded(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                if (asm.GetName().Name == name) return true;
            return false;
        }

        // ── Program asset repair ──────────────────────────────────────────────────

        static void RepairSilent()
        {
            Type paType = null;
            FieldInfo scriptField = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType("UdonSharp.UdonSharpProgramAsset");
                if (t == null) continue;
                paType = t;
                scriptField = t.GetField("sourceCsScript",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                break;
            }
            if (paType == null || scriptField == null) return;

            var covered = new HashSet<MonoScript>();
            foreach (string paGuid in AssetDatabase.FindAssets("t:UdonSharpProgramAsset"))
            {
                var pa = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                    AssetDatabase.GUIDToAssetPath(paGuid));
                if (pa == null) continue;
                var ms = scriptField.GetValue(pa) as MonoScript;
                if (ms != null) covered.Add(ms);
            }

            const string kOutput = "Assets/PuruWorldUtils/ProgramAssets";
            int created = 0;

            foreach (string guid in AssetDatabase.FindAssets("t:MonoScript"))
            {
                string csPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!csPath.Contains("com.pururut.world-utils") || csPath.Contains("/Editor/")) continue;

                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(csPath);
                Type type = script?.GetClass();
                if (type == null || !typeof(UdonSharpBehaviour).IsAssignableFrom(type) || type.IsAbstract)
                    continue;

                if (covered.Contains(script)) continue;

                if (!AssetDatabase.IsValidFolder("Assets/PuruWorldUtils"))
                    AssetDatabase.CreateFolder("Assets", "PuruWorldUtils");
                if (!AssetDatabase.IsValidFolder(kOutput))
                    AssetDatabase.CreateFolder("Assets/PuruWorldUtils", "ProgramAssets");

                string assetPath = $"{kOutput}/{type.Name}.asset";
                var pa = (ScriptableObject)ScriptableObject.CreateInstance(paType);
                scriptField.SetValue(pa, script);
                AssetDatabase.CreateAsset(pa, assetPath);
                created++;
                Debug.Log($"[PWU AutoSetup] Created program asset: {assetPath}");
            }

            if (created > 0)
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"[PWU AutoSetup] Repaired {created} missing program assets.");
            }
        }
    }
}
#endif
