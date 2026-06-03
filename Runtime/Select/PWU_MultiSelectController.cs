using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    [AddComponentMenu("PWU/Select/PWU_MultiSelectController [Utility]")]
    [PWU_Note("Network-synced radio selector: exactly one of targets[] active at a time. index -1 disables all. Use alongside PWU_MultiSelectButton.")]
    public class PWU_MultiSelectController : UdonSharpBehaviour
    {
        [Header("Targets (mutually exclusive)")]
        [Tooltip("List of GameObjects. Exactly one enabled by index, others disabled. -1 = all disabled.")]
        public GameObject[] targets;

        [Header("Defaults")]
        [Tooltip("Starting index for the first-ever instance. -1 = all disabled.")]
        public int defaultSelectedIndex = -1;

        [Header("Networking")]
        [Tooltip("Broadcast Apply() to all clients immediately without waiting for deserialization.")]
        public bool broadcastApplyForInstantFeedback = true;

        [UdonSynced(UdonSyncMode.None)] private int  selectedIndex = -1;
        [UdonSynced(UdonSyncMode.None)] private bool initialized;

        void Start()
        {
            if (Networking.IsOwner(gameObject) && !initialized)
            {
                selectedIndex = defaultSelectedIndex;
                initialized   = true;
                RequestSerialization();
                Apply();
                if (broadcastApplyForInstantFeedback)
                    SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Apply));
            }
            else
            {
                Apply();
            }
        }

        public override void OnDeserialization()
        {
            Apply();
        }

        public void SelectIndex(int index)
        {
            var lp = Networking.LocalPlayer;
            if (lp == null) return;

            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(lp, gameObject);

            int clamped = ClampIndex(index);
            if (selectedIndex == clamped) return;

            selectedIndex = clamped;
            RequestSerialization();
            Apply();

            if (broadcastApplyForInstantFeedback)
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Apply));
        }

        public void SelectToggle(int index)
        {
            SelectIndex(selectedIndex == index ? -1 : index);
        }

        public void SelectNone()
        {
            SelectIndex(-1);
        }

        public override void Interact()
        {
            SelectIndex(NextIndex(selectedIndex));
        }

        public void Apply()
        {
            int count = targets == null ? 0 : targets.Length;
            int idx   = selectedIndex;

            for (int i = 0; i < count; i++)
                if (targets[i] != null) targets[i].SetActive(i == idx);
        }

        private int ClampIndex(int index)
        {
            if (targets == null || targets.Length == 0) return -1;
            if (index < -1) return -1;
            if (index >= targets.Length) return targets.Length - 1;
            return index;
        }

        private int NextIndex(int current)
        {
            int n = targets == null ? 0 : targets.Length;
            if (n <= 0) return -1;
            if (current < -1) return 0;
            if (current >= n - 1) return -1;
            return current + 1;
        }
    }
}
