using UdonSharp;
using UnityEngine;
using ArchiTech.ProTV;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/ProTV/PWU_ProTVAmbientFade [Utility]")]
    [PWU_Note("ProTV plugin: fades ambient AudioSource[] to silence when playback starts, restores original volumes when playback stops. Configurable fadeDownDuration / fadeUpDuration.")]
    public class PWU_ProTVAmbientFade : TVPlugin
    {
        [Header("Ambient Sources")]
        [Tooltip("AudioSources to mute/restore when ProTV playback starts/stops.")]
        public AudioSource[] sources;

        [Header("Fade Timings")]
        [Tooltip("Seconds to fade sources to silence when playback starts.")]
        public float fadeDownDuration = 1.5f;
        [Tooltip("Seconds to restore sources to original volume when playback stops.")]
        public float fadeUpDuration = 1.5f;

        private float[] _originalVolumes;
        private float[] _fromVolumes;
        private float[] _toVolumes;
        private float   _fadeTimer;
        private float   _fadeDuration;
        private bool    _fading;

        public override void Start()
        {
            if (init) return;
            base.Start();

            if (sources == null || sources.Length == 0) return;
            _originalVolumes = new float[sources.Length];
            for (int i = 0; i < sources.Length; i++)
                if (sources[i] != null) _originalVolumes[i] = sources[i].volume;
        }

        public override void _TvPlaybackStart()
        {
            if (sources == null || sources.Length == 0) return;
            BeginFade(0f, fadeDownDuration);
        }

        public override void _TvPlaybackEnd()
        {
            if (sources == null || sources.Length == 0) return;
            BeginFadeToOriginal(fadeUpDuration);
        }

        private void Update()
        {
            if (!_fading || sources == null) return;

            _fadeTimer += Time.deltaTime;
            float t = _fadeDuration <= 0f ? 1f : Mathf.Clamp01(_fadeTimer / _fadeDuration);

            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null) continue;
                sources[i].volume = Mathf.Lerp(_fromVolumes[i], _toVolumes[i], t);
            }

            if (t >= 1f) _fading = false;
        }

        private void BeginFade(float targetVolume, float duration)
        {
            EnsureArrays();
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null) continue;
                _fromVolumes[i] = sources[i].volume;
                _toVolumes[i]   = targetVolume;
            }
            _fadeTimer    = 0f;
            _fadeDuration = duration;
            _fading       = true;
        }

        private void BeginFadeToOriginal(float duration)
        {
            EnsureArrays();
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null) continue;
                _fromVolumes[i] = sources[i].volume;
                _toVolumes[i]   = _originalVolumes != null && i < _originalVolumes.Length
                    ? _originalVolumes[i] : 1f;
            }
            _fadeTimer    = 0f;
            _fadeDuration = duration;
            _fading       = true;
        }

        private void EnsureArrays()
        {
            if (_fromVolumes == null || _fromVolumes.Length != sources.Length)
                _fromVolumes = new float[sources.Length];
            if (_toVolumes == null || _toVolumes.Length != sources.Length)
                _toVolumes = new float[sources.Length];
        }
    }
}
