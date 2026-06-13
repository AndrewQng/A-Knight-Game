#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.InputSystem;
using System.IO;
using System.Collections.Generic;
using KnightGame.Combat2D.FSM;
using KnightGame.Combat2D.Core;
using KnightGame.Combat2D.Combat;
using KnightGame.Combat2D.Input;
using KnightGame.Combat2D.Data;
using KnightGame.Combat2D.UI;

namespace KnightGame.Combat2D.EditorTools
{
    public class Combat2DSetupEditor : Editor
    {
        [MenuItem("Tools/Combat 2D/Setup FSM Player")]
        public static void SetupFsmPlayer()
        {
            // 1. Tìm hoặc khởi tạo nhân vật HeroKnight trong Scene
            GameObject player = GameObject.Find("HeroKnight_FSM") ?? GameObject.Find("HeroKnight") ?? GameObject.FindWithTag("Player");
            
            if (player == null)
            {
                string[] prefabGuids = AssetDatabase.FindAssets("HeroKnight t:Prefab");
                if (prefabGuids.Length == 0)
                {
                    EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Prefab HeroKnight trong dự án. Hãy chắc chắn gói asset đã được import.", "OK");
                    return;
                }
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
                GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.name = "HeroKnight_FSM";
                player.transform.position = Vector3.zero;
                player.tag = "Player";
                Undo.RegisterCreatedObjectUndo(player, "Create FSM Player");
            }
            else
            {
                player.name = "HeroKnight_FSM";
            }

            // 2. Gỡ bỏ các Controller cũ để tránh xung đột
            MonoBehaviour legacyScript = player.GetComponent("HeroKnight") as MonoBehaviour;
            if (legacyScript != null) Undo.DestroyObjectImmediate(legacyScript);
            
            MonoBehaviour legacyController = player.GetComponent("KnightPlayerController") as MonoBehaviour;
            if (legacyController != null) Undo.DestroyObjectImmediate(legacyController);
            
            MonoBehaviour legacyHealth = player.GetComponent("Health") as MonoBehaviour;
            if (legacyHealth != null) Undo.DestroyObjectImmediate(legacyHealth);

            // 3. Thêm các Component FSM mới vào nhân vật
            CharacterMovement movement = player.GetComponent<CharacterMovement>();
            if (movement == null)
            {
                movement = Undo.AddComponent<CharacterMovement>(player);
            }

            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats == null)
            {
                stats = Undo.AddComponent<CharacterStats>(player);
            }

            PlayerInputHandler inputHandler = player.GetComponent<PlayerInputHandler>();
            if (inputHandler == null)
            {
                inputHandler = Undo.AddComponent<PlayerInputHandler>(player);
            }

            CharacterStateMachine stateMachine = player.GetComponent<CharacterStateMachine>();
            if (stateMachine == null)
            {
                stateMachine = Undo.AddComponent<CharacterStateMachine>(player);
            }

            CharacterAnimationEvents animEvents = player.GetComponent<CharacterAnimationEvents>();
            if (animEvents == null)
            {
                animEvents = Undo.AddComponent<CharacterAnimationEvents>(player);
            }

            // Khóa xoay trục Z và tăng lực cản vật lý (Drag) của Player để tránh trơn trượt
            Rigidbody2D pRb = player.GetComponent<Rigidbody2D>();
            if (pRb != null)
            {
                pRb.constraints = RigidbodyConstraints2D.FreezeRotation;
                pRb.linearDamping = 3f;
            }

            // Tự động tìm cấu hình Input Action Asset của New Input System
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = Undo.AddComponent<PlayerInput>(player);
            }
            string[] actionAssetGuids = AssetDatabase.FindAssets("InputSystem_Actions t:InputActionAsset");
            if (actionAssetGuids.Length > 0)
            {
                string actionAssetPath = AssetDatabase.GUIDToAssetPath(actionAssetGuids[0]);
                InputActionAsset actionAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(actionAssetPath);
                if (actionAsset != null)
                {
                    playerInput.actions = actionAsset;
                    playerInput.defaultActionMap = "Player";
                    Debug.Log($"Đã gán thành công Input Action: {actionAsset.name}");
                }
            }

            // 4. Thiết lập vùng tấn công (Hitbox)
            Transform hitboxTransform = player.transform.Find("Hitbox");
            if (hitboxTransform == null)
            {
                GameObject hitboxObj = new GameObject("Hitbox");
                hitboxObj.transform.SetParent(player.transform);
                hitboxObj.transform.localPosition = new Vector3(0.8f, 0.6f, 0f);
                hitboxTransform = hitboxObj.transform;
                Undo.RegisterCreatedObjectUndo(hitboxObj, "Create Hitbox GameObject");
            }
            
            BoxCollider2D hitboxCollider = hitboxTransform.GetComponent<BoxCollider2D>();
            if (hitboxCollider == null)
            {
                hitboxCollider = Undo.AddComponent<BoxCollider2D>(hitboxTransform.gameObject);
            }
            hitboxCollider.isTrigger = true;
            hitboxCollider.size = new Vector2(1.2f, 1.2f);
            
            Hitbox hitbox = hitboxTransform.GetComponent<Hitbox>();
            if (hitbox == null)
            {
                hitbox = Undo.AddComponent<Hitbox>(hitboxTransform.gameObject);
            }

