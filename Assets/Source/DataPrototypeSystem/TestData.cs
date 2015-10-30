using UnityEngine;

namespace DataPrototypes {

	public enum TestEnum {
		One, Two, Three
	}

    [ExecuteInEditMode]
    public class TestData : DataPrototype {
        public long longValue;
        public int intValue;
        public bool boolValue;
        public float floatValue;
        public double doubleValue;
        public string stringValue;
        public Color colorValue;
        public UnityEngine.Object objectValue;
        public LayerMask layerMaskValue;
        public TestEnum enumValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public Rect rectValue;
        public char charValue;
        public Bounds boundsValue;
    }
}