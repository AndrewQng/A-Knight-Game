using UnityEngine;

namespace KnightGame.Combat2D.FSM.States
{
    public class BlockState : CharacterState
    {
        public BlockState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            // Đánh dấu đang đỡ đòn trên hệ thống chỉ số để giảm sát thương
            m_stateMachine.Stats.IsBlocking = true;

            // Dừng nhân vật di chuyển ngang
            m_stateMachine.Movement.Stop();

            // Kích hoạt hoạt ảnh đỡ đòn
            m_stateMachine.SetAnimTrigger("Block");
            m_stateMachine.SetAnimBool("IdleBlock", true);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            // Trở lại IdleState khi người chơi nhả phím đỡ đòn (Crouch/Block)
            if (!m_stateMachine.InputHandler.BlockHeld)
            {
                m_stateMachine.ChangeState(m_stateMachine.IdleState);
            }
        }

        public override void Exit()
        {
            base.Exit();

            // Tắt trạng thái đỡ đòn
            m_stateMachine.Stats.IsBlocking = false;
            m_stateMachine.SetAnimBool("IdleBlock", false);
        }
    }
}
