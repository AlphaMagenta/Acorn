using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    [ExecuteInEditMode]
    public class BranchPoint : MonoBehaviour {

        public float radius = 1f;
        public Color color;

        private BranchGenerator ownBranch;
        private BranchGenerator parentBranch;

        public BranchGenerator GetParentBranch() {
            if (parentBranch == null) {
                parentBranch = transform.parent.GetComponent<BranchGenerator>();
            }
            return parentBranch;
        }

        public BranchGenerator GetOwnBranch() {
            if (ownBranch == null) {
                ownBranch = GetComponent<BranchGenerator>();
            }
            return ownBranch;
        }

        #if UNITY_EDITOR
        void Update() {
            if (!Application.isPlaying) {
                if (transform.hasChanged) {
                    GetParentBranch()?.SetDirty(true);
                    transform.hasChanged = false;
                }
            }
        }

        void OnValidate() {
            GetParentBranch()?.SetDirty(true);
            GetOwnBranch()?.SetDirty(true);
        }
        #endif

    }

}
