using System.Collections.Generic;
using UnityEngine;

namespace Code.Scripts.Enemy
{
    public class VisionConeMeshRenderer : MonoBehaviour
    {
        private const int IgnoreLayerMask = ~(1 << 2);
        private const float ArcLengthDelta = 0.5f;
        private const float Epsilon = 1e-4f;

        [SerializeField] private float range = 5f;
        [SerializeField] private float arcAngle = 45f;

        private Mesh mesh;
        private float rayCount;
        private float arcAngleDelta;

        // Start is called before the first frame update
        void Start()
        {
            mesh = GetComponent<MeshFilter>().mesh = new Mesh();
            rayCount = (int) (range * (arcAngle * Mathf.Deg2Rad) / ArcLengthDelta);
            arcAngleDelta = arcAngle / rayCount;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            DrawMesh(transform);
        }

        private void DrawMesh(Transform t)
        {
            var vertices = BuildVertices(t);
            var triangles = GetTris(vertices);

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
        }

        private static Quaternion RotY(float theta)
        {
            return Quaternion.Euler(0, theta, 0);
        }

        private Vector3[] BuildVertices(Transform t)
        {
            var vertices = new List<Vector3> { t.position };
            var eye = t.forward;

            var castDirection = RotY(-arcAngle * 0.5f); // We start at negative half total arc angle.
            var prevCastDirection = castDirection;

            var hasPrevHit = false;
            var edgeHitDistance = 0f;

            for (var i = 0; i < rayCount; ++i)
            {
                castDirection *= RotY(arcAngleDelta);
                var hasHit = Physics.Raycast(t.position, castDirection * eye, out var hit, range, IgnoreLayerMask);

                if (i == 0) hasPrevHit = hasHit; // Properly initialize on first iteration.
                // If an intersection existed in the last arc (but not this one), or vice versa, we draw a tri at the edge
                // of intersection to prevent 'jumping' artifacts from switching arcs.
                if (hasHit != hasPrevHit)
                {
                    // --- Object Edge Hit Detection (binary search) ---
                    // Combining both cases of intersection -> no intersection and no intersection -> intersection
                    float low = 0f, high = arcAngleDelta;
                    do
                    {
                        var mid = low + (high - low) / 2f;
                        var hasHitEdge = Physics.Raycast(t.position, prevCastDirection * RotY(mid) * eye, out var edgeHit, range, IgnoreLayerMask);

                        low = !(hasHitEdge ^ hasHit) ? low : mid + Epsilon;
                        high = (hasHitEdge ^ hasHit) ? high : mid - Epsilon;
                        edgeHitDistance = Mathf.Max(edgeHitDistance, edgeHit.distance);
                    } while (low <= high);
                    var edgeDirection = prevCastDirection * RotY(hasHit ? low : high);

                    Debug.DrawRay(t.position, castDirection * eye * range, Color.blue);
                    Debug.DrawRay(t.position, edgeDirection * eye * range, Color.white);
                    Debug.DrawRay(t.position, prevCastDirection * eye * range, Color.red);

                    var v0 = edgeDirection * Vector3.forward * range;
                    var v1 = edgeDirection * Vector3.forward * edgeHitDistance;
                    vertices.AddRange(hasHit ? new[] {v0, v1} : new[] {v1, v0});
                }

                // Add end vertex for iteration arc
                vertices.Add(castDirection * Vector3.forward * (hasHit ? hit.distance : range));
                // Update previous value trackers
                hasPrevHit = hasHit;
                prevCastDirection = castDirection;
            }
            return vertices.ToArray();
        }


        private static int[] GetTris(IReadOnlyCollection<Vector3> vertices)
        {
            var triangles = new List<int>();
            for(var i = 0; i < vertices.Count - 2; ++i)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i + 2);
            }

            return triangles.ToArray();
        }
    }
}
