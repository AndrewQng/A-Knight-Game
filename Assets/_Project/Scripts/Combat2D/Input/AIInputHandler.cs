using UnityEngine;

namespace KnightGame.Combat2D.Input
{
    public class AIInputHandler : MonoBehaviour, ICharacterInput
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool DashPressed { get; private set; }
        public bool BasicAttackPressed { get; private set; }
        public bool SpecialAttackPressed { get; private set; }
        public bool BlockHeld { get; private set; }

        public void SetMoveInput(Vector2 move)
        {
            MoveInput = move;
        }

        public void SetJump(bool pressed)
        {
            JumpPressed = pressed;
        }

        public void SetDash(bool pressed)
        {
            DashPressed = pressed;
        }

        public void SetBasicAttack(bool pressed)
        {
            BasicAttackPressed = pressed;
        }

        public void SetSpecialAttack(bool pressed)
        {
            SpecialAttackPressed = pressed;
        }

        public void SetBlock(bool held)
        {
            BlockHeld = held;
        }

        public void ClearBuffer()
        {
            JumpPressed = false;
            DashPressed = false;
            BasicAttackPressed = false;
            SpecialAttackPressed = false;
        }
    }
}
