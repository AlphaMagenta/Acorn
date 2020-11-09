using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    [ExecuteInEditMode]
    public class MeshMod : MeshGenerator {

        public Mesh referenceMesh;
        public Vector2 uvScale = Vector2.one;
        public Vector2 uvOffset = Vector2.zero;
        public bool copyOriginalUv = true;
        public bool editNormals = false;
        public Vector3 normalOrigin = Vector3.zero;

        protected override void DoGenerate() {
            var mesh = new Mesh();
            mesh.vertices = referenceMesh.vertices;
            mesh.uv = referenceMesh.uv
                .Select(uv => Vector2.Scale(uv, uvScale) + uvOffset)
                .ToArray();
            if (copyOriginalUv) {
                mesh.uv2 = referenceMesh.uv;
            }
            if (editNormals) {
                mesh.normals = mesh.vertices.Select(v => (v - normalOrigin).normalized).ToArray();
            } else {
                mesh.normals = referenceMesh.normals;
            }
            mesh.triangles = referenceMesh.triangles;
            mesh.bounds = referenceMesh.bounds;
            mesh.RecalculateTangents();
            mainMesh = mesh;
        }

        void OnDrawMesh() {
            if (mainMesh == null) {
                return;
            }
            var bounds = mainMesh.bounds;
            Gizmos.DrawWireCube(transform.position, bounds.size);
        }
    }

}
