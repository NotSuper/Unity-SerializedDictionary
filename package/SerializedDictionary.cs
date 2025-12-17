using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
    
    [SerializeField] private List<TKey> serializedKeys = new();
    [SerializeField] private List<TValue> serializedValues = new();

    public void OnAfterDeserialize() {
        int keyCount = serializedKeys.Count;
        int valueCount = serializedValues.Count;

        if(keyCount > valueCount) {
            serializedValues.AddRange(Enumerable.Repeat(default(TValue), keyCount - valueCount));
        }

        if(keyCount < valueCount) {
            serializedValues.RemoveRange(keyCount, valueCount - keyCount);
        }

        for(int i = 0; i < keyCount; i++) {
            this[serializedKeys[i]] = serializedValues[i];
        }
    }

    public void OnBeforeSerialize() {
        if(serializedKeys.Count != serializedValues.Count) {
            serializedKeys = new(Keys);
            serializedValues = new(Values);
        }
    }

}