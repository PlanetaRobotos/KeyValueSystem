using UnityEditor;
using UnityEngine;

namespace Galaxy4Games.KeyValueSystem.Editor
{
    public class StringLibraryEditor<TConfig> : LibraryEditor<TConfig>
    {
        protected override TConfig GetField(Rect rect, TConfig config) =>
            (TConfig)(object)EditorGUI.TextField(new Rect(rect.x + rect.width * 0.5f + 8f, rect.y,
                rect.width * 0.5f - 8f,
                EditorGUIUtility.singleLineHeight), config == null ? "" : config.ToString());
    }
}