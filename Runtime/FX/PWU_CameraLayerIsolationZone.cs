using UdonSharp;
using UnityEngine;
using VRC.SDK3.Rendering;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/FX/PWU_CameraLayerIsolationZone [Utility]")]
    [PWU_Note("Zone-based camera layer isolation. Strips remote players and/or nameplates from ScreenCamera.CullingMask while local player is inside the trigger. Restores on exit or respawn. Requires VRChat SDK 3.9+.")]
    public class PWU_CameraLayerIsolationZone : UdonSharpBehaviour
    {
        [Header("Mode")]
        public bool stripRemotePlayers = true;
        public bool stripNameplates = true;

        [Header("Layers")]
        [Tooltip("Layer 9 — remote players (VRChat Layers).")]
        public int layerPlayerRemote = 9;
        [Tooltip("Layer 12 — UiMenu (nameplates etc).")]
        public int layerUiMenu = 12;

        [Header("Start Behavior")]
        [Tooltip("Check if player is already inside the zone on Start (2-frame delay).")]
        public bool evaluateOnStart = true;

        bool _editor;
        bool _insideZone;
        int _baselineCullingMask;
        bool _hasBaseline;
        bool _layersStripped;

        private void Start()
        {
            _editor = Networking.LocalPlayer == null;
            CacheBaselineCullingMask();
            if (evaluateOnStart && !_editor)
                SendCustomEventDelayedFrames(nameof(_EvaluateStart), 2);
        }

        private void OnDestroy()
        {
            RestoreCullingMaskIfNeeded();
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            _insideZone = true;
            ApplyLayerStripToScreenCamera();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            _insideZone = false;
            RestoreCullingMaskIfNeeded();
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            _insideZone = false;
            RestoreCullingMaskIfNeeded();
            SendCustomEventDelayedFrames(nameof(_EvaluateStart), 2);
        }

        public override void OnVRCCameraSettingsChanged(VRCCameraSettings cameraSettings)
        {
            if (_editor || cameraSettings == null) return;
            if (cameraSettings != VRCCameraSettings.ScreenCamera) return;
            if (!_insideZone || !_hasBaseline) return;
            ApplyLayerStripToScreenCamera();
        }

        public void _EvaluateStart()
        {
            VRCPlayerApi lp = Networking.LocalPlayer;
            if (lp == null) return;
            Collider col = GetComponent<Collider>();
            if (col == null || !col.isTrigger) return;
            if (col.bounds.Contains(lp.GetPosition()))
            {
                _insideZone = true;
                ApplyLayerStripToScreenCamera();
            }
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
            if (bits == 0) return;

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
