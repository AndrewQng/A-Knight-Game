using UnityEngine;
using System.Collections;

namespace KnightGame
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Health))]
    public class DummyController : MonoBehaviour
    {
        [Header("Animator Controllers")]
        [SerializeField] private RuntimeAnimatorController m_idleController;
        [SerializeField] private RuntimeAnimatorController m_hitController;
        [SerializeField] private RuntimeAnimatorController m_deathController;

        private Animator m_animator;
        private Health m_health;
        private Coroutine m_actionCoroutine;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (m_health != null)
            {
                m_health.OnTakeDamage.AddListener(HandleTakeDamage);
                m_health.OnDeath.AddListener(HandleDeath);
            }
            ResetToIdle();
        }

        private void OnDisable()
        {
            if (m_health != null)
            {
                m_health.OnTakeDamage.RemoveListener(HandleTakeDamage);
                m_health.OnDeath.RemoveListener(HandleDeath);
            }
        }

        private void ResetToIdle()
        {
            m_animator.speed = 1f;
            if (m_idleController != null)
            {
                m_animator.runtimeAnimatorController = m_idleController;
            }
        }

        private void HandleTakeDamage()
        {
            if (m_health.CurrentHealth <= 0) return;

            if (m_actionCoroutine != null)
            {
                StopCoroutine(m_actionCoroutine);
            }
            m_actionCoroutine = StartCoroutine(PlayHitCoroutine());
        }

        private IEnumerator PlayHitCoroutine()
        {
            if (m_hitController != null)
            {
                m_animator.runtimeAnimatorController = m_hitController;
                // Wait for the Hit animation length (9 frames at 10FPS = 0.9s)
                yield return new WaitForSeconds(0.9f);
            }

            ResetToIdle();
        }

        private void HandleDeath()
        {
            if (m_actionCoroutine != null)
            {
                StopCoroutine(m_actionCoroutine);
            }

            if (m_deathController != null)
            {
                m_animator.runtimeAnimatorController = m_deathController;
            }

            // Start coroutine to fade out and destroy the dummy after the animation finishes
            StartCoroutine(DeathCoroutine());
        }

        private IEnumerator DeathCoroutine()
        {
            // Wait for one frame so the animator starts loading the new controller
            yield return null;

            float timeout = 2.0f;
            float elapsed = 0f;
            
            AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
            // Wait until the animation reaches 95% of its first cycle
            while (stateInfo.normalizedTime < 0.95f && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
                stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
            }

            // Pause the animator so it stays on the last frame (lying down)
            m_animator.speed = 0f;

            // Stay dead on the ground for a short moment
            yield return new WaitForSeconds(0.4f);

            // Smooth fade out of the sprite renderer
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
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

            // Destroy the dummy completely
            Destroy(gameObject);
        }
    }
}
