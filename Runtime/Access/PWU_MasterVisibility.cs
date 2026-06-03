using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Access/PWU_MasterVisibility [Utility]")]
    [PWU_Note("Enables masterObjects for the current master player. Re-evaluates on OnPlayerLeft when master changes. Do not rely on for security — master is the oldest player, not a verified role.")]
    public class PWU_MasterVisibility : UdonSharpBehaviour
    {
        [Header("NOTE: Do not use for security — master is oldest player, not verified owner")]

        [Header("Enabled for master, disabled for others")]
        public GameObject[] masterObjects;

        [Header("Disabled for master, enabled for others")]
        public GameObject[] nonMasterObjects;

        private bool _wasMaster;

        private void Start()
        {
            VRCPlayerApi lp = Networking.LocalPlayer;
            if (lp == null) return;

            _wasMaster = Networking.IsMaster;
            Apply(_wasMaster);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (_wasMaster) return;

            bool isMaster = Networking.IsMaster;
            if (!isMaster) return;

            _wasMaster = true;
            Apply(true);
        }

        private void Apply(bool isMaster)
        {
            for (int i = 0; i < masterObjects.Length; i++)
                if (masterObjects[i] != null) masterObjects[i].SetActive(isMaster);

            for (int i = 0; i < nonMasterObjects.Length; i++)
                if (nonMasterObjects[i] != null) nonMasterObjects[i].SetActive(!isMaster);
        }
    }
}
