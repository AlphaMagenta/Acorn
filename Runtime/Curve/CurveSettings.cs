using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    [System.Serializable]
    public class CurveSettings {
        [Range(0, 2)]
        public float alpha = 0.5f;
        [Range(2, 128)]
        public int segments = 4;
        [Range(2, 128)]
        public int sides = 12;

        public Vector3 startRotation = new Vector3(90f, 0, 0);

        public Vector2 uvScale = Vector2.one;
        public Vector2 uvOffset = Vector2.zero;
        public bool useContinuousV = true;

        public CurveSettings Clone() {
            return this.MemberwiseClone() as CurveSettings;
        }
    }

}
