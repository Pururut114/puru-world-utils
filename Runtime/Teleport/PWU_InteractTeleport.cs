using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Teleport/PWU_InteractTeleport [Utility]")]
    [PWU_Note("Teleports the local player to destination on Interact. Optional PostFX blackout transition. triggerOnce disables the collider after first use.")]
    public class PWU_InteractTeleport : UdonSharpBehaviour
    {
        [Header("Teleport")]
        [Tooltip("Where the local player teleports to.")]
        public Transform destination;

        [Header("Fade (optional)")]
        [Tooltip("PostProcessVolume for blackout transition. Empty = instant teleport, no fade.")]
        public PostProcessVolume fadeVolume;

        [Tooltip("Seconds to fade to black and back.")]
        public float fadeDuration = 0.5f;

        [Tooltip("Seconds to hold at full black before teleporting.")]
        public float holdSeconds = 0.2f;

        [Header("Options")]
        [Tooltip("Disable after first use.")]
        public bool triggerOnce = false;

        private bool _running;
        private bool _used;
        private float _t0;
        private int   _phase; // 1 = fade in, 2 = hold, 3 = fade out

        private void Start()
        {
            if (fadeVolume != null) fadeVolume.weight = 0f;
        }

        public override void Interact()
        {
            if (_running) return;
            if (triggerOnce && _used) return;
            _used = true;

            if (fadeVolume == null) { TeleportPlayer(); return; }

            _running = true;
            _phase   = 1;
            _t0      = Time.time;
            SendCustomEventDelayedFrames(nameof(_Tick), 1);
        }

        public void _Tick()
        {
            if (!_running) return;
            float e = Time.time - _t0;

            if (_phase == 1)
            {
                float t = e / Mathf.Max(0.0001f, fadeDuration);
                fadeVolume.weight = Mathf.Clamp01(t);
                if (t >= 1f) { fadeVolume.weight = 1f; TeleportPlayer(); _phase = 2; _t0 = Time.time; }
            }
            else if (_phase == 2)
            {
                if (e >= holdSeconds) { _phase = 3; _t0 = Time.time; }
            }
            else if (_phase == 3)
            {
                float t = e / Mathf.Max(0.0001f, fadeDuration);
                fadeVolume.weight = Mathf.Clamp01(1f - t);
                if (t >= 1f) { fadeVolume.weight = 0f; _running = false; return; }
            }

            SendCustomEventDelayedFrames(nameof(_Tick), 1);
        }

        private void TeleportPlayer()
        {
            var lp = Networking.LocalPlayer;
            if (destination == null || lp == null) return;
            lp.TeleportTo(destination.position, destination.rotation);
        }
    }
}
