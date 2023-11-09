using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galaxy4Games.KeyValueSystem
{
    public class LibrarySO<T> : ScriptableObject
    {
        [HideInInspector] public string keyTitle = "Key Type";
        [HideInInspector] public string valueTitle = "Value Type";
        [HideInInspector] public List<NamePair<T>> pairsList = new();

        public LibraryConstants libraryConstants;
        
        [HideInInspector] public List<int> itemIndexes = new();
        [HideInInspector] public int itemIndex;
        
        [HideInInspector] public bool isEditItem;
        [HideInInspector] public int editableItemIndex;
    }
    
    [Serializable]
    public class KeyName
    {
        public int key;
        public string name;
    }
}