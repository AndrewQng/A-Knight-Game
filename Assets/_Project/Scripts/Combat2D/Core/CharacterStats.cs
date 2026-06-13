using UnityEngine;
using System;
using KnightGame;

namespace KnightGame.Combat2D.Core
{
    public class CharacterStats : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float m_maxHealth = 100f;
        private float m_currentHealth;

        [Header("Stamina Settings")]
        [SerializeField] private float m_maxStamina = 100f;
        [SerializeField] private float m_staminaRegenRate = 25f; // Tốc độ hồi phục thể lực mỗi giây
        private float m_currentStamina;

        [Header("Mana Settings (Magic)")]
        [SerializeField] private float m_maxMana = 100f;
        [SerializeField] private float m_manaRegenRate = 5f; // Tốc độ hồi phục mana mỗi giây
        private float m_currentMana;

        [Header("Combat Settings")]
        [SerializeField] private float m_attackSpeed = 1.0f; // Hệ số tốc độ đánh (1.0 = mặc định)

        // C# Actions cho UI hoặc FSM lắng nghe
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnStaminaChanged;
        public event Action<float, float> OnManaChanged;
        public event Action OnTakeDamage;
        public event Action OnDeath;

        public float MaxHealth => m_maxHealth;
        public float CurrentHealth => m_currentHealth;
        public float MaxStamina => m_maxStamina;
        public float CurrentStamina => m_currentStamina;
        public float MaxMana => m_maxMana;
        public float CurrentMana => m_currentMana;
        public float AttackSpeed => m_attackSpeed;

        public void SetAttackSpeed(float speed)
        {
            m_attackSpeed = speed;
        }
        
        public bool IsDead => m_currentHealth <= 0;
        public bool IsBlocking { get; set; } = false;

        private void Awake()
        {
            m_currentHealth = m_maxHealth;
            m_currentStamina = m_maxStamina;
            m_currentMana = 0f; // Bắt đầu trận đấu bằng 0 hoặc đầy năng lượng tùy ý
        }

        private void Start()
        {
            // Kích hoạt sự kiện lần đầu
            OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
            OnStaminaChanged?.Invoke(m_currentStamina, m_maxStamina);
            OnManaChanged?.Invoke(m_currentMana, m_maxMana);
        }

        private void Update()
        {
            if (IsDead) return;

            // Tự động hồi phục Năng lượng Phép (Mana)
            if (m_currentMana < m_maxMana)
            {
                GainMana(m_manaRegenRate * Time.deltaTime);
            }

            // Tự động hồi phục Thể lực (Stamina)
            if (m_currentStamina < m_maxStamina)
            {
                GainStamina(m_staminaRegenRate * Time.deltaTime);
            }
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            if (IsBlocking)
            {
                damage *= 0.2f; // Giảm 80% sát thương khi đang đỡ đòn
            }

            m_currentHealth -= damage;
            m_currentHealth = Mathf.Max(m_currentHealth, 0f);

            // Hiển thị Floating Damage Text (Sử dụng hệ thống text có sẵn)
            FloatingText.Create(transform.position, damage);

            OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
            OnTakeDamage?.Invoke();

            if (m_currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            m_currentHealth += amount;
            m_currentHealth = Mathf.Min(m_currentHealth, m_maxHealth);
            OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
        }

        public void GainStamina(float amount)
        {
            if (IsDead) return;

            m_currentStamina += amount;
            m_currentStamina = Mathf.Min(m_currentStamina, m_maxStamina);
            OnStaminaChanged?.Invoke(m_currentStamina, m_maxStamina);
        }

        public bool UseStamina(float amount)
        {
            if (m_currentStamina < amount) return false;

            m_currentStamina -= amount;
            OnStaminaChanged?.Invoke(m_currentStamina, m_maxStamina);
            return true;
        }

        public void GainMana(float amount)
        {
            if (IsDead) return;

            m_currentMana += amount;
            m_currentMana = Mathf.Min(m_currentMana, m_maxMana);
            OnManaChanged?.Invoke(m_currentMana, m_maxMana);
        }

        public bool UseMana(float amount)
        {
            if (m_currentMana < amount) return false;

            m_currentMana -= amount;
            OnManaChanged?.Invoke(m_currentMana, m_maxMana);
            return true;
        }

        private void Die()
        {
            OnDeath?.Invoke();
        }
    }
}
