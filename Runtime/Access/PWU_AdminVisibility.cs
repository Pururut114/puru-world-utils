using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Access/PWU_AdminVisibility [Utility]")]
    [PWU_Note("Enables adminObjects for players in adminNames. Non-admin players are untouched — their scene defaults are preserved.")]
    public class PWU_AdminVisibility : UdonSharpBehaviour
    {
        [Header("Admin display names")]
        public string[] adminNames;

        [Header("Enabled for admins — non-admins: scene defaults (untouched)")]
        public GameObject[] adminObjects;

        private void Start()
        {
            VRCPlayerApi lp = Networking.LocalPlayer;
            if (lp == null) return;

            if (!IsAdmin(lp.displayName)) return;

            for (int i = 0; i < adminObjects.Length; i++)
                if (adminObjects[i] != null) adminObjects[i].SetActive(true);
        }

        private bool IsAdmin(string name)
        {
            for (int i = 0; i < adminNames.Length; i++)
                if (adminNames[i] == name) return true;
            return false;
        }
    }
}
