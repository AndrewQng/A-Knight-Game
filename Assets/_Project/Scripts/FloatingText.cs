using UnityEngine;

namespace KnightGame
{
    public class FloatingText : MonoBehaviour
    {
        private TextMesh m_textMesh;
        private float m_moveSpeed = 1.2f;
        private float m_fadeSpeed = 2.5f;
        private float m_lifetime = 0.6f;
        private Color m_color;

        private void Awake()
        {
            m_textMesh = GetComponent<TextMesh>();
            if (m_textMesh != null)
            {
                m_color = m_textMesh.color;
            }
        }

        private void Update()
        {
            // Float upwards slowly
            transform.Translate(Vector3.up * m_moveSpeed * Time.deltaTime);

            // Countdown lifetime
            m_lifetime -= Time.deltaTime;
            if (m_lifetime <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                // Fade out text alpha
                if (m_textMesh != null)
                {
                    m_color.a = Mathf.Lerp(m_color.a, 0f, m_fadeSpeed * Time.deltaTime);
                    m_textMesh.color = m_color;
                }
            }
        }

        /// <summary>
        /// Creates a floating damage number at the specified position.
        /// </summary>
        public static void Create(Vector3 spawnPosition, float damageAmount)
        {
            GameObject textObj = new GameObject("DamageText");
            
            // Spawn slightly above pivot with a small random horizontal offset
            float offsetX = Random.Range(-0.3f, 0.3f);
            float offsetY = Random.Range(1.0f, 1.4f);
            textObj.transform.position = spawnPosition + new Vector3(offsetX, offsetY, 0);

            TextMesh tm = textObj.AddComponent<TextMesh>();
            tm.text = Mathf.RoundToInt(damageAmount).ToString();
            tm.fontSize = 28;
            tm.characterSize = 0.08f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.9f, 0.15f, 0.15f, 1f); // Red color for player attacks

            // Make sure the MeshRenderer sorts in front of the 2D sprites
            MeshRenderer mr = textObj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sortingOrder = 100;
            }

            // Attach self-animating component
            textObj.AddComponent<FloatingText>();
        }
    }
}
