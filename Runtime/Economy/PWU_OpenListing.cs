using UdonSharp;
using UnityEngine;
using VRC.Economy;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Economy/PWU_OpenListing [Utility]")]
    [PWU_Note("Opens a listing purchase screen on Interact. Optional timedObject briefly activates (e.g. an ad poster) and auto-hides after activeDuration seconds.")]
    public class PWU_OpenListing : UdonSharpBehaviour
    {
        [Header("Listing")]
        [Tooltip("Listing ID from VRChat.com (starts with prod_).")]
        public string listingId = "prod_82611801-2436-4533-9a94-b7c0f29e35bb";

        [Header("Timed Object")]
        [Tooltip("Object to activate after opening the listing (e.g. an ad poster). Leave empty to skip.")]
        public GameObject timedObject;

        [Tooltip("Seconds to keep the object active. 0 or less = keep active indefinitely.")]
        public float activeDuration = 2f;

        public override void Interact()
        {
            Open();
        }

        public void Open()
        {
            if (string.IsNullOrEmpty(listingId))
            {
                Debug.LogError($"{name}: Listing ID is empty.");
                return;
            }

            Store.OpenListing(listingId);

            if (timedObject == null) return;

            timedObject.SetActive(true);

            if (activeDuration > 0f)
                SendCustomEventDelayedSeconds(nameof(_HideTimedObject), activeDuration);
        }

        public void _HideTimedObject()
        {
            if (timedObject != null)
                timedObject.SetActive(false);
        }
    }
}
