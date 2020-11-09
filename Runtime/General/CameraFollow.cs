using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Acorn {

    [ExecuteInEditMode]
    public class CameraFollow : MonoBehaviour {

        public float distance;
        public Vector3 offset;
        public Transform followObject;

        public float positionDampTime = .25f;

        public Vector2 shiftRange = Vector2.one;
        public Vector2 angleMin = Vector2.zero;
        public Vector2 angleMax = Vector2.zero;
        public float angleDampTime = .5f;

        private Camera cam;

        private Vector3 targetPos;
        private Vector3 posDamp;

        private Vector2 currentAngle;
        private Vector2 targetAngle;
        private Vector2 angleDamp;

        private Vector2 lastCoords;
        private Vector2 shiftFactor;

        void Awake() {
            cam = GetComponent<Camera>();
            currentAngle = (angleMin + angleMax) / 2;
            targetAngle = currentAngle;
            targetPos = transform.position;
            shiftFactor = Vector2.zero;
            lastCoords= transform.position.xz();
        }

        void Update() {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                Awake();
            }
            #endif
            if (!followObject) {
                return;
            }
            UpdateShift();
            UpdateTargetPosition();
            UpdateTargetRotation();
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                transform.position = targetPos;
            }
            #endif
        }

        void LateUpdate() {
            ApplyTransformRotation();
            ApplyTransformPosition();
        }

        void UpdateShift() {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                // Applying shifting in editor results in nasty behaviour at runtime
                shiftFactor = Vector2.zero;
                return;
            }
            #endif
            if (!followObject) {
                return;
            }
            Vector2 delta = followObject.position.xz() - lastCoords;
            lastCoords = followObject.position.xz();
            shiftFactor = new Vector2(
                Mathf.Clamp(shiftFactor.x + (delta.x / shiftRange.x), -1, 1),
                Mathf.Clamp(shiftFactor.y + (delta.y / shiftRange.y), -1, 1));
        }

        void UpdateTargetPosition() {
            if (!followObject) {
                return;
            }
            // Vector3 shiftOffset = new Vector3(positionShift * shiftFactor, 0, 0);
            Vector3 pos = followObject.position;
            targetPos = pos - transform.rotation * Vector3.forward * distance + offset;
        }

        void ApplyTransformPosition() {
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref posDamp, positionDampTime);
        }

        void UpdateTargetRotation() {
            if (!followObject) {
                return;
            }
            var t = (shiftFactor + Vector2.one) * .5f;
            // Important: t.x represents the X axis shift and affects Y angle; t.z is Z axis shift and affects X angle
            targetAngle = new Vector2(
                Mathf.Lerp(angleMin.x, angleMax.x, t.y),
                Mathf.Lerp(angleMin.y, angleMax.y, t.x));
        }

        void ApplyTransformRotation() {
            if (!Utilx.IsApproxEqual(currentAngle, targetAngle)) {
                currentAngle = Vector2.SmoothDamp(currentAngle, targetAngle, ref angleDamp, angleDampTime);
            }
            Vector3 angles = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentAngle.x, currentAngle.y, angles.z);
        }

    }

}
