using UnityEngine;

namespace KnightGame.Combat2D.FSM.States
{
    public class BasicAttackState : AttackState
    {
        public BasicAttackState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        protected override void CheckComboCancel()
        {
            // 1. Combo Cancel sang Special Attack (chỉ khi đòn chém thường đánh trúng mục tiêu)
            if (m_hasHit && m_stateMachine.InputHandler.SpecialAttackPressed)
            {
                float specialEnergyCost = m_stateMachine.SpecialAttackState.GetEnergyCost();
                if (m_stateMachine.Stats.CurrentMana >= specialEnergyCost)
                {
                    Debug.Log("Combo Cancel! Hủy đòn chém thường chuyển sang Đòn chém Đặc biệt.");
                    m_stateMachine.ChangeState(m_stateMachine.SpecialAttackState);
                    return;
                }
            }

            // 2. Chaining Basic Attack (Hủy động tác Recovery để nối tiếp đòn tiếp theo trong combo 3 nhịp)
            // Cho phép nối chiêu khi đang ở giai đoạn Active hoặc Recovery
            if ((m_currentPhase == AttackPhase.Active || m_currentPhase == AttackPhase.Recovery) && m_stateMachine.InputHandler.BasicAttackPressed)
            {
                Debug.Log("Combo Chain! Nối tiếp nhịp chém thường tiếp theo.");
                m_stateMachine.InitiateBasicAttack();
            }
        }
    }
}
