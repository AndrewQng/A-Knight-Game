using UnityEngine;

namespace KnightGame.Combat2D.FSM.States
{
    public class JumpState : CharacterState
    {
        private bool m_hasJumped;

        public JumpState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            m_hasJumped = false;

            // Thiết lập trạng thái lơ lửng trên Animator an toàn
            m_stateMachine.SetAnimBool("Grounded", false);

            // Nếu nhân vật đang đứng trên đất, thực hiện đẩy lực nhảy lên
            if (m_stateMachine.Movement.IsGrounded())
            {
                m_stateMachine.SetAnimTrigger("Jump");
                m_stateMachine.Movement.SetVelocity(new Vector2(m_stateMachine.Rb.linearVelocity.x, m_stateMachine.Movement.JumpForce));
                m_hasJumped = true;
            }
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            // Cho phép đánh trên không (nếu muốn mở rộng thêm)
            if (m_stateMachine.InputHandler.BasicAttackPressed)
            {
                m_stateMachine.InitiateBasicAttack();
                return;
            }

            if (m_stateMachine.InputHandler.SpecialAttackPressed)
            {
                m_stateMachine.ChangeState(m_stateMachine.SpecialAttackState);
                return;
            }

            // Kiểm tra hạ cánh
            // Đảm bảo nhân vật đã thực sự rơi xuống (linearVelocity.y <= 0) để tránh nhảy rồi nhận ground check ngay lập tức
            if (m_stateMachine.Movement.IsGrounded() && m_stateMachine.Rb.linearVelocity.y <= 0.05f)
            {
                m_stateMachine.SetAnimBool("Grounded", true);

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

            // Cập nhật giá trị AirSpeedY lên Animator để hiển thị hoạt ảnh Jump/Fall tương ứng an toàn
            m_stateMachine.SetAnimFloat("AirSpeedY", m_stateMachine.Rb.linearVelocity.y);

            // Cho phép điều khiển di chuyển ngang một phần khi ở trên không (Air steering)
            float moveInputX = m_stateMachine.InputHandler.MoveInput.x;
            m_stateMachine.Movement.SetHorizontalVelocity(moveInputX * m_stateMachine.Movement.RunSpeed * 0.8f);
            m_stateMachine.Movement.CheckFlip(moveInputX);
        }

        public override void Exit()
        {
            base.Exit();
            m_stateMachine.SetAnimBool("Grounded", true);
        }
    }
}
