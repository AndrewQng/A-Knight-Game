using UnityEngine;
using UnityEngine.InputSystem;

namespace KnightGame.Combat2D.Input
{
    public class PlayerInputHandler : MonoBehaviour, ICharacterInput
    {
        [Header("New Input System Settings")]
        [SerializeField] private bool m_useNewInputSystem = true;
        [SerializeField] private PlayerInput m_playerInput;

        // Các thuộc tính của Interface ICharacterInput
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool DashPressed { get; private set; }
        public bool BasicAttackPressed { get; private set; }
        public bool SpecialAttackPressed { get; private set; }
        public bool BlockHeld { get; private set; }

        // Khai báo các Input Actions
        private InputAction m_moveAction;
        private InputAction m_jumpAction;
        private InputAction m_dashAction;
        private InputAction m_basicAttackAction;
        private InputAction m_specialAttackAction;
        private InputAction m_blockAction;

        private void Awake()
        {
            if (m_playerInput == null)
            {
                m_playerInput = GetComponent<PlayerInput>();
            }
            InitializeInputActions();
        }

        private void InitializeInputActions()
        {
            if (m_playerInput != null && m_playerInput.actions != null)
            {
                m_moveAction = m_playerInput.actions.FindAction("Move");
                m_jumpAction = m_playerInput.actions.FindAction("Jump");
                
                // Roll / Sprint đại diện cho Dash
                m_dashAction = m_playerInput.actions.FindAction("Sprint");
                if (m_dashAction == null) m_dashAction = m_playerInput.actions.FindAction("Roll");
                if (m_dashAction == null) m_dashAction = m_playerInput.actions.FindAction("Dash");

                // Attack 1 (Basic)
                m_basicAttackAction = m_playerInput.actions.FindAction("Attack");
                if (m_basicAttackAction == null) m_basicAttackAction = m_playerInput.actions.FindAction("BasicAttack");

                // Attack 2 (Special)
                m_specialAttackAction = m_playerInput.actions.FindAction("SpecialAttack");
                if (m_specialAttackAction == null) m_specialAttackAction = m_playerInput.actions.FindAction("Special");

                // Crouch / Block đại diện cho Block
                m_blockAction = m_playerInput.actions.FindAction("Crouch");
                if (m_blockAction == null) m_blockAction = m_playerInput.actions.FindAction("Block");

                // Enable all actions
                m_moveAction?.Enable();
                m_jumpAction?.Enable();
                m_dashAction?.Enable();
                m_basicAttackAction?.Enable();
                m_specialAttackAction?.Enable();
                m_blockAction?.Enable();
            }
        }

        private void Update()
        {
            ReadInputs();
        }

        private void ReadInputs()
        {
            if (m_useNewInputSystem && m_playerInput != null)
            {
                // Đọc di chuyển (X, Y)
                if (m_moveAction != null)
                {
                    MoveInput = m_moveAction.ReadValue<Vector2>();
                }

                // Đọc trạng thái nhảy, lướt, tấn công thường & đặc biệt theo dạng trigger
                if (m_jumpAction != null && m_jumpAction.WasPressedThisFrame()) JumpPressed = true;
                if (m_dashAction != null && m_dashAction.WasPressedThisFrame()) DashPressed = true;
                if (m_basicAttackAction != null && m_basicAttackAction.WasPressedThisFrame()) BasicAttackPressed = true;
                if (m_specialAttackAction != null && m_specialAttackAction.WasPressedThisFrame()) SpecialAttackPressed = true;

                // Đọc BlockHeld (đang giữ nút Crouch/Block hoặc Chuột phải)
                BlockHeld = false;
                if (m_blockAction != null && m_blockAction.IsPressed())
                {
                    BlockHeld = true;
                }
                else if (Mouse.current != null && Mouse.current.rightButton.isPressed)
                {
                    BlockHeld = true;
                }
                return;
            }

            // Fallback sang hệ thống Input Manager cũ (Legacy Input)
            float x = UnityEngine.Input.GetAxisRaw("Horizontal");
            float y = UnityEngine.Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(x, y);

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) JumpPressed = true;
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift)) DashPressed = true;
            if (UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetKeyDown(KeyCode.J)) BasicAttackPressed = true;
            if (UnityEngine.Input.GetMouseButtonDown(1) || UnityEngine.Input.GetKeyDown(KeyCode.K)) SpecialAttackPressed = true;
            BlockHeld = UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.I) || UnityEngine.Input.GetMouseButton(1);
        }

        public void ClearBuffer()
        {
            JumpPressed = false;
            DashPressed = false;
            BasicAttackPressed = false;
            SpecialAttackPressed = false;
        }
    }
}
