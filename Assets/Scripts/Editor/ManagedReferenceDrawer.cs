using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public abstract class ManagedReferenceDrawer<TBase> : PropertyDrawer
    {
        private static Dictionary<string, Type> _typeMap;

        protected abstract string SelectPrompt { get; }
        protected abstract string EmptyPrompt { get; }
        protected abstract string UndoLabel { get; }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_typeMap == null) BuildTypeMap();

            Rect typeRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect contentRect = new(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width,
                position.height - EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(position, label, property);

            string typeName = property.managedReferenceFullTypename;
            string displayName = GetShortTypeName(typeName) ?? SelectPrompt;

            if (EditorGUI.DropdownButton(typeRect, new GUIContent(displayName), FocusType.Keyboard))
            {
                GenericMenu menu = new();

                if (_typeMap == null || _typeMap.Count == 0)
                {
                    menu.AddDisabledItem(new GUIContent(EmptyPrompt));
                }
                else
                {
                    foreach ((string name, Type type) in _typeMap)
                    {
                        menu.AddItem(new GUIContent(name), type.FullName == typeName, () =>
                        {
                            Undo.RecordObject(property.serializedObject.targetObject, UndoLabel);
                            property.managedReferenceValue = Activator.CreateInstance(type);
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                }

                menu.ShowAsContext();
            }

            if (property.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(contentRect, property, GUIContent.none, true);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.managedReferenceValue != null)
            {
                height += EditorGUI.GetPropertyHeight(property, label, true);
            }

            return height;
        }

        private static void BuildTypeMap()
        {
            Type baseType = typeof(TBase);
            _typeMap = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm =>
                {
                    try
                    {
                        return asm.GetTypes();
                    }
                    catch
                    {
                        return Type.EmptyTypes;
                    }
                })
                .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
                .GroupBy(t => ObjectNames.NicifyVariableName(t.Name))
                .ToDictionary(g => g.Key, g => g.First());
        }

        private static string GetShortTypeName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName)) return null;
            string[] parts = fullTypeName.Split(' ');
            return parts.Length > 1 ? parts[1].Split('.').Last() : fullTypeName;
        }
    }
}
