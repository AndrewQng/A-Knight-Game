using UnityEngine;

namespace KnightGame.Combat2D.FSM.States
{
    public class RunState : CharacterState
    {
        public RunState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            // Thiết lập trạng thái Chạy trên Animator (AnimState = 1)
            m_stateMachine.SetAnimInteger("AnimState", 1);
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

            // Dừng chạy nếu không còn giữ phím di chuyển
            float moveInputX = m_stateMachine.InputHandler.MoveInput.x;
            if (Mathf.Abs(moveInputX) <= 0.01f)
            {
                m_stateMachine.ChangeState(m_stateMachine.IdleState);
                return;
            }

            // Kiểm tra rơi tự do khi đi hụt chân khỏi bục
            if (!m_stateMachine.Movement.IsGrounded())
            {
                m_stateMachine.ChangeState(m_stateMachine.JumpState);
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            float moveInputX = m_stateMachine.InputHandler.MoveInput.x;
            m_stateMachine.Movement.SetHorizontalVelocity(moveInputX * m_stateMachine.Movement.RunSpeed);
            m_stateMachine.Movement.CheckFlip(moveInputX);
        }
    }
}
