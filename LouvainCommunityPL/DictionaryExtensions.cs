using System.Collections;
using System.Collections.Generic;

namespace LouvainCommunityPL
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dicitionary, TKey key, TValue defaultValue)
        {
            return dicitionary.ContainsKey(key) ? dicitionary[key] : defaultValue;
        }
    }
}