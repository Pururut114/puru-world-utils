using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/FX/PWU_FadeOnJoin [Utility]")]
    [PWU_Note("Fades PostProcessVolume.weight from 1 to 0 on world join. Supports pre-fade hold and auto-disable after fade to free GPU. Call Begin() for deferred or repeated use.")]
    public class PWU_FadeOnJoin : UdonSharpBehaviour
    {
        [Header("Volume")]
        [Tooltip("PostProcessVolume to fade. Empty = grab from this object.")]
        public PostProcessVolume volume;

        [Header("Timing")]
        [Tooltip("Force weight=1 for this many frames before hold starts. Prevents flash during scene init.")]
        public int forceDarkFrames = 3;

        [Tooltip("Seconds to hold full darkness before fade begins.")]
        public float holdSeconds = 1f;

        [Tooltip("Seconds to fade weight from 1 to 0.")]
        public float fadeDuration = 2f;

        [Header("After Fade")]
        [Tooltip("Disable the volume GameObject after fade completes. Frees GPU.")]
        public bool disableAfterFade = true;

        [Tooltip("Seconds after fade before disabling. Ignored if disableAfterFade = false.")]
        public float disableDelay = 0.5f;

        [Header("Behaviour")]
        [Tooltip("Automatically begin on Start.")]
        public bool autoStart = true;

        private bool  _running;
        private float _t0;

        private void Start()
        {
            if (volume == null) volume = GetComponent<PostProcessVolume>();
            if (volume == null) { Debug.LogWarning("[PWU_FadeOnJoin] No PostProcessVolume found"); return; }
            volume.weight = 1f;
            if (autoStart)
                SendCustomEventDelayedFrames(nameof(_BeginHold), Mathf.Max(1, forceDarkFrames));
        }

        public void Begin()
        {
            if (_running) return;
            if (volume == null) volume = GetComponent<PostProcessVolume>();
            if (volume == null) return;
            volume.weight = 1f;
            SendCustomEventDelayedFrames(nameof(_BeginHold), Mathf.Max(1, forceDarkFrames));
        }

        public void _BeginHold()
        {
            if (holdSeconds > 0f)
                SendCustomEventDelayedSeconds(nameof(_BeginFade), holdSeconds);
            else
                _BeginFade();
        }

        public void _BeginFade()
        {
            if (volume == null) return;
            _running = true;
            _t0 = Time.time;
            SendCustomEventDelayedFrames(nameof(_Tick), 1);
        }

        public void _Tick()
        {
            if (!_running || volume == null) return;
            float t = (Time.time - _t0) / Mathf.Max(0.0001f, fadeDuration);
            if (t >= 1f)
            {
                volume.weight = 0f;
                _running = false;
                if (disableAfterFade)
                    SendCustomEventDelayedSeconds(nameof(_Finish), Mathf.Max(0f, disableDelay));
                return;
            }
            volume.weight = 1f - t;
            SendCustomEventDelayedFrames(nameof(_Tick), 1);
        }

        public void _Finish()
        {
            if (volume != null) volume.weight = 0f;
            volume.gameObject.SetActive(false);
        }
    }
}
