using UnityEngine;
using KnightGame.Combat2D.Combat;

namespace KnightGame.Combat2D.FSM.States
{
    public class DeadState : CharacterState
    {
        public DeadState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            // Dừng hoàn toàn chuyển động vật lý
            m_stateMachine.Movement.Stop();
            m_stateMachine.Rb.bodyType = RigidbodyType2D.Kinematic;

            // Vô hiệu hóa toàn bộ Collider (để không cản trở nhân vật khác di chuyển qua)
            var colliders = m_stateMachine.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // Vô hiệu hóa toàn bộ Hurtbox để không nhận thêm sát thương nữa
            var hurtboxes = m_stateMachine.GetComponentsInChildren<Hurtbox>();
            foreach (var hb in hurtboxes)
            {
                hb.enabled = false;
            }

            // Kích hoạt hoạt ảnh gục ngã (Death) an toàn
            m_stateMachine.SetAnimTrigger("Death");
            Debug.Log($"{m_stateMachine.gameObject.name} đã bị tiêu diệt.");

            // Bắt đầu chuỗi tự hủy làm mờ dần nếu được kích hoạt (như trên Dummy)
            m_stateMachine.StartDeathSequence();
        }
    }
}
