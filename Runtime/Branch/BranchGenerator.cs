using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Acorn {

    [ExecuteInEditMode]
    public class BranchGenerator : MeshGenerator {

        public Vector2 radiusRange = new Vector2(1f, 0.1f);
        public CurveSettings settings;
        public bool generateCollider = false;

        private CurvePoint[] controlPoints;
        private MeshCollider meshCollider;

        protected override void DoGenerate() {
            controlPoints = CollectPoints().ToArray();
            if (controlPoints.Length < 2) {
                return;
            }
            var curveMesh = new CurveMesh(settings, controlPoints);
            mainMesh = curveMesh.GenerateMesh();
        }

        public CurvePoint[] GetControlPoints() {
            if (controlPoints == null) {
                DoGenerate();
            }
            return controlPoints;
        }

        CurvePoint[] CollectPoints() {
            var children = GetChildrenPoints();
            if (children.Length < 2) {
                return new CurvePoint[0];
            }
            var points = new List<CurvePoint>();
            var inherited = GetInheritedPoints(children);
            points.AddRange(inherited);
            for (int i = 0; i < children.Length; i++) {
                var p = children[i];
                float t = (float)i / (children.Length - 1);
                points.Add(ConvertBranchPoint(p, t));
            }
            var lastPoint = points.Last();
            points.Add(new CurvePoint {
                pos = lastPoint.pos + Vector3.up,
                radius = radiusRange.y,
                color = lastPoint.color,
            });
            return points.ToArray();
        }

        CurvePoint[] GetInheritedPoints(BranchPoint[] children) {
            // Branches rely on hierarchy: if Branch component is applied to BranchPoint,
            // then it "grows" from its parent tree (inherits two previous points)
            var parentPoint = GetComponent<BranchPoint>();
            // We start with assumption that trailing point is 1 meter below the first child
            var firstPoint = parentPoint ?? children.First();
            var leading = new CurvePoint {
                pos = Vector3.down,
                radius = radiusRange.x,
                color = firstPoint.color,
            };
            if (!parentPoint) {
                // Only leading point is inherited
                return new CurvePoint[] { leading };
            }
            // If parent exists, then we try and inherit two points from it:
            // previous sibling becomes leading, this parent point becomes the first
            var parentBranch = parentPoint.GetParentBranch();
            var siblings = parentBranch.GetChildrenPoints();
            var index = System.Array.IndexOf(siblings, parentPoint);
            if (index > 0) {
                var sibling = siblings[index - 1];
                return new CurvePoint[] {
                    ConvertBranchPoint(sibling, 0),
                    ConvertBranchPoint(parentPoint, 0)
                };
            } else {
                // Not enough siblings: just reuse the parent point
                return new CurvePoint[] { leading, ConvertBranchPoint(parentPoint, 0) };
            }
        }

        CurvePoint ConvertBranchPoint(BranchPoint point, float t) {
            float r = Mathf.Lerp(radiusRange.x, radiusRange.y, t) * point.radius;
            return new CurvePoint {
                pos = transform.InverseTransformPoint(point.transform.position),
                radius = r,
                color = point.color,
            };
        }

        public BranchPoint[] GetChildrenPoints() {
            return transform.GetChildren()
                .Select(tr => tr.GetComponent<BranchPoint>())
                .Where(_ => _ != null)
                .ToArray();
        }

        protected override void InitComponents() {
            base.InitComponents();
            if (generateCollider) {
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
                meshCollider.sharedMesh = mainMesh;
            }
        }

    }

}
