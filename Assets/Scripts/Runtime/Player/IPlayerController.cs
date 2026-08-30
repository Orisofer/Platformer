using System;

public interface IPlayerController
{
    public PlayerControllerConfiguration PlayerConfiguration { get; }
    public event Action<PlayerContext> PlayerGrounded;
}
