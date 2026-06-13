using UnityEngine;

namespace KnightGame.Combat2D.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Movement Parameters")]
        [SerializeField] private float m_runSpeed = 6.0f;
        [SerializeField] private float m_jumpForce = 12.0f;
        [SerializeField] private float m_dashSpeed = 16.0f;
        [SerializeField] private float m_dashDuration = 0.25f;

        [Header("Ground Check")]
        [SerializeField] private Transform m_groundCheckPoint;
        [SerializeField] private float m_groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask m_groundLayer;

        // Properties
        public float RunSpeed => m_runSpeed;
        public float JumpForce => m_jumpForce;
        public float DashSpeed => m_dashSpeed;
        public float DashDuration => m_dashDuration;
        public int FacingDirection { get; private set; } = 1;

        private Rigidbody2D m_rb;
        private Sensor_HeroKnight m_heroKnightSensor;

        private void Awake()
        {
            m_rb = GetComponent<Rigidbody2D>();

            // Tìm cảm biến HeroKnight gốc trên đối tượng con
            m_heroKnightSensor = GetComponentInChildren<Sensor_HeroKnight>();

            // Điểm kiểm tra mặt đất mặc định
            if (m_groundCheckPoint == null)
            {
                m_groundCheckPoint = transform;
            }
        }

        /// <summary>
        /// Kiểm tra xem nhân vật có đang đứng trên mặt đất không.
        /// Ưu tiên dùng Sensor_HeroKnight có sẵn của prefab gốc, nếu không có sẽ fallback về OverlapCircle.
        /// </summary>
        public bool IsGrounded()
        {
            if (m_heroKnightSensor != null)
            {
                return m_heroKnightSensor.State();
            }

            if (m_groundCheckPoint == null) return false;

            // Resolve LayerMask mặc định nếu chưa gán
            LayerMask mask = m_groundLayer;
            if (mask.value == 0)
            {
                // Mặc định bỏ qua layer của chính Player và Ignore Raycast
                mask = ~LayerMask.GetMask("Player", "Ignore Raycast");
            }

            Collider2D col = Physics2D.OverlapCircle(m_groundCheckPoint.position, m_groundCheckRadius, mask);
            return col != null;
        }

        /// <summary>
        /// Tạm thời vô hiệu hóa cảm biến mặt đất (hữu ích khi thực hiện nhảy lên).
        /// </summary>
        public void DisableSensor(float duration)
        {
            if (m_heroKnightSensor != null)
            {
                m_heroKnightSensor.Disable(duration);
            }
        }

        /// <summary>
        /// Thiết lập vận tốc ngang (X) và giữ nguyên vận tốc dọc (Y).
        /// </summary>
        public void SetHorizontalVelocity(float xVelocity)
        {
            m_rb.linearVelocity = new Vector2(xVelocity, m_rb.linearVelocity.y);
        }

        /// <summary>
        /// Thiết lập toàn bộ vận tốc vector.
        /// </summary>
        public void SetVelocity(Vector2 velocity)
        {
            m_rb.linearVelocity = velocity;
        }

        /// <summary>
        /// Dừng chuyển động vật lý.
        /// </summary>
        public void Stop()
        {
            m_rb.linearVelocity = Vector2.zero;
        }

        /// <summary>
        /// Đẩy nhân vật lùi lại (Knockback) khi trúng chiêu.
        /// </summary>
        public void ApplyKnockback(Vector2 force)
        {
            m_rb.linearVelocity = Vector2.zero;
            m_rb.AddForce(force, ForceMode2D.Impulse);
        }

        /// <summary>
        /// Kiểm tra xoay hướng nhân vật dựa trên hướng di chuyển của người chơi.
        /// </summary>
        public void CheckFlip(float inputX)
        {
            if (inputX > 0.01f && FacingDirection == -1)
            {
                Flip();
            }
            else if (inputX < -0.01f && FacingDirection == 1)
            {
                Flip();
            }
        }

        private void Flip()
        {
            FacingDirection *= -1;
            
            // Xoay góc trục Y để đảo hướng toàn bộ phần con (gồm cả Hitbox/Hurtbox)
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * FacingDirection;
            transform.localScale = currentScale;
        }

        /// <summary>
        /// Thay đổi Gravity Scale của Rigidbody2D (hữu dụng khi Dash hoặc Jump).
        /// </summary>
        public void SetGravityScale(float scale)
        {
            m_rb.gravityScale = scale;
        }

        private void OnDrawGizmosSelected()
        {
            if (m_groundCheckPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(m_groundCheckPoint.position, m_groundCheckRadius);
            }
        }
    }
}
