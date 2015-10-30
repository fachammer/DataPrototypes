using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace DataPrototypes {

    [Serializable]
    public class SerializableObject : ISerializationCallbackReceiver {
        public long longValue;
        public int intValue;
        public bool boolValue;
        public float floatValue;
        public double doubleValue;
        public string stringValue;
        public Color colorValue;
        public UnityEngine.Object objectValue;
        public LayerMask layerMaskValue;
        public SerializableEnum enumValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public Rect rectValue;
        public char charValue;
        public AnimationCurve curveValue;
        public Bounds boundsValue;

        [SerializeField]
        private string typeName;

        public Type Type { get; set; }

        public object GetValue() {
            Assert.IsNotNull(Type);
            if(Type == typeof(int))
                return intValue;
            else if(Type == typeof(long))
                return longValue;
            else if(Type == typeof(bool))
                return boolValue;
            else if(Type == typeof(float))
                return floatValue;
            else if(Type == typeof(double))
                return doubleValue;
            else if(Type == typeof(string))
                return stringValue;
            else if(Type == typeof(Color))
                return colorValue;
            else if(Type.IsSubtypeOrEqualTo(typeof(UnityEngine.Object)))
                return objectValue;
            else if(Type == typeof(LayerMask))
                return layerMaskValue;
            else if(Type.IsSubtypeOrEqualTo(typeof(Enum)))
                return enumValue.EnumValue;
            else if(Type == typeof(Vector2))
                return vector2Value;
            else if(Type == typeof(Vector3))
                return vector3Value;
            else if(Type == typeof(Vector4))
                return vector4Value;
            else if(Type == typeof(Rect))
                return rectValue;
            else if(Type == typeof(char))
                return charValue;
            else if(Type == typeof(AnimationCurve))
                return curveValue;
            else if(Type == typeof(Bounds))
                return boundsValue;

            throw new ArgumentException(string.Format("SerializableObject has no value of type {0}", Type), "type");
        }

        public void SetValueOfType(Type newType, object value) {
            Assert.IsNotNull(newType);
            Type = newType;

            if(Type == typeof(int))
                intValue = (int) value;
            else if(Type == typeof(long))
                longValue = (long) value;
            else if(Type == typeof(bool))
                boolValue = (bool) value;
            else if(Type == typeof(float))
                floatValue = (float) value;
            else if(Type == typeof(double))
                doubleValue = (double) value;
            else if(Type == typeof(string))
                stringValue = (string) value;
            else if(Type == typeof(Color))
                colorValue = (Color) value;
            else if(Type.IsSubtypeOrEqualTo(typeof(UnityEngine.Object)))
                objectValue = (UnityEngine.Object) value;
            else if(Type == typeof(LayerMask))
                layerMaskValue = (LayerMask) value;
            else if(Type.IsSubtypeOrEqualTo(typeof(Enum)))
                enumValue = new SerializableEnum((Enum) value, Type);
            else if(Type == typeof(Vector2))
                vector2Value = (Vector2) value;
            else if(Type == typeof(Vector3))
                vector3Value = (Vector3) value;
            else if(Type == typeof(Vector4))
                vector4Value = (Vector4) value;
            else if(Type == typeof(Rect))
                rectValue = (Rect) value;
            else if(Type == typeof(char))
                charValue = (char) value;
            else if(Type == typeof(AnimationCurve))
                curveValue = (AnimationCurve) value;
            else if(Type == typeof(Bounds))
                boundsValue = (Bounds) value;
        }

        public void OnAfterDeserialize() {
            if(typeName == null || typeName.Length == 0)
                return;

            Type = GetTypeFromName(typeName);
        }

        public void OnBeforeSerialize() {
            typeName = Type.FullName;
        }

        private static Type GetTypeFromName(string typeName) {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies) {
                var potentialType = assembly.GetType(typeName);
                if(potentialType != null)
                    return potentialType;
            }

            return null;
        }
    }
}