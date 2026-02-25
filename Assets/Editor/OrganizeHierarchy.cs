using System.Collections.Generic;
using Game.Enemy;
using Game.LevelSystem;
using Game.NPC;
using Game.Quest;
using Game.Weather;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Меню: Tools → Organize Scene Hierarchy — создаёт папки и раскладывает объекты сцены по типам.
    /// </summary>
    public static class OrganizeHierarchy
    {
        [MenuItem("Tools/Organize Scene Hierarchy")]
        [MenuItem("Window/Organize Scene Hierarchy")]
        public static void Run()
        {
            var roots = new List<Transform>();
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.transform.parent == null)
                    roots.Add(go.transform);
            }

            var folders = CreateFolders();
            int moved = 0;

            foreach (Transform t in roots)
            {
                if (folders.ContainsValue(t)) continue;

                Transform target = GetTargetFolder(t, folders);
                if (target != null)
                {
                    Undo.SetTransformParent(t, target, "Organize Hierarchy");
                    moved++;
                }
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[Organize Hierarchy] Разложено объектов: {moved}. Папки: 1_Managers, 2_Environment, 3_Platforms, 4_NPCs, 5_Player, 6_Enemies, 7_UI, 8_Items, 9_Other.");
        }

        private static Dictionary<string, Transform> CreateFolders()
        {
            var names = new[] { "1_Managers", "2_Environment", "3_Platforms", "4_NPCs", "5_Player", "6_Enemies", "7_UI", "8_Items", "9_Other" };
            var folders = new Dictionary<string, Transform>();

            foreach (string n in names)
            {
                var existing = GameObject.Find(n);
                if (existing != null)
                {
                    folders[n] = existing.transform;
                    continue;
                }
                var go = new GameObject(n);
                Undo.RegisterCreatedObjectUndo(go, "Organize Hierarchy");
                folders[n] = go.transform;
            }

            return folders;
        }

        private static Transform GetTargetFolder(Transform t, Dictionary<string, Transform> folders)
        {
            GameObject go = t.gameObject;

            if (go.GetComponent<LevelManager>() != null) return folders["1_Managers"];
            if (go.GetComponent<QuestManager>() != null) return folders["1_Managers"];
            if (go.GetComponent<WeatherManager>() != null) return folders["1_Managers"];

            if (go.GetComponent<DeathBounds>() != null) return folders["2_Environment"];
            if (go.GetComponent<Checkpoint>() != null) return folders["2_Environment"];
            if (go.GetComponent<Ladder>() != null) return folders["2_Environment"];
            if (go.GetComponent<LevelPortal>() != null) return folders["2_Environment"];
            if (go.GetComponent<OneWayPlatform>() != null) return folders["2_Environment"];

            if (go.GetComponent<MovingPlatform>() != null) return folders["3_Platforms"];
            if (go.GetComponent<BreakablePlatform>() != null) return folders["3_Platforms"];
            if (go.GetComponent<QuestDoorPlatform>() != null) return folders["3_Platforms"];
            if (go.name.ToLowerInvariant().Contains("platform")) return folders["3_Platforms"];

            if (go.GetComponent<TalkableNPC>() != null) return folders["4_NPCs"];

            if (go.CompareTag("Player")) return folders["5_Player"];

            if (go.GetComponent<EnemyHealth>() != null) return folders["6_Enemies"];
            if (go.GetComponent<EnemyAI>() != null) return folders["6_Enemies"];

            if (go.GetComponent<Canvas>() != null) return folders["7_UI"];
            if (go.GetComponent<UnityEngine.UI.CanvasScaler>() != null) return folders["7_UI"];
            if (t.parent != null && t.parent.GetComponent<Canvas>() != null) return null;

            if (go.GetComponent<Game.Inventory.ItemPickup>() != null) return folders["8_Items"];

            if (go.GetComponent<Camera>() != null) return null;
            if (go.name.StartsWith("1_") || go.name.StartsWith("2_") || go.name.StartsWith("3_") || go.name.StartsWith("4_") || go.name.StartsWith("5_") || go.name.StartsWith("6_") || go.name.StartsWith("7_") || go.name.StartsWith("8_") || go.name.StartsWith("9_")) return null;

            return folders["9_Other"];
        }
    }
}
