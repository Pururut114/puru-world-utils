using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/Select/PWU_MultiSelectButton [Utility]")]
    [PWU_Note("Interact button for PWU_MultiSelectController. Calls SelectIndex / SelectToggle / SelectNone on interact. toggleMode deselects on repeat press.")]
    public class PWU_MultiSelectButton : UdonSharpBehaviour
    {
        [Header("Controller")]
        public PWU_MultiSelectController controller;

        [Header("Selection")]
        [Tooltip("Index to select on interact. -1 = SelectNone.")]
        public int indexToSelect = -1;

        [Tooltip("If true: interacting when this index is already active deselects it (-1).")]
        public bool toggleMode = false;

        public override void Interact()
        {
            if (controller == null) return;

            if (indexToSelect < 0)
                controller.SelectNone();
            else if (toggleMode)
                controller.SelectToggle(indexToSelect);
            else
                controller.SelectIndex(indexToSelect);
        }

        public void Trigger()
        {
            Interact();
        }
    }
}
