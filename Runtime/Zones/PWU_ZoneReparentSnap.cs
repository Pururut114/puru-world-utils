using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Zones/PWU_ZoneReparentSnap [Utility]")]
    [PWU_Note("Reparents targetRoot to enterMarker when the local player enters the zone, with optional position/rotation/scale snap. exitMarker reparents on exit.")]
    public class PWU_ZoneReparentSnap : UdonSharpBehaviour
    {
        [Header("Target")]
        [Tooltip("Object to reparent and snap.")]
        public Transform targetRoot;

        [Header("Markers")]
        [Tooltip("Marker to reparent targetRoot to on zone enter.")]
        public Transform enterMarker;

        [Tooltip("Marker to reparent targetRoot to on zone exit. Empty = do nothing on exit.")]
        public Transform exitMarker;

        [Header("Snap Options")]
        public bool snapPosition = true;
        public bool snapRotation = true;
        public bool snapScale    = true;

        [Header("Zone Colliders")]
        [Tooltip("Trigger colliders for the zone. Empty = auto-collect from this object and children.")]
        public Collider[] zoneColliders;

        [Header("Start Check")]
        [Tooltip("Check if player starts inside the zone and apply immediately.")]
        public bool evaluateOnStart = true;
        public int startEvalDelayFrames = 2;

        private void Start()
        {
            if (zoneColliders == null || zoneColliders.Length == 0)
            {
                Collider c = GetComponent<Collider>();
                zoneColliders = c != null
                    ? new Collider[] { c }
                    : GetComponentsInChildren<Collider>(true);
            }

            if (evaluateOnStart)
                SendCustomEventDelayedFrames(nameof(_EvalNow), startEvalDelayFrames);
        }

        public void _EvalNow()
        {
            var lp = Networking.LocalPlayer;
            if (lp == null) return;
            if (IsPointInside(lp.GetPosition())) Apply(enterMarker);
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player == null || !player.isLocal) return;
            Apply(enterMarker);
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (player == null || !player.isLocal) return;
            if (exitMarker != null) Apply(exitMarker);
        }

        private void Apply(Transform marker)
        {
            if (targetRoot == null || marker == null) return;
            targetRoot.SetParent(marker, false);
            if (snapPosition) targetRoot.localPosition = Vector3.zero;
            if (snapRotation) targetRoot.localRotation = Quaternion.identity;
            if (snapScale)    targetRoot.localScale    = Vector3.one;
        }

        private bool IsPointInside(Vector3 pos)
        {
            if (zoneColliders == null) return false;
            for (int i = 0; i < zoneColliders.Length; i++)
            {
                Collider col = zoneColliders[i];
                if (col == null) continue;
                if ((col.ClosestPoint(pos) - pos).sqrMagnitude < 1e-6f) return true;
            }
            return false;
        }
    }
}
