using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    /**
    * Utility class for building meshes which automatically welds vertices within certain threshold
    * and interpolates uvs and colors for existing vertices.
    *
    * Note: you should only use either AddVertex(v) or AddVertex(v, uv, color)
    * and never mix the two.
    */
    public class MeshBuffer {

        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<Color> colors = new List<Color>();
        public List<int> tris = new List<int>();

        private Dictionary<Vector3, int> vmap = new Dictionary<Vector3, int>();
        private float precision = 0.001f;

        public int AddVertex(Vector3 v) {
            int idx;
            Vector3 trunc = v.Roundf(precision);
            if (!vmap.TryGetValue(trunc, out idx)) {
                idx = vertices.Count;
                vertices.Add(v);
                vmap[trunc] = idx;
            }
            return idx;
        }

        public int AddVertex(Vector3 v, Vector2 uv, Color color) {
            int idx;
            Vector3 trunc = v.Roundf(precision);
            if (vmap.TryGetValue(trunc, out idx)) {
                uvs[idx] = (uvs[idx] + uv) * .5f;
                colors[idx] = (colors[idx] + color) * .5f;
            } else {
                idx = vertices.Count;
                vertices.Add(v);
                vmap[trunc] = idx;
                uvs.Add(uv);
                colors.Add(color);
            }
            return idx;
        }

        public void AddTriange(int a, int b, int c) {
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
        }

    }

}
