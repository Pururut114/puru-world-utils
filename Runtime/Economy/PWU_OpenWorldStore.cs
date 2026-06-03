using UdonSharp;
using UnityEngine;
using VRC.Economy;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Economy/PWU_OpenWorldStore [Utility]")]
    [PWU_Note("Opens the world store page on Interact or via Open(). Requires Creator Economy to be enabled for the world.")]
    public class PWU_OpenWorldStore : UdonSharpBehaviour
    {
        public override void Interact()
        {
            Store.OpenWorldStorePage();
        }

        public void Open()
        {
            Store.OpenWorldStorePage();
        }
    }
}
