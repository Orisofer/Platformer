using UnityEngine;

[System.Serializable]
public class PlayerContext
{
    public FrameInput FrameInput;
    public Vector2 Velocity;
    public double TimeJumpWasPressed;
    public bool Grounded;
    public bool Jumping;
    public bool HasJumpToConsume;
}
