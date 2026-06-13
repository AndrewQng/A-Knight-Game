using UnityEngine;
using System.Collections.Generic;
using KnightGame.Combat2D.Core;

namespace KnightGame.Combat2D.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class Hitbox : MonoBehaviour
    {
        [Header("Default Hit Settings")]
        [SerializeField] private float m_defaultDamage = 10f;
        [SerializeField] private Vector2 m_defaultKnockback = new Vector2(4f, 2f);
        [SerializeField] private float m_defaultHitstun = 0.25f;

        private Collider2D m_collider;
        private GameObject m_owner;
        private CharacterMovement m_ownerMovement;
        
        // Theo dõi các mục tiêu đã trúng trong lượt active này để tránh gây sát thương nhiều lần
        private List<Hurtbox> m_alreadyHitList = new List<Hurtbox>();

        // Các thông số đòn đánh hiện tại (được gán động từ AttackState)
        private float m_currentDamage;
        private Vector2 m_currentKnockback;
        private float m_currentHitstun;

        // Sự kiện báo về cho AttackState khi đòn đánh trúng mục tiêu (hỗ trợ Combo Cancel)
        public System.Action OnHitSuccess;

        private void Awake()
        {
            m_collider = GetComponent<Collider2D>();
            m_collider.isTrigger = true;
            m_collider.enabled = false; // Mặc định tắt hitbox

            // Tìm chủ sở hữu ở lớp cha cao nhất
            m_owner = transform.root.gameObject;
            m_ownerMovement = m_owner.GetComponent<CharacterMovement>();

            ResetHitDataToDefault();
        }

        public void ResetHitDataToDefault()
        {
            m_currentDamage = m_defaultDamage;
            m_currentKnockback = m_defaultKnockback;
            m_currentHitstun = m_defaultHitstun;
        }

        public void Configure(float damage, Vector2 knockback, float hitstun)
        {
            m_currentDamage = damage;
            m_currentKnockback = knockback;
            m_currentHitstun = hitstun;
        }

        /// <summary>
        /// Bật Hitbox và xóa danh sách mục tiêu cũ để chuẩn bị đợt quét mới.
        /// </summary>
        public void EnableHitbox()
        {
            m_alreadyHitList.Clear();
            m_collider.enabled = true;
        }

        /// <summary>
        /// Tắt Hitbox sau khi kết thúc giai đoạn Active frames.
        /// </summary>
        public void DisableHitbox()
        {
            m_collider.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Tránh tự đánh bản thân
            if (other.gameObject == m_owner || other.transform.root.gameObject == m_owner) return;

            Hurtbox hurtbox = other.GetComponent<Hurtbox>();
            if (hurtbox != null && !m_alreadyHitList.Contains(hurtbox))
            {
                m_alreadyHitList.Add(hurtbox);

                // Tính toán hướng đẩy (Knockback) dựa theo hướng quay mặt của nhân vật tấn công
                int direction = m_ownerMovement != null ? m_ownerMovement.FacingDirection : 1;
                Vector2 finalKnockback = new Vector2(m_currentKnockback.x * direction, m_currentKnockback.y);

                // Tạo thông tin đòn đánh
                HitData hitData = new HitData(
                    m_currentDamage,
                    finalKnockback,
                    m_currentHitstun,
                    m_owner
                );

                // Gửi thông tin trúng đòn đến Hurtbox đối thủ
                hurtbox.TakeHit(hitData);

                // Kích hoạt sự kiện báo trúng đòn thành công
                OnHitSuccess?.Invoke();
            }
        }
    }
}
