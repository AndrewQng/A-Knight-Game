namespace KnightGame.Combat2D.FSM
{
    /// <summary>
    /// Lớp trừu tượng định nghĩa cấu trúc của một Trạng thái trong nhân vật.
    /// </summary>
    public abstract class CharacterState
    {
        protected CharacterStateMachine m_stateMachine;

        public CharacterState(CharacterStateMachine stateMachine)
        {
            m_stateMachine = stateMachine;
        }

        /// <summary>
        /// Được gọi khi nhân vật bắt đầu chuyển vào trạng thái này.
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// Được gọi trước khi nhân vật thoát khỏi trạng thái này để chuyển sang trạng thái khác.
        /// </summary>
        public virtual void Exit() { }

        /// <summary>
        /// Được gọi mỗi Frame trong phương thức Update() của StateMachine.
        /// Thích hợp để xử lý logic chuyển đổi trạng thái và đọc input.
        /// </summary>
        public virtual void LogicUpdate() { }

        /// <summary>
        /// Được gọi trong FixedUpdate() của StateMachine.
        /// Thích hợp để xử lý các phép toán vật lý liên quan tới di chuyển, trọng lực.
        /// </summary>
        public virtual void PhysicsUpdate() { }
    }
}
