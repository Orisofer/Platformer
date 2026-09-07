using UnityEngine;

namespace OriGame.Input
{
    [System.Serializable]
    public struct FrameInput
    {
        public Vector2 Direction;
        public double Time;
        public bool JumpPressed;
        public bool JumpHeld;
        public bool JumpReleased;
        public bool MoveStarted;
        public bool MoveFinished;
        public bool DashPressed;
        public bool DashReleased;
    }
}
