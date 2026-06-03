using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Zones/PWU_ZoneHeadToggle [Utility]")]
    [PWU_Note("Activates enableTargets / deactivates disableTargets while the local player's head is inside the BoxCollider. OBB-correct — works with rotated or scaled colliders.")]
    public class PWU_ZoneHeadToggle : UdonSharpBehaviour
    {
        [Header("Targets")]
        [Tooltip("Enabled while head is inside the zone.")]
        public GameObject[] enableTargets;
        [Tooltip("Disabled while head is inside the zone.")]
        public GameObject[] disableTargets;

        private BoxCollider _zone;
        private bool _wasInside;

        private void Start()
        {
            _zone = GetComponent<BoxCollider>();
        }

        private void LateUpdate()
        {
            if (_zone == null) return;
            bool inside = IsHeadInside();
            if (inside == _wasInside) return;
            _wasInside = inside;
            Apply(inside);
        }

        private bool IsHeadInside()
        {
            Vector3 headWorld = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            Vector3 local = _zone.transform.InverseTransformPoint(headWorld);
            Vector3 half = _zone.size * 0.5f;
            Vector3 c = _zone.center;
            return local.x > c.x - half.x && local.x < c.x + half.x
                && local.y > c.y - half.y && local.y < c.y + half.y
                && local.z > c.z - half.z && local.z < c.z + half.z;
        }

        private void Apply(bool inside)
        {
            for (int i = 0; i < enableTargets.Length; i++)
                if (enableTargets[i] != null) enableTargets[i].SetActive(inside);
            for (int i = 0; i < disableTargets.Length; i++)
                if (disableTargets[i] != null) disableTargets[i].SetActive(!inside);
        }
    }
}
