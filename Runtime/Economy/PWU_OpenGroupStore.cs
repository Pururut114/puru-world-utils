using UdonSharp;
using UnityEngine;
using VRC.Economy;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Economy/PWU_OpenGroupStore [Utility]")]
    [PWU_Note("Opens the group store or group info page on Interact or via Open(). Set groupId (grp_...) and toggle openToStorePage.")]
    public class PWU_OpenGroupStore : UdonSharpBehaviour
    {
        [Header("Group")]
        public string groupId = "grp_dfe7e1fe-87c0-47bc-9480-a01ff3d9bb4c";

        [Tooltip("Open the group store page instead of the group info page.")]
        public bool openToStorePage = true;

        public override void Interact()
        {
            Open();
        }

        public void Open()
        {
            if (string.IsNullOrEmpty(groupId))
            {
                Debug.LogError($"{name}: Group ID is empty.");
                return;
            }

            if (openToStorePage)
                Store.OpenGroupStorePage(groupId);
            else
                Store.OpenGroupPage(groupId);
        }
    }
}
