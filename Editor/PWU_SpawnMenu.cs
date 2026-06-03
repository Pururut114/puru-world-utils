#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PuruWorldUtils.Editor
{
    public static class PWU_SpawnMenu
    {
        // ── Zones ─────────────────────────────────────────────────────────────

        [MenuItem("Tools/World Utils/Spawn/Zones/Zone — Enable While Inside")]
        static void SpawnZoneEnableWhileInside()
        {
            var go = new GameObject("PWU_Zone_EnableWhileInside");

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(4f, 3f, 4f);

            go.AddComponent<PWU_ZoneEnableWhileInside>();

            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Zone Enable While Inside");
        }

        [MenuItem("Tools/World Utils/Spawn/Zones/Zone — Reparent Snap")]
        static void SpawnZoneReparentSnap()
        {
            var go = new GameObject("PWU_Zone_ReparentSnap");

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(4f, 3f, 4f);

            go.AddComponent<PWU_ZoneReparentSnap>();

            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Zone Reparent Snap");
        }

        [MenuItem("Tools/World Utils/Spawn/Zones/Zone — Head Toggle")]
        static void SpawnZoneHeadToggle()
        {
            var go = new GameObject("PWU_Zone_HeadToggle");

            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(4f, 3f, 4f);

            go.AddComponent<PWU_ZoneHeadToggle>();

            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Zone Head Toggle");
        }

        [MenuItem("Tools/World Utils/Spawn/Zones/Fall Zone — Blackout Teleport")]
        static void SpawnFallZoneBlackoutTeleport()
        {
            var go = new GameObject("PWU_Zone_BlackoutTeleport");

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(4f, 2f, 4f);

            go.AddComponent<PWU_FallZoneBlackoutTeleport>();

            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Fall Zone Blackout Teleport");
        }

        // ── Persistence ──────────────────────────────────────────────────────

        [MenuItem("Tools/World Utils/Spawn/Persistence/Position Persistence")]
        static void SpawnPositionPersistence()
        {
            var go = new GameObject("PWU_PositionPersistence");
            go.AddComponent<PWU_PositionPersistence>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Position Persistence");
        }

        // ── Teleport ─────────────────────────────────────────────────────────

        [MenuItem("Tools/World Utils/Spawn/Teleport/Interact Teleport")]
        static void SpawnInteractTeleport()
        {
            var go = new GameObject("PWU_InteractTeleport");
            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 2f, 0.1f);
            go.AddComponent<PWU_InteractTeleport>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Interact Teleport");
        }

        [MenuItem("Tools/World Utils/Spawn/Teleport/Pickup Portal")]
        static void SpawnPickupPortal()
        {
            var go = new GameObject("PWU_PickupPortal");
            go.AddComponent<PWU_PickupPortal>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Pickup Portal");
        }

        // ── FX ───────────────────────────────────────────────────────────────

        [MenuItem("Tools/World Utils/Spawn/FX/Fade On Join")]
        static void SpawnFadeOnJoin()
        {
            var go = new GameObject("PWU_FadeOnJoin");
            go.AddComponent<PWU_FadeOnJoin>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Fade On Join");
        }

        [MenuItem("Tools/World Utils/Spawn/FX/Web Image Frame")]
        static void SpawnWebImageFrame()
        {
            var go = new GameObject("PWU_WebImageFrame");
            go.AddComponent<PWU_WebImageFrame>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Web Image Frame");
        }

        [MenuItem("Tools/World Utils/Spawn/FX/Material Cycler")]
        static void SpawnMaterialCycler()
        {
            var go = new GameObject("PWU_MaterialCycler");
            go.AddComponent<PWU_MaterialCycler>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Material Cycler");
        }

        [MenuItem("Tools/World Utils/Spawn/FX/Camera Layer Isolation")]
        static void SpawnCameraLayerIsolation()
        {
            var go = new GameObject("PWU_CameraLayerIsolation");
            go.AddComponent<PWU_CameraLayerIsolation>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Camera Layer Isolation");
        }

        [MenuItem("Tools/World Utils/Spawn/FX/Camera Layer Isolation — Zone")]
        static void SpawnCameraLayerIsolationZone()
        {
            var go = new GameObject("PWU_CameraLayerIsolation_Zone");

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(4f, 3f, 4f);

            go.AddComponent<PWU_CameraLayerIsolationZone>();

            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Camera Layer Isolation Zone");
        }

        // ── Select ───────────────────────────────────────────────────────────

        [MenuItem("Tools/World Utils/Spawn/Select/Multi-Select Controller")]
        static void SpawnMultiSelectController()
        {
            var go = new GameObject("PWU_MultiSelectController");
            go.AddComponent<PWU_MultiSelectController>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Multi-Select Controller");
        }

        [MenuItem("Tools/World Utils/Spawn/Select/Multi-Select Button")]
        static void SpawnMultiSelectButton()
        {
            var go = new GameObject("PWU_MultiSelectButton");
            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(0.5f, 0.5f, 0.1f);
            go.AddComponent<PWU_MultiSelectButton>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Multi-Select Button");
        }

        // ── Access ───────────────────────────────────────────────────────────

        [MenuItem("Tools/World Utils/Spawn/Access/Admin Visibility")]
        static void SpawnAdminVisibility()
        {
            var go = new GameObject("PWU_AdminVisibility");
            go.AddComponent<PWU_AdminVisibility>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Admin Visibility");
        }

        [MenuItem("Tools/World Utils/Spawn/Access/Admin Visibility Full")]
        static void SpawnAdminVisibilityFull()
        {
            var go = new GameObject("PWU_AdminVisibilityFull");
            go.AddComponent<PWU_AdminVisibilityFull>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Admin Visibility Full");
        }

        [MenuItem("Tools/World Utils/Spawn/Access/Instance Owner Visibility")]
        static void SpawnInstanceOwnerVisibility()
        {
            var go = new GameObject("PWU_InstanceOwnerVisibility");
            go.AddComponent<PWU_InstanceOwnerVisibility>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Instance Owner Visibility");
        }

        [MenuItem("Tools/World Utils/Spawn/Access/Master Visibility")]
        static void SpawnMasterVisibility()
        {
            var go = new GameObject("PWU_MasterVisibility");
            go.AddComponent<PWU_MasterVisibility>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Master Visibility");
        }

        [MenuItem("Tools/World Utils/Spawn/Access/Zone — Admin Visibility")]
        static void SpawnZoneAdminVisibility()
        {
            var go = new GameObject("PWU_Zone_AdminVisibility");

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(4f, 3f, 4f);

            go.AddComponent<PWU_ZoneAdminVisibility>();

            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Zone Admin Visibility");
        }

        [MenuItem("Tools/World Utils/Spawn/Access/Zone — Admin Visibility Full")]
        static void SpawnZoneAdminVisibilityFull()
        {
            var go = new GameObject("PWU_Zone_AdminVisibilityFull");

            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(4f, 3f, 4f);

            go.AddComponent<PWU_ZoneAdminVisibilityFull>();

            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Zone Admin Visibility Full");
        }

        // ── Economy ──────────────────────────────────────────────────────────

        [MenuItem("Tools/World Utils/Spawn/Economy/Product Toggle")]
        static void SpawnProductToggle()
        {
            var go = new GameObject("PWU_ProductToggle");
            go.AddComponent<PWU_ProductToggle>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Product Toggle");
        }

        [MenuItem("Tools/World Utils/Spawn/Economy/Product Toggle Full")]
        static void SpawnProductToggleFull()
        {
            var go = new GameObject("PWU_ProductToggleFull");
            go.AddComponent<PWU_ProductToggleFull>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Product Toggle Full");
        }

        [MenuItem("Tools/World Utils/Spawn/Economy/Open World Store")]
        static void SpawnOpenWorldStore()
        {
            var go = new GameObject("PWU_OpenWorldStore");
            go.AddComponent<PWU_OpenWorldStore>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Open World Store");
        }

        [MenuItem("Tools/World Utils/Spawn/Economy/Open Group Store")]
        static void SpawnOpenGroupStore()
        {
            var go = new GameObject("PWU_OpenGroupStore");
            go.AddComponent<PWU_OpenGroupStore>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Open Group Store");
        }

        [MenuItem("Tools/World Utils/Spawn/Economy/Open Listing")]
        static void SpawnOpenListing()
        {
            var go = new GameObject("PWU_OpenListing");
            go.AddComponent<PWU_OpenListing>();
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU Open Listing");
        }

        // ── ProTV (conditional) ───────────────────────────────────────────────

#if PWU_PROTV_INSTALLED
        [MenuItem("Tools/World Utils/Spawn/ProTV/ProTV Access Gate")]
        static void SpawnProTVAccessGate()
        {
            var go = new GameObject("PWU_ProTVAccessGate");
            var type = FindType("PuruWorldUtils.PWU_ProTVAccessGate");
            if (type != null) go.AddComponent(type);
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU ProTV Access Gate");
        }

        [MenuItem("Tools/World Utils/Spawn/ProTV/ProTV Ambient Fade")]
        static void SpawnProTVAmbientFade()
        {
            var go = new GameObject("PWU_ProTVAmbientFade");
            var type = FindType("PuruWorldUtils.PWU_ProTVAmbientFade");
            if (type != null) go.AddComponent(type);
            PlaceInSceneView(go);
            RegisterAndSelect(go, "Create PWU ProTV Ambient Fade");
        }
#endif

        // ── Helpers ───────────────────────────────────────────────────────────

        static System.Type FindType(string fullName)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }

        static void PlaceInSceneView(GameObject go)
        {
            SceneView sv = SceneView.lastActiveSceneView;
            if (sv != null)
                go.transform.position = sv.pivot;
        }

        static void RegisterAndSelect(GameObject go, string undoName)
        {
            Undo.RegisterCreatedObjectUndo(go, undoName);
            Selection.activeGameObject = go;
        }
    }
}
#endif
