using System;

public interface IPlayerController
{
    public PlayerControllerConfiguration PlayerConfiguration { get; }
    public event Action<PlayerContext> PlayerJumped;
    public event Action<PlayerContext> PlayerFalling;
    public event Action<PlayerContext> PlayerGrounded;
}
