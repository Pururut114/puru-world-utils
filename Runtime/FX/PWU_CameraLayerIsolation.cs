using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/FX/PWU_CameraLayerIsolation [Utility]")]
    [PWU_Note("Globally strips selected layers from ScreenCamera.CullingMask. Toggle remote players and nameplates independently. Enable()/Disable() for external control. Requires VRChat SDK 3.9+.")]
    public class PWU_CameraLayerIsolation : UdonSharpBehaviour
    {
        [Header("Mode")]
        public bool effectEnabled = true;
        public bool stripRemotePlayers = true;
        public bool stripNameplates = true;

        [Header("Layers")]
        [Tooltip("Layer 9 — remote players (VRChat Layers).")]
        public int layerPlayerRemote = 9;
        [Tooltip("Layer 12 — UiMenu (nameplates etc).")]
        public int layerUiMenu = 12;

        bool _editor;
        int _baselineCullingMask;
        bool _hasBaseline;
        bool _layersStripped;

        private void Start()
        {
            _editor = Networking.LocalPlayer == null;
            CacheBaselineCullingMask();
        }

        private void OnEnable()
        {
            CacheBaselineCullingMask();
        }

        private void OnDisable()
        {
            RestoreCullingMaskIfNeeded();
        }

        private void OnDestroy()
        {
            RestoreCullingMaskIfNeeded();
        }

        public override void OnVRCCameraSettingsChanged(VRCCameraSettings cameraSettings)
        {
            if (_editor || cameraSettings == null) return;
            if (cameraSettings != VRCCameraSettings.ScreenCamera) return;
            if (!effectEnabled || !_hasBaseline) return;
            ApplyLayerStripToScreenCamera();
        }

        private void LateUpdate()
        {
            if (_editor) return;
            if (!effectEnabled)
            {
                RestoreCullingMaskIfNeeded();
                return;
            }
            ApplyLayerStripToScreenCamera();
        }

        public void Enable()
        {
            SetEffectEnabled(true);
        }

        public void Disable()
        {
            SetEffectEnabled(false);
        }

        public void SetEffectEnabled(bool enabled)
        {
            effectEnabled = enabled;
            RestoreCullingMaskIfNeeded();
            if (!enabled) return;
            CacheBaselineCullingMask();
        }

        int GetExcludeBits()
        {
            int bits = 0;
            if (stripRemotePlayers) bits |= (1 << layerPlayerRemote);
            if (stripNameplates)    bits |= (1 << layerUiMenu);
            return bits;
        }

        void CacheBaselineCullingMask()
        {
            if (_editor) return;
            VRCCameraSettings cam = VRCCameraSettings.ScreenCamera;
            if (cam == null) return;
            _baselineCullingMask = cam.CullingMask;
            _hasBaseline = true;
        }

        void ApplyLayerStripToScreenCamera()
        {
            if (!_hasBaseline) CacheBaselineCullingMask();
            if (!_hasBaseline) return;

            int bits = GetExcludeBits();
            if (bits == 0)
            {
                RestoreCullingMaskIfNeeded();
                return;
            }

            VRCCameraSettings cam = VRCCameraSettings.ScreenCamera;
            if (cam == null) return;
            cam.CullingMask = _baselineCullingMask & ~bits;
            _layersStripped = true;
        }

        void RestoreCullingMaskIfNeeded()
        {
            if (!_layersStripped || !_hasBaseline) return;
            VRCCameraSettings cam = VRCCameraSettings.ScreenCamera;
            if (cam != null) cam.CullingMask = _baselineCullingMask;
            _layersStripped = false;
        }
    }
}