            // 5. Thiết lập vùng nhận đòn (Hurtbox)
            Transform hurtboxTransform = player.transform.Find("Hurtbox");
            if (hurtboxTransform == null)
            {
                GameObject hurtboxObj = new GameObject("Hurtbox");
                hurtboxObj.transform.SetParent(player.transform);
                hurtboxObj.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                hurtboxTransform = hurtboxObj.transform;
                Undo.RegisterCreatedObjectUndo(hurtboxObj, "Create Hurtbox GameObject");
            }
            
            BoxCollider2D hurtboxCollider = hurtboxTransform.GetComponent<BoxCollider2D>();
            if (hurtboxCollider == null)
            {
                hurtboxCollider = Undo.AddComponent<BoxCollider2D>(hurtboxTransform.gameObject);
            }
            hurtboxCollider.isTrigger = true;
            hurtboxCollider.size = new Vector2(0.8f, 1.6f);
            
            Hurtbox hurtbox = hurtboxTransform.GetComponent<Hurtbox>();
            if (hurtbox == null)
            {
                hurtbox = Undo.AddComponent<Hurtbox>(hurtboxTransform.gameObject);
            }

            // 6. Tạo tự động các file ScriptableObject cấu hình đòn đánh (Đã giảm 50% lực đẩy, tăng thời lượng ra chiêu để giảm tốc độ đánh mặc định)
            AttackDataSO basic1 = GetOrCreateAttackData("BasicAttack1Data", "Attack1", 0.18f, 0.1f, 0.22f, 12f, new Vector2(1.5f, 0f), 0.2f, 0f, 1.2f);
            AttackDataSO basic2 = GetOrCreateAttackData("BasicAttack2Data", "Attack2", 0.18f, 0.1f, 0.22f, 15f, new Vector2(1.8f, 0f), 0.22f, 0f, 1.5f);
            AttackDataSO basic3 = GetOrCreateAttackData("BasicAttack3Data", "Attack3", 0.25f, 0.12f, 0.35f, 25f, new Vector2(2.5f, 0.5f), 0.35f, 0f, 2f);
            AttackDataSO special = GetOrCreateAttackData("SpecialAttackData", "Attack3", 0.25f, 0.2f, 0.45f, 40f, new Vector2(4f, 1f), 0.6f, 30f, 3.5f);

            // Gán dữ liệu SO vào StateMachine qua SerializedObject
            SerializedObject smSo = new SerializedObject(stateMachine);
            
            // Cấu hình mảng combo đòn chém thường
            SerializedProperty comboProp = smSo.FindProperty("m_basicAttackCombo");
            comboProp.ClearArray();
            comboProp.InsertArrayElementAtIndex(0);
            comboProp.GetArrayElementAtIndex(0).objectReferenceValue = basic1;
            comboProp.InsertArrayElementAtIndex(1);
            comboProp.GetArrayElementAtIndex(1).objectReferenceValue = basic2;
            comboProp.InsertArrayElementAtIndex(2);
            comboProp.GetArrayElementAtIndex(2).objectReferenceValue = basic3;

            smSo.FindProperty("m_specialAttackData").objectReferenceValue = special;
            smSo.ApplyModifiedProperties();

            // Cấu hình điểm Ground Check cho Movement từ Sensor có sẵn của HeroKnight
            Transform groundCheck = player.transform.Find("GroundSensor");
            if (groundCheck != null)
            {
                SerializedObject mvSo = new SerializedObject(movement);
                mvSo.FindProperty("m_groundCheckPoint").objectReferenceValue = groundCheck;
                mvSo.ApplyModifiedProperties();
            }

