#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UdonSharpEditor;

namespace PuruWorldUtils.Editor
{
    public class PWU_StandaloneUtilityEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            var note = (PWU_NoteAttribute)Attribute.GetCustomAttribute(
                target.GetType(), typeof(PWU_NoteAttribute));
            if (note != null)
                EditorGUILayout.HelpBox(note.Text, MessageType.None);

            DrawDefaultInspector();
        }
    }
}
#endif
