using UnityEditor;
using UnityEngine;

namespace Galaxy4Games.KeyValueSystem.Editor
{
    public class ObjectLibraryEditor<TConfig> : LibraryEditor<TConfig> where TConfig : Object
    {
        protected override TConfig GetField(Rect rect, TConfig config) =>
            (TConfig)EditorGUI.ObjectField(new Rect(rect.x + rect.width * 0.5f + 8f, rect.y, rect.width * 0.5f - 8f,
                EditorGUIUtility.singleLineHeight), config, typeof(TConfig), false);
    }
}