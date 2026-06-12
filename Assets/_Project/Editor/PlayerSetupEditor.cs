#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using KnightGame.Player;
using KnightGame;

namespace KnightGame.EditorTools
{
    public class PlayerSetupEditor : Editor
    {
        [MenuItem("Tools/A Knight Game/Setup Player in Scene")]
        public static void SetupPlayerInScene()
        {
            // 1. Find the HeroKnight prefab
            string[] prefabGuids = AssetDatabase.FindAssets("HeroKnight t:Prefab");
            if (prefabGuids.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Could not find HeroKnight prefab in the project. Please make sure the 'Hero Knight - Pixel Art' pack is imported.", "OK");
                return;
            }

            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (playerPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to load prefab at {prefabPath}", "OK");
                return;
            }

            // 2. Check if a player already exists in the active scene
            GameObject existingPlayer = GameObject.Find("HeroKnight") ?? GameObject.FindWithTag("Player");
            if (existingPlayer != null)
            {
                if (!EditorUtility.DisplayDialog("Player Already Exists", $"A player object '{existingPlayer.name}' was found in the scene. Do you want to re-setup/update this object?", "Yes", "No"))
                {
                    return;
                }
            }

            GameObject playerInstance;
            if (existingPlayer != null)
            {
                playerInstance = existingPlayer;
            }
            else
            {
                // Instantiate the prefab in the scene
                playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerInstance.name = "HeroKnight";
                playerInstance.transform.position = Vector3.zero;
                playerInstance.tag = "Player";
                Undo.RegisterCreatedObjectUndo(playerInstance, "Create HeroKnight Player");
            }

            // 3. Remove the legacy HeroKnight script if it exists
            MonoBehaviour legacyScript = playerInstance.GetComponent("HeroKnight") as MonoBehaviour;
            if (legacyScript != null)
            {
                Undo.DestroyObjectImmediate(legacyScript);
                Debug.Log("Removed legacy HeroKnight controller script from player instance.");
            }

            // 4. Add the new KnightPlayerController script
            KnightPlayerController newController = playerInstance.GetComponent<KnightPlayerController>();
            if (newController == null)
            {
                newController = Undo.AddComponent<KnightPlayerController>(playerInstance);
                Debug.Log("Added KnightPlayerController script to player instance.");
            }

            // 5. Setup PlayerInput component
            PlayerInput playerInput = playerInstance.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = Undo.AddComponent<PlayerInput>(playerInstance);
                Debug.Log("Added PlayerInput component to player instance.");
            }

            // Find InputSystem_Actions asset
            string[] actionAssetGuids = AssetDatabase.FindAssets("InputSystem_Actions t:InputActionAsset");
            if (actionAssetGuids.Length > 0)
            {
                string actionAssetPath = AssetDatabase.GUIDToAssetPath(actionAssetGuids[0]);
                InputActionAsset actionAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(actionAssetPath);
                if (actionAsset != null)
                {
                    playerInput.actions = actionAsset;
                    playerInput.defaultActionMap = "Player";
                    Debug.Log($"Assigned Input Action Asset '{actionAsset.name}' to PlayerInput.");
                }
            }
            else
            {
                Debug.LogWarning("Could not find 'InputSystem_Actions' input action asset in the project. Please assign it manually to the PlayerInput component.");
            }

            // 6. Ensure sensors are linked
            // (Our script automatically auto-locates them via transform.Find in Awake, but let's make sure it's clean)
            EditorUtility.SetDirty(playerInstance);

            // 7. Check if we need to create a simple ground for testing
            CreateGroundIfNoneExists();

            Selection.activeGameObject = playerInstance;
            EditorUtility.DisplayDialog("Success", "HeroKnight player setup completed successfully!\n\n- KnightPlayerController attached\n- PlayerInput attached & configured\n- A test platform has been created if no ground collider was found.", "Awesome!");
        }

