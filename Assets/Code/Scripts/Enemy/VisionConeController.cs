using System;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Scripts.Enemy
{
    public class VisionConeController : MonoBehaviour
    {
        [SerializeField] private float detectionRateSeconds = 1.0f;
        [SerializeField] public float range = 5f;
        [SerializeField] public float arcAngle = 45f;

        [NonSerialized] public float DetectionProgress = 0.0f;

        private Vector2 ranges;
        private Vector2 angles;
        private UnityEvent onFieldChange = new UnityEvent();
        public void OnFieldChangeCallback(UnityAction callback)
        {
            onFieldChange.AddListener(callback);
        }

        // Start is called before the first frame update
        private void Start()
        {
            ranges = new Vector2(range, range + 1f);
            angles = new Vector2(arcAngle, arcAngle + 10f);
        }

        public void ResetDetectionProgress()
        {
            DetectionProgress = 0.0f;
        }

        /** Assumed to be called *once* per `FixedUpdate()`*/
        public void UpdateDetectionProgress(bool hasDetectedPlayer)
        {
            DetectionProgress += Time.deltaTime / detectionRateSeconds * (hasDetectedPlayer ? 1 : -0.5f);
            DetectionProgress = Mathf.Clamp(DetectionProgress, 0.0f, 1.0f);

            // TODO: Fix ugly code
            var _ = 0.0f;
            if (DetectionProgress >= 1.0f)
            {
                range = Mathf.SmoothDamp(range, ranges[1], ref _, 0.2f);
                arcAngle = Mathf.SmoothDamp(arcAngle, angles[1], ref _, 0.2f);
                onFieldChange.Invoke();
            }
            else if (Mathf.Abs(range - ranges[0]) >= 1e-4f)
            {
                range = Mathf.SmoothDamp(range, ranges[0], ref _, 0.2f);
                arcAngle = Mathf.SmoothDamp(arcAngle, angles[0], ref _, 0.2f);
                onFieldChange.Invoke();
            }
        }
    }
}