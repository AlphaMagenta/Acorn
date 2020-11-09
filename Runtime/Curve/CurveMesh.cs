using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    public class CurveMesh {

        public CurveSettings settings;
        public CurvePoint[] controlPoints;

        // Rings are always the same if settings.sides is unchanged, so are cached
        private Vector3[] _ring = new Vector3[0];
        // Cache some intermediate buffers
        private CurvePoint[] _segmentBuffer = new CurvePoint[0];
        private Vector3[] _vertices = new Vector3[0];
        private Vector3[] _normals = new Vector3[0];
        private Color[] _colors = new Color[0];
        private Vector2[] _uvs = new Vector2[0];
        private int[] _tris = new int[0];

        public CurveMesh(CurveSettings settings, CurvePoint[] controlPoints) {
            this.settings = settings;
            this.controlPoints = controlPoints;
        }

        public CurvePoint[] GeneratePoints() {
            if (controlPoints.Length < 2) {
                return new CurvePoint[0];
            }
            int size = settings.segments * (controlPoints.Length - 3) + 1;
            // Reuse buffer to reduce GC stress
            CurvePoint[] buf = new CurvePoint[size];
            Quaternion startRot = Quaternion.Euler(settings.startRotation);
            Quaternion rot = startRot;
            float length = 0;
            Vector3 lastPos = Vector3.zero;
            Vector3 lastTangent = rot * Vector3.forward;
            int iMax = controlPoints.Length - 4;
            for (int i = 0; i <= iMax; i++) {
                CurvePoint p0 = controlPoints[i];
                CurvePoint p1 = controlPoints[i + 1];
                CurvePoint p2 = controlPoints[i + 2];
                CurvePoint p3 = controlPoints[i + 3];
                // Last point of each segment is repeated by next segment,
                // except for the last segment
                int n = i == iMax ? settings.segments + 1 : settings.segments;
                var points = GenerateSingleSegment(p0, p1, p2, p3);
                for (int j = 0; j < n; j++) {
                    int index = i * settings.segments + j;
                    float t = (float)index / (size - 1);
                    float t01 = (float)j / n;
                    var p = points[j];
                    p.radius = Mathf.Lerp(p1.radius, p2.radius, t01);
                    p.color = Color.Lerp(p1.color, p2.color, t01);
                    // p.color = p1.color;
                    p.t = t;
                    // Calc next rotation
                    // Minimal twist algorithm: take initial ring rotation aligned with first tangent,
                    // then rotate every ring with a minimal angle between two tangents, around
                    // the axis formed by cross product of two tangents.
                    // https://github.com/blender/blender/blob/dd62d956872fa68120da24c7f2b4f8292e4946e8/source/blender/blenkernel/intern/curve.c#L2346-L2360
                    float angle = Vector3.Angle(lastTangent, p.tangent);
                    if (!Utilx.IsApproxEqual(angle, 0f)) {
                        Vector3 cross = Vector3.Cross(lastTangent, p.tangent);
                        rot = Quaternion.AngleAxis(angle, cross) * rot;
                    }
                    p.rot = rot;
                    // TODO length can be used for non-stretching UVs
                    p.length = length;
                    length += (p.pos - lastPos).magnitude;
                    lastTangent = p.tangent;
                    lastPos = p.pos;
                    buf[index] = p;
                }
            }
            return buf;
        }

        CurvePoint[] GenerateSingleSegment(CurvePoint p0, CurvePoint p1, CurvePoint p2, CurvePoint p3) {
            // Reuse buffer to reduce GC stress
            int size = settings.segments + 1;
            CurvePoint[] buf = _segmentBuffer.Length == size ? _segmentBuffer : new CurvePoint[size];
            _segmentBuffer = buf;
            float t0 = 0;
            float t1 = EvalT(t0, p0.pos, p1.pos);
            float t2 = EvalT(t1, p1.pos, p2.pos);
            float t3 = EvalT(t2, p2.pos, p3.pos);
            for (int i = 0; i < size; i++) {
                float t01 = (float)i / (size - 1); // [0;1]
                float t = Mathf.Lerp(t1, t2, t01);
                // Calculate curve points
                Vector3 A1 = ((t1 - t) * p0 + (t - t0) * p1) / (t1 - t0);
                Vector3 A2 = ((t2 - t) * p1 + (t - t1) * p2) / (t2 - t1);
                Vector3 A3 = ((t3 - t) * p2 + (t - t2) * p3) / (t3 - t2);
                Vector3 B1 = ((t2 - t) * A1 + (t - t0) * A2) / (t2 - t0);
                Vector3 B2 = ((t3 - t) * A2 + (t - t1) * A3) / (t3 - t1);
                Vector3 C = ((t2 - t) * B1 + (t - t1) * B2) / (t2 - t1);
                // Calculate tangents
                Vector3 dA1 = (p1 - p0) / (t1 - t0);
                Vector3 dA2 = (p2 - p1) / (t2 - t1);
                Vector3 dA3 = (p3 - p2) / (t3 - t2);
                Vector3 dB1 = ((A2 - A1) + (t2 - t) * dA1 + (t - t0) * dA2) / (t2 - t0);
                Vector3 dB2 = ((A3 - A2) + (t3 - t) * dA2 + (t - t1) * dA3) / (t3 - t1);
                Vector3 dC = ((B2 - B1) + (t2 - t) * dB1 + (t - t1) * dB2) / (t2 - t1);
                buf[i] = new CurvePoint {
                    pos = C,
                    tangent = dC,
                };
            }
            return buf;
        }

        float EvalT(float prevT, Vector3 p0, Vector3 p1) {
            return Mathf.Pow((p1 - p0).magnitude, settings.alpha) + prevT;
        }

        // Populate mesh buffers (_vertices, _uvs, _normals) with data, so that mesh can be assembled from them
        void GenerateMeshData() {
            var points = GeneratePoints();
            int n = settings.sides + 1; // Vertices per ring
            int m = points.Length;  // Rings count
            int verticesCount = n * m;
            int trisCount = settings.sides * 2 * (m - 1) * 3;
            // Init buffers
            _vertices = new Vector3[verticesCount];
            _normals = new Vector3[verticesCount];
            _colors = new Color[verticesCount];
            _uvs = new Vector2[verticesCount];
            _tris = new int[trisCount];
            // Each ring is rotated according to its curve tangent (w/ minimal twist algorithm)
            Vector3[] ring = GetUnitRing();
            for (int j = 0; j < m; j++) {
                var p = points[j];
                // Generate radial vertices (rings)
                for (int i = 0; i < n; i++) {
                    // The "global" index of vertex in mesh
                    int vertexIndex = j * n + i;
                    // We just generate points in polar coordinates of given radius in XY plane...
                    // ... then we rotate them with a current rot quaternion
                    var vertex = p.pos + p.rot * (ring[i] * p.radius);
                    var normal = (vertex - p.pos).normalized;
                    _vertices[vertexIndex] = vertex;
                    _normals[vertexIndex] = normal;
                    _colors[vertexIndex] = p.color;
                    // UVs
                    // - U goes along with i (the position inside the ring)
                    // - V is lerped across major segments
                    float u = (float)i / settings.sides;
                    float v = settings.useContinuousV ? p.length : (float)j / settings.segments;
                    Vector2 uv = Vector2.Scale(new Vector2(u, v) + settings.uvOffset, settings.uvScale);
                    _uvs[vertexIndex] = uv;
                }
            }
            // Generate triangles
            for (int j = 0; j < (m - 1); j++) {
                for (int i = 0; i < (n - 1); i++) {
                    int vertexIndex = j * n + i;
                    int triIdx = 6 * (j * (n - 1) + i);
                    _tris[triIdx] = vertexIndex;
                    _tris[triIdx + 1] = vertexIndex + n + 1;
                    _tris[triIdx + 2] = vertexIndex + n;
                    _tris[triIdx + 3] = vertexIndex;
                    _tris[triIdx + 4] = vertexIndex + 1;
                    _tris[triIdx + 5] = vertexIndex + n + 1;
                }
            }
        }

        // Generate a ring in XY plane
        Vector3[] GetUnitRing() {
            int n = settings.sides + 1;
            if (_ring.Length == n) {
                return _ring;
            }
            // Generate radial vertices (rings)
            _ring = new Vector3[n];
            for (int i = 0; i < n; i++) {
                float a = i * 2f * Mathf.PI / settings.sides;
                float x = Mathf.Cos(a);
                float y = Mathf.Sin(a);
                _ring[i] = new Vector3(x, y, 0);
            }
            return _ring;
        }

        public Mesh GenerateMesh() {
            GenerateMeshData();
            var mesh = new Mesh();
            mesh.vertices = _vertices;
            mesh.normals = _normals;
            mesh.colors = _colors;
            mesh.uv = _uvs;
            mesh.triangles = _tris;
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

    }

}
