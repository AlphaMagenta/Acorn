using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    [ExecuteInEditMode]
    public class MeshTerrain : MeshGenerator {

        public MeshTerrainSettings settings;
        public TerrainBuffer buffer = new TerrainBuffer();

        private MeshCollider meshCollider;
        private Mesh collisionMesh;

        public void AddCell(Hex hex, byte data) {
            var offset = hex.ToOffset();
            var x = Mathf.Clamp(offset.x, 0, buffer.size);
            var y = Mathf.Clamp(offset.y, 0, buffer.size);
            buffer.Set(x, y, data);
            dirty = true;
        }

        public void Clear() {
            buffer.Clear();
            dirty = true;
        }

        protected override void DoGenerate() {
            var builder = new MeshTerrainBuilder(buffer, settings);
            builder.GenerateMeshes();
            mainMesh = builder.GetMainMesh();
            collisionMesh = builder.GetCollisionMesh();
        }

        protected override void InitComponents() {
            base.InitComponents();
            if (settings.generateCollider) {
                meshCollider = gameObject.GetOrAddComponent<MeshCollider>();
                meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation |
                    MeshColliderCookingOptions.UseFastMidphase;
            }
        }

        protected override void ApplyChangesToComponents() {
            base.ApplyChangesToComponents();
            if (meshCollider) {
                if (meshCollider.sharedMesh) {
                    Destroy(meshCollider.sharedMesh);
                }
                meshCollider.sharedMesh = collisionMesh;
            }
        }

        public Bounds GetBounds() {
            var max = Hex.FromOffset(buffer.size - 1, buffer.size - 1).ToPlanar(settings.radius);
            var b = new Bounds();
            b.SetMinMax(Vector3.zero, max.WithZ(settings.depth));
            return b;
        }

        #if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;
            var bounds = GetBounds();
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
        #endif

    }

}
