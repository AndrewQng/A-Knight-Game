using UnityEngine;
using KnightGame.Combat2D.Combat;

namespace KnightGame.Combat2D.FSM.States
{
    public class DashState : CharacterState
    {
        private float m_dashTimer;
        private float m_originalGravity;
        private int m_dashDirection;
        private Hurtbox[] m_hurtboxes;

        public DashState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            m_dashTimer = 0f;

            // Tìm toàn bộ Hurtbox trên cơ thể nhân vật để kích hoạt bất tử (I-frames)
            m_hurtboxes = m_stateMachine.GetComponentsInChildren<Hurtbox>();
            SetHurtboxesInvincible(true);

            // Xác định hướng lướt dựa theo hướng nhân vật quay mặt
            m_dashDirection = m_stateMachine.Movement.FacingDirection;

            // Tạm thời tắt trọng lực để đường lướt ngang thẳng tắp
            m_originalGravity = m_stateMachine.Rb.gravityScale;
            m_stateMachine.Movement.SetGravityScale(0f);

            // Kích hoạt hoạt ảnh lướt (Roll) an toàn
            m_stateMachine.SetAnimTrigger("Roll");
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
            m_dashTimer += Time.deltaTime;

            // Chuyển sang trạng thái khác sau khi hết thời gian lướt
            if (m_dashTimer >= m_stateMachine.Movement.DashDuration)
            {
                float moveInputX = m_stateMachine.InputHandler.MoveInput.x;
                if (Mathf.Abs(moveInputX) > 0.01f)
                {
                    m_stateMachine.ChangeState(m_stateMachine.RunState);
                }
                else
                {
                    m_stateMachine.ChangeState(m_stateMachine.IdleState);
                }
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
            // Duy trì vận tốc lướt dọc theo hướng ban đầu
            m_stateMachine.Movement.SetVelocity(new Vector2(m_dashDirection * m_stateMachine.Movement.DashSpeed, 0f));
        }

        public override void Exit()
        {
            base.Exit();
            // Khôi phục lại trọng lực
            m_stateMachine.Movement.SetGravityScale(m_originalGravity);

            // Tắt trạng thái vô địch
            SetHurtboxesInvincible(false);

            // Triệt tiêu vận tốc thừa sau khi kết thúc lướt
            m_stateMachine.Movement.Stop();
        }

        private void SetHurtboxesInvincible(bool isInvincible)
        {
            if (m_hurtboxes == null) return;
            foreach (var hb in m_hurtboxes)
            {
                if (hb != null)
                {
                    hb.IsInvincible = isInvincible;
                }
            }
        }
    }
}
