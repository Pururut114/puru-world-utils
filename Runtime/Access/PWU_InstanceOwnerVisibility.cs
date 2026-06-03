using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Access/PWU_InstanceOwnerVisibility [Utility]")]
    [PWU_Note("Enables ownerObjects / disables nonOwnerObjects for the instance owner. Only works in Invite, Friends, and Friends+ instances — false in Public and Group.")]
    public class PWU_InstanceOwnerVisibility : UdonSharpBehaviour
    {
        [Header("NOTE: IsInstanceOwner = false in Public and Group instances")]

        [Header("Enabled for instance owner, disabled for others")]
        public GameObject[] ownerObjects;

        [Header("Disabled for instance owner, enabled for others")]
        public GameObject[] nonOwnerObjects;

        private void Start()
        {
            VRCPlayerApi lp = Networking.LocalPlayer;
            if (lp == null) return;

            Apply(Networking.IsInstanceOwner);
        }

        private void Apply(bool isOwner)
        {
            for (int i = 0; i < ownerObjects.Length; i++)
                if (ownerObjects[i] != null) ownerObjects[i].SetActive(isOwner);

            for (int i = 0; i < nonOwnerObjects.Length; i++)
                if (nonOwnerObjects[i] != null) nonOwnerObjects[i].SetActive(!isOwner);
        }
    }
}
