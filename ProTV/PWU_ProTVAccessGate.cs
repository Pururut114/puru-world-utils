using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using ArchiTech.ProTV;

namespace PuruWorldUtils
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("PWU/ProTV/PWU_ProTVAccessGate [Utility]")]
    [PWU_Note("ProTV access gate: checks TVManager + TVManagedWhitelist auth and applies teleport, avatar scaling, object/pickup restriction. Requires PWU_PROTV_INSTALLED.")]
    public class PWU_ProTVAccessGate : UdonSharpBehaviour
    {
        [Header("ProTV references (auto-fetched from parent if empty)")]
        public TVManagedWhitelist whitelist;
        public TVManager tvManager;

        [Header("Panel teleport")]
        [Tooltip("Object to move between positions based on access state.")]
        public Transform panelObject;
        [Tooltip("Position when player has NO access.")]
        public Transform unauthorizedPosition;
        [Tooltip("Position when player HAS access.")]
        public Transform authorizedPosition;

        [Header("Avatar scaling for authorized")]
        public bool enableScalingForAuthorized = true;
        [Tooltip("true = world sets an exact eye height; false = set min/max range for player-controlled scaling")]
        public bool setExactHeight = false;
        [Tooltip("Exact eye height in meters (0.1-100). Used when setExactHeight = true.")]
        public float exactEyeHeight = 1.8f;
        [Tooltip("Min eye height in meters (0.2-5.0). Used when setExactHeight = false.")]
        public float authorizedMin = 0.2f;
        [Tooltip("Max eye height in meters (0.2-5.0). Used when setExactHeight = false.")]
        public float authorizedMax = 5.0f;
        public bool restoreOnLoseAccess = true;

        [Header("Access gate: objects")]
        [Tooltip("Disabled (SetActive false) for authorized players.")]
        public GameObject[] disableWhenAuthorized;
        [Tooltip("Enabled (SetActive true) for authorized players.")]
        public GameObject[] enableWhenAuthorized;
        [Tooltip("Colliders disabled for authorized players.")]
        public Collider[] disableCollidersWhenAuthorized;
        [Tooltip("Colliders enabled for authorized players.")]
        public Collider[] enableCollidersWhenAuthorized;
        public bool restoreObjectsOnLoseAccess = true;

        [Header("Access gate: pickups")]
        [Tooltip("VRC_Pickup components on these objects are locally restricted for unauthorized players.")]
        public GameObject[] restrictPickupsForNonAuthorized;
        public bool restorePickupsOnLoseAccess = true;

        private bool  _scalingSnapshotTaken;
        private bool  _origManualAllowed;
        private float _origMin, _origMax;

        private bool   _objSnapshotTaken;
        private bool[] _origActive_disable;
        private bool[] _origActive_enable;
        private bool[] _origEnabled_disableCols;
        private bool[] _origEnabled_enableCols;

        private bool       _pickupsSnapshotTaken;
        private VRC_Pickup[] _cachedPickups;
        private bool[]     _origPickupable;

        private bool _lastAccessKnown;
        private bool _lastHasAccess;

        public void Start()
        {
#if UNITY_2022_3_OR_NEWER
            if (whitelist  == null) whitelist  = GetComponentInParent<TVManagedWhitelist>(true);
            if (tvManager  == null) tvManager  = GetComponentInParent<TVManager>(true);
#else
            if (whitelist  == null) whitelist  = GetComponentInParent<TVManagedWhitelist>();
            if (tvManager  == null) tvManager  = GetComponentInParent<TVManager>();
#endif
            if (whitelist != null) whitelist._RegisterListener(this);
            SendCustomEventDelayedFrames(nameof(UpdatePanelPosition), 2);
        }

        public void UpdateUI()  { UpdatePanelPosition(); }
        public void _TvReady() { UpdatePanelPosition(); }

        public void UpdatePanelPosition()
        {
            var lp = Networking.LocalPlayer;
            if (lp == null) return;

            if (!_scalingSnapshotTaken)
            {
                _origManualAllowed = lp.GetManualAvatarScalingAllowed();
                _origMin           = lp.GetAvatarEyeHeightMinimumAsMeters();
                _origMax           = lp.GetAvatarEyeHeightMaximumAsMeters();
                _scalingSnapshotTaken = true;
            }

            bool tvAccess = (tvManager != null) &&
                            (tvManager._IsSuperAuthorized(lp, true) || tvManager._IsAuthorized(lp, true));
            bool wlAccess = (whitelist != null) &&
                            (whitelist._IsSuperUser(lp) || whitelist._IsAuthorizedUser(lp));
            bool hasAccess = tvAccess || wlAccess;

            if (panelObject != null)
            {
                Transform t = hasAccess ? authorizedPosition : unauthorizedPosition;
                if (t != null) panelObject.SetPositionAndRotation(t.position, t.rotation);
            }

            if (enableScalingForAuthorized)
                ApplyScaling(hasAccess, lp);

            ApplyAccessObjects(hasAccess);
            ApplyPickupAccess(hasAccess);

            _lastHasAccess  = hasAccess;
            _lastAccessKnown = true;
        }

        private void ApplyScaling(bool hasAccess, VRCPlayerApi lp)
        {
            if (hasAccess)
            {
                if (setExactHeight)
                {
                    lp.SetManualAvatarScalingAllowed(false);
                    lp.SetAvatarEyeHeightByMeters(Mathf.Clamp(exactEyeHeight, 0.1f, 100f));
                }
                else
                {
                    lp.SetManualAvatarScalingAllowed(true);
                    lp.SetAvatarEyeHeightMinimumByMeters(Mathf.Max(0.2f, authorizedMin));
                    lp.SetAvatarEyeHeightMaximumByMeters(Mathf.Min(5.0f, authorizedMax));
                }
            }
            else if (restoreOnLoseAccess && _scalingSnapshotTaken)
            {
                lp.SetManualAvatarScalingAllowed(_origManualAllowed);
                lp.SetAvatarEyeHeightMinimumByMeters(_origMin);
                lp.SetAvatarEyeHeightMaximumByMeters(_origMax);
            }
        }

        private void ApplyAccessObjects(bool hasAccess)
        {
            if (!_objSnapshotTaken)
            {
                TakeObjectSnapshot();
                _objSnapshotTaken = true;
            }

            if (_lastAccessKnown && hasAccess == _lastHasAccess) return;

            if (hasAccess)
            {
                if (disableWhenAuthorized != null)
                    for (int i = 0; i < disableWhenAuthorized.Length; i++)
                        if (disableWhenAuthorized[i] != null) disableWhenAuthorized[i].SetActive(false);

                if (enableWhenAuthorized != null)
                    for (int i = 0; i < enableWhenAuthorized.Length; i++)
                        if (enableWhenAuthorized[i] != null) enableWhenAuthorized[i].SetActive(true);

                if (disableCollidersWhenAuthorized != null)
                    for (int i = 0; i < disableCollidersWhenAuthorized.Length; i++)
                        if (disableCollidersWhenAuthorized[i] != null) disableCollidersWhenAuthorized[i].enabled = false;

                if (enableCollidersWhenAuthorized != null)
                    for (int i = 0; i < enableCollidersWhenAuthorized.Length; i++)
                        if (enableCollidersWhenAuthorized[i] != null) enableCollidersWhenAuthorized[i].enabled = true;
            }
            else
            {
                if (!restoreObjectsOnLoseAccess) return;

                if (disableWhenAuthorized != null && _origActive_disable != null)
                    for (int i = 0; i < disableWhenAuthorized.Length; i++)
                        if (disableWhenAuthorized[i] != null) disableWhenAuthorized[i].SetActive(_origActive_disable[i]);

                if (enableWhenAuthorized != null && _origActive_enable != null)
                    for (int i = 0; i < enableWhenAuthorized.Length; i++)
                        if (enableWhenAuthorized[i] != null) enableWhenAuthorized[i].SetActive(_origActive_enable[i]);

                if (disableCollidersWhenAuthorized != null && _origEnabled_disableCols != null)
                    for (int i = 0; i < disableCollidersWhenAuthorized.Length; i++)
                        if (disableCollidersWhenAuthorized[i] != null) disableCollidersWhenAuthorized[i].enabled = _origEnabled_disableCols[i];

                if (enableCollidersWhenAuthorized != null && _origEnabled_enableCols != null)
                    for (int i = 0; i < enableCollidersWhenAuthorized.Length; i++)
                        if (enableCollidersWhenAuthorized[i] != null) enableCollidersWhenAuthorized[i].enabled = _origEnabled_enableCols[i];
            }
        }

        private void TakeObjectSnapshot()
        {
            if (disableWhenAuthorized != null && disableWhenAuthorized.Length > 0)
            {
                _origActive_disable = new bool[disableWhenAuthorized.Length];
                for (int i = 0; i < disableWhenAuthorized.Length; i++)
                    _origActive_disable[i] = disableWhenAuthorized[i] != null && disableWhenAuthorized[i].activeSelf;
            }
            if (enableWhenAuthorized != null && enableWhenAuthorized.Length > 0)
            {
                _origActive_enable = new bool[enableWhenAuthorized.Length];
                for (int i = 0; i < enableWhenAuthorized.Length; i++)
                    _origActive_enable[i] = enableWhenAuthorized[i] != null && enableWhenAuthorized[i].activeSelf;
            }
            if (disableCollidersWhenAuthorized != null && disableCollidersWhenAuthorized.Length > 0)
            {
                _origEnabled_disableCols = new bool[disableCollidersWhenAuthorized.Length];
                for (int i = 0; i < disableCollidersWhenAuthorized.Length; i++)
                    _origEnabled_disableCols[i] = disableCollidersWhenAuthorized[i] != null && disableCollidersWhenAuthorized[i].enabled;
            }
            if (enableCollidersWhenAuthorized != null && enableCollidersWhenAuthorized.Length > 0)
            {
                _origEnabled_enableCols = new bool[enableCollidersWhenAuthorized.Length];
                for (int i = 0; i < enableCollidersWhenAuthorized.Length; i++)
                    _origEnabled_enableCols[i] = enableCollidersWhenAuthorized[i] != null && enableCollidersWhenAuthorized[i].enabled;
            }
        }

        private void ApplyPickupAccess(bool hasAccess)
        {
            if (!_pickupsSnapshotTaken)
            {
                CachePickups();
                _pickupsSnapshotTaken = true;
            }

            if (_cachedPickups == null || _cachedPickups.Length == 0) return;
            if (_lastAccessKnown && hasAccess == _lastHasAccess) return;

            if (hasAccess)
            {
                if (restorePickupsOnLoseAccess && _origPickupable != null)
                {
                    for (int i = 0; i < _cachedPickups.Length; i++)
                        if (_cachedPickups[i] != null) _cachedPickups[i].pickupable = _origPickupable[i];
                }
                else
                {
                    for (int i = 0; i < _cachedPickups.Length; i++)
                        if (_cachedPickups[i] != null && !_cachedPickups[i].pickupable)
                            _cachedPickups[i].pickupable = true;
                }
            }
            else
            {
                for (int i = 0; i < _cachedPickups.Length; i++)
                {
                    var p = _cachedPickups[i];
                    if (p == null) continue;
                    p.Drop();
                    p.pickupable = false;
                }
            }
        }

        private void CachePickups()
        {
            if (restrictPickupsForNonAuthorized == null || restrictPickupsForNonAuthorized.Length == 0)
            {
                _cachedPickups  = new VRC_Pickup[0];
                _origPickupable = new bool[0];
                return;
            }

            int total = 0;
            for (int i = 0; i < restrictPickupsForNonAuthorized.Length; i++)
            {
                var go = restrictPickupsForNonAuthorized[i];
                if (go == null) continue;
                var single = (VRC_Pickup)go.GetComponent(typeof(VRC_Pickup));
                if (single != null) { total++; continue; }
                var many = go.GetComponentsInChildren<VRC_Pickup>(true);
                if (many != null) total += many.Length;
            }

            _cachedPickups  = new VRC_Pickup[total];
            _origPickupable = new bool[total];

            int idx = 0;
            for (int i = 0; i < restrictPickupsForNonAuthorized.Length; i++)
            {
                var go = restrictPickupsForNonAuthorized[i];
                if (go == null) continue;
                var single = (VRC_Pickup)go.GetComponent(typeof(VRC_Pickup));
                if (single != null)
                {
                    _cachedPickups[idx]  = single;
                    _origPickupable[idx] = single.pickupable;
                    idx++;
                    continue;
                }
                var many = go.GetComponentsInChildren<VRC_Pickup>(true);
                if (many == null) continue;
                for (int j = 0; j < many.Length; j++)
                {
                    _cachedPickups[idx]  = many[j];
                    _origPickupable[idx] = many[j] != null && many[j].pickupable;
                    idx++;
                }
            }
        }
    }
}
