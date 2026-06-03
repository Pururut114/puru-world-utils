using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Economy/PWU_ProductToggleFull [Utility]")]
    [PWU_Note("Two-list toggle by product ownership: enableWhenOwned[] on / enableWhenNotOwned[] off for owners, inverted for others. Players in adminNames bypass the purchase check.")]
    public class PWU_ProductToggleFull : UdonSharpBehaviour
    {
        [Header("Product")]
        public UdonProduct udonProduct;

        [Header("Targets")]
        [Tooltip("Objects enabled when product IS owned (or admin override).")]
        public GameObject[] enableWhenOwned;

        [Tooltip("Objects enabled when product is NOT owned.")]
        public GameObject[] enableWhenNotOwned;

        [Header("Mode")]
        [Tooltip("React to local player ownership only. If false, reacts when any player in the instance owns the product.")]
        public bool localOnly = true;

        [Header("Admin Override")]
        [Tooltip("Players with these display names are treated as if they own the product.")]
        public string[] adminNames;

        [Tooltip("Case-insensitive admin name matching.")]
        public bool adminMatchIgnoreCase = true;

        private bool _ready;

        public override void OnPurchasesLoaded(IProduct[] products, VRCPlayerApi player)
        {
            if (player.isLocal) _ready = true;
            if (!_ready) return;
            if (localOnly && !player.isLocal) return;
            Refresh();
        }

        public override void OnPurchaseConfirmedMultiple(IProduct product, VRCPlayerApi buyer, bool isNew, int quantity)
        {
            if (!_ready || !isNew) return;
            if (udonProduct == null || product.ID != udonProduct.ID) return;
            if (localOnly && !buyer.isLocal) return;
            Refresh();
        }

        public override void OnPurchaseExpired(IProduct product, VRCPlayerApi buyer)
        {
            if (!_ready) return;
            if (udonProduct == null || product.ID != udonProduct.ID) return;
            if (localOnly && !buyer.isLocal) return;
            Refresh();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!_ready || localOnly || udonProduct == null) return;
            Refresh();
        }

        private bool IsLocalAdmin()
        {
            VRCPlayerApi lp = Networking.LocalPlayer;
            if (lp == null || adminNames == null) return false;

            string name = adminMatchIgnoreCase ? lp.displayName.ToLowerInvariant() : lp.displayName;
            for (int i = 0; i < adminNames.Length; i++)
            {
                string a = adminNames[i];
                if (string.IsNullOrEmpty(a)) continue;
                if (adminMatchIgnoreCase) a = a.ToLowerInvariant();
                if (name == a) return true;
            }
            return false;
        }

        private void Refresh()
        {
            bool owned = false;
            if (udonProduct != null)
                owned = localOnly
                    ? Store.DoesPlayerOwnProduct(Networking.LocalPlayer, udonProduct)
                    : Store.DoesAnyPlayerOwnProduct(udonProduct);

            bool effective = owned || IsLocalAdmin();

            SetListActive(enableWhenOwned, effective);
            SetListActive(enableWhenNotOwned, !effective);
        }

        private void SetListActive(GameObject[] list, bool active)
        {
            if (list == null) return;
            for (int i = 0; i < list.Length; i++)
                if (list[i] != null && list[i].activeSelf != active)
                    list[i].SetActive(active);
        }
    }
}
