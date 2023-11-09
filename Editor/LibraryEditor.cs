using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Galaxy4Games.KeyValueSystem.Editor
{
    [CustomEditor(typeof(LibrarySO<>), true)]
    public abstract class LibraryEditor<TConfig> : UnityEditor.Editor
    {
        private const string SaveLibraryTitle = "Save Library";
        private const string CustomSectionTitle = "Customize Window";
        private const string ConstantName = "None";

        // private KeyName[] _keys;
        private ReorderableList _reorderableList;
        private bool _showCustomizeSection;

        private LibrarySO<TConfig> Library => target as LibrarySO<TConfig>;

        private void OnEnable()
        {
            if (Library == null) return;

            _reorderableList = new ReorderableList(Library.pairsList, typeof(TConfig),
                true, true, true, true);

            _reorderableList.drawHeaderCallback += DrawHeader;
            _reorderableList.drawElementCallback += DrawElement;

            _reorderableList.onAddCallback += AddItem;
            _reorderableList.onRemoveCallback += RemoveItem;
        }

        private void OnDisable()
        {
            if (_reorderableList == null) return;

            _reorderableList.drawHeaderCallback -= DrawHeader;
            _reorderableList.drawElementCallback -= DrawElement;

            _reorderableList.onAddCallback -= AddItem;
            _reorderableList.onRemoveCallback -= RemoveItem;
        }

        private void DrawHeader(Rect rect)
        {
            string labelText =
                $"Dependencies between {Library.keyTitle} and {Library.valueTitle}";

            GUI.Label(rect, labelText, EditorStyles.boldLabel);
        }

        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            NamePair<TConfig> item = Library.pairsList[index];

            EditorGUI.BeginChangeCheck();
            
            if (Library.editableItemIndex != index)
            {
                var keyName = Library.libraryConstants.constantsList.FirstOrDefault(x => x.key == item.key);

                EditorGUI.LabelField(new Rect(rect.x + 30f, rect.y, rect.width * 0.5f,
                    EditorGUIUtility.singleLineHeight), keyName == null ? ConstantName : keyName.name);
            }

            if (Library.editableItemIndex == index && Library.isEditItem)
            {
                string[] names = Library.libraryConstants.constantsList.Select(x => x.name).ToArray();

                Library.itemIndex = EditorGUI.Popup(new Rect(rect.x + 30f, rect.y, rect.width * 0.5f - 30f,
                    EditorGUIUtility.singleLineHeight), Library.itemIndex, names);

                item.key = Library.libraryConstants.constantsList[Library.itemIndex].key;
            }

            item.config = GetField(rect, item.config);

            if (EditorGUI.LinkButton(new Rect(rect.x, rect.y, 25f,
                    EditorGUIUtility.singleLineHeight), "Edit"))
            {
                Library.isEditItem = !Library.isEditItem;

                if (Library.editableItemIndex == index)
                    Library.editableItemIndex = -1;
                else
                    Library.editableItemIndex = index;
            }

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(target);
        }

        protected abstract TConfig GetField(Rect rect, TConfig config);

        private static void AddItem(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);
        }

        private void RemoveItem(ReorderableList list)
        {
            Library.pairsList.RemoveAt(list.index);

            EditorUtility.SetDirty(target);
        }


        public override void OnInspectorGUI()
        {
            if (Library.libraryConstants != null)
            {
                // Library.libraryConstants.constantsList = Library.libraryConstants.constantsList.ToArray();
                // ConvertIntValuesFromKeys(_keys);
            }

            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (_reorderableList == null) return;

            _reorderableList.DoLayoutList();

            EditorGUILayout.Space();

            // Save Button
            if (GUILayout.Button(SaveLibraryTitle, GUILayout.ExpandWidth(true), GUILayout.Height(32f)))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.Space();

            // Custom Section
            _showCustomizeSection = EditorGUILayout.Foldout(_showCustomizeSection, CustomSectionTitle);
            if (_showCustomizeSection)
            {
                EditorGUI.indentLevel++;

                GUILayout.Label("Dependency Titles", EditorStyles.boldLabel);

                Library.keyTitle = EditorGUILayout.TextField("Key Title", Library.keyTitle);

                GUILayout.Space(1);

                Library.valueTitle = EditorGUILayout.TextField("Value Title", Library.valueTitle);

                EditorGUI.indentLevel--;
            }
        }

        private static void ConvertIntValuesFromKeys(IReadOnlyCollection<string> input)
        {
            var options = new int[input.Count];

            for (var i = 0; i < options.Length; i++)
                options[i] = i;
        }
    }

    [Serializable]
    public class KeyName
    {
        public int key;
        public string name;
    }
}