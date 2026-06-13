using UnityEngine;
using KnightGame.Combat2D.FSM;

namespace KnightGame.Combat2D.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class Hurtbox : MonoBehaviour
    {
        private Collider2D m_collider;
        private CharacterStateMachine m_stateMachine;

        /// <summary>
        /// Trạng thái vô địch (không nhận sát thương/choáng) - VD: khi đang Roll hoặc nằm sân.
        /// </summary>
        public bool IsInvincible { get; set; } = false;

        private void Awake()
        {
            m_collider = GetComponent<Collider2D>();
            m_collider.isTrigger = true; // Hurtbox luôn là Trigger

            // Tìm CharacterStateMachine ở gốc đối tượng nhân vật
            m_stateMachine = transform.root.GetComponent<CharacterStateMachine>();
            if (m_stateMachine == null)
            {
                Debug.LogWarning($"Không tìm thấy CharacterStateMachine trên root của {gameObject.name}");
            }
        }

        /// <summary>
        /// Nhận sát thương từ Hitbox và chuyển tiếp tới CharacterStateMachine.
        /// </summary>
        public void TakeHit(HitData hitData)
        {
            if (IsInvincible) return;

            if (m_stateMachine != null)
            {
                m_stateMachine.ApplyHit(hitData);
            }
        }
    }
}
