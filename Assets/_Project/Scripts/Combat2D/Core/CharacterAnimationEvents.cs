using UnityEngine;
using KnightGame.Combat2D.Combat;

namespace KnightGame.Combat2D.Core
{
    /// <summary>
    /// Nhận và xử lý các sự kiện Animation Events từ Unity Animator.
    /// Giúp đồng bộ âm thanh, hiệu ứng hạt (particle) và kích hoạt hitbox chính xác.
    /// </summary>
    public class CharacterAnimationEvents : MonoBehaviour
    {
        private Hitbox m_hitbox;

        [Header("Visual Effects Prefabs")]
        [SerializeField] private GameObject m_slideDustPrefab;
        [SerializeField] private Transform m_dustSpawnPoint;

        private void Awake()
        {
            m_hitbox = GetComponentInChildren<Hitbox>();
        }

        /// <summary>
        /// Gọi từ Animator để bật Hitbox (thay thế hoặc bổ sung cho cơ chế tự động chạy theo giây).
        /// </summary>
        public void AE_EnableHitbox()
        {
            if (m_hitbox != null)
            {
                m_hitbox.EnableHitbox();
            }
        }

        /// <summary>
        /// Gọi từ Animator để tắt Hitbox.
        /// </summary>
        public void AE_DisableHitbox()
        {
            if (m_hitbox != null)
            {
                m_hitbox.DisableHitbox();
            }
        }

        /// <summary>
        /// Tạo hiệu ứng khói bụi khi lướt.
        /// </summary>
        public void AE_SlideDust()
        {
            if (m_slideDustPrefab != null)
            {
                Vector3 spawnPos = m_dustSpawnPoint != null ? m_dustSpawnPoint.position : transform.position;
                Quaternion spawnRot = m_dustSpawnPoint != null ? m_dustSpawnPoint.rotation : transform.rotation;
                
                GameObject dust = Instantiate(m_slideDustPrefab, spawnPos, spawnRot);
                
                // Đồng bộ hướng xoay của bụi theo hướng nhân vật
                var movement = GetComponentInParent<CharacterMovement>();
                if (movement != null)
                {
                    dust.transform.localScale = new Vector3(movement.FacingDirection, 1, 1);
                }
            }
        }
    }
}
