using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    public static class Extensions {

        // Dictionaries

        public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV)) {
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }

        // Vectors

        public static Vector2 xy(this Vector3 vec) { return new Vector2(vec.x, vec.y); }
        public static Vector2 xz(this Vector3 vec) { return new Vector2(vec.x, vec.z); }
        public static Vector2 yz(this Vector3 vec) { return new Vector2(vec.y, vec.z); }
        public static Vector3 WithZ(this Vector2 vec, float z) { return new Vector3(vec.x, vec.y, z); }
        public static Vector3 WithZ(this Vector3 vec, float z) { return new Vector3(vec.x, vec.y, z); }
        public static Vector3 OnXZ(this Vector2 vec, float y = 0f) { return new Vector3(vec.x, y, vec.y); }

        public static Vector3 Roundf(this Vector3 vec, float precision = 0.01f) {
            float x = Mathf.Round(vec.x / precision) * precision;
            float y = Mathf.Round(vec.y / precision) * precision;
            float z = Mathf.Round(vec.z / precision) * precision;
            return new Vector3(x, y, z);
        }

        // Enumerables

        public static void Iterate<T>(this IEnumerable<T> iter, System.Action<T, int> fn) {
            var it = iter.GetEnumerator();
            int i = 0;
            while (it.MoveNext()) {
                fn(it.Current, i);
                i += 1;
            }
        }

        // Layer masks

        public static bool Contains(this LayerMask mask, int layer) {
            // return (mask & layer) == layer;
            return mask == (mask | (1 << layer));
        }

        // Transforms

        public static Transform[] GetChildren(this Transform transform) {
            Transform[] children = new Transform[transform.childCount];
            for (int i = 0; i < children.Length; i++) {
                children[i] = transform.GetChild(i);
            }
            return children;
        }

        public static bool Contains(this Transform transform, GameObject obj) {
            if (transform.gameObject == obj) {
                return true;
            }
            foreach (Transform child in transform) {
                if (child.Contains(obj)) {
                    return true;
                }
            }
            return false;
        }

        // GameObjects

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component {
            var res = obj.GetComponent<T>();
            if (res == null) {
                res = obj.AddComponent<T>();
            }
            return res;
        }

    }

}