            // 7. Cấu hình Camera tự động bám theo nhân vật
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
                if (camFollow == null)
                {
                    camFollow = Undo.AddComponent<CameraFollow>(mainCam.gameObject);
                }
                SerializedObject camSo = new SerializedObject(camFollow);
                camSo.FindProperty("m_target").objectReferenceValue = player.transform;
                camSo.ApplyModifiedProperties();
                Debug.Log("Đã cấu hình Main Camera tự động bám theo Player FSM.");
            }

            EditorUtility.SetDirty(player);
            EditorUtility.DisplayDialog("FSM Setup Thành công", "Đã cấu hình nhân vật HeroKnight FSM thành công!\n\n- Gỡ bỏ controller cũ\n- Gán FSM State Machine và Components phụ trợ\n- Tạo cấu trúc Hitbox/Hurtbox tiêu chuẩn\n- Tự động tạo và gán AttackData ScriptableObjects\n- Tự động cấu hình Main Camera bám theo Player", "Tuyệt vời!");
        }

        [MenuItem("Tools/Combat 2D/Setup FSM Training Dummy")]
        public static void SetupFsmDummy()
        {
            // 1. Tìm hoặc khởi tạo Dummy trong Scene
            GameObject dummy = GameObject.Find("StrawTrainingDummy_FSM") ?? GameObject.Find("StrawTrainingDummy");
            if (dummy == null)
            {
                string[] prefabGuids = AssetDatabase.FindAssets("Straw Training Dummy Idle t:Prefab");
                if (prefabGuids.Length == 0)
                {
                    EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Prefab 'Straw Training Dummy Idle'. Hãy kiểm tra lại thư mục dự án.", "OK");
                    return;
                }
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[0]);
                GameObject dummyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                dummy = (GameObject)PrefabUtility.InstantiatePrefab(dummyPrefab);
                dummy.name = "StrawTrainingDummy_FSM";
                dummy.transform.position = new Vector3(3f, -1.3f, 0f);
                Undo.RegisterCreatedObjectUndo(dummy, "Create FSM Dummy");
            }
            else
            {
                dummy.name = "StrawTrainingDummy_FSM";
            }

            // 2. Gỡ bỏ controller và hệ thống máu cũ
            MonoBehaviour legacyController = dummy.GetComponent("DummyController") as MonoBehaviour;
            if (legacyController != null) Undo.DestroyObjectImmediate(legacyController);

            MonoBehaviour legacyHealth = dummy.GetComponent("Health") as MonoBehaviour;
            if (legacyHealth != null) Undo.DestroyObjectImmediate(legacyHealth);

            // 3. Gán các thành phần FSM mới
            CharacterMovement movement = dummy.GetComponent<CharacterMovement>();
            if (movement == null)
            {
                movement = dummy.AddComponent<CharacterMovement>();
            }

            CharacterStats stats = dummy.GetComponent<CharacterStats>();
            if (stats == null)
            {
                stats = dummy.AddComponent<CharacterStats>();
            }

            AIInputHandler aiInput = dummy.GetComponent<AIInputHandler>();
            if (aiInput == null)
            {
                aiInput = dummy.AddComponent<AIInputHandler>();
            }

            CharacterStateMachine stateMachine = dummy.GetComponent<CharacterStateMachine>();
            if (stateMachine == null)
            {
                stateMachine = dummy.AddComponent<CharacterStateMachine>();
            }

            CharacterAnimationEvents animEvents = dummy.GetComponent<CharacterAnimationEvents>();
            if (animEvents == null)
            {
                animEvents = dummy.AddComponent<CharacterAnimationEvents>();
            }

            // Khóa xoay trục Z và tăng lực cản vật lý (Drag) của Dummy
            Rigidbody2D dRb = dummy.GetComponent<Rigidbody2D>();
            if (dRb == null)
            {
                dRb = dummy.AddComponent<Rigidbody2D>();
            }
            if (dRb != null)
            {
                dRb.constraints = RigidbodyConstraints2D.FreezeRotation;
                dRb.linearDamping = 3f;
            }

            // 4. Thiết lập Hurtbox nhận sát thương
            Transform hurtboxTransform = dummy.transform.Find("Hurtbox");
            if (hurtboxTransform == null)
            {
                GameObject hurtboxObj = new GameObject("Hurtbox");
                hurtboxObj.transform.SetParent(dummy.transform);
                hurtboxObj.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                hurtboxTransform = hurtboxObj.transform;
            }
            
            BoxCollider2D hurtboxCollider = hurtboxTransform.GetComponent<BoxCollider2D>();
            if (hurtboxCollider == null)
            {
                hurtboxCollider = hurtboxTransform.gameObject.AddComponent<BoxCollider2D>();
            }
            if (hurtboxCollider != null)
            {
                hurtboxCollider.isTrigger = true;
                hurtboxCollider.size = new Vector2(0.8f, 1.8f);
            }
            
            Hurtbox hurtbox = hurtboxTransform.GetComponent<Hurtbox>();
            if (hurtbox == null)
            {
                hurtbox = hurtboxTransform.gameObject.AddComponent<Hurtbox>();
            }

            // 5. Cấu hình Body Collider vật lý gốc cho Dummy (để Dummy đứng trên đất không bị rơi xuyên sàn)
            BoxCollider2D bodyCol = dummy.GetComponent<BoxCollider2D>();
            if (bodyCol == null)
            {
                bodyCol = dummy.AddComponent<BoxCollider2D>();
            }
            if (bodyCol != null)
            {
                bodyCol.isTrigger = false;
                bodyCol.size = new Vector2(0.6074529f, 1.749486f);
                bodyCol.offset = new Vector2(-0.00668406f, -0.1488094f);
            }

            // 6. Cấu hình Animator Controller tự động cho Dummy (Tạo Controller hợp nhất tránh lỗi lặp hoán đổi ở runtime)
            RuntimeAnimatorController idleCtrl = null;
            RuntimeAnimatorController hitCtrl = null;
            RuntimeAnimatorController deathCtrl = null;

            string[] idleGuids = AssetDatabase.FindAssets("Idle Straw Training Dummy Animator Controller t:RuntimeAnimatorController");
            if (idleGuids.Length > 0) idleCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath(idleGuids[0]));

            string[] hitGuids = AssetDatabase.FindAssets("Hit Straw Training Dummy Animator Controller t:RuntimeAnimatorController");
            if (hitGuids.Length > 0) hitCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath(hitGuids[0]));

            string[] deathGuids = AssetDatabase.FindAssets("Death Straw Training Dummy Animator Controller t:RuntimeAnimatorController");
            if (deathGuids.Length > 0) deathCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath(deathGuids[0]));

            RuntimeAnimatorController unifiedCtrl = GetOrCreateUnifiedDummyController(idleCtrl, hitCtrl, deathCtrl);
            if (unifiedCtrl != null)
            {
                dummy.GetComponent<Animator>().runtimeAnimatorController = unifiedCtrl;
            }

            SerializedObject smSo = new SerializedObject(stateMachine);
            smSo.FindProperty("m_destroyOnDeath").boolValue = true; // Bật cờ tự hủy sau khi chết cho Dummy
            smSo.ApplyModifiedProperties();

            EditorUtility.SetDirty(dummy);
            EditorUtility.DisplayDialog("FSM Dummy Thành công", "Đã cấu hình Straw Training Dummy FSM thành công!\n\n- Gán FSM, Stats, Movement và AIInputHandler.\n- Thiết lập Hurtbox đón đòn đánh vật lý.\n- Hợp nhất và tạo Animator Controller Dummy_Unified_Controller thành công (Đỡ lỗi NullReference cho Editor)!\n- Khóa constraints.FreezeRotation để Dummy đứng thẳng!", "Tuyệt vời!");
        }

        private static RuntimeAnimatorController GetOrCreateUnifiedDummyController(
            RuntimeAnimatorController idleCtrl, 
            RuntimeAnimatorController hitCtrl, 
            RuntimeAnimatorController deathCtrl)
        {
            string folderPath = "Assets/_Project/Data/Combat2D";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string assetPath = $"{folderPath}/Dummy_Unified_Controller.controller";
            var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(assetPath);
            if (controller != null) return controller;

            // Tạo Animator Controller mới
            controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(assetPath);
            
            // Thêm các tham số trigger cần thiết
            controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            // Lấy các clip hoạt ảnh từ các controller cũ
            AnimationClip idleClip = (idleCtrl != null && idleCtrl.animationClips.Length > 0) ? idleCtrl.animationClips[0] : null;
            AnimationClip hitClip = (hitCtrl != null && hitCtrl.animationClips.Length > 0) ? hitCtrl.animationClips[0] : null;
            AnimationClip deathClip = (deathCtrl != null && deathCtrl.animationClips.Length > 0) ? deathCtrl.animationClips[0] : null;

            // Thêm các trạng thái hoạt ảnh vào layer mặc định
            var rootStateMachine = controller.layers[0].stateMachine;

            var idleState = rootStateMachine.AddState("Idle");
            idleState.motion = idleClip;

            if (hitClip != null)
            {
                var hitState = rootStateMachine.AddState("Hit");
                hitState.motion = hitClip;
                
                // AnyState -> Hit khi có trigger "Hurt"
                var transition = rootStateMachine.AddAnyStateTransition(hitState);
                transition.AddCondition(AnimatorConditionMode.If, 0, "Hurt");
                transition.duration = 0f;
                
                // Hit -> Idle sau khi kết thúc
                var backTransition = hitState.AddTransition(idleState);
                backTransition.hasExitTime = true;
                backTransition.exitTime = 1f;
                backTransition.duration = 0.1f;
            }

            if (deathClip != null)
            {
                var deathState = rootStateMachine.AddState("Death");
                deathState.motion = deathClip;
                
                // AnyState -> Death khi có trigger "Death"
                var transition = rootStateMachine.AddAnyStateTransition(deathState);
                transition.AddCondition(AnimatorConditionMode.If, 0, "Death");
                transition.duration = 0f;
            }

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static AttackDataSO GetOrCreateAttackData(string fileName, string animTrigger, float startup, float active, float recovery, float damage, Vector2 knockback, float hitstun, float energyCost, float forwardForce)
        {
            string folderPath = "Assets/_Project/Data/Combat2D";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string assetPath = $"{folderPath}/{fileName}.asset";
            AttackDataSO data = AssetDatabase.LoadAssetAtPath<AttackDataSO>(assetPath);
            bool isNew = false;

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<AttackDataSO>();
                isNew = true;
            }

            // Đồng bộ hóa/Cập nhật các thông số đòn đánh
            data.AnimationTrigger = animTrigger;
            data.StartupDuration = startup;
            data.ActiveDuration = active;
            data.RecoveryDuration = recovery;
            data.Damage = damage;
            data.KnockbackForce = knockback;
            data.HitstunDuration = hitstun;
            data.EnergyCost = energyCost;
            data.AttackerForwardForce = forwardForce;

            if (isNew)
            {
                AssetDatabase.CreateAsset(data, assetPath);
                Debug.Log($"Đã tạo file asset AttackDataSO mới tại: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(data);
                Debug.Log($"Đã cập nhật file asset AttackDataSO hiện tại tại: {assetPath}");
            }

            AssetDatabase.SaveAssets();
            return data;
        }

        [MenuItem("Tools/Combat 2D/Setup Combat UI HUD")]
        public static void SetupCombatUiHud()
        {
            // 1. Tìm hoặc tạo Canvas HUD
            GameObject canvasObj = GameObject.Find("CombatHUDCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("CombatHUDCanvas", typeof(RectTransform));
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            }

            // 2. Tìm hoặc tạo Panel chứa HUD
            Transform panelTrans = canvasObj.transform.Find("PlayerHUDPanel");
            GameObject panelObj;
            if (panelTrans == null)
            {
                panelObj = new GameObject("PlayerHUDPanel", typeof(RectTransform));
                panelObj.transform.SetParent(canvasObj.transform, false);
                panelTrans = panelObj.transform;
                Undo.RegisterCreatedObjectUndo(panelObj, "Create Player HUD Panel");
            }
            else
            {
                panelObj = panelTrans.gameObject;
            }

            // Cấu hình Panel vị trí Top-Left
            RectTransform panelRt = panelObj.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0f, 1f);
            panelRt.anchorMax = new Vector2(0f, 1f);
            panelRt.pivot = new Vector2(0f, 1f);
            panelRt.anchoredPosition = new Vector2(40f, -40f);
            panelRt.sizeDelta = new Vector2(350f, 160f);

            // Thêm VerticalLayoutGroup để tự động căn lề các thanh
            var layout = panelObj.GetComponent<UnityEngine.UI.VerticalLayoutGroup>() ?? panelObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing = 15f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Xóa các thanh cũ nếu có để tránh trùng lặp khi chạy lại setup
            for (int i = panelTrans.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(panelTrans.GetChild(i).gameObject);
            }

            // Tìm nạp các Sprites từ Asset Pack
            Sprite bgSprite = GetSpriteByName("HealthBar DARK");
            Sprite hpSprite = GetSpriteByName("redblue (1)");
            Sprite staminaSprite = GetSpriteByName("greenbar (1)");
            Sprite manaSprite = GetSpriteByName("redblue (2)");

            // 3. Tạo thanh Máu (HP) - Đỏ tươi, thanh Ease màu đỏ sẫm
            var hpSliders = CreateSliderWithEase("HP_Bar", panelTrans, bgSprite, hpSprite, 
                new Color(1f, 0.2f, 0.2f), new Color(0.5f, 0.1f, 0.1f));
            ConfigureLayoutHeight(hpSliders.mainSlider.transform.parent.gameObject, 30f);

            // 4. Tạo thanh Thể lực (Stamina) - Xanh lá tươi (hoặc Vàng), thanh Ease màu xanh sẫm
            var staminaSliders = CreateSliderWithEase("Stamina_Bar", panelTrans, bgSprite, staminaSprite, 
                new Color(0.2f, 1f, 0.2f), new Color(0.1f, 0.5f, 0.1f));
            ConfigureLayoutHeight(staminaSliders.mainSlider.transform.parent.gameObject, 25f);

            // 5. Tạo thanh Phép (Mana) - Xanh dương tươi, thanh Ease màu xanh dương sẫm
            var manaSliders = CreateSliderWithEase("Mana_Bar", panelTrans, bgSprite, manaSprite, 
                new Color(0.2f, 0.6f, 1f), new Color(0.1f, 0.3f, 0.5f));
            ConfigureLayoutHeight(manaSliders.mainSlider.transform.parent.gameObject, 25f);

            // 6. Đính kèm CombatHUD và gán tham chiếu
            CombatHUD hud = canvasObj.GetComponent<CombatHUD>() ?? canvasObj.AddComponent<CombatHUD>();
            
            // Dùng SerializedObject để gán tham chiếu an toàn trong Editor
            SerializedObject hudSo = new SerializedObject(hud);
            hudSo.FindProperty("m_healthSlider").objectReferenceValue = hpSliders.mainSlider;
            hudSo.FindProperty("m_healthEaseSlider").objectReferenceValue = hpSliders.easeSlider;
            hudSo.FindProperty("m_staminaSlider").objectReferenceValue = staminaSliders.mainSlider;
            hudSo.FindProperty("m_staminaEaseSlider").objectReferenceValue = staminaSliders.easeSlider;
            hudSo.FindProperty("m_manaSlider").objectReferenceValue = manaSliders.mainSlider;
            hudSo.FindProperty("m_manaEaseSlider").objectReferenceValue = manaSliders.easeSlider;
            
            // Tìm player tự động và gán vào HUD
            GameObject player = GameObject.Find("HeroKnight_FSM") ?? GameObject.FindWithTag("Player");
            if (player != null)
            {
                hudSo.FindProperty("m_playerStats").objectReferenceValue = player.GetComponent<CharacterStats>();
            }
            hudSo.ApplyModifiedProperties();

            // 7. Tạo EventSystem nếu chưa có
            if (GameObject.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }

            EditorUtility.DisplayDialog("Setup HUD Thành công", "Đã tạo Canvas và cấu hình 3 thanh trạng thái HP, Stamina, Mana thành công sử dụng Health Bar Asset Pack!", "Tuyệt vời!");
        }

        private static void ConfigureLayoutHeight(GameObject obj, float height)
        {
            var le = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
            le.preferredHeight = height;
        }

        private static (Slider mainSlider, Slider easeSlider) CreateSliderWithEase(string name, Transform parent, Sprite bgSprite, Sprite fillSprite, Color mainColor, Color easeColor)
        {
            GameObject container = new GameObject(name, typeof(RectTransform));
            container.transform.SetParent(parent, false);

            // 1. Tạo Ease Slider (ở phía sau)
            Slider easeSlider = CreateUiSlider(name + "_Ease", container.transform, bgSprite, fillSprite, easeColor);
            
            // 2. Tạo Main Slider (ở phía trước)
            Slider mainSlider = CreateUiSlider(name + "_Main", container.transform, null, fillSprite, mainColor);
            
            // Xóa background của main slider để lộ background của ease slider
            Transform bgTrans = mainSlider.transform.Find("Background");
            if (bgTrans != null)
            {
                DestroyImmediate(bgTrans.gameObject);
            }

            // Đồng bộ kích thước của cả hai slider để khít khịt với container
            RectTransform mainRt = mainSlider.GetComponent<RectTransform>();
            mainRt.anchorMin = Vector2.zero;
            mainRt.anchorMax = Vector2.one;
            mainRt.sizeDelta = Vector2.zero;

            RectTransform easeRt = easeSlider.GetComponent<RectTransform>();
            easeRt.anchorMin = Vector2.zero;
            easeRt.anchorMax = Vector2.one;
            easeRt.sizeDelta = Vector2.zero;

            return (mainSlider, easeSlider);
        }

        private static Slider CreateUiSlider(string name, Transform parent, Sprite bgSprite, Sprite fillSprite, Color fillColor)
        {
            GameObject sliderObj = new GameObject(name, typeof(RectTransform));
            sliderObj.transform.SetParent(parent, false);
            
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // Xóa các lề thừa của Slider mặc định
            RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
            sliderRt.anchorMin = Vector2.zero;
            sliderRt.anchorMax = Vector2.one;
            sliderRt.sizeDelta = Vector2.zero;

            // 1. Background Image
            if (bgSprite != null)
            {
                GameObject bgObj = new GameObject("Background", typeof(RectTransform));
                bgObj.transform.SetParent(sliderObj.transform, false);
                Image bgImg = bgObj.AddComponent<Image>();
                bgImg.sprite = bgSprite;
                bgImg.color = Color.white;

                // Stretch Background
                RectTransform bgRt = bgObj.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.sizeDelta = Vector2.zero;
            }

            // 2. Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRt = fillAreaObj.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.sizeDelta = Vector2.zero;

            // 3. Fill Image
            GameObject fillObj = new GameObject("Fill", typeof(RectTransform));
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.sprite = fillSprite;
            fillImg.color = fillColor;
            
            if (fillSprite != null)
            {
                fillImg.type = Image.Type.Simple; // Dùng ảnh từ gói
            }
            else
            {
                fillImg.type = Image.Type.Sliced; // Màu solid fallback
            }

            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;

            slider.fillRect = fillRt;
            
            return slider;
        }

        private static Sprite GetSpriteByName(string name)
        {
            // 1. Tìm kiếm Texture trực tiếp
            string[] guids = AssetDatabase.FindAssets($"{name} t:Texture2D");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
                
                object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var asset in assets)
                {
                    if (asset is Sprite sprite && (sprite.name == name || assets.Length == 2))
                    {
                        return sprite;
                    }
                }
            }
            
            // 2. Fallback: Nếu tên chứa dấu "_" (như idle_0), tìm trong các sheet có tên cha (như idle)
            if (name.Contains("_"))
            {
                string parentName = name.Split('_')[0];
                string[] parentGuids = AssetDatabase.FindAssets($"{parentName} t:Texture2D");
                foreach (var pGuid in parentGuids)
                {
                    string pPath = AssetDatabase.GUIDToAssetPath(pGuid);
                    
                    TextureImporter importer = AssetImporter.GetAtPath(pPath) as TextureImporter;
                    if (importer != null && importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.SaveAndReimport();
                    }

                    object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(pPath);
                    foreach (var sAsset in subAssets)
                    {
                        if (sAsset is Sprite sprite && sprite.name == name)
                        {
                            return sprite;
                        }
                    }
                }
            }

            return null;
        }

        private static void RegisterEnemyTag()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets != null && assets.Length > 0)
            {
                SerializedObject tagManager = new SerializedObject(assets[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");
                bool found = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Enemy")
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = "Enemy";
                    tagManager.ApplyModifiedProperties();
                    Debug.Log("Đã tự động thêm tag 'Enemy' vào Project Settings.");
                }
            }
        }

        [MenuItem("Tools/Combat 2D/Setup FSM Slime Monster")]
        public static void SetupFsmSlimeMonster()
        {
            // 1. Tìm và xóa đối tượng Slime_Monster_FSM cũ để tạo mới sạch sẽ tránh lỗi Component hỏng từ lần trước
            GameObject oldMonster = GameObject.Find("Slime_Monster_FSM");
            if (oldMonster != null)
            {
                DestroyImmediate(oldMonster);
            }

            GameObject monster = new GameObject("Slime_Monster_FSM");
            monster.transform.position = new Vector3(5f, -1.3f, 0f);
            Undo.RegisterCreatedObjectUndo(monster, "Create FSM Slime Monster");

            // Tự động đăng ký tag "Enemy" nếu chưa có
            RegisterEnemyTag();
            monster.tag = "Enemy";

            // Gán SpriteRenderer và Animator an toàn
            SpriteRenderer sr = monster.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = monster.AddComponent<SpriteRenderer>();
            }

            Sprite defaultSprite = GetSpriteByName("idle_0");
            if (defaultSprite != null)
            {
                sr.sprite = defaultSprite;
            }

            Animator anim = monster.GetComponent<Animator>();
            if (anim == null)
            {
                anim = monster.AddComponent<Animator>();
            }

            // 2. Thêm các Component FSM và AI
            CharacterMovement movement = monster.GetComponent<CharacterMovement>();
            if (movement == null)
            {
                movement = monster.AddComponent<CharacterMovement>();
            }

            CharacterStats stats = monster.GetComponent<CharacterStats>();
            if (stats == null)
            {
                stats = monster.AddComponent<CharacterStats>();
            }

            AIInputHandler aiInput = monster.GetComponent<AIInputHandler>();
            if (aiInput == null)
            {
                aiInput = monster.AddComponent<AIInputHandler>();
            }

            SimpleAIController aiController = monster.GetComponent<SimpleAIController>();
            if (aiController == null)
            {
                aiController = monster.AddComponent<SimpleAIController>();
            }

            CharacterStateMachine stateMachine = monster.GetComponent<CharacterStateMachine>();
            if (stateMachine == null)
            {
                stateMachine = monster.AddComponent<CharacterStateMachine>();
            }

            CharacterAnimationEvents animEvents = monster.GetComponent<CharacterAnimationEvents>();
            if (animEvents == null)
            {
                animEvents = monster.AddComponent<CharacterAnimationEvents>();
            }

            // Cấu hình vật lý Rigidbody2D cho Slime
            Rigidbody2D mRb = monster.GetComponent<Rigidbody2D>();
            if (mRb == null)
            {
                mRb = monster.AddComponent<Rigidbody2D>();
            }
            if (mRb != null)
            {
                mRb.constraints = RigidbodyConstraints2D.FreezeRotation;
                mRb.linearDamping = 3f;
            }

            // Cấu hình BoxCollider2D vật lý (Slime dẹt và ngắn hơn Player)
            BoxCollider2D bodyCol = monster.GetComponent<BoxCollider2D>();
            if (bodyCol == null)
            {
                bodyCol = monster.AddComponent<BoxCollider2D>();
            }
            if (bodyCol != null)
            {
                bodyCol.isTrigger = false;
                bodyCol.size = new Vector2(0.8f, 0.6f);
                bodyCol.offset = new Vector2(0f, -0.3f);
            }

            // 3. Thiết lập vùng nhận đòn Hurtbox
            Transform hurtboxTransform = monster.transform.Find("Hurtbox");
            if (hurtboxTransform == null)
            {
                GameObject hurtboxObj = new GameObject("Hurtbox");
                hurtboxObj.transform.SetParent(monster.transform);
                hurtboxObj.transform.localPosition = new Vector3(0f, -0.3f, 0f);
                hurtboxTransform = hurtboxObj.transform;
            }
            
            BoxCollider2D hurtboxCollider = hurtboxTransform.GetComponent<BoxCollider2D>();
            if (hurtboxCollider == null)
            {
                hurtboxCollider = hurtboxTransform.gameObject.AddComponent<BoxCollider2D>();
            }
            if (hurtboxCollider != null)
            {
                hurtboxCollider.isTrigger = true;
                hurtboxCollider.size = new Vector2(0.8f, 0.6f);
            }
            
            Hurtbox hurtbox = hurtboxTransform.GetComponent<Hurtbox>();
            if (hurtbox == null)
            {
                hurtbox = hurtboxTransform.gameObject.AddComponent<Hurtbox>();
            }

            // 4. Thiết lập vùng tấn công Hitbox
            Transform hitboxTransform = monster.transform.Find("Hitbox");
            if (hitboxTransform == null)
            {
                GameObject hitboxObj = new GameObject("Hitbox");
                hitboxObj.transform.SetParent(monster.transform);
                hitboxObj.transform.localPosition = new Vector3(0.6f, -0.3f, 0f);
                hitboxTransform = hitboxObj.transform;
            }
            
            BoxCollider2D hitboxCollider = hitboxTransform.GetComponent<BoxCollider2D>();
            if (hitboxCollider == null)
            {
                hitboxCollider = hitboxTransform.gameObject.AddComponent<BoxCollider2D>();
            }
            if (hitboxCollider != null)
            {
                hitboxCollider.isTrigger = true;
                hitboxCollider.size = new Vector2(1.0f, 0.6f);
            }
            
            Hitbox hitbox = hitboxTransform.GetComponent<Hitbox>();
            if (hitbox == null)
            {
                hitbox = hitboxTransform.gameObject.AddComponent<Hitbox>();
            }

            // 5. Cấu hình chỉ số (Stats) cho Slime
            SerializedObject statsSo = new SerializedObject(stats);
            statsSo.FindProperty("m_maxHealth").floatValue = 50f;
            statsSo.FindProperty("m_maxStamina").floatValue = 100f;
            statsSo.FindProperty("m_maxMana").floatValue = 50f;
            statsSo.FindProperty("m_attackSpeed").floatValue = 0.85f; // Đòn đánh Slime chậm hơn Knight một chút
            statsSo.ApplyModifiedProperties();

            // 6. Tạo/Tải các clip hoạt ảnh từ Sprite Sheets của Slime
            AnimationClip idleClip = GetOrCreateSlimeClip("Slime_Idle", "idle.png", 12f, true);
            AnimationClip walkClip = GetOrCreateSlimeClip("Slime_Walk", "walk.png", 12f, true);
            AnimationClip attackClip = GetOrCreateSlimeClip("Slime_Attack", "attack.png", 12f, false);
            AnimationClip hitClip = GetOrCreateSlimeClip("Slime_Hurt", "hurt.png", 8f, false);
            AnimationClip deathClip = GetOrCreateSlimeClip("Slime_Death", "death.png", 10f, false);

            // 7. Tạo và gán Animator Controller
            RuntimeAnimatorController controller = GetOrCreateSlimeController(idleClip, walkClip, attackClip, hitClip, deathClip);
            if (controller != null)
            {
                anim.runtimeAnimatorController = controller;
            }

            // 8. Tạo và gán dữ liệu đòn đánh cho Slime
            AttackDataSO slimeAttack = GetOrCreateAttackData("SlimeAttackData", "Attack1", 0.35f, 0.15f, 0.4f, 10f, new Vector2(2f, 0.5f), 0.3f, 0f, 1f);
            
            SerializedObject smSo = new SerializedObject(stateMachine);
            smSo.FindProperty("m_destroyOnDeath").boolValue = true; // Tự hủy sau khi chết
            smSo.FindProperty("m_basicAttackData").objectReferenceValue = slimeAttack; // Gán làm đòn cơ bản fallback
            smSo.ApplyModifiedProperties();

            EditorUtility.SetDirty(monster);
            EditorUtility.DisplayDialog("FSM Slime Monster Setup", "Đã cấu hình Slime Monster FSM thành công!\n\n- Tự động cắt và tạo Animation Clips\n- Tự động sinh Animator Controller cho Slime\n- Cấu hình chỉ số máu = 50, tốc độ đánh = 0.85\n- Thiết lập AIInputHandler và SimpleAIController tự động bám đuổi/tấn công người chơi", "Tuyệt vời!");
        }

        private static AnimationClip GetOrCreateSlimeClip(string clipName, string textureSubPath, float frameRate, bool loop)
        {
            string folderPath = "Assets/_Project/Data/Combat2D";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string savePath = $"{folderPath}/{clipName}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
            if (clip != null) return clip;

            string fullTexturePath = $"Assets/Monsters Creatures Fantasy 2/Sprites/Slime/{textureSubPath}";
            TextureImporter importer = AssetImporter.GetAtPath(fullTexturePath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            object[] assets = AssetDatabase.LoadAllAssetsAtPath(fullTexturePath);
            List<Sprite> sprites = new List<Sprite>();
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }

            if (sprites.Count == 0)
            {
                Debug.LogWarning($"Không tìm thấy sprite nào trong {fullTexturePath}");
                return null;
            }

            // Sắp xếp các sprite theo số ở cuối tên
            sprites.Sort((a, b) => GetNumberFromName(a.name).CompareTo(GetNumberFromName(b.name)));

            clip = new AnimationClip();
            clip.frameRate = frameRate;

            if (loop)
            {
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
            float timePerFrame = 1f / frameRate;
            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * timePerFrame,
                    value = sprites[i]
                };
            }

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            AssetDatabase.CreateAsset(clip, savePath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Đã tạo clip hoạt ảnh Slime: {savePath}");
            return clip;
        }

        private static RuntimeAnimatorController GetOrCreateSlimeController(
            AnimationClip idleClip, 
            AnimationClip runClip, 
            AnimationClip attackClip, 
            AnimationClip hitClip, 
            AnimationClip deathClip)
        {
            string folderPath = "Assets/_Project/Data/Combat2D";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string assetPath = $"{folderPath}/Slime_Unified_Controller.controller";
            var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(assetPath);
            if (controller != null) return controller;

            // Tạo Animator Controller mới
            controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(assetPath);
            
            // Thêm các tham số
            controller.AddParameter("AnimState", AnimatorControllerParameterType.Int);
            controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Attack1", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;

            var idleState = rootStateMachine.AddState("Idle");
            idleState.motion = idleClip;

            var runState = rootStateMachine.AddState("Run");
            runState.motion = runClip;

            // Transitions giữa Idle và Run dựa trên AnimState
            var toRun = idleState.AddTransition(runState);
            toRun.AddCondition(AnimatorConditionMode.Equals, 1, "AnimState");
            toRun.duration = 0.1f;

            var toIdle = runState.AddTransition(idleState);
            toIdle.AddCondition(AnimatorConditionMode.Equals, 0, "AnimState");
            toIdle.duration = 0.1f;

            if (attackClip != null)
            {
                var attackState = rootStateMachine.AddState("Attack");
                attackState.motion = attackClip;
                
                var transition = rootStateMachine.AddAnyStateTransition(attackState);
                transition.AddCondition(AnimatorConditionMode.If, 0, "Attack1");
                transition.duration = 0f;
                
                var backTransition = attackState.AddTransition(idleState);
                backTransition.hasExitTime = true;
                backTransition.exitTime = 1f;
                backTransition.duration = 0.1f;
            }

            if (hitClip != null)
            {
                var hitState = rootStateMachine.AddState("Hit");
                hitState.motion = hitClip;
                
                var transition = rootStateMachine.AddAnyStateTransition(hitState);
                transition.AddCondition(AnimatorConditionMode.If, 0, "Hurt");
                transition.duration = 0f;
                
                var backTransition = hitState.AddTransition(idleState);
                backTransition.hasExitTime = true;
                backTransition.exitTime = 1f;
                backTransition.duration = 0.1f;
            }

            if (deathClip != null)
            {
                var deathState = rootStateMachine.AddState("Death");
                deathState.motion = deathClip;
                
                var transition = rootStateMachine.AddAnyStateTransition(deathState);
                transition.AddCondition(AnimatorConditionMode.If, 0, "Death");
                transition.duration = 0f;
            }

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static int GetNumberFromName(string name)
        {
            string numberStr = "";
            for (int i = name.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(name[i]))
                {
                    numberStr = name[i] + numberStr;
                }
                else if (numberStr.Length > 0)
                {
                    break;
                }
            }
            int result;
            if (int.TryParse(numberStr, out result)) return result;
            return 0;
        }
    }
}
#endif
