using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    [System.Serializable]
    public class CellTerrainSettings {
        public float radius = .5f;
        public float depth = .5f;
        public float edgeVertsInset = .5f;
        public float edgeInflate = .25f;
        public int lateralLoops = 1;
        public Vector2 uvScale = Vector2.one;
        public float uvBackOffset = 1f;
        public Vector2 noiseScale = Vector2.one;
        public Vector3 displacement = Vector3.zero;
        public bool generateCollider = false;
        public float colliderMidpoint = 0.5f;
        public bool cullInsideCaps = false;
    }

}
