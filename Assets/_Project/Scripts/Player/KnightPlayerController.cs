using UnityEngine;
using UnityEngine.InputSystem;
using KnightGame;

namespace KnightGame.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class KnightPlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float m_speed = 4.0f;
        [SerializeField] private float m_jumpForce = 7.5f;
        [SerializeField] private float m_rollForce = 6.0f;
        [SerializeField] private float m_rollDuration = 8.0f / 14.0f;

        [Header("Sensors")]
        [SerializeField] private Sensor_HeroKnight m_groundSensor;
        [SerializeField] private Sensor_HeroKnight m_wallSensorR1;
        [SerializeField] private Sensor_HeroKnight m_wallSensorR2;
        [SerializeField] private Sensor_HeroKnight m_wallSensorL1;
        [SerializeField] private Sensor_HeroKnight m_wallSensorL2;

        [Header("Visual Effects")]
        [SerializeField] private GameObject m_slideDust;
        [SerializeField] private bool m_noBlood = false;

        [Header("Combat Settings")]
        [SerializeField] private Transform m_attackPoint;
        [SerializeField] private float m_attackRange = 0.8f;
        [SerializeField] private float m_attackDamage = 20f;
        [SerializeField] private LayerMask m_enemyLayers;

        [Header("Input Configurations")]
        [Tooltip("Optional: Link the PlayerInput component. If empty, will try to get it on Awake.")]
        [SerializeField] private PlayerInput m_playerInput;

        // Components
        private Animator m_animator;
        private Rigidbody2D m_body2d;
        private SpriteRenderer m_spriteRenderer;

        // Input Actions
        private InputAction m_moveAction;
        private InputAction m_jumpAction;
        private InputAction m_attackAction;
        private InputAction m_blockAction;
        private InputAction m_rollAction;

        // State Variables
        private bool m_isWallSliding = false;
        private bool m_grounded = false;
        private bool m_rolling = false;
        private int m_facingDirection = 1;
        private int m_currentAttack = 0;
        private float m_timeSinceAttack = 0.0f;
        private float m_delayToIdle = 0.0f;
        private float m_rollCurrentTime;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_body2d = GetComponent<Rigidbody2D>();
            m_spriteRenderer = GetComponent<SpriteRenderer>();

            // Auto-locate sensors if not assigned
            if (m_groundSensor == null) m_groundSensor = transform.Find("GroundSensor")?.GetComponent<Sensor_HeroKnight>();
            if (m_wallSensorR1 == null) m_wallSensorR1 = transform.Find("WallSensor_R1")?.GetComponent<Sensor_HeroKnight>();
            if (m_wallSensorR2 == null) m_wallSensorR2 = transform.Find("WallSensor_R2")?.GetComponent<Sensor_HeroKnight>();
            if (m_wallSensorL1 == null) m_wallSensorL1 = transform.Find("WallSensor_L1")?.GetComponent<Sensor_HeroKnight>();
            if (m_wallSensorL2 == null) m_wallSensorL2 = transform.Find("WallSensor_L2")?.GetComponent<Sensor_HeroKnight>();

            // Auto-locate or create AttackPoint
            if (m_attackPoint == null)
            {
                Transform foundPoint = transform.Find("AttackPoint");
                if (foundPoint != null)
                {
                    m_attackPoint = foundPoint;
                }
                else
                {
                    GameObject newPoint = new GameObject("AttackPoint");
                    newPoint.transform.SetParent(this.transform);
                    newPoint.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
                    m_attackPoint = newPoint.transform;
                }
            }

            // Set up input actions
            if (m_playerInput == null)
                m_playerInput = GetComponent<PlayerInput>();

            InitializeInputs();
        }

        private void InitializeInputs()
        {
            if (m_playerInput == null)
                m_playerInput = GetComponent<PlayerInput>();

            if (m_playerInput != null)
            {
                // Bind actions based on standard InputSystem_Actions map names
                m_moveAction = m_playerInput.actions.FindAction("Move");
                m_jumpAction = m_playerInput.actions.FindAction("Jump");
                m_attackAction = m_playerInput.actions.FindAction("Attack");
                
                // Map Block to Crouch action (or custom Block if it exists)
                m_blockAction = m_playerInput.actions.FindAction("Crouch");
                if (m_blockAction == null)
                    m_blockAction = m_playerInput.actions.FindAction("Block");

                // Map Roll to Sprint action (or custom Roll if it exists)
                m_rollAction = m_playerInput.actions.FindAction("Sprint");
                if (m_rollAction == null)
                    m_rollAction = m_playerInput.actions.FindAction("Roll");

                // Enable the input actions manually to guarantee they track state
                m_moveAction?.Enable();
                m_jumpAction?.Enable();
                m_attackAction?.Enable();
                m_blockAction?.Enable();
                m_rollAction?.Enable();
            }
        }

        private void Update()
        {
            // Increase timers
            m_timeSinceAttack += Time.deltaTime;
            if (m_rolling)
            {
                m_rollCurrentTime += Time.deltaTime;
                if (m_rollCurrentTime > m_rollDuration)
                    m_rolling = false;
            }

            // Ground check
            bool groundSensorState = m_groundSensor != null && m_groundSensor.State();
            if (!m_grounded && groundSensorState)
            {
                m_grounded = true;
                m_animator.SetBool("Grounded", m_grounded);
            }
            else if (m_grounded && !groundSensorState)
            {
                m_grounded = false;
                m_animator.SetBool("Grounded", m_grounded);
            }

            // -- Input Reading (Hybrid New Input System & Legacy Fallback) --
            float inputX = 0f;
            bool jumpPressed = false;
            bool attackPressed = false;
            bool blockPressed = false;
            bool blockReleased = false;
            bool rollPressed = false;

            if (m_playerInput != null && m_moveAction != null)
            {
                inputX = m_moveAction.ReadValue<Vector2>().x;
                jumpPressed = m_jumpAction != null && m_jumpAction.WasPressedThisFrame();
                attackPressed = m_attackAction != null && m_attackAction.WasPressedThisFrame();
                blockPressed = m_blockAction != null && m_blockAction.WasPressedThisFrame();
                blockReleased = m_blockAction != null && m_blockAction.WasReleasedThisFrame();
                rollPressed = m_rollAction != null && m_rollAction.WasPressedThisFrame();
            }
            else
            {
                // Fallback to legacy Unity Input Manager
                inputX = Input.GetAxis("Horizontal");
                jumpPressed = Input.GetKeyDown(KeyCode.Space);
                attackPressed = Input.GetMouseButtonDown(0);
                blockPressed = Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.S);
                blockReleased = Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.S);
                rollPressed = Input.GetKeyDown(KeyCode.LeftShift);
            }

            // Handle horizontal flipping and attack point alignment
            if (!m_rolling)
            {
                if (inputX > 0)
                {
                    m_spriteRenderer.flipX = false;
                    m_facingDirection = 1;
                    if (m_attackPoint != null)
                    {
                        m_attackPoint.localPosition = new Vector3(Mathf.Abs(m_attackPoint.localPosition.x), m_attackPoint.localPosition.y, 0);
                    }
                }
                else if (inputX < 0)
                {
                    m_spriteRenderer.flipX = true;
                    m_facingDirection = -1;
                    if (m_attackPoint != null)
                    {
                        m_attackPoint.localPosition = new Vector3(-Mathf.Abs(m_attackPoint.localPosition.x), m_attackPoint.localPosition.y, 0);
                    }
                }
            }

            // Movement Physics
            if (!m_rolling)
            {
                m_body2d.linearVelocity = new Vector2(inputX * m_speed, m_body2d.linearVelocity.y);
            }

            // Set Animator variables
            m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);

            // Wall Slide check
            bool wallR = m_wallSensorR1 != null && m_wallSensorR2 != null && m_wallSensorR1.State() && m_wallSensorR2.State();
            bool wallL = m_wallSensorL1 != null && m_wallSensorL2 != null && m_wallSensorL1.State() && m_wallSensorL2.State();
            m_isWallSliding = wallR || wallL;
            m_animator.SetBool("WallSlide", m_isWallSliding);

            // Actions & Animation triggering
            HandleActions(inputX, jumpPressed, attackPressed, blockPressed, blockReleased, rollPressed);
        }

        private void HandleActions(float inputX, bool jumpPressed, bool attackPressed, bool blockPressed, bool blockReleased, bool rollPressed)
        {
            // Jump
            if (jumpPressed && m_grounded && !m_rolling)
            {
                m_animator.SetTrigger("Jump");
                m_grounded = false;
                m_animator.SetBool("Grounded", m_grounded);
                m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
                if (m_groundSensor != null)
                {
                    m_groundSensor.Disable(0.2f);
                }
            }
            // Attack
            else if (attackPressed && m_timeSinceAttack > 0.25f && !m_rolling)
            {
                m_currentAttack++;

                // Loop back to one after third attack
                if (m_currentAttack > 3)
                    m_currentAttack = 1;

                // Reset Attack combo if time since last attack is too large
                if (m_timeSinceAttack > 1.0f)
                    m_currentAttack = 1;

                // Call one of three attack animations
                m_animator.SetTrigger("Attack" + m_currentAttack);

                // Inflict damage after animation delay
                StartCoroutine(AttackDamageCoroutine(m_currentAttack));

                // Reset timer
                m_timeSinceAttack = 0.0f;
            }
            // Block (Hold to block)
            else if (blockPressed && !m_rolling)
            {
                m_animator.SetTrigger("Block");
                m_animator.SetBool("IdleBlock", true);
            }
            else if (blockReleased)
            {
                m_animator.SetBool("IdleBlock", false);
            }
            // Roll / Dash
            else if (rollPressed && !m_rolling && !m_isWallSliding)
            {
                m_rolling = true;
                m_rollCurrentTime = 0.0f;
                m_animator.SetTrigger("Roll");
                m_body2d.linearVelocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.linearVelocity.y);
            }
            // Run / Idle animation transitions
            else if (Mathf.Abs(inputX) > Mathf.Epsilon)
            {
                m_delayToIdle = 0.05f;
                m_animator.SetInteger("AnimState", 1);
            }
            else
            {
                m_delayToIdle -= Time.deltaTime;
                if (m_delayToIdle < 0)
                    m_animator.SetInteger("AnimState", 0);
            }
        }

        private System.Collections.IEnumerator AttackDamageCoroutine(int comboIndex)
        {
            // Wait slightly for the animation swing to align with visual impact
            float delay = (comboIndex == 3) ? 0.15f : 0.1f;
            yield return new WaitForSeconds(delay);

            if (m_attackPoint == null) yield break;

            // Resolve LayerMask: if empty/unset, default to Everything
            LayerMask mask = m_enemyLayers;
            if (mask.value == 0)
            {
                mask = ~0; // Detect everything
            }

            // Detect damageable objects
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(m_attackPoint.position, m_attackRange, mask);
            foreach (Collider2D enemy in hitEnemies)
            {
                // Skip self
                if (enemy.gameObject == gameObject) continue;

                Health enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(m_attackDamage);
                }
            }
        }

        // Exposed trigger functions for external calls (like UI buttons, enemies, hazards)
        public void TriggerHurt()
        {
            if (!m_rolling)
            {
                m_animator.SetTrigger("Hurt");
            }
        }

        public void TriggerDeath()
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }

        // Animation Event Called in slide animation
        public void AE_SlideDust()
        {
            Vector3 spawnPosition = Vector3.zero;

            if (m_facingDirection == 1 && m_wallSensorR2 != null)
                spawnPosition = m_wallSensorR2.transform.position;
            else if (m_facingDirection == -1 && m_wallSensorL2 != null)
                spawnPosition = m_wallSensorL2.transform.position;

            if (m_slideDust != null && spawnPosition != Vector3.zero)
            {
                GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation);
                dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_attackPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(m_attackPoint.position, m_attackRange);
            }
        }
    }
}
