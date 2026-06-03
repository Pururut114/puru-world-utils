using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using VRC.SDKBase;
using VRC.SDK3.Components;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Teleport/PWU_PickupPortal [Utility]")]
    [PWU_Note("Teleports the local player to playerDestination when the pickup is used. Optional PostFX blackout, drop after use, and pickup respawn to remoteRespawnPoint.")]
    public class PWU_PickupPortal : UdonSharpBehaviour
    {
        [Header("Teleport")]
        [Tooltip("Where the local player teleports to on use.")]
        public Transform playerDestination;

        [Header("Remote Respawn")]
        [Tooltip("Where this object (the pickup) teleports after use. Empty = stays in place.")]
        public Transform remoteRespawnPoint;

        [Header("Fade (optional)")]
        [Tooltip("PostProcessVolume for blackout transition. Empty = instant teleport.")]
        public PostProcessVolume fadeVolume;

        [Tooltip("Seconds to fade to black and back.")]
        public float fadeDuration = 0.5f;

        [Tooltip("Seconds to hold at full black before teleporting.")]
        public float holdSeconds = 0.2f;

        [Header("Pickup")]
        [Tooltip("VRC_Pickup component. Empty = grab from this object.")]
        public VRC_Pickup pickup;

        [Tooltip("VRCObjectSync component. Empty = grab from this object.")]
        public VRCObjectSync objectSync;

        [Tooltip("Only trigger when held by the local player.")]
        public bool requireHeld = true;

        [Tooltip("Drop the pickup after use.")]
        public bool dropAfterUse = true;

        private bool _running;
        private float _t0;
        private int   _phase; // 1 = fade in, 2 = hold, 3 = fade out

        private void Start()
        {
            if (pickup     == null) pickup     = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
            if (objectSync == null) objectSync = (VRCObjectSync)GetComponent(typeof(VRCObjectSync));
            if (fadeVolume != null) fadeVolume.weight = 0f;
        }

        public override void OnPickupUseDown()
        {
            if (_running) return;

            if (requireHeld)
            {
                if (pickup == null || !pickup.IsHeld) return;
                if (pickup.currentPlayer == null || !pickup.currentPlayer.isLocal) return;
            }

            if (fadeVolume == null) { TeleportPlayer(); RespawnRemote(); return; }

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
                if (t >= 1f)
                {
                    fadeVolume.weight = 0f;
                    _running = false;
                    RespawnRemote();
                    return;
                }
            }

            SendCustomEventDelayedFrames(nameof(_Tick), 1);
        }

        private void TeleportPlayer()
        {
            var lp = Networking.LocalPlayer;
            if (playerDestination == null || lp == null) return;
            lp.TeleportTo(playerDestination.position, playerDestination.rotation);
        }

        private void RespawnRemote()
        {
            var lp = Networking.LocalPlayer;

            if (dropAfterUse && pickup != null && pickup.IsHeld &&
                pickup.currentPlayer != null && pickup.currentPlayer.isLocal)
                pickup.Drop();

            if (remoteRespawnPoint == null) return;

            if (lp != null && !Networking.IsOwner(gameObject))
                Networking.SetOwner(lp, gameObject);

            if (objectSync != null) objectSync.FlagDiscontinuity();

            transform.SetPositionAndRotation(remoteRespawnPoint.position, remoteRespawnPoint.rotation);

            var rb = (Rigidbody)GetComponent(typeof(Rigidbody));
            if (rb != null)
            {
                rb.velocity        = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }
        }
    }
}
