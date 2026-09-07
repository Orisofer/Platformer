using OriGame.Input;

namespace OriGame.Player
{
    public interface IPlayerAbility
    {
        public bool Enabled { get; set; }
        
        public void OnUpdate(in FrameInput frameInput, float deltaTime);
        
        public PlayerMovementRequest OnFixedUpdate(float fixedDeltaTime);
    }
}