        [MenuItem("Tools/A Knight Game/Create Straw Training Dummy")]
        public static void SetupStrawDummy()
        {
            // 1. Find the Straw Training Dummy Idle prefab
            string[] prefabGuids = AssetDatabase.FindAssets("Straw Training Dummy Idle t:Prefab");
            if (prefabGuids.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Could not find 'Straw Training Dummy Idle' prefab in your project. Please make sure the 'Straw Training Dummy - Pixel Art' pack is imported.", "OK");
                return;
            }

            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
            GameObject dummyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (dummyPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to load prefab at {prefabPath}", "OK");
                return;
            }

            // 2. Instantiate in the scene
            GameObject dummyInstance = (GameObject)PrefabUtility.InstantiatePrefab(dummyPrefab);
            dummyInstance.name = "StrawTrainingDummy";
            dummyInstance.transform.position = new Vector3(3f, -1.3f, 0f); // Default placement in front of player
            Undo.RegisterCreatedObjectUndo(dummyInstance, "Create Straw Training Dummy");

            // 3. Add Health component
            Health health = Undo.AddComponent<Health>(dummyInstance);

            // 4. Add BoxCollider2D if missing
            BoxCollider2D collider = dummyInstance.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = Undo.AddComponent<BoxCollider2D>(dummyInstance);
            }
            // Pixel-perfect collider settings matching the straw dummy sprite shape
            collider.size = new Vector2(0.6074529f, 1.749486f);
            collider.offset = new Vector2(-0.00668406f, -0.1488094f);

            // 5. Add and setup DummyController
            DummyController dummyController = Undo.AddComponent<DummyController>(dummyInstance);
            SerializedObject controllerSo = new SerializedObject(dummyController);

            // Assign Idle controller
            string[] idleGuids = AssetDatabase.FindAssets("Idle Straw Training Dummy Animator Controller t:RuntimeAnimatorController");
            if (idleGuids.Length > 0)
            {
                RuntimeAnimatorController idleCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath(idleGuids[0]));
                controllerSo.FindProperty("m_idleController").objectReferenceValue = idleCtrl;
            }

            // Assign Hit controller
            string[] hitGuids = AssetDatabase.FindAssets("Hit Straw Training Dummy Animator Controller t:RuntimeAnimatorController");
            if (hitGuids.Length > 0)
            {
                RuntimeAnimatorController hitCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath(hitGuids[0]));
                controllerSo.FindProperty("m_hitController").objectReferenceValue = hitCtrl;
            }

            // Assign Death controller
            string[] deathGuids = AssetDatabase.FindAssets("Death Straw Training Dummy Animator Controller t:RuntimeAnimatorController");
            if (deathGuids.Length > 0)
            {
                RuntimeAnimatorController deathCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath(deathGuids[0]));
                controllerSo.FindProperty("m_deathController").objectReferenceValue = deathCtrl;
            }
            controllerSo.ApplyModifiedProperties();

            // Select in editor
            Selection.activeGameObject = dummyInstance;
            EditorUtility.SetDirty(dummyInstance);

            EditorUtility.DisplayDialog("Success", "Straw Training Dummy created successfully!\n\n- Idle Prefab instantiated\n- Health & DummyController attached\n- Idle/Hit/Death animation controller loops configured.", "Awesome!");
        }

        private static void CreateGroundIfNoneExists()
        {
            Collider2D[] colliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            bool hasGround = false;
            foreach (var col in colliders)
            {
                if (!col.isTrigger && (col.gameObject.name.ToLower().Contains("ground") || col.gameObject.name.ToLower().Contains("platform")))
                {
                    hasGround = true;
                    break;
                }
            }

            if (!hasGround)
            {
                GameObject ground = new GameObject("TestGround");
                ground.transform.position = new Vector3(0, -2.5f, 0);
                ground.transform.localScale = new Vector3(20, 1, 1);

                // Add a sprite renderer to make it visible
                SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
                // Try to find a default square sprite
                string[] squareGuids = AssetDatabase.FindAssets("Square t:Sprite");
                if (squareGuids.Length > 0)
                {
                    string squarePath = AssetDatabase.GUIDToAssetPath(squareGuids[0]);
                    sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(squarePath);
                }
                sr.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark grey ground

                BoxCollider2D collider = ground.AddComponent<BoxCollider2D>();
                
                Undo.RegisterCreatedObjectUndo(ground, "Create Test Ground");
                Debug.Log("Created a default TestGround platform at (0, -2.5, 0) with BoxCollider2D.");
            }
        }
    }
}
#endif
