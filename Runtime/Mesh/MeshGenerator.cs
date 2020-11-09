using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Acorn {

    public abstract class MeshGenerator : MonoBehaviour {

        public Material material;
        public bool castShadows = true;
        public bool receiveShadows = true;

        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;

        protected bool dirty = false;
        protected Mesh mainMesh;

        protected virtual void Awake() {
            if (Application.isPlaying) {
                InitComponents();
            }
            Generate(true);
        }

        public void Generate(bool force = false) {
            if (!force && !dirty) {
                return;
            }
            dirty = false;
            DoGenerate();
            #if UNITY_EDITOR
            if (Application.isPlaying) {
                ApplyChangesToComponents();
            }
            #else
            ApplyChangesToComponents();
            #endif
        }

        public void SetDirty(bool dirty) {
            this.dirty = dirty;
        }

        protected abstract void DoGenerate();

        protected virtual void InitComponents() {
            meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
            meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            meshRenderer.receiveShadows = receiveShadows;
        }

        protected virtual void ApplyChangesToComponents() {
            if (meshFilter.sharedMesh) {
                Destroy(meshFilter.sharedMesh);
            }
            meshFilter.sharedMesh = mainMesh;
            meshRenderer.sharedMaterial = material;
        }

        #if UNITY_EDITOR
        void Update() {
            // Allows drawing mesh in editor without having MeshFilter
            // which unnecessarily serializes the mesh in a scene
            if (!Application.isPlaying) {
                Generate();
                Graphics.DrawMesh(mainMesh, transform.localToWorldMatrix,
                    material,
                    gameObject.layer,
                    camera: null,
                    submeshIndex: 0,
                    properties: null,
                    castShadows: castShadows,
                    receiveShadows: receiveShadows,
                    useLightProbes: false);
            }
        }

        void OnValidate() {
            SetDirty(true);
        }
        #endif

    }

}
