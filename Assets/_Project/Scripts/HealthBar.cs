using UnityEngine;
using UnityEngine.UI;

namespace KnightGame
{
    public class HealthBar : MonoBehaviour
    {
        [Header("Health Reference")]
        [SerializeField] private Health m_health;

        [Header("UI Sliders")]
        [SerializeField] private Slider m_mainSlider;
        [SerializeField] private Slider m_easeSlider; // Secondary slider for smooth catch-up visual effect

        [Header("Ease Settings")]
        [SerializeField] private float m_easeSpeed = 3f;
        [SerializeField] private bool m_hideWhenFull = true;

        private CanvasGroup m_canvasGroup;
        private float m_targetFill = 1f;

        private void Awake()
        {
            m_canvasGroup = GetComponent<CanvasGroup>();
            if (m_canvasGroup == null)
            {
                m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (m_mainSlider == null)
            {
                m_mainSlider = GetComponent<Slider>();
            }
        }

        private void OnEnable()
        {
            // If m_health is not assigned, search in parent objects
            if (m_health == null)
            {
                m_health = GetComponentInParent<Health>();
            }

            if (m_health != null)
            {
                m_health.OnHealthChanged += HandleHealthChanged;
                
                // Initialize values
                float fillRatio = m_health.CurrentHealth / m_health.MaxHealth;
                m_targetFill = fillRatio;
                
                if (m_mainSlider != null) m_mainSlider.value = fillRatio;
                if (m_easeSlider != null) m_easeSlider.value = fillRatio;

                ToggleVisibility(fillRatio);
            }
            else
            {
                Debug.LogWarning($"Health component not found for HealthBar on {gameObject.name}");
            }
        }

        private void OnDisable()
        {
            if (m_health != null)
            {
                m_health.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(float current, float max)
        {
            float fillRatio = current / max;
            m_targetFill = fillRatio;

            // Instantly update the main health slider
            if (m_mainSlider != null)
            {
                m_mainSlider.value = fillRatio;
            }

            // Make sure the health bar is visible if health is damaged
            ToggleVisibility(fillRatio);
        }

        private void Update()
        {
            // Smoothly lerp the catch-up ease slider to the main slider's value
            if (m_easeSlider != null && m_mainSlider != null)
            {
                if (Mathf.Abs(m_easeSlider.value - m_targetFill) > 0.001f)
                {
                    m_easeSlider.value = Mathf.Lerp(m_easeSlider.value, m_targetFill, Time.deltaTime * m_easeSpeed);
                }
                else
                {
                    m_easeSlider.value = m_targetFill;
                }
            }
        }

        private void ToggleVisibility(float currentFillRatio)
        {
            if (m_canvasGroup == null) return;

            if (m_hideWhenFull && currentFillRatio >= 1.0f)
            {
                m_canvasGroup.alpha = 0f;
            }
            else if (currentFillRatio <= 0f)
            {
                // Optionally hide when dead after a short delay, but for now we keep it visible or hide
                m_canvasGroup.alpha = 0.5f; 
            }
            else
            {
                m_canvasGroup.alpha = 1f;
            }
        }
    }
}
