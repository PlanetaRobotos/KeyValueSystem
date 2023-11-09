using System;

namespace Galaxy4Games.KeyValueSystem
{
    [Serializable]
    public class NamePair<T>
    {
        public int key;
        // public string name;
        public T config;
    }
}