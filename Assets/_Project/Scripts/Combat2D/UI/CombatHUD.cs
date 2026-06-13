using UnityEngine;
using UnityEngine.UI;
using KnightGame.Combat2D.Core;

namespace KnightGame.Combat2D.UI
{
    public class CombatHUD : MonoBehaviour
    {
        [Header("Character Reference")]
        [SerializeField] private CharacterStats m_playerStats;

        [Header("Health Bar (Máu)")]
        [SerializeField] private Slider m_healthSlider;
        [SerializeField] private Slider m_healthEaseSlider;

        [Header("Stamina Bar (Thể lực)")]
        [SerializeField] private Slider m_staminaSlider;
        [SerializeField] private Slider m_staminaEaseSlider;

        [Header("Mana Bar (Phép)")]
        [SerializeField] private Slider m_manaSlider;
        [SerializeField] private Slider m_manaEaseSlider;

        [Header("Visual Ease Settings")]
        [SerializeField] private float m_easeSpeed = 3f;

        // Các tỉ lệ mục tiêu cho hiệu ứng Lerp
        private float m_healthTargetRatio = 1f;
        private float m_staminaTargetRatio = 1f;
        private float m_manaTargetRatio = 0f;

        private void Start()
        {
            // Tự động tìm kiếm Player trong Scene nếu chưa được gán
            if (m_playerStats == null)
            {
                GameObject player = GameObject.FindWithTag("Player") ?? GameObject.Find("HeroKnight_FSM");
                if (player != null)
                {
                    m_playerStats = player.GetComponent<CharacterStats>();
                }
            }

            if (m_playerStats != null)
            {
                // Đăng ký sự kiện thay đổi
                m_playerStats.OnHealthChanged += HandleHealthChanged;
                m_playerStats.OnStaminaChanged += HandleStaminaChanged;
                m_playerStats.OnManaChanged += HandleManaChanged;

                // Khởi tạo giá trị ban đầu cho các thanh trượt
                InitializeBars();
            }
            else
            {
                Debug.LogWarning("CombatHUD: Không tìm thấy component CharacterStats của Player trong Scene!");
            }
        }

        private void OnDestroy()
        {
            if (m_playerStats != null)
            {
                m_playerStats.OnHealthChanged -= HandleHealthChanged;
                m_playerStats.OnStaminaChanged -= HandleStaminaChanged;
                m_playerStats.OnManaChanged -= HandleManaChanged;
            }
        }

        private void InitializeBars()
        {
            // Máu
            float hpRatio = m_playerStats.MaxHealth > 0 ? m_playerStats.CurrentHealth / m_playerStats.MaxHealth : 0f;
            m_healthTargetRatio = hpRatio;
            if (m_healthSlider != null) m_healthSlider.value = hpRatio;
            if (m_healthEaseSlider != null) m_healthEaseSlider.value = hpRatio;

            // Thể lực
            float staminaRatio = m_playerStats.MaxStamina > 0 ? m_playerStats.CurrentStamina / m_playerStats.MaxStamina : 0f;
            m_staminaTargetRatio = staminaRatio;
            if (m_staminaSlider != null) m_staminaSlider.value = staminaRatio;
            if (m_staminaEaseSlider != null) m_staminaEaseSlider.value = staminaRatio;

            // Phép
            float manaRatio = m_playerStats.MaxMana > 0 ? m_playerStats.CurrentMana / m_playerStats.MaxMana : 0f;
            m_manaTargetRatio = manaRatio;
            if (m_manaSlider != null) m_manaSlider.value = manaRatio;
            if (m_manaEaseSlider != null) m_manaEaseSlider.value = manaRatio;
        }

        private void HandleHealthChanged(float current, float max)
        {
            m_healthTargetRatio = max > 0 ? current / max : 0f;
            if (m_healthSlider != null)
            {
                m_healthSlider.value = m_healthTargetRatio;
            }
        }

        private void HandleStaminaChanged(float current, float max)
        {
            m_staminaTargetRatio = max > 0 ? current / max : 0f;
            if (m_staminaSlider != null)
            {
                m_staminaSlider.value = m_staminaTargetRatio;
            }
        }

        private void HandleManaChanged(float current, float max)
        {
            m_manaTargetRatio = max > 0 ? current / max : 0f;
            if (m_manaSlider != null)
            {
                m_manaSlider.value = m_manaTargetRatio;
            }
        }

        private void Update()
        {
            // Hiệu ứng mượt mà (Ease Slider) đuổi theo Slider chính
            UpdateEaseBar(m_healthEaseSlider, m_healthTargetRatio);
            UpdateEaseBar(m_staminaEaseSlider, m_staminaTargetRatio);
            UpdateEaseBar(m_manaEaseSlider, m_manaTargetRatio);
        }

        private void UpdateEaseBar(Slider easeSlider, float targetRatio)
        {
            if (easeSlider == null) return;

            if (Mathf.Abs(easeSlider.value - targetRatio) > 0.001f)
            {
                easeSlider.value = Mathf.Lerp(easeSlider.value, targetRatio, Time.deltaTime * m_easeSpeed);
            }
            else
            {
                easeSlider.value = targetRatio;
            }
        }
    }
}
