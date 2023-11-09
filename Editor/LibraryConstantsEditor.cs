using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Galaxy4Games.KeyValueSystem.Editor
{
    [CustomEditor(typeof(LibraryConstants))]
    public class LibraryConstantsEditor : UnityEditor.Editor
    {
        private enum NewConstantsGroupState
        {
            Add,
            Save
        }

        private GUIStyle boxStyle;
        private GUIStyle addButtonStyle;
        private GUIStyle removeButtonStyle;
        private GUIStyle keyStyle;
        private GUIStyle valueStyle;

        private string newKeyName = "NewKey";

        private const string CustomSectionTitle = "Customize Window";
        private const string SaveConfigTitle = "Save Config";

        private string _newConstantKey = string.Empty;
        private StringBuilder _stringBuilder;
        private bool _showCustomizeSection;
        private bool _showConstantsSection;

        private NewConstantsGroupState _constantsGroupState = NewConstantsGroupState.Add;

        private LibraryConstants MapConstants => (LibraryConstants)target;

        private void OnEnable()
        {
            boxStyle = new GUIStyle("box")
            {
                padding = new RectOffset(15, 15, 10, 10)
            };

            addButtonStyle = new GUIStyle("button")
            {
            };

            removeButtonStyle = new GUIStyle("button")
            {
                fixedWidth = 20,
                normal =
                {
                    textColor = Color.red
                }
            };

            keyStyle = new GUIStyle("textfield")
            {
                fixedWidth = 100
            };

            valueStyle = new GUIStyle("textfield")
            {
                normal =
                {
                    textColor = Color.white
                }
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (MapConstants == null) return;

            // NestedDictionarySection();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (GUILayout.Button("Get From File"))
            {
                if (MapConstants.nestedClassType == null)
                    GetClassType();

                MapConstants.constantsList = MapConstants.GenerateList(MapConstants.nestedClassType);
            }

            EditorGUILayout.Space();

            ExportSection();

            EditorGUILayout.Space();

            AddKeySection();

            EditorGUILayout.Space();

            if (KeysSection()) return;

            EditorGUILayout.Space();

            if (GUILayout.Button(SaveConfigTitle, GUILayout.ExpandWidth(true), GUILayout.Height(32)))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Export to file", GUILayout.ExpandWidth(true), GUILayout.Height(32)))
            {
                ExportKeysToClass();
                // ExportKeysToNestedClasses(MapConstants);
            }

            EditorGUILayout.Space();

            SetupInspectorSection();
        }

        private void NestedDictionarySection()
        {
            if (MapConstants.nestedDictionary != null)
                DrawNestedDictionary(MapConstants.nestedDictionary, 0);
            else if (MapConstants.nestedClassType != null || GetClassType())
                MapConstants.nestedDictionary = MapConstants.GenerateNestedDictionary(MapConstants.nestedClassType);
            else
                EditorGUILayout.LabelField("Dictionary is empty. Please, generate it from file.",
                    EditorStyles.boldLabel);

            EditorGUILayout.Space();

            if (GUILayout.Button("Get Dictionary From File"))
            {
                if (MapConstants.nestedClassType == null)
                    GetClassType();

                MapConstants.nestedDictionary = MapConstants.GenerateNestedDictionary(MapConstants.nestedClassType);
            }
        }

        private bool GetClassType()
        {
            if (string.IsNullOrEmpty(MapConstants.enumsPath))
                return false;

            string[] scriptGUIDs = AssetDatabase.FindAssets("t:script", new[] { MapConstants.enumsPath });

            foreach (var guid in scriptGUIDs)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
                Type scriptType = script.GetClass();

                if (scriptType != null && scriptType.Name == MapConstants.EnumClassName)
                {
                    MapConstants.nestedClassType = scriptType;
                    return true;
                }
            }

            return false;
        }

        /*private TutorLibraryConst GetFile()
        {
            string path = $"{MapConstants.enumsPath}/{MapConstants.EnumClassName}";

            Assembly asm = AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(s => s.GetType(MapConstants.EnumClassName) != null);

            object instance = Activator.CreateInstance(asm.GetType(MapConstants.EnumClassName));

            return instance as TutorLibraryConst;
        }*/

        private void SetupInspectorSection()
        {
            _showCustomizeSection = EditorGUILayout.Foldout(_showCustomizeSection, CustomSectionTitle);
            if (_showCustomizeSection)
            {
                EditorGUI.indentLevel++;

                MapConstants.EntityType = EditorGUILayout.TextField("Entity Type", MapConstants.EntityType);

                GUILayout.Space(1);

                MapConstants.EnumClassName = EditorGUILayout.TextField("Enum Class Name", MapConstants.EnumClassName);

                EditorGUI.indentLevel--;
            }
        }

        private bool KeysSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"List of {MapConstants.EntityType} Keys:", EditorStyles.largeLabel);

            if (MapConstants.constantsList == null) return true;

            // _showConstantsSection = EditorGUILayout.Foldout(_showConstantsSection, "General Constants Section");
            if (true)
            {
                EditorGUI.indentLevel++;

                for (var i = 0; i < MapConstants.constantsList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    // MapConstants.constantsList[i].name = EditorGUILayout.TextField(MapConstants.constantsList[i].name);

                    EditorGUILayout.LabelField(MapConstants.constantsList[i].name, GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField(MapConstants.constantsList[i].key.ToString(), GUILayout.Width(32));

                    if (GUILayout.Button("Remove")) MapConstants.constantsList.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            return false;
        }

        private void AddKeySection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"Add New {MapConstants.EntityType} Keys:", EditorStyles.largeLabel);

            _newConstantKey = EditorGUILayout.TextField("New Key:", _newConstantKey).Replace(' ', '_').ToUpper();

            if (GUILayout.Button($"Add New {MapConstants.EntityType} Key", GUILayout.ExpandWidth(true),
                    GUILayout.Height(32)))
            {
                MapConstants.constantsList.Add(new KeyValueSystem.KeyName
                    { key = MapConstants.lastIndex + MapConstants.sequenceMultiplier, name = _newConstantKey });

                MapConstants.lastIndex += MapConstants.sequenceMultiplier;
            }

            EditorGUILayout.EndVertical();
        }

        private void ExportSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Export:", EditorStyles.largeLabel);

            EditorGUILayout.BeginHorizontal();

            MapConstants.enumsPath =
                EditorGUILayout.TextField("Path", MapConstants.enumsPath, GUILayout.ExpandWidth(true));

            /*if (!string.IsNullOrEmpty(MapConstants.enumsPath))
                EditorGUILayout.LabelField("Path", MapConstants.enumsPath, GUILayout.ExpandWidth(true));*/

            // if (GUILayout.Button("Pick", GUILayout.Width(96)))
            //     MapConstants.enumsPath = EditorUtility.OpenFolderPanel("Pick The Folder", "", "");

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void ExportKeysToEnum(LibraryConstants libraryConstants)
        {
            _stringBuilder = new StringBuilder();

            _stringBuilder.Append($"public enum {MapConstants} : byte\n");

            _stringBuilder.Append("{\n");

            for (var i = 0; i < libraryConstants.constantsList.Count; i++)
            {
                string coma = i < libraryConstants.constantsList.Count - 1 ? "," : string.Empty;
                _stringBuilder.Append($"\t{libraryConstants.constantsList[i].name.ToUpper()} = {i}{coma}\n");
            }

            _stringBuilder.Append("}");

            var filename = $"{MapConstants.EnumClassName}.cs";
            File.WriteAllText(Path.Combine(libraryConstants.enumsPath, filename), _stringBuilder.ToString());

            AssetDatabase.Refresh();
        }

        private void DrawNestedDictionary(IDictionary<string, object> dictionary, int indent)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;

            foreach ((string key, object value) in new Dictionary<string, object>(dictionary))
            {
                EditorGUILayout.BeginVertical(boxStyle);

                GUILayout.Space(indent * 4);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("-", removeButtonStyle))
                {
                    dictionary.Remove(key);
                    continue;
                }

                string newKey = EditorGUILayout.TextField(key, keyStyle);
                if (newKey != key)
                {
                    // Key name has changed, update the dictionary
                    dictionary.Remove(key);
                    dictionary[newKey] = value;
                }

                EditorGUILayout.EndHorizontal();

                if (value is Dictionary<string, object> nestedDict)
                {
                    DrawNestedDictionary(nestedDict, indent + 1);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    object newValue = null;

                    if (value != null)
                    {
                        EditorGUILayout.TextField(value.ToString(), valueStyle);
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        dictionary[key] = newValue;
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent * 5);

            if (GUILayout.Button("Add Value", addButtonStyle))
            {
                dictionary[newKeyName] = null;
            }

            if (GUILayout.Button("Add Dictionary", addButtonStyle))
            {
                Dictionary<string, object> newGroup = new();
                dictionary[newKeyName] = newGroup;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void ExportKeysToNestedClasses(LibraryConstants libraryConstants)
        {
            if (libraryConstants.nestedDictionary == null)
            {
                Debug.LogError("Nested dictionary is null. Export canceled.");
                return;
            }

            _stringBuilder = new StringBuilder();
            ExportDictionary(libraryConstants.nestedDictionary, _stringBuilder, MapConstants.EnumClassName, 0);

            var filename = $"{MapConstants.EnumClassName}.cs";

            string dataPath = Application.dataPath;
            int assetsIndex = dataPath.IndexOf("/Assets", StringComparison.Ordinal);
            dataPath = assetsIndex != -1 ? dataPath[..assetsIndex] : "Assets";
            string path = Path.Combine(dataPath, libraryConstants.enumsPath, filename);

            File.WriteAllText(path, _stringBuilder.ToString());

            AssetDatabase.Refresh();
        }

        private static void ExportDictionary(Dictionary<string, object> dict, StringBuilder sb, string className,
            int depth)
        {
            string indent = new string('\t', depth);
            sb.AppendLine($"{indent}public class {className}");
            sb.AppendLine($"{indent}{{");

            foreach (var kvp in dict)
            {
                if (kvp.Value is Dictionary<string, object> nestedDict)
                    ExportDictionary(nestedDict, sb, kvp.Key, depth + 1);
                else
                    sb.AppendLine($"{indent}\tpublic const int {kvp.Key.ToUpper()} = {1};");
            }

            sb.AppendLine($"{indent}}}");
        }

        private void ExportKeysToClass()
        {
            _stringBuilder = new StringBuilder();

            _stringBuilder.Append($"public class {MapConstants.EnumClassName}\n");
            _stringBuilder.Append("{\n");

            foreach (KeyValueSystem.KeyName keyName in MapConstants.constantsList)
                _stringBuilder.Append($"\tpublic const int {keyName.name.ToUpper()} = {keyName.key};\n");

            _stringBuilder.Append("}");

            var filename = $"{MapConstants.EnumClassName}.cs";
            File.WriteAllText(Path.Combine(MapConstants.enumsPath, filename), _stringBuilder.ToString());

            AssetDatabase.Refresh();
        }

        private static string GetCSharpType(object value)
        {
            return value switch
            {
                int => "int",
                float => "float",
                string => "string",
                _ => "object"
            };
        }
    }
}