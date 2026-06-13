using UnityEngine;

namespace KnightGame.Combat2D.Core
{
    /// <summary>
    /// Giúp camera bám theo nhân vật mượt mà, tránh hiện tượng giật màn hình bằng SmoothDamp ở LateUpdate.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform m_target;
        [SerializeField] private Vector3 m_offset = new Vector3(0f, 1f, -10f);
        [SerializeField] private float m_smoothTime = 0.15f;

        private Vector3 m_velocity = Vector3.zero;

        public void SetTarget(Transform target)
        {
            m_target = target;
        }

        private void LateUpdate()
        {
            if (m_target == null) return;

            // Xác định vị trí camera cần đến
            Vector3 targetPosition = m_target.position + m_offset;

            // Di chuyển camera mượt mà
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref m_velocity, m_smoothTime);
        }
    }
}
