using UnityEngine;

namespace KnightGame.Combat2D.FSM.States
{
    public class IdleState : CharacterState
    {
        public IdleState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            // Dừng chuyển động nhân vật
            m_stateMachine.Movement.Stop();
            // Thiết lập trạng thái Idle trên Animator (giá trị AnimState = 0)
            m_stateMachine.SetAnimInteger("AnimState", 0);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            // Chuyển sang Đỡ đòn (Block) khi nhấn giữ
            if (m_stateMachine.InputHandler.BlockHeld)
            {
                m_stateMachine.ChangeState(m_stateMachine.BlockState);
                return;
            }

            // Chuyển sang Tấn công thường (Combo)
            if (m_stateMachine.InputHandler.BasicAttackPressed)
            {
                m_stateMachine.InitiateBasicAttack();
                return;
            }

            // Chuyển sang Tấn công đặc biệt
            if (m_stateMachine.InputHandler.SpecialAttackPressed)
            {
                m_stateMachine.ChangeState(m_stateMachine.SpecialAttackState);
                return;
            }

            // Chuyển sang Lướt (Tiêu hao 25 Thể lực)
            if (m_stateMachine.InputHandler.DashPressed)
            {
                if (m_stateMachine.Stats.UseStamina(25f))
                {
                    m_stateMachine.ChangeState(m_stateMachine.DashState);
                    return;
                }
            }

            // Chuyển sang Nhảy (Tiêu hao 10 Thể lực)
            if (m_stateMachine.InputHandler.JumpPressed && m_stateMachine.Movement.IsGrounded())
            {
                if (m_stateMachine.Stats.UseStamina(10f))
                {
                    m_stateMachine.ChangeState(m_stateMachine.JumpState);
                    return;
                }
            }

            // Chuyển sang Chạy khi có di chuyển ngang
            float moveInputX = m_stateMachine.InputHandler.MoveInput.x;
            if (Mathf.Abs(moveInputX) > 0.01f)
            {
                m_stateMachine.ChangeState(m_stateMachine.RunState);
                return;
            }

            // Kiểm tra rơi tự do (đề phòng nhân vật rơi từ trên cao xuống)
            if (!m_stateMachine.Movement.IsGrounded())
            {
                m_stateMachine.ChangeState(m_stateMachine.JumpState);
            }
        }
    }
}
