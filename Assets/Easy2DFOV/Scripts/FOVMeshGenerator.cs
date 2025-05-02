using UnityEngine;
using UnityEngine.Rendering;

namespace Easy2DFOV
{
    public class FOVMeshGenerator : MonoBehaviour
    {
        [SerializeField] private MeshFilter fovMeshFilter;
        [Space]
        [SerializeField] private int rayCount = 15;
        [SerializeField] private float maxDistance = 15;
        [SerializeField] private float fov = 80;
        [Space]
        [SerializeField] private LayerMask obstacleLayerMask;

        private Mesh _mesh;
        private Vector3[] _vertices;
        private Vector2[] _uv;
        private int[] _triangles;
        private float _angleDelta;
        
    #if UNITY_EDITOR
        private void OnValidate()
        {
            SetFOV(fov);
            if (Application.isPlaying && _mesh != null)
            {
                SetRayCount(rayCount);
            }
        }
    #endif

        private void Awake()
        {
            _mesh = new Mesh();
            fovMeshFilter.mesh = _mesh;
            
            SetRayCount(rayCount);
            SetFOV(fov);
        }

        public void SetFOV(float value)
        {
            fov = value;
            _angleDelta = fov / rayCount;
        }

        public void SetMaxDistance(float value)
        {
            maxDistance = value;
        }

        public void SetRayCount(int rayCount)
        {
            _vertices = new Vector3[rayCount + 1 + 1];
            _uv = new Vector2[_vertices.Length];
            _triangles = new int[rayCount * 3];
            this.rayCount = rayCount;
            
            int vertexIndex = 1;
            int triangleIndex = 0;
            
            for (int i = 0; i <= rayCount; i++)
            {
                if (i > 0)
                {
                    _triangles[triangleIndex + 0] = 0;
                    _triangles[triangleIndex + 1] = vertexIndex - 1;
                    _triangles[triangleIndex + 2] = vertexIndex;
                    triangleIndex += 3;
                }

                vertexIndex++;
            }

            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;
            _mesh.uv = _uv;
        }
        
        private void Update()
        {
            GenerateFOVMesh();
        }

        private void GenerateFOVMesh()
        {
            Quaternion offsetRotation = Quaternion.Euler(Vector3.back * _angleDelta);
            Vector3 currentDirection = Quaternion.Euler(Vector3.forward * fov / 2) * transform.up;
            Quaternion inverseRotation = Quaternion.Inverse(transform.rotation);

            int vertexIndex = 1;
            _vertices[0] = Vector3.zero;
            
            for (int i = 0; i <= rayCount; i++)
            {
                Vector3 vertexPos = Vector3HitPoint(transform.position, currentDirection, maxDistance);
                _vertices[vertexIndex] = inverseRotation * (vertexPos - transform.position);
                _vertices[vertexIndex].z = 0;
                currentDirection = offsetRotation * currentDirection;
                vertexIndex++;
            }

            _mesh.vertices = _vertices;
            _mesh.uv = _uv;
            _mesh.triangles = _triangles;       
            _mesh.RecalculateBounds(MeshUpdateFlags.DontValidateIndices);
        }

        private Vector3 Vector3HitPoint(Vector3 origin, Vector3 direction, float distance)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, direction:direction, distance:distance, layerMask:obstacleLayerMask);

            if (hit)
            {
                return hit.point;
            }

            return origin + direction * distance;
        }
    }
}
