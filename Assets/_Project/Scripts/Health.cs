using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace KnightGame
{
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float m_maxHealth = 100f;
        private float m_currentHealth;

        [Header("Events")]
        public UnityEvent OnTakeDamage;
        public UnityEvent OnDeath;

        // C# Action for health UI updates
        public System.Action<float, float> OnHealthChanged;

        private Animator m_animator;
        private SpriteRenderer m_spriteRenderer;
        private Color m_originalColor;
        private Coroutine m_flashCoroutine;

        public float CurrentHealth => m_currentHealth;
        public float MaxHealth => m_maxHealth;

        private void Awake()
        {
            m_currentHealth = m_maxHealth;
        }

        private void Start()
        {
            m_animator = GetComponent<Animator>();
            m_spriteRenderer = GetComponent<SpriteRenderer>();
            if (m_spriteRenderer != null)
            {
                m_originalColor = m_spriteRenderer.color;
            }
        }

        public void TakeDamage(float damage)
        {
            if (m_currentHealth <= 0) return;

            m_currentHealth -= damage;
            m_currentHealth = Mathf.Max(m_currentHealth, 0);

            // Spawn floating damage text
            FloatingText.Create(transform.position, damage);

            // Invoke events
            OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
            OnTakeDamage?.Invoke();

            // Try to trigger Animator Hurt state if it exists
            if (m_animator != null)
            {
                if (HasParameter("Hurt", m_animator))
                {
                    m_animator.SetTrigger("Hurt");
                }
            }

            // Flash red visual effect
            if (m_spriteRenderer != null)
            {
                if (m_flashCoroutine != null)
                {
                    StopCoroutine(m_flashCoroutine);
                }
                m_flashCoroutine = StartCoroutine(FlashRedCoroutine());
            }

            if (m_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (m_currentHealth <= 0) return; // Can't heal the dead

            m_currentHealth += amount;
            m_currentHealth = Mathf.Min(m_currentHealth, m_maxHealth);
            OnHealthChanged?.Invoke(m_currentHealth, m_maxHealth);
        }

        private void Die()
        {
            OnDeath?.Invoke();

            // Try to trigger Animator Death state
            if (m_animator != null)
            {
                if (HasParameter("Death", m_animator))
                {
                    m_animator.SetTrigger("Death");
                }
            }

            Debug.Log($"{gameObject.name} has died.");

            // For default Dummies, if there is no death animation, we can fade them out or disable collider
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        private IEnumerator FlashRedCoroutine()
        {
            m_spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f); // Reddish-pink flash
            yield return new WaitForSeconds(0.15f);
            m_spriteRenderer.color = m_originalColor;
        }

        private bool HasParameter(string paramName, Animator animator)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName) return true;
            }
            return false;
        }
    }
}
