using UnityEngine;

namespace KnightGame.Combat2D.Data
{
    [CreateAssetMenu(fileName = "NewAttackData", menuName = "Combat 2D/Attack Data")]
    public class AttackDataSO : ScriptableObject
    {
        [Header("Animation settings")]
        [Tooltip("Trigger name in the Animator")]
        public string AnimationTrigger;

        [Header("Timings (in Seconds)")]
        public float StartupDuration = 0.15f;
        public float ActiveDuration = 0.1f;
        public float RecoveryDuration = 0.2f;

        [Header("Damage & Hit settings")]
        public float Damage = 15f;
        public Vector2 KnockbackForce = new Vector2(5f, 2f);
        public float HitstunDuration = 0.3f;

        [Header("Cost settings")]
        public float EnergyCost = 0f;

        [Header("Attacker Movement")]
        [Tooltip("Lực đẩy lao về phía trước khi ra chiêu (đà tấn công)")]
        public float AttackerForwardForce = 2f;
    }
}
