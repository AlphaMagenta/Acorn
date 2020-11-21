using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    [System.Serializable]
    public struct CurvePoint {
        // Control point values (initial data)
        public Vector3 pos;
        public float radius;
        public Color color;

        // Intermediate point values (computed data)
        public Vector3 tangent;
        public Quaternion rot;
        public float length;
        // public float t;

        // Arithmetics
        public static Vector3 operator +(CurvePoint a, CurvePoint b) => a.pos + b.pos;
        public static Vector3 operator -(CurvePoint a, CurvePoint b) => a.pos - b.pos;
        public static Vector3 operator *(float t, CurvePoint a) => t * a.pos;
        public static Vector3 operator *(CurvePoint a, float t) => t * a.pos;
    }

}
