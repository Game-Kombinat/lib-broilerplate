using System;
using System.Collections.Generic;
using UnityEngine;

namespace Broilerplate.Data {
    [CreateAssetMenu(menuName = "Game Kombinat/Blackboard/New Blackboard Data", fileName = "Blackboard Data")]
    public class BlackboardData : ScriptableObject {
        public List<BlackboardEntryData> entries = new();

        public Blackboard GetNewBlackboard() {
            var bb = new Blackboard();
            PopulateBlackboard(bb);

            return bb;
        }

        public void PopulateBlackboard(Blackboard bb) {
            for (int i = 0; i < entries.Count; i++) {
                entries[i].SetValueOnBlackboard(bb);
            }
        }
    }

    [Serializable]
    public class BlackboardEntryData : ISerializationCallbackReceiver {
        public string keyName;
        public AnyValue.ValueType valueType;
        public AnyValue value;

        // this is extremely filthy, I hate it. 
        // But I don't see any other way out of this, either.
        private static Dictionary<AnyValue.ValueType, Action<Blackboard, BlackboardKey, AnyValue>> BlackboardSetters = new() {
            { AnyValue.ValueType.Int , (blackboard, key, val) => blackboard.Set<int>(key, val)},
            { AnyValue.ValueType.Float , (blackboard, key, val) => blackboard.Set<float>(key, val)},
            { AnyValue.ValueType.Vector3 , (blackboard, key, val) => blackboard.Set<Vector3>(key, val)}
        };

        public void SetValueOnBlackboard(Blackboard bb) {
            BlackboardSetters[valueType](bb, keyName, value);
        }
        
        public void OnBeforeSerialize() {
        }

        public void OnAfterDeserialize() {
            value.type = valueType;
        }
    }

    /// <summary>
    /// It's basically a union.
    /// But of course it's not.
    /// Keep calm. I know it's not.
    /// </summary>
    [Serializable]
    public struct AnyValue {
        // you can represent a bool with an int and a quaternion with euler angles, a vector3.
        // a vector2 can be represented by a vector 3, albeit the third component being a waste on memory. 
        // Oh dear. 4 bytes down the drain.
        // And if you're trying to blackboard a string - well shit you're doing something funky.
        // Therefore, these mighty 3 will suffice. Keeping stuff lean and mean, or something.
        public enum ValueType {Int, Float, Vector3}

        public ValueType type;

        public int intValue;

        public float floatValue;

        public Vector3 vector3Value;

        #region implicit casts
        public static implicit operator int(AnyValue value) => value.ValueToGeneric<int>();
        public static implicit operator float(AnyValue value) => value.ValueToGeneric<float>();
        public static implicit operator Vector3(AnyValue value) => value.ValueToGeneric<Vector3>();
        #endregion
        // all this to get away without boxing.
        #region generic non-boxing shenanigans
        private T ValueToGeneric<T>() {
            return type switch {
                ValueType.Int => AsInt<T>(intValue),
                ValueType.Float => AsFloat<T>(floatValue),
                ValueType.Vector3 => AsVector3<T>(vector3Value),
                _ => throw new ArgumentOutOfRangeException($"Unsupported value type {type}")
            };
        }
        

        private T AsInt<T>(int value) {
            return typeof(T) == typeof(int) && value is T cast ? cast : default;
        }
        
        private T AsFloat<T>(float value) {
            return typeof(T) == typeof(float) && value is T cast ? cast : default;
        }
        
        private T AsVector3<T>(Vector3 value) {
            return typeof(T) == typeof(Vector3) && value is T cast ? cast : default;
        }
        
        #endregion
    }
}