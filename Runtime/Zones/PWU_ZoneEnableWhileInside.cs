using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Zones/PWU_ZoneEnableWhileInside [Utility]")]
    [PWU_Note("Enables targets[] while the local player is inside the trigger collider. Set invert = true to enable while outside instead.")]
    public class PWU_ZoneEnableWhileInside : UdonSharpBehaviour
    {
        [Header("Objects to enable while local player is inside the zone")]
        public GameObject[] targets;

        [Header("Invert: objects enabled while OUTSIDE the zone")]
        public bool invert = false;

        [Header("Zone trigger colliders (isTrigger = true). Empty = auto-collect from self/children")]
        public Collider[] zoneColliders;

        [Header("Check player position on start (handles spawn-inside and far-away cases)")]
        public bool evaluateOnStart = true;
        public int startEvalDelayFrames = 2;

        private bool _inside;

        private void Start()
        {
            if (zoneColliders == null || zoneColliders.Length == 0)
                AutoCollectColliders();

            if (evaluateOnStart)
                SendCustomEventDelayedFrames(nameof(_EvalNow), startEvalDelayFrames);
        }

        private void AutoCollectColliders()
        {
            Collider[] all = GetComponentsInChildren<Collider>();

            int count = 0;
            for (int i = 0; i < all.Length; i++)
                if (all[i].isTrigger) count++;

            if (count == 0) return;

            zoneColliders = new Collider[count];
            int idx = 0;
            for (int i = 0; i < all.Length; i++)
                if (all[i].isTrigger) zoneColliders[idx++] = all[i];
        }

        public void _EvalNow()
        {
            VRCPlayerApi lp = Networking.LocalPlayer;
            if (lp == null) return;

            _inside = IsPointInside(lp.GetPosition());
            Apply();
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            _inside = true;
            Apply();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;
            _inside = false;
            Apply();
        }

        private bool IsPointInside(Vector3 pos)
        {
            if (zoneColliders == null) return false;
            for (int i = 0; i < zoneColliders.Length; i++)
            {
                Collider col = zoneColliders[i];
                if (col == null) continue;
                Vector3 closest = col.ClosestPoint(pos);
                if ((closest - pos).sqrMagnitude < 1e-6f) return true;
            }
            return false;
        }

        private void Apply()
        {
            if (targets == null) return;
            bool state = invert ? !_inside : _inside;
            for (int i = 0; i < targets.Length; i++)
                if (targets[i] != null)
                    targets[i].SetActive(state);
        }
    }
}
