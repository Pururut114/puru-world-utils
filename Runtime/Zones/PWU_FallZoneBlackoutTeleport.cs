using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEngine.Rendering.PostProcessing;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Zones/PWU_FallZoneBlackoutTeleport [Utility]")]
    [PWU_Note("Teleports the local player on trigger enter with a PostFX blackout transition: fade in → hold → teleport → fade out.")]
    public class PWU_FallZoneBlackoutTeleport : UdonSharpBehaviour
    {
        [Header("PostFX (PPS v2) — нужен профиль с затемнением")]
        [Tooltip("PostProcessVolume с эффектом затемнения. Пусто = взять с этого объекта.")]
        public PostProcessVolume postVolume;
        [Tooltip("Объект который включается/выключается вместе с PostFX. Пусто = postVolume.gameObject.")]
        public GameObject postObject;

        [Header("Teleport")]
        public Transform teleportTarget;
        public float teleportYOffset = 0f;

        [Header("Timings (seconds)")]
        [Tooltip("Fade 0→1 (сек).")]
        public float fadeInSeconds = 1.0f;
        [Tooltip("Держать полное затемнение (сек). extraDelayAfterBlack должен быть меньше этого значения.")]
        public float holdSeconds = 1.0f;
        [Tooltip("Задержка от начала полного затемнения до телепорта (сек). Автоматически ограничивается holdSeconds - 0.1.")]
        public float extraDelayAfterBlack = 0.5f;
        [Tooltip("Fade 1→0 (сек).")]
        public float fadeOutSeconds = 1.0f;
        [Tooltip("Задержка перед выключением PostFX после завершения fade-out (сек).")]
        public float disableDelaySeconds = 1.0f;

        private bool  _sequenceActive;
        private float _phaseStartTime;

        private void Start()
        {
            if (postVolume == null) postVolume = GetComponent<PostProcessVolume>();
            if (postObject == null && postVolume != null) postObject = postVolume.gameObject;
            if (postVolume != null) postVolume.weight = 0f;
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            if (_sequenceActive) return;
            if (postVolume == null || teleportTarget == null) return;
            StartSequence();
        }

        private void StartSequence()
        {
            _sequenceActive = true;

            if (postObject != null && !postObject.activeSelf)
                postObject.SetActive(true);
            postVolume.weight = 0f;

            _phaseStartTime = Time.time;
            SendCustomEventDelayedFrames(nameof(_FadeInTick), 1);

            float safeExtra = Mathf.Min(extraDelayAfterBlack, Mathf.Max(0f, holdSeconds - 0.1f));
            float tpDelay   = Mathf.Max(0f, fadeInSeconds + safeExtra);
            SendCustomEventDelayedSeconds(nameof(_DoTeleport), tpDelay);
        }

        public void _FadeInTick()
        {
            float t = (Time.time - _phaseStartTime) / Mathf.Max(0.0001f, fadeInSeconds);
            if (t >= 1f)
            {
                postVolume.weight = 1f;
                SendCustomEventDelayedSeconds(nameof(_StartFadeOut), Mathf.Max(0f, holdSeconds));
                return;
            }
            postVolume.weight = Mathf.Clamp01(t);
            SendCustomEventDelayedFrames(nameof(_FadeInTick), 1);
        }

        public void _DoTeleport()
        {
            VRCPlayerApi lp = Networking.LocalPlayer;
            if (lp == null || teleportTarget == null) return;

            Vector3 pos = teleportTarget.position;
            if (teleportYOffset != 0f) pos.y += teleportYOffset;
            lp.TeleportTo(pos, teleportTarget.rotation);
        }

        public void _StartFadeOut()
        {
            _phaseStartTime = Time.time;
            SendCustomEventDelayedFrames(nameof(_FadeOutTick), 1);
        }

        public void _FadeOutTick()
        {
            float t = (Time.time - _phaseStartTime) / Mathf.Max(0.0001f, fadeOutSeconds);
            if (t >= 1f)
            {
                postVolume.weight = 0f;
                SendCustomEventDelayedSeconds(nameof(_DisablePost), Mathf.Max(0f, disableDelaySeconds));
                return;
            }
            postVolume.weight = 1f - Mathf.Clamp01(t);
            SendCustomEventDelayedFrames(nameof(_FadeOutTick), 1);
        }

        public void _DisablePost()
        {
            if (postObject != null) postObject.SetActive(false);
            _sequenceActive = false;
        }
    }
}
