using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Access/PWU_AdminVisibilityFull [Utility]")]
    [PWU_Note("Enables adminObjects and disables nonAdminObjects for admins; inverted for non-admins. Explicit two-state control for both groups.")]
    public class PWU_AdminVisibilityFull : UdonSharpBehaviour
    {
        [Header("Admin display names")]
        public string[] adminNames;

        [Header("Admin state: enabled for admins, disabled for non-admins")]
        public GameObject[] adminObjects;

        [Header("Non-admin state: disabled for admins, enabled for non-admins")]
        public GameObject[] nonAdminObjects;

        private void Start()
        {
            VRCPlayerApi lp = Networking.LocalPlayer;
            if (lp == null) return;

            Apply(IsAdmin(lp.displayName));
        }

        private bool IsAdmin(string name)
        {
            for (int i = 0; i < adminNames.Length; i++)
                if (adminNames[i] == name) return true;
            return false;
        }

        private void Apply(bool isAdmin)
        {
            for (int i = 0; i < adminObjects.Length; i++)
                if (adminObjects[i] != null) adminObjects[i].SetActive(isAdmin);

            for (int i = 0; i < nonAdminObjects.Length; i++)
                if (nonAdminObjects[i] != null) nonAdminObjects[i].SetActive(!isAdmin);
        }
    }
}
