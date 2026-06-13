using UnityEngine;
using KnightGame.Combat2D.Combat;
using KnightGame.Combat2D.Data;

namespace KnightGame.Combat2D.FSM.States
{
    public abstract class AttackState : CharacterState
    {
        protected enum AttackPhase { Startup, Active, Recovery }

        protected AttackDataSO m_attackData;
        protected AttackPhase m_currentPhase;
        protected float m_timer;
        protected bool m_hasHit; // Cờ báo hiệu đòn đánh đã trúng đích
        protected Hitbox m_hitbox;

        protected float m_scaledStartup;
        protected float m_scaledActive;
        protected float m_scaledRecovery;

        public AttackState(CharacterStateMachine stateMachine) : base(stateMachine) { }

        /// <summary>
        /// Gán dữ liệu đòn đánh cụ thể trước khi vào trạng thái.
        /// </summary>
        public void SetAttackData(AttackDataSO data)
        {
            m_attackData = data;
        }

        public override void Enter()
        {
            base.Enter();

            if (m_attackData == null)
            {
                Debug.LogError("Chưa gán AttackDataSO cho AttackState!");
                m_stateMachine.ChangeState(m_stateMachine.IdleState);
                return;
            }

            m_timer = 0f;
            m_currentPhase = AttackPhase.Startup;
            m_hasHit = false;

            // Tính toán thời gian các giai đoạn dựa theo tốc độ đánh (AttackSpeed)
            float attackSpeed = m_stateMachine.Stats != null ? m_stateMachine.Stats.AttackSpeed : 1f;
            if (attackSpeed <= 0.01f) attackSpeed = 1f;

            m_scaledStartup = m_attackData.StartupDuration / attackSpeed;
            m_scaledActive = m_attackData.ActiveDuration / attackSpeed;
            m_scaledRecovery = m_attackData.RecoveryDuration / attackSpeed;

            // Đồng bộ tốc độ hoạt ảnh
            if (m_stateMachine.Anim != null)
            {
                m_stateMachine.Anim.speed = attackSpeed;
            }

            // Dừng di chuyển ban đầu và áp dụng lực lướt tới trước (đà tấn công) theo hướng quay mặt
            m_stateMachine.Movement.Stop();
            if (m_attackData.AttackerForwardForce > 0.01f)
            {
                m_stateMachine.Movement.SetHorizontalVelocity(m_attackData.AttackerForwardForce * m_stateMachine.Movement.FacingDirection);
            }

            // Tìm Hitbox trong các đối tượng con
            m_hitbox = m_stateMachine.GetComponentInChildren<Hitbox>();
            if (m_hitbox != null)
            {
                // Cấu hình thông số sát thương và lực giật cho Hitbox
                m_hitbox.Configure(m_attackData.Damage, m_attackData.KnockbackForce, m_attackData.HitstunDuration);
                m_hitbox.OnHitSuccess += HandleHitSuccess;
            }

            // Kích hoạt animation tương ứng an toàn
            if (!string.IsNullOrEmpty(m_attackData.AnimationTrigger))
            {
                m_stateMachine.SetAnimTrigger(m_attackData.AnimationTrigger);
            }
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
            m_timer += Time.deltaTime;

            // Thực hiện kiểm tra Combo Cancel ở các lớp con (BasicAttack, SpecialAttack)
            CheckComboCancel();

            // Tiến trình các giai đoạn đòn đánh
            switch (m_currentPhase)
            {
                case AttackPhase.Startup:
                    if (m_timer >= m_scaledStartup)
                    {
                        m_currentPhase = AttackPhase.Active;
                        m_timer = 0f;
                        
                        // Kích hoạt Hitbox gây sát thương
                        if (m_hitbox != null)
                        {
                            m_hitbox.EnableHitbox();
                        }
                    }
                    break;

                case AttackPhase.Active:
                    if (m_timer >= m_scaledActive)
                    {
                        m_currentPhase = AttackPhase.Recovery;
                        m_timer = 0f;

                        // Tắt Hitbox khi hết Active frames
                        if (m_hitbox != null)
                        {
                            m_hitbox.DisableHitbox();
                        }

                        // Dừng hoàn toàn đà lướt tới khi vào pha Recovery
                        m_stateMachine.Movement.Stop();
                    }
                    break;

                case AttackPhase.Recovery:
                    if (m_timer >= m_scaledRecovery)
                    {
                        // Đòn đánh hoàn thành, quay lại Idle
                        m_stateMachine.ChangeState(m_stateMachine.IdleState);
                    }
                    break;
            }
        }

        public override void Exit()
        {
            base.Exit();

            // Khôi phục lại tốc độ hoạt ảnh gốc
            if (m_stateMachine.Anim != null)
            {
                m_stateMachine.Anim.speed = 1f;
            }

            // Đảm bảo tắt Hitbox khi thoát trạng thái giữa chừng (VD: bị đánh trúng)
            if (m_hitbox != null)
            {
                m_hitbox.DisableHitbox();
                m_hitbox.OnHitSuccess -= HandleHitSuccess;
            }
        }

        /// <summary>
        /// Kích hoạt khi Hitbox báo cáo đã gây sát thương thành công lên đối thủ.
        /// </summary>
        protected virtual void HandleHitSuccess()
        {
            m_hasHit = true;
        }

        /// <summary>
        /// Lớp con triển khai logic hủy animation và chuyển đổi chiêu thức (Cancel).
        /// </summary>
        protected abstract void CheckComboCancel();
    }
}
