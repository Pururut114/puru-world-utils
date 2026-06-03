using UdonSharp;
using UnityEngine;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/FX/PWU_WebImageFrame [Utility]")]
    [PWU_Note("Downloads imageUrl and applies it to the renderer slot via MaterialPropertyBlock. Call BeginDownload() to re-fetch. Shows fallbackTexture before load and on error.")]
    public class PWU_WebImageFrame : UdonSharpBehaviour
    {
        [Header("Target")]
        public Renderer targetRenderer;
        [Range(0, 15)] public int materialIndex;

        [Header("Shader Properties")]
        public string albedoProperty = "_MainTex";
        public string emissionProperty = "_EmissionMap";
        public string emissionColorProperty = "_EmissionColor";
        public bool applyToAlbedo = true;
        public bool applyToEmission = true;
        public Color emissionTint = Color.white;
        [Range(0f, 10f)] public float emissionIntensity = 1f;

        [Header("Image")]
        public Texture fallbackTexture;
        public VRCUrl imageUrl;

        private VRCImageDownloader _downloader;
        private IVRCImageDownload _currentDownload;
        private MaterialPropertyBlock _mpb;
        private TextureInfo _texInfo;

        private void Start()
        {
            if (targetRenderer == null) return;

            ApplyToSlot(fallbackTexture);

            _texInfo = new TextureInfo();
            _texInfo.FilterMode = FilterMode.Bilinear;
            _texInfo.WrapModeU = TextureWrapMode.Clamp;
            _texInfo.WrapModeV = TextureWrapMode.Clamp;
            _texInfo.AnisoLevel = 0;
            _texInfo.MaterialProperty = albedoProperty;

            if (imageUrl != null && !string.IsNullOrEmpty(imageUrl.Get()))
                BeginDownload();
        }

        public void BeginDownload()
        {
            DisposeDownloader();
            _downloader = new VRCImageDownloader();
            _currentDownload = _downloader.DownloadImage(imageUrl, null, (UdonBehaviour)(object)this, _texInfo);
        }

        public void OnImageLoadSuccess(IVRCImageDownload d)
        {
            ApplyToSlot(d.Result != null ? d.Result : fallbackTexture);
        }

        public void OnImageLoadError(IVRCImageDownload d)
        {
            ApplyToSlot(fallbackTexture);
        }

        private void ApplyToSlot(Texture tex)
        {
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(_mpb, materialIndex);

            if (applyToAlbedo && !string.IsNullOrEmpty(albedoProperty))
                _mpb.SetTexture(albedoProperty, tex);

            if (applyToEmission && !string.IsNullOrEmpty(emissionProperty))
            {
                _mpb.SetTexture(emissionProperty, tex);
                if (!string.IsNullOrEmpty(emissionColorProperty))
                    _mpb.SetColor(emissionColorProperty,
                        emissionTint * Mathf.LinearToGammaSpace(Mathf.Max(0f, emissionIntensity)));
            }

            targetRenderer.SetPropertyBlock(_mpb, materialIndex);
        }

        private void DisposeDownloader()
        {
            if (_downloader != null)
            {
                _downloader.Dispose();
                _downloader = null;
            }
            _currentDownload = null;
        }

        private void OnDisable()
        {
            DisposeDownloader();
        }
    }
}
