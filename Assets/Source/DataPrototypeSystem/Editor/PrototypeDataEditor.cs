using DataPrototypes;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Extensions {

    public static object GetUnityEditorInputFieldValue(this Type type, object currentValue) {
        if(type.IsSubtypeOrEqualToOneOf(typeof(int), typeof(short), typeof(byte)))
            return EditorGUILayout.IntField((int) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(long)))
            return EditorGUILayout.LongField((long) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(float)))
            return EditorGUILayout.FloatField((float) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(double)))
            return EditorGUILayout.DoubleField((double) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(string)))
            return EditorGUILayout.TextField(((string) currentValue) ?? "");
        else if(type.IsSubtypeOrEqualTo(typeof(char)))
            return CharField((char) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(bool)))
            return EditorGUILayout.Toggle((bool) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(Enum)))
            return EnumField(type, currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(Color)))
            return EditorGUILayout.ColorField((Color) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(LayerMask)))
            return LayerMaskField((LayerMask) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(Vector2)))
            return EditorGUILayout.Vector2Field("", (Vector2) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(Vector3)))
            return EditorGUILayout.Vector3Field("", (Vector3) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(Vector4)))
            return EditorGUILayout.Vector4Field("", (Vector4) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(Rect)))
            return EditorGUILayout.RectField((Rect) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(Bounds)))
            return EditorGUILayout.BoundsField((Bounds) currentValue);
        else if(type.IsSubtypeOrEqualTo(typeof(AnimationCurve)))
            return EditorGUILayout.CurveField(((AnimationCurve) currentValue) ?? new AnimationCurve());
        else if(type.IsSubtypeOrEqualTo(typeof(UnityEngine.Object)))
            return ObjectField(type, (UnityEngine.Object) currentValue);

        throw new ArgumentException(string.Format("type {0} has no Unity Editor input field", type), "type");
    }

    public static Enum EnumField(Type enumType, object currentValue) {
        bool isValueValid = currentValue != null && Enum.IsDefined(enumType, currentValue);
        return EditorGUILayout.EnumPopup(isValueValid ? (Enum) currentValue : (Enum) Enum.GetValues(enumType).GetValue(0));
    }

    public static LayerMask LayerMaskFromInt(int mask) {
        var layerMask = new LayerMask() { value = mask };
        return layerMask;
    }

    public static LayerMask LayerMaskField(LayerMask layerMask) {
        List<string> layers = new List<string>();
        List<int> layerNumbers = new List<int>();

        for(int i = 0; i < 32; i++) {
            string layerName = LayerMask.LayerToName(i);
            if(layerName != "") {
                layers.Add(layerName);
                layerNumbers.Add(i);
            }
        }
        int maskWithoutEmpty = 0;
        for(int i = 0; i < layerNumbers.Count; i++) {
            if(((1 << layerNumbers[i]) & layerMask.value) > 0)
                maskWithoutEmpty |= (1 << i);
        }
        maskWithoutEmpty = EditorGUILayout.MaskField(maskWithoutEmpty, layers.ToArray());
        int mask = 0;
        for(int i = 0; i < layerNumbers.Count; i++) {
            if((maskWithoutEmpty & (1 << i)) > 0)
                mask |= (1 << layerNumbers[i]);
        }
        layerMask.value = mask;
        return layerMask;
    }

    public static char CharField(char value) {
        string textInput = EditorGUILayout.TextField(value.ToString());
        return textInput.Length > 0 ? textInput[0] : ' ';
    }

    public static UnityEngine.Object ObjectField(Type objectType, UnityEngine.Object currentValue) {
        return EditorGUILayout.ObjectField(currentValue, objectType, true);
    }

    public static void SetDirtyIfGUIChanged(this UnityEngine.Object target) {
        if(GUI.changed)
            EditorUtility.SetDirty(target);
    }

    private static IEnumerable<string> GetLayerNames() {
        for(int i = 0; i < 32; i++) {
            var layerName = LayerMask.LayerToName(i);
            if(layerName.Length > 0)
                yield return layerName;
        }
    }
}

[CustomEditor(typeof(DataPrototype), true)]
public class PrototypeDataEditor : Editor {
    private const float PROTOTYPE_DELEGATION_TOGGLE_WIDTH = 15f;

    // this is static readonly because making it const makes the compiler complain
    private static readonly Type[] VALID_PROPERTY_TYPES = {
                                                  typeof(int), typeof(short), typeof(byte), typeof(long),
                                                  typeof(double), typeof(float),
                                                  typeof(string), typeof(char),
                                                  typeof(bool), typeof(Enum),
                                                  typeof(Color), typeof(LayerMask), typeof(AnimationCurve),
                                                  typeof(Vector2), typeof(Vector3), typeof(Vector4),
                                                  typeof(Rect), typeof(Bounds),
                                                  typeof(UnityEngine.Object)
                                                };

    public override void OnInspectorGUI() {
        DataPrototype data = (DataPrototype) serializedObject.targetObject;
        foreach(var property in data.GetProperties())
            DrawPropertyUserInput(property, data);

        target.SetDirtyIfGUIChanged();
    }

    private static void DrawPropertyUserInput(DataPrototype.Property property, DataPrototype data) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(property.Name);

        if(DoesInputFieldExistForProperty(property))
            QueryPropertyUserInput(property, data);

        EditorGUILayout.EndHorizontal();
    }

    private static bool DoesInputFieldExistForProperty(DataPrototype.Property property) {
        return property.Type.IsSubtypeOrEqualToOneOf(VALID_PROPERTY_TYPES);
    }

    private static void QueryPropertyUserInput(DataPrototype.Property property, DataPrototype data) {
        var input = GetUnityEditorPropertyInputFieldValue(property);

        if(property.DelegatesToPrototype)
            property.ProtoProperty = GetProtoPropertyUserInput(property, data);
        else
            property.Value = input;

        property.DelegatesToPrototype = GetPrototypeDelegationUserInput(property);
    }

    private static object GetUnityEditorPropertyInputFieldValue(DataPrototype.Property property) {
        GUI.enabled = !property.DelegatesToPrototype;
        return property.Type.GetUnityEditorInputFieldValue(property.Value);
    }

    private static DataPrototype.Property GetProtoPropertyUserInput(DataPrototype.Property property, DataPrototype data) {
        GUI.enabled = property.DelegatesToPrototype;

        var currentProtoPropertyOwner = property.ProtoProperty == null ? null : property.ProtoProperty.Owner;
        var protoPropertyOwner = (DataPrototype) EditorGUILayout.ObjectField(currentProtoPropertyOwner, data.GetType(), true);

        if(protoPropertyOwner != null)
            return protoPropertyOwner.GetProperty(property.Name);

        return null;
    }

    private static bool GetPrototypeDelegationUserInput(DataPrototype.Property property) {
        GUI.enabled = true;
        return EditorGUILayout.Toggle(property.DelegatesToPrototype, GUILayout.Width(PROTOTYPE_DELEGATION_TOGGLE_WIDTH));
    }
}