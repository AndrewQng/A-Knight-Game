using UnityEngine;
using KnightGame.Combat2D.Combat;

namespace KnightGame.Combat2D.FSM.States
{
    public class HitState : CharacterState
    {
        private float m_timer;
        private HitData m_hitData;

        public HitState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        public void SetHitData(HitData data)
        {
            m_hitData = data;
        }

        public override void Enter()
        {
            base.Enter();
            m_timer = 0f;

            // Kích hoạt hoạt ảnh bị thương (Hurt) an toàn
            m_stateMachine.SetAnimTrigger("Hurt");

            // Áp dụng lực đẩy lùi (Knockback) vật lý lên Rigidbody2D
            m_stateMachine.Movement.ApplyKnockback(m_hitData.KnockbackForce);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
            m_timer += Time.deltaTime;

            // Sau khi hết thời gian bị đơ (Hitstun), nhân vật trở lại trạng thái đứng chờ Idle
            if (m_timer >= m_hitData.HitstunDuration)
            {
                m_stateMachine.ChangeState(m_stateMachine.IdleState);
            }
        }

        public override void Exit()
        {
            base.Exit();
            // Dừng nhân vật lại sau khi hết bị đẩy lùi
            m_stateMachine.Movement.Stop();
        }
    }
}
