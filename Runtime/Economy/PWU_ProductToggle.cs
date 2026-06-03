using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Economy/PWU_ProductToggle [Utility]")]
    [PWU_Note("Toggles targets[] based on UdonProduct ownership. defaultState = active state when not owned. localOnly = track local player only.")]
    public class PWU_ProductToggle : UdonSharpBehaviour
    {
        [Header("Product")]
        public UdonProduct udonProduct;

        [Header("Targets")]
        [Tooltip("Objects to toggle based on product ownership.")]
        public GameObject[] targets;

        [Tooltip("Active state when product is NOT owned. False = objects disabled by default, enabled when owned.")]
        public bool defaultState;

        [Header("Mode")]
        [Tooltip("React to local player ownership only. If false, reacts when any player in the instance owns the product.")]
        public bool localOnly = true;

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

        private void Refresh()
        {
            if (udonProduct == null) return;

            bool owned = localOnly
                ? Store.DoesPlayerOwnProduct(Networking.LocalPlayer, udonProduct)
                : Store.DoesAnyPlayerOwnProduct(udonProduct);

            bool active = owned ? !defaultState : defaultState;

            for (int i = 0; i < targets.Length; i++)
                if (targets[i] != null) targets[i].SetActive(active);
        }
    }
}
