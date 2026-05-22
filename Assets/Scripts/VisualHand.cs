using UnityEngine;
using TheBlob.Data;

namespace TheBlob.Visual
{
    /// <summary>
    /// Generates a procedural mesh for one hand, stretching from shoulder to tip
    /// with a bezier curve for smooth bending. Tiles a seamless texture along the arm.
    /// 
    /// Attach to the same GameObject as HandController.
    /// Requires a MeshFilter and MeshRenderer (auto-added).
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class VisualHand : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TheBlob.Physics.HandController _handController;

        [Header("Curve Shape")]
        [Tooltip("How far the control point bows outward from the midpoint.")]
        [Range(0f, 3f)]
        [SerializeField] private float _curveBow = 0.8f;

        [Tooltip("Direction bias for the curve bow. Leave zero to auto-calculate.")]
        [SerializeField] private Vector2 _bowDirectionBias = Vector2.zero;

        [Header("Jelly Effect")]
        [Range(0f, 2f)]
        [SerializeField] private float _jellyAmplitude = 0.3f;

        [Range(0f, 10f)]
        [SerializeField] private float _jellyFrequency = 3f;

        [Header("Material")]
        [SerializeField] private Material _handMaterial;

        private Mesh _mesh;
        private Vector3[] _vertices;
        private Vector2[] _uvs;
        private int[] _triangles;
        private bool _trianglesApplied;
        private float _velocityMagnitude;
        private Vector2 _prevTip;

        private HandSettings Settings => _handController.Settings;
        private int SegCount => Settings.meshSegments;
        private int VertCount => (SegCount + 1) * 2;

        private void Awake()
        {
            var mf = GetComponent<MeshFilter>();
            var mr = GetComponent<MeshRenderer>();

            _mesh = new Mesh { name = "HandMesh" };
            _mesh.MarkDynamic();
            mf.mesh = _mesh;

            if (_handMaterial != null)
                mr.material = _handMaterial;

            _vertices = new Vector3[VertCount];
            _uvs = new Vector2[VertCount];
            _triangles = BuildTriangleIndices();
            _trianglesApplied = false;
        }

        private int[] BuildTriangleIndices()
        {
            var tris = new int[SegCount * 6];
            int idx = 0;
            for (int i = 0; i < SegCount; i++)
            {
                int bl = i * 2, br = i * 2 + 1;
                int tl = (i + 1) * 2, tr = (i + 1) * 2 + 1;
                tris[idx++] = bl; tris[idx++] = tl; tris[idx++] = br;
                tris[idx++] = br; tris[idx++] = tl; tris[idx++] = tr;
            }
            return tris;
        }

        private void LateUpdate()
        {
            var shoulder = _handController.ShoulderPosition;
            var tip = _handController.TipPosition;

            if (Time.deltaTime > 0f)
            {
                float v = (tip - _prevTip).magnitude / Time.deltaTime;
                _velocityMagnitude = Mathf.Lerp(_velocityMagnitude, v, 8f * Time.deltaTime);
            }

            GenerateVertices(shoulder, tip);

            // CRITICAL: set vertices FIRST, then triangles on first frame
            _mesh.vertices = _vertices;
            _mesh.uv = _uvs;

            if (!_trianglesApplied)
            {
                _mesh.triangles = _triangles;
                _trianglesApplied = true;
            }

            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _prevTip = tip;
        }

        private void GenerateVertices(Vector2 shoulder, Vector2 tip)
        {
            var settings = Settings;
            var armVec = tip - shoulder;
            float armLen = armVec.magnitude;

            var armDir = armLen > 0.001f ? armVec / armLen : Vector2.up;
            var perp = new Vector2(-armDir.y, armDir.x);

            // Bezier control point
            var mid = (shoulder + tip) * 0.5f;
            var bowDir = _bowDirectionBias.sqrMagnitude > 0.01f
                ? _bowDirectionBias.normalized : perp;

            float jelly = Mathf.Sin(Time.time * _jellyFrequency) * _jellyAmplitude
                          * Mathf.Clamp01(_velocityMagnitude * 0.15f);
            float scaledBow = _curveBow * Mathf.Clamp01(armLen * 0.5f);
            var cp = mid + bowDir * (scaledBow + jelly);

            float totalLen = BezierLength(shoulder, cp, tip);

            for (int i = 0; i <= SegCount; i++)
            {
                float t = (float)i / SegCount;
                var pt = Bezier(shoulder, cp, tip, t);
                var tan = BezierDeriv(shoulder, cp, tip, t);
                if (tan.sqrMagnitude < 0.0001f) tan = armDir;
                tan.Normalize();
                var n = new Vector2(-tan.y, tan.x);

                float stretch = _handController.NormalizedStretch;
                float thin = 1f - settings.stretchThinning * stretch;
                float w = WidthProfile(t) * settings.handWidth * thin;

                int vi = i * 2;
                _vertices[vi] = transform.InverseTransformPoint(pt + n * w);
                _vertices[vi + 1] = transform.InverseTransformPoint(pt - n * w);

                float v = totalLen > 0.01f ? t * totalLen * settings.uvTilingY : t * settings.uvTilingY;
                _uvs[vi] = new Vector2(0f, v);
                _uvs[vi + 1] = new Vector2(1f, v);
            }
        }

        private static float WidthProfile(float t)
        {
            return 1f - 0.7f * t * t; // full at shoulder, tapers toward tip
        }

        private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private static Vector2 BezierDeriv(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            return 2f * (1f - t) * (b - a) + 2f * t * (c - b);
        }

        private static float BezierLength(Vector2 a, Vector2 b, Vector2 c, int steps = 8)
        {
            float len = 0f;
            var prev = a;
            for (int i = 1; i <= steps; i++)
            {
                var cur = Bezier(a, b, c, (float)i / steps);
                len += Vector2.Distance(prev, cur);
                prev = cur;
            }
            return len;
        }
    }
}