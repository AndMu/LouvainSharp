// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. 
//  
//  This software was derieved from "LouvainSharp" by markusmobius, which was licensed under the MIT License   
//  Copyright (c) 2015 markusmobius
//  https://github.com/markusmobius/LouvainSharp
//
// -----------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace LouvainCommunityPL
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dicitionary, TKey key, TValue defaultValue = default(TValue))
        {
            return dicitionary.ContainsKey(key) ? dicitionary[key] : defaultValue;
        }
    }
}