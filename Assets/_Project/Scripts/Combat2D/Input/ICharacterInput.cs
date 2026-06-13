using UnityEngine;

namespace KnightGame.Combat2D.Input
{
    /// <summary>
    /// Interface định nghĩa luồng Input độc lập của nhân vật.
    /// Giúp tách biệt nguồn Input (Bàn phím, Tay cầm, hay AI).
    /// </summary>
    public interface ICharacterInput
    {
        Vector2 MoveInput { get; }
        bool JumpPressed { get; }
        bool DashPressed { get; }
        bool BasicAttackPressed { get; }
        bool SpecialAttackPressed { get; }
        bool BlockHeld { get; }

        /// <summary>
        /// Reset các trạng thái trigger (như Button Press) sau khi đã được FSM đọc.
        /// </summary>
        void ClearBuffer();
    }
}
