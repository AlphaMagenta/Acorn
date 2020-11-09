using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Acorn {

    public class CameraPanPanel : MonoBehaviour, IDragHandler {

        public Vector2 sensitivity = Vector2.one;
        public Transform cameraTarget;
        public Bounds bounds;

        public void OnDrag(PointerEventData data) {
            float dx = sensitivity.x * data.delta.x / Screen.width;
            float dy = sensitivity.y * data.delta.y / Screen.height;
            // TODO correct projection coordinates to camera rotation
            float x = Mathf.Clamp(cameraTarget.position.x + dx, bounds.min.x, bounds.max.x);
            float z = Mathf.Clamp(cameraTarget.position.z + dy, bounds.min.z, bounds.max.z);
            cameraTarget.position = new Vector3(x, 0, z);
        }

    }

}
