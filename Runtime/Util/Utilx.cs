using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Acorn {

    public static class Utilx {

        public static float SQRT_2 = Mathf.Sqrt(2);
        public static float SQRT_3 = Mathf.Sqrt(3);

        public static Rect INFITITY_RECT = Rect.MinMaxRect(
            Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.Infinity, Mathf.Infinity);

        public static bool IsApproxEqual(float a, float b, float precision = 0.01f) {
            return Mathf.Abs(a - b) < precision;
        }

        public static bool IsApproxEqual(Vector3 a, Vector3 b, float precision = 0.01f) {
            return Mathf.Abs(a.x - b.x) < precision &&
                Mathf.Abs(a.y - b.y) < precision &&
                Mathf.Abs(a.z - b.z) < precision;
        }

        public static bool IsApproxEqual(Color a, Color b, float precision = 0.01f) {
            return Mathf.Abs(a.r - b.r) < precision &&
                Mathf.Abs(a.g - b.g) < precision &&
                Mathf.Abs(a.b - b.b) < precision &&
                Mathf.Abs(a.a - b.a) < precision;
        }

        public static float NormalizeAngle(float angle) {
            return ((angle % 360 + 540) % 360) - 180;
        }

        public static float PerlinNoise3D(Vector3 v) {
            return PerlinNoise3D(v.x, v.y, v.z);
        }

        public static float PerlinNoise3D(float x, float y, float z) {
            float xy = Mathf.PerlinNoise(x, y);
            // float xz = Mathf.PerlinNoise(x, z);
            float yz = Mathf.PerlinNoise(y, z);
            // float yx = Mathf.PerlinNoise(y, x);
            float zx = Mathf.PerlinNoise(z, x);
            // float zy = Mathf.PerlinNoise(z, y);
            // return (xy + xz + yz + yx + zx + zy) / 6;
            return 0.3333f * (xy + yz + zx);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation) {
            if (prefab == null) {
                return null;
            }
            return Object.Instantiate(prefab, position, rotation);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position) {
            return Spawn(prefab, position, Quaternion.identity);
        }

        public static GameObject SpawnIn(Transform parent, GameObject prefab, Vector3 position, Quaternion rotation) {
            if (prefab == null) {
                return null;
            }
            return Object.Instantiate(prefab, position, rotation, parent);
        }

        public static GameObject SpawnIn(Transform parent, GameObject prefab, Vector3 position) {
            return SpawnIn(parent, prefab, position, Quaternion.identity);
        }

    }

}
