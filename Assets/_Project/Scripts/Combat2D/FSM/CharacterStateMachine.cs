using UnityEngine;
using System.Collections.Generic;
using KnightGame.Combat2D.Input;
using KnightGame.Combat2D.Core;
using KnightGame.Combat2D.FSM.States;
using KnightGame.Combat2D.Data;
using KnightGame.Combat2D.Combat;

namespace KnightGame.Combat2D.FSM
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterMovement))]
    [RequireComponent(typeof(CharacterStats))]
    public class CharacterStateMachine : MonoBehaviour
    {
        [Header("State Machine Debug")]
        [SerializeField] private string m_currentStateName;

        [Header("Attack Configurations")]
        [SerializeField] private AttackDataSO m_basicAttackData; // Fallback khi không gán combo
        [SerializeField] private AttackDataSO[] m_basicAttackCombo; // Danh sách combo 3 đòn chém thường
        [SerializeField] private AttackDataSO m_specialAttackData;

        [Header("Death Settings")]
        [SerializeField] private bool m_destroyOnDeath = false; // Bật lên cho Dummy để tự biến mất sau khi ngã

        // Các thành phần tham chiếu cốt lõi
        public ICharacterInput InputHandler { get; private set; }
        public CharacterMovement Movement { get; private set; }
        public CharacterStats Stats { get; private set; }
        public Animator Anim { get; private set; }
        public Rigidbody2D Rb { get; private set; }

        // Trạng thái hiện tại
        public CharacterState CurrentState { get; private set; }

        // Danh sách các trạng thái (để tránh cấp phát bộ nhớ liên tục khi đổi trạng thái)
        public IdleState IdleState { get; private set; }
        public RunState RunState { get; private set; }
        public JumpState JumpState { get; private set; }
        public DashState DashState { get; private set; }
        public BlockState BlockState { get; private set; }
        public BasicAttackState BasicAttackState { get; private set; }
        public SpecialAttackState SpecialAttackState { get; private set; }
        public HitState HitState { get; private set; }
        public DeadState DeadState { get; private set; }

        // Quản lý Combo
        public int CurrentComboIndex { get; set; } = 0;
        public float LastAttackTime { get; set; } = 0f;

        // Danh sách cache tham số Animator
        private HashSet<string> m_animatorParams = new HashSet<string>();

        private void Awake()
        {
            // Tìm và gán các Component
            InputHandler = GetComponent<ICharacterInput>();
            if (InputHandler == null)
            {
                InputHandler = gameObject.AddComponent<PlayerInputHandler>();
            }

            Movement = GetComponent<CharacterMovement>();
            Stats = GetComponent<CharacterStats>();
            Anim = GetComponent<Animator>();
            Rb = GetComponent<Rigidbody2D>();

            // Khởi tạo các trạng thái
            IdleState = new IdleState(this);
            RunState = new RunState(this);
            JumpState = new JumpState(this);
            DashState = new DashState(this);
            BlockState = new BlockState(this);
            BasicAttackState = new BasicAttackState(this);
            SpecialAttackState = new SpecialAttackState(this);
            HitState = new HitState(this);
            DeadState = new DeadState(this);

            // Gán dữ liệu đòn đánh đặc biệt mặc định
            if (m_specialAttackData != null) SpecialAttackState.SetAttackData(m_specialAttackData);

            // Cache các tham số của Animator để tránh báo lỗi Missing Parameter
            CacheAnimatorParameters();
        }

        private void Start()
        {
            // Bắt đầu bằng trạng thái Idle
            Initialize(IdleState);
        }

        public void Initialize(CharacterState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
            m_currentStateName = CurrentState.GetType().Name;
        }

        /// <summary>
        /// Thay đổi trạng thái của nhân vật. Hỗ trợ cờ "force" để ép trạng thái tự khởi động lại (dùng cho combo chém thường liên tiếp).
        /// </summary>
        public void ChangeState(CharacterState newState, bool force = false)
        {
            if (CurrentState == newState && !force) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
            m_currentStateName = CurrentState.GetType().Name;
            Debug.Log($"[{gameObject.name}] Chuyển trạng thái sang: {m_currentStateName}");
        }

        private void Update()
        {
            if (CurrentState != null)
            {
                CurrentState.LogicUpdate();
            }

            // Xóa buffer đầu vào ở cuối mỗi Frame để tránh trôi lệch input giữa các frame
            InputHandler?.ClearBuffer();
        }

        private void FixedUpdate()
        {
            if (CurrentState != null)
            {
                CurrentState.PhysicsUpdate();
            }
        }

        /// <summary>
        /// Khởi tạo và kích hoạt nhịp combo chém tiếp theo trong chuỗi Basic Attack.
        /// </summary>
        public void InitiateBasicAttack()
        {
            if (m_basicAttackCombo == null || m_basicAttackCombo.Length == 0)
            {
                // Fallback nếu không cấu hình mảng combo
                if (m_basicAttackData != null)
                {
                    BasicAttackState.SetAttackData(m_basicAttackData);
                    ChangeState(BasicAttackState, true);
                }
                return;
            }

            // Reset nhịp combo về nhịp đầu tiên (Attack1) nếu người chơi chờ quá lâu (> 1.2 giây)
            if (Time.time - LastAttackTime > 1.2f)
            {
                CurrentComboIndex = 0;
            }

            // Lấy nhịp đòn đánh hiện tại
            AttackDataSO attackData = m_basicAttackCombo[CurrentComboIndex];
            BasicAttackState.SetAttackData(attackData);

            // Tăng chỉ số combo và lưu thời gian tung chiêu
            CurrentComboIndex = (CurrentComboIndex + 1) % m_basicAttackCombo.Length;
            LastAttackTime = Time.time;

            // Chuyển sang trạng thái tấn công thường và dùng cờ "force = true" để ép chạy lại Enter()
            ChangeState(BasicAttackState, true);
        }

        // Đóng vai trò cầu nối khi nhận sát thương từ Hurtbox
        public void ApplyHit(Combat.HitData hitData)
        {
            if (Stats.IsDead) return;

            // Áp dụng sát thương
            Stats.TakeDamage(hitData.Damage);

            if (Stats.IsDead)
            {
                ChangeState(DeadState);
            }
            else
            {
                // Chuyển sang trạng thái Hitstun và truyền thông số lực đẩy
                HitState.SetHitData(hitData);
                ChangeState(HitState);
            }
        }

        /// <summary>
        /// Bắt đầu chuỗi hủy tự động (cho Dummy) sau khi nằm sân.
        /// </summary>
        public void StartDeathSequence()
        {
            if (m_destroyOnDeath)
            {
                StartCoroutine(DummyDeathCoroutine());
            }
        }

        private System.Collections.IEnumerator DummyDeathCoroutine()
        {
            // Chờ 0.9 giây cho hoạt ảnh ngã xuống chạy đến khung cuối cùng
            yield return new WaitForSeconds(0.9f);

            // Khóa hoạt ảnh ở khung cuối cùng (không cho lặp lại/loop)
            if (Anim != null)
            {
                Anim.speed = 0f;
            }

            // Chờ thêm 0.6 giây trước khi bắt đầu làm mờ
            yield return new WaitForSeconds(0.6f);

            // Làm mờ dần SpriteRenderer
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                float fadeDuration = 0.5f;
                float fadeTimer = 0f;
                Color startColor = sr.color;
                while (fadeTimer < fadeDuration)
                {
                    fadeTimer += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
                    sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }

            // Hủy hoàn toàn Dummy
            Destroy(gameObject);
        }

        #region Safe Animator Helpers

        private void CacheAnimatorParameters()
        {
            if (Anim == null || Anim.runtimeAnimatorController == null) return;
            m_animatorParams.Clear();
            System.Text.StringBuilder sb = new System.Text.StringBuilder($"[{gameObject.name}] Animator Parameters đã cache: ");
            foreach (var param in Anim.parameters)
            {
                m_animatorParams.Add(param.name);
                sb.Append(param.name).Append(", ");
            }
            Debug.Log(sb.ToString());
        }

        public void SetAnimInteger(string name, int value)
        {
            if (Anim == null) return;

            if (m_animatorParams.Contains(name))
            {
                Anim.SetInteger(name, value);
            }
        }

        public void SetAnimBool(string name, bool value)
        {
            if (Anim != null && m_animatorParams.Contains(name))
            {
                Anim.SetBool(name, value);
            }
        }

        public void SetAnimFloat(string name, float value)
        {
            if (Anim != null && m_animatorParams.Contains(name))
            {
                Anim.SetFloat(name, value);
            }
        }

        public void SetAnimTrigger(string name)
        {
            if (Anim == null) return;

            if (m_animatorParams.Contains(name))
            {
                Anim.SetTrigger(name);
            }
        }

        #endregion
    }
}
