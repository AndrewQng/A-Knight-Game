using UnityEngine;

namespace KnightGame.Combat2D.Combat
{
    /// <summary>
    /// Chứa thông tin chi tiết về đòn đánh được truyền từ Hitbox sang Hurtbox.
    /// </summary>
    public struct HitData
    {
        public float Damage;
        public Vector2 KnockbackForce; // Hướng và độ lớn lực giật lùi
        public float HitstunDuration;  // Thời gian bị đơ/choáng (giây)
        public GameObject Attacker;    // Đối tượng tung đòn đánh

        public HitData(float damage, Vector2 knockbackForce, float hitstunDuration, GameObject attacker)
        {
            Damage = damage;
            KnockbackForce = knockbackForce;
            HitstunDuration = hitstunDuration;
            Attacker = attacker;
        }
    }
}
