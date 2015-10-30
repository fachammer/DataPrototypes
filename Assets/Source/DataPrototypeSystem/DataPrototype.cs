using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace DataPrototypes {

    public static class PrototypeDataExtensions {

        public static bool IsSubtypeOrEqualToOneOf(this Type type, params Type[] types) {
            Assert.IsTrue(types.Length > 0);
            return types.Any(type.IsSubtypeOrEqualTo);
        }

        public static bool IsSubtypeOrEqualTo(this Type type, Type otherType) {
            return type != null && (type == otherType || type.IsSubclassOf(otherType));
        }
    }

    public class DataPrototype : MonoBehaviour, ISerializationCallbackReceiver {
        private const BindingFlags PROPERTY_BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance;

        [SerializeField]
        private List<Property> properties = new List<Property>();

        public DataPrototype() {
            RefreshProperties();
        }

        public bool HasProperty(string property) {
            return properties.Any(p => p.Name == property);
        }

        public Property GetProperty(string property) {
            if(!HasProperty(property))
                return null;

            return properties.First(p => p.Name == property);
        }

        public IEnumerable<Property> GetProperties() {
            return properties;
        }

        public virtual void OnAfterDeserialize() {
        }

        public virtual void OnBeforeSerialize() {
            RefreshProperties();
        }

        private void RefreshProperties() {
            FieldInfo[] fields = GetType().GetFields(PROPERTY_BINDING_FLAGS);

            foreach (var field in fields
                .Where(field => !properties.Any(p => p.MatchesField(field)))) {
				SetupPropertyFromField(field);
			}

            var propertiesToRemove = properties.Where(property => !fields.Any(property.MatchesField));
            properties.RemoveAll(propertiesToRemove.Contains);

            SortProperties();
        }

        private void Awake() {
            if(Application.isPlaying) {
                Debug.LogWarning(gameObject + "has a PrototypeData component during runtime! This is not recommended as it is quite performance-heavy. Instead, consider putting the PrototypeData component on a prefab and let " + gameObject + " reference the PrototypeData component on that prefab");
            }
        }

        private void SetupPropertyFromField(FieldInfo field) {
            var property = Property.FromField(field, this);
            property.ValueChanged += OnValueChanged;
            properties.Add(property);
        }

        private void SortProperties() {
            properties.Sort((a, b) => GetType().GetField(a.Name).MetadataToken.CompareTo(GetType().GetField(b.Name).MetadataToken));
        }

        private void OnValueChanged(Property property, object newValue) {
            var field = GetType().GetField(property.Name);
            if(newValue != null && !newValue.GetType().IsSubtypeOrEqualTo(field.FieldType))
                return;

            field.SetValue(this, newValue);
        }

        [Serializable]
        public class Property : ISerializationCallbackReceiver {

            [SerializeField]
            private string name;

            [SerializeField]
            private bool delegatesToPrototype;

            private Property protoProperty;

            [SerializeField]
            private DataPrototype protoPropertyOwner;

            [SerializeField]
            private DataPrototype owner;

            private Type type = typeof(object);

            [SerializeField]
            private object value;

            [SerializeField]
            private SerializableObject serializableObject = new SerializableObject();

            public string Name {
                get { return name; }
                set { this.name = value; }
            }

            public bool DelegatesToPrototype {
                get { return delegatesToPrototype; }
                set {
                    if(delegatesToPrototype == value)
                        return;

                    delegatesToPrototype = value;
                    ValueChanged(this, Value);
                }
            }

            public Property ProtoProperty {
                get { return protoProperty; }
                set {
                    if(protoProperty == value || (value != null && value.CanReach(this)))
                        return;

                    if(protoProperty != null)
                        protoProperty.ValueChanged -= OnProtoPropertyValueChanged;

                    this.protoProperty = value;

                    if(protoProperty != null)
                        protoProperty.ValueChanged += OnProtoPropertyValueChanged;

                    ValueChanged(this, Value);
                }
            }

            public DataPrototype Owner {
                get { return owner; }
                set { this.owner = value; }
            }

            public Type Type {
                get { return type; }
                set { type = value; }
            }

            public object Value {
                get {
                    if(DelegatesToPrototype && ProtoProperty != null)
                        return ProtoProperty.Value;

                    return value;
                }
                set {
                    this.value = value;
                    ValueChanged(this, this.value);
                }
            }

            public event Action<Property, object> ValueChanged = (_1, _2) => { };

            public static Property FromField(FieldInfo field, DataPrototype propertyOwner) {
                var property = new Property() {
                    Owner = propertyOwner,
                    Name = field.Name,
                    Value = field.GetValue(propertyOwner),
                    DelegatesToPrototype = false,
                    ProtoProperty = null,
                    Type = field.FieldType,
                };
                return property;
            }

            public void OnAfterDeserialize() {
                value = serializableObject.GetValue();

                if(protoPropertyOwner != null)
                    protoProperty = protoPropertyOwner.GetProperty(Name);
            }

            public void OnBeforeSerialize() {
                serializableObject.SetValueOfType(type, value);

                if(protoProperty != null)
                    protoPropertyOwner = protoProperty.Owner;
            }

            public bool MatchesField(FieldInfo field) {
                return field.Name == Name && field.FieldType == Type;
            }

            private bool CanReach(Property otherProtoProperty) {
                if(this == otherProtoProperty)
                    return true;
                if(protoProperty == null || otherProtoProperty == null)
                    return false;

                return protoProperty.CanReach(otherProtoProperty);
            }

            private void OnProtoPropertyValueChanged(Property protoProperty, object newValue) {
                if(DelegatesToPrototype)
                    ValueChanged(this, newValue);
            }
        }
    }
}