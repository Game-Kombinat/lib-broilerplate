using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Broilerplate.Editor.Broilerplate.Data {
    
    /// <summary>
    /// It makes me a little uneasy, this. But there is no other way I can think of
    /// right now to make it happen more elegantly.
    ///
    /// And also ... if it works, is it really a bad idea?
    /// </summary>
    public static class SerializedPropertyHelper {
        public static FieldInfo GetFieldInfoFromProperty(SerializedProperty prop) {
            if (prop == null) {
                return null;
            }

            var targetObject = prop.serializedObject.targetObject;
            if (targetObject == null) {
                Debug.LogError("Target object missing. Cannot retrieve field info");
                return null;
            }

            // Handle property paths like "myField.Array.data[0].nestedField"
            var pathParts = prop.propertyPath.Replace(".Array.data[", "[").Split('.');

            FieldInfo fieldInfo = null;
            Type currentType = targetObject.GetType();
            Object source = targetObject;

            foreach (var part in pathParts) {
                if (string.IsNullOrEmpty(part)) {
                    continue;
                }

                // This looks like a list
                if (part.Contains("[")) {
                    var fieldName = part.Substring(0, part.IndexOf('['));
                    fieldInfo = GetFieldInfoRecursive(currentType, fieldName, source);
                    if (fieldInfo == null) {
                        return null;
                    }

                    // Get the element type of the array/list
                    var fieldType = fieldInfo.FieldType;
                    if (fieldType.IsArray) {
                        currentType = fieldType.GetElementType();
                        source = fieldInfo.GetValue(source);
                    }
                    else if (fieldType.IsGenericType) {
                        currentType = fieldType.GetGenericArguments()[0];
                        source = fieldInfo.GetValue(source);
                    }
                    else {
                        return null;
                    }
                }
                else {
                    fieldInfo = GetFieldInfoRecursive(currentType, part, source);

                    if (fieldInfo == null) {
                        return null;
                    }

                    currentType = fieldInfo.FieldType;
                    source = fieldInfo.GetValue(source);
                }
            }

            return fieldInfo;
        }

        private static FieldInfo GetFieldInfoRecursive(Type type, string fieldName, Object source) {
            if (type == null) {
                return null;
            }

            // of note: the type here could potentially be an interface (on account of the field definition).
            // but unitys path assumes a path through concrete types so we have to use it where ever possible.
            var currentType = source != null ? source.GetType() : type;
            while (currentType != null) {
                var fieldInfo = currentType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo != null) {
                    return fieldInfo;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }
    }
}