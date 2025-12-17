using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SerializedDictionary<,>))]
public class SerializedDictionaryDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        SerializedProperty serializedKeys = property.FindPropertyRelative("serializedKeys");
        SerializedProperty serializedValues = property.FindPropertyRelative("serializedValues");

        Assert.IsNotNull(property);
        Assert.IsNotNull(serializedKeys);
        Assert.IsNotNull(serializedValues);

        VisualElement root = new();
        
        Foldout container = new();
        container.text = property.displayName;
        container.value = property.isExpanded;
        container.RegisterValueChangedCallback(e => { property.isExpanded = e.newValue; });

        Label warningLabel = new Label();
        warningLabel.text = "⚠";
        warningLabel.style.position = Position.Absolute;
        warningLabel.style.display = DisplayStyle.None;
        warningLabel.style.right = 0;
        warningLabel.style.color = Color.yellow;
        warningCheck(warningLabel, serializedKeys);

        ListView pairList = new();
        pairList.bindingPath = serializedKeys.propertyPath;
        pairList.showAddRemoveFooter = true;
        pairList.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
        pairList.showBorder = true;
        pairList.showBoundCollectionSize = true;
        pairList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        pairList.bindItem = (VisualElement entryGui, int entryIndex) => {
            entryGui.Clear();

            SerializedProperty entryKey = serializedKeys.GetArrayElementAtIndex(entryIndex);
            SerializedProperty entryValue = serializedValues.GetArrayElementAtIndex(entryIndex);

            PropertyField keyField = new PropertyField(entryKey);
            keyField.label = "Key";
            entryGui.Add(keyField);

            PropertyField valueField = new PropertyField(entryValue);
            valueField.label = "Value";
            entryGui.Add(valueField);

            /* yes this runs a lot and a little bit inefficient, no i don't care i'm officially fed up */
            /* i've now spent several days reading documentation trying to make it O(n) and unity is simply
               too arcane so i'm throwing in the towel */
            highlightCheck(entryGui, serializedKeys, entryIndex);
            warningCheck(warningLabel, serializedKeys);
            entryGui.TrackPropertyValue(serializedKeys, _ => {
                if(entryIndex >= serializedKeys.arraySize) { return; }
                highlightCheck(entryGui, serializedKeys, entryIndex);
            });

            entryGui.TrackPropertyValue(entryKey, _ => {
                warningCheck(warningLabel, serializedKeys);
            });

            entryGui.Bind(property.serializedObject);
        };

        root.Add(warningLabel);
        container.Add(pairList);
        root.Add(container);

        return root;
    }

    private bool listHasDuplicates(SerializedProperty keyList) {
        HashSet<object> seen = new();

        for(int i = 0; i < keyList.arraySize; i++) {
            SerializedProperty key = keyList.GetArrayElementAtIndex(i);
            if(!seen.Add(key.boxedValue)) {
                return true;
            }
        }

        return false;
    }

    private bool isKeyDuplicate(SerializedProperty keyList, int keyIndex) {
        SerializedProperty key = keyList.GetArrayElementAtIndex(keyIndex);
        for(int i = 0; i < keyList.arraySize; i++) {
            if(i == keyIndex) { continue; }
            if(keyList.GetArrayElementAtIndex(i).boxedValue.Equals(key.boxedValue)) {
                return true;
            }
        }
        return false;
    }

    private void highlightCheck(VisualElement entryGui, SerializedProperty keyList, int keyIndex) {
        bool isDuplicate = isKeyDuplicate(keyList, keyIndex);
        entryGui.style.backgroundColor = isDuplicate ? new Color(0.5f, 0.5f, 0.3f, 0.5f) : Color.clear;
        entryGui.tooltip = isDuplicate ? "Warning: Duplicated Key" : "";
    }

    private void warningCheck(Label label, SerializedProperty keyList) {
        label.style.display = listHasDuplicates(keyList) ? DisplayStyle.Flex : DisplayStyle.None;
    }
}