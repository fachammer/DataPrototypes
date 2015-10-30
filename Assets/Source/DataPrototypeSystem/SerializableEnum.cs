using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace DataPrototypes {

    [Serializable]
    public class SerializableEnum : ISerializationCallbackReceiver {
        private Enum enumValue;
        private Type enumType;

        [SerializeField]
        private string enumName = null;

        [SerializeField]
        private int enumObjectId = -1;

        public Enum EnumValue { get { return enumValue; } }

        public Type EnumType { get { return enumType; } }

        public SerializableEnum(Enum enumValue, Type enumType) {
            Assert.IsNotNull<Type>(enumType);
            Assert.IsTrue(enumType.IsEnum);
            this.enumValue = enumValue;
            this.enumType = enumType;
        }

        public void OnAfterDeserialize() {
            if(enumName == null || enumName.Length == 0 || enumObjectId == -1)
                return;

            enumType = Type.GetType(enumName);
            enumValue = (Enum) Enum.ToObject(enumType, enumObjectId);
        }

        public void OnBeforeSerialize() {
            if(enumType == null)
                return;

            enumName = enumType.FullName;
            Array enumValues = Enum.GetValues(enumType);
            for(int i = 0; i < enumValues.Length; i++) {
                if(enumValues.GetValue(i).Equals(enumValue))
                    enumObjectId = (int) enumValues.GetValue(i);
            }
        }
    }
}