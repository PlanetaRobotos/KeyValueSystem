using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Galaxy4Games.KeyValueSystem
{
    [CreateAssetMenu(fileName = "LibraryConstants", menuName = "Library/LibraryConstants")]
    public class LibraryConstants : ScriptableObject
    {
        private const int NestedLevelsAmount = 1;

        public int sequenceMultiplier = 3;

        [HideInInspector] public string enumsPath;
        [HideInInspector] public List<KeyName> constantsList = new();

        [HideInInspector] public string EntityType = "Window Type";
        [HideInInspector] public string EnumClassName = "Enum Class Name";

        [HideInInspector] public string NewGroupName = "New Group Name";

        public int lastIndex;

        public Dictionary<string, object> nestedDictionary;
        public Type nestedClassType;


        public List<KeyName> GenerateList(Type rootClass)
        {
            if (rootClass == null)
            {
                Debug.LogError("Root class is null. Generation canceled.");
                return null;
            }

            List<KeyName> list = new();

            FieldInfo[] fields =
                rootClass.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (FieldInfo field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(int))
                    list.Add(new KeyName { key = (int)field.GetValue(null), name = field.Name });
                else
                    Debug.LogWarning($"Field {field.Name} is not a constant.");
            }

            return list;
        }

        public Dictionary<string, object> GenerateNestedDictionary(Type rootClass)
        {
            if (rootClass != null)
                return GenerateDictionaryFromNestedClass(rootClass);

            Debug.LogError("Root class is null. Generation canceled.");
            return null;
        }

        private Dictionary<string, object> GenerateDictionaryFromNestedClass(Type nestedClass)
        {
            Dictionary<string, object> dict = new();

            var nestedTypes = nestedClass.GetTypeInfo().DeclaredNestedTypes;

            FieldInfo[] fields =
                nestedClass.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (FieldInfo field in fields) dict.AddKeyValuePair(field.Name, null);

            foreach (TypeInfo nested in nestedTypes)
                dict.AddKeyValuePair(nested.Name, GenerateDictionaryFromNestedClass(nested));

            return dict;
        }

        private Dictionary<string, object> CreateNestedDictionary(int levels, int keysPerLevel)
        {
            if (levels <= 0)
                return null;

            var nestedDict = new Dictionary<string, object>();

            for (int i = 0; i < keysPerLevel; i++)
            {
                string key = $"Key{i}_Depth{NestedLevelsAmount - levels}";
                nestedDict[key] = CreateNestedDictionary(levels - 1, keysPerLevel);
            }

            return nestedDict;
        }

        public void AddKeyValuePair(string key, object value)
        {
            nestedDictionary[key] = value;
        }

        public void EditKeyValuePair(string key, object newValue)
        {
            if (nestedDictionary.ContainsKey(key))
            {
                nestedDictionary[key] = newValue;
            }
        }

        public void RemoveKeyValuePair(string key)
        {
            nestedDictionary.Remove(key);
        }
    }
}

public static class DictionaryExtensions
{
    public static void AddKeyValuePair(this IDictionary<string, object> dict, string key, object value)
    {
        dict[key] = value;
    }

    public static void EditKeyValuePair(this IDictionary<string, object> dict, string key, object newValue)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = newValue;
        }
    }

    public static void RemoveKeyValuePair(this IDictionary<string, object> dict, string key)
    {
        dict.Remove(key);
    }
}