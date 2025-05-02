using UnityEngine;

namespace Easy2DFOV
{
    public class FOVCameraSetupManager : MonoBehaviour
    {
        private static readonly int _FOVTexId = Shader.PropertyToID("_FOVTex");
        private static readonly int _ViewTexId = Shader.PropertyToID("_HideableTex");
        
        [SerializeField] private Material material;
        [SerializeField] private Camera fovRenderCam;
        [SerializeField] private Camera hideableRenderCam;
        
        [Header("Quality")] 
        [Tooltip("Decreases the resolution of FOV Camera.")]
        [SerializeField, Range(1, 8)] private byte _fovCameraQualityDivider = 2;
        
        private RenderTexture _fovRenderTexture;
        private RenderTexture _viewRenderTexture;

        private void Awake()
        {
            InitTextures();
        }

        private void InitTextures()
        {
            _fovRenderTexture = new RenderTexture(Screen.width / _fovCameraQualityDivider, Screen.height / _fovCameraQualityDivider, 0);
            _viewRenderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            
            _fovRenderTexture.Create();
            _viewRenderTexture.Create();
            
            fovRenderCam.targetTexture = _fovRenderTexture;
            hideableRenderCam.targetTexture = _viewRenderTexture;
            
            material.SetTexture(_FOVTexId, _fovRenderTexture);
            material.SetTexture(_ViewTexId, _viewRenderTexture);
        }
        
        private void ReleaseTextures()
        {
            _fovRenderTexture.Release();
            _viewRenderTexture.Release();
        }

        public void RestartTextures()
        {
            ReleaseTextures();
            InitTextures();
        }

        private void OnDestroy()
        {
            ReleaseTextures();
        }
    }
}
