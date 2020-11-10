using UnityEngine;
using UnityEditor;

namespace Acorn {

    [CustomEditor(typeof(CellTerrain))]
    public class CellTerrainEditor : Editor {

        static int DANGEROUS_PRESS_COUNT = 5;
        static string[] tileBrushLabels = new string[] { "Delete", "R", "G", "B" };

        bool brushEnabled = false;
        int brushMode = 0;
        Hex brushPos = Hex.zero;

        int clearPressed = 0;

        CellTerrain GetSelectedTerrain() {
            var go = Selection.activeGameObject;
            var terrain = go?.GetComponent<CellTerrain>();
            return terrain;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            DrawButtons();
        }

        void OnSceneGUI() {
            CellTerrain terrain = GetSelectedTerrain();
            if (!terrain || !brushEnabled) {
                return;
            }
            var evt = Event.current;
            if (evt.shift || evt.alt || evt.control) {
                return;
            }
            switch (evt.type) {
                case EventType.MouseMove:
                    UpdateBrushPos(terrain);
                    evt.Use();
                    break;
                case EventType.MouseDrag:
                    if (evt.button == 0) {
                        evt.Use();
                        UpdateBrushPos(terrain);
                        DoPaint(terrain);
                    }
                    break;
                case EventType.MouseDown:
                    if (evt.button == 0) {
                        evt.Use();
                        DoPaint(terrain);
                    }
                    break;
                case EventType.Layout:
                    HandleUtility.AddDefaultControl(0);
                    break;
            }
            DrawBrushDisc(terrain);
        }

        void UpdateBrushPos(CellTerrain terrain) {
            var tr = terrain.transform;
            var plane = new Plane(tr.rotation * Vector3.back, tr.position);
            var pos = terrain.transform.InverseTransformPoint(GetMousePosOnPlane(plane));
            var hex = Hex.FromPlanar(pos, terrain.settings.radius);
            var offset = hex.ToOffset();
            if (!terrain.buffer.Contains(offset.x, offset.y)) {
                return;
            }
            brushPos = hex;
        }

        void DoPaint(CellTerrain terrain) {
            clearPressed = 0;
            Undo.RecordObject(terrain, "Paint cells");
            terrain.AddCell(brushPos, (byte)brushMode);
            terrain.Generate();
        }

        void DoClear(CellTerrain terrain) {
            clearPressed = 0;
            Undo.RecordObject(terrain, "Clear cells");
            terrain.Clear();
            terrain.Generate();
        }

        void DrawBrushDisc(CellTerrain terrain) {
            Handles.color = GetColor();
            var hexPos = brushPos.ToPlanar(terrain.settings.radius).WithZ(0);
            var pos = terrain.transform.TransformPoint(hexPos);
            var rot = terrain.transform.rotation * Vector3.forward;
            Handles.DrawSolidDisc(pos, rot, terrain.settings.radius);
        }

        void DrawButtons() {
            var terrain = GetSelectedTerrain();
            brushEnabled = GUILayout.Toggle(brushEnabled, "Paint");
            if (brushEnabled) {
                brushMode = GUILayout.Toolbar(brushMode, tileBrushLabels);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear " + (DANGEROUS_PRESS_COUNT - clearPressed))) {
                    clearPressed += 1;
                    if (clearPressed == DANGEROUS_PRESS_COUNT) {
                        DoClear(terrain);
                        clearPressed = 0;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        Color GetColor() {
            return brushMode == 1 ? Color.red :
                brushMode == 2 ? Color.green :
                brushMode == 3 ? Color.blue : Color.black;
        }

        Vector3 GetMousePosOnPlane(Plane plane) {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float distance = 0f;
            plane.Raycast(ray, out distance);
            return ray.GetPoint(distance);
        }

    }

}
