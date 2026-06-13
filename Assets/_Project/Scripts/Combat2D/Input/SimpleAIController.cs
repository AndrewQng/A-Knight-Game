using UnityEngine;
using KnightGame.Combat2D.Core;

namespace KnightGame.Combat2D.Input
{
    [RequireComponent(typeof(AIInputHandler))]
    public class SimpleAIController : MonoBehaviour
    {
        [Header("AI Seek Settings")]
        [SerializeField] private float m_detectionRange = 15f;
        [SerializeField] private float m_attackRange = 1.2f;
        [SerializeField] private float m_attackCooldown = 1.5f;

        private AIInputHandler m_aiInput;
        private CharacterStats m_stats;
        private Transform m_playerTransform;
        private float m_cooldownTimer;

        private void Awake()
        {
            m_aiInput = GetComponent<AIInputHandler>();
            m_stats = GetComponent<CharacterStats>();
        }

        private void Start()
        {
            // Tìm Player trong Scene
            GameObject player = GameObject.Find("HeroKnight_FSM") ?? GameObject.FindWithTag("Player");
            if (player != null)
            {
                m_playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (m_aiInput == null || m_stats == null || m_stats.IsDead)
            {
                if (m_aiInput != null) m_aiInput.SetMoveInput(Vector2.zero);
                return;
            }

            // Nếu chưa tìm thấy Player, tiếp tục tìm kiếm
            if (m_playerTransform == null)
            {
                GameObject player = GameObject.Find("HeroKnight_FSM") ?? GameObject.FindWithTag("Player");
                if (player != null)
                {
                    m_playerTransform = player.transform;
                }
                m_aiInput.SetMoveInput(Vector2.zero);
                return;
            }

            m_cooldownTimer -= Time.deltaTime;

            float distanceToPlayer = Vector2.Distance(transform.position, m_playerTransform.position);

            if (distanceToPlayer <= m_detectionRange)
            {
                // Hướng ngang tới Player
                float directionX = m_playerTransform.position.x - transform.position.x;
                int moveDir = directionX > 0.1f ? 1 : (directionX < -0.1f ? -1 : 0);

                if (distanceToPlayer > m_attackRange)
                {
                    // Di chuyển tiếp cận Player
                    m_aiInput.SetMoveInput(new Vector2(moveDir, 0f));
                }
                else
                {
                    // Ở trong tầm đánh: Dừng di chuyển và tấn công
                    m_aiInput.SetMoveInput(Vector2.zero);

                    if (m_cooldownTimer <= 0f)
                    {
                        m_aiInput.SetBasicAttack(true);
                        m_cooldownTimer = m_attackCooldown;
                    }
                }
            }
            else
            {
                // Player ngoài tầm phát hiện: Đứng yên
                m_aiInput.SetMoveInput(Vector2.zero);
            }
        }
    }
}
