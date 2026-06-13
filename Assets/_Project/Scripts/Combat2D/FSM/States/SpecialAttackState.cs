using UnityEngine;

namespace KnightGame.Combat2D.FSM.States
{
    public class SpecialAttackState : AttackState
    {
        public SpecialAttackState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            // Khấu trừ năng lượng khi thi triển chiêu đặc biệt
            if (m_attackData != null)
            {
                bool success = m_stateMachine.Stats.UseMana(m_attackData.EnergyCost);
                if (!success)
                {
                    Debug.LogWarning("Không đủ mana phép để thi triển Đòn đánh Đặc biệt!");
                    m_stateMachine.ChangeState(m_stateMachine.IdleState);
                }
            }
        }

        public float GetEnergyCost()
        {
            return m_attackData != null ? m_attackData.EnergyCost : 0f;
        }

        protected override void CheckComboCancel()
        {
            // Mặc định đòn đánh đặc biệt không thể cancel thêm (trừ phi mở rộng hệ thống combo nâng cao)
        }
    }
}
