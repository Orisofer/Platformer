using System;
using System.Collections.Generic;
using OriGame.Core;
using UnityEngine;

namespace OriGame.Player
{
    public class PlayerController : MonoBehaviour, IPlayerController, IUpdate, IFixedUpdate
    {
        private const int PLAYER_UPDATE_PRIORITY = -999;
        private const float SKIN_WIDTH = 0.05f;
    
        [SerializeField] private PlayerControllerConfiguration m_PlayerControllerConfiguration;
        [SerializeField] private Rigidbody2D m_Rigidbody;
        [SerializeField] private BoxCollider2D m_BoxCollider;
        [SerializeField] private PlayerContext m_PlayerContext;
        [SerializeField] private LayerMask m_PlayerLayer;
    
        private InputManager m_InputManager;
        private UpdateManager m_UpdateManager;
        private CollisionDetection m_CollisionDetection;
        private IGameLogger m_Logger;
    
        // Abilities
        // ----------------
        private PlayerMovementRequest[] m_MovementRequests;
        private IAbilityResolver m_AbilityResolver;
        private PlayerJump m_PlayerJump;
        private PlayerHorizontalMove m_PlayerHorizontalMove;
    
    
        public event Action<PlayerContext> PlayerJumped;
        public event Action<PlayerContext> PlayerFalling;
        public event Action<PlayerContext> PlayerGrounded;
        public PlayerControllerConfiguration PlayerConfiguration => m_PlayerControllerConfiguration;
        public PlayerContext PlayerContext => m_PlayerContext;
        [field: SerializeField] public bool EnableCollisionVisualizers { get; set; }
        public int UpdatePriority { get; set; }
        public int FixedUpdatePriority { get; set; }
        public bool EnableUpdate { get; set; }
        public bool EnableFixedUpdate { get; set; }

        public void Initialize(IServiceLocator serviceLocator)
        {
            if (!InitializeServices(serviceLocator))
            {
                return;
            }
        
            InitializePlayerContext();
            InitializeCollisionDetections();
            InitializePlayerAbilities();
            InitializeUpdate();
        
            m_Logger.Log("[Player Controller] Initialized");
        }

        private void InitializePlayerAbilities()
        {
            m_PlayerHorizontalMove = new PlayerHorizontalMove(this);
            m_PlayerJump =  new PlayerJump(this);
            m_AbilityResolver = new AbilityResolverBasic();

            m_MovementRequests = new PlayerMovementRequest[3];
        }

        private bool InitializeServices(IServiceLocator serviceLocator)
        {
            m_Logger = serviceLocator.GetService<IGameLogger>();
            m_InputManager = serviceLocator.GetService<InputManager>();
            m_UpdateManager  = serviceLocator.GetService<UpdateManager>();

            if (m_InputManager == null)
            {
                m_Logger.LogError("[ Player Controller ] Input Manager could not be fetched from service locator");
                return false;
            }

            if (m_UpdateManager == null)
            {
                m_Logger.LogError("[ Player Controller ] Update Manager could not be fetched from service locator");
                return false;
            }

            return true;
        }

        private void InitializeUpdate()
        {
            UpdatePriority = PLAYER_UPDATE_PRIORITY;
        
            m_UpdateManager.AddToUpdate(this);
            m_UpdateManager.AddToFixedUpdate(this);
        
            SetUpdate(true);
        }

        private void InitializePlayerContext()
        {
            m_PlayerContext = new PlayerContext();
            m_PlayerContext.ColliderBody = m_BoxCollider;
            m_PlayerContext.PlayerLayer = m_PlayerLayer;
            m_PlayerContext.SkinWidth = SKIN_WIDTH;
            m_PlayerContext.FacingRight = true;
        }

        private void InitializeCollisionDetections()
        {
            m_CollisionDetection = new CollisionDetection(this, m_PlayerContext, EnableCollisionVisualizers);
        }

        public void OnUpdate(float deltaTime)
        {
            UpdatePlayerInput();
        
            m_PlayerHorizontalMove.OnUpdate(deltaTime);
            m_PlayerJump.OnUpdate(deltaTime);
        
            UpdateCoyoteTime(deltaTime);
        }

        private void UpdatePlayerInput()
        {
            if (m_InputManager.FrameInput.JumpPressed)
            {
                m_PlayerContext.JumpPressed = true;
            }
            else
            {
                m_PlayerContext.JumpPressed = false;
            }

            if (m_InputManager.FrameInput.JumpHeld)
            {
                m_PlayerContext.JumpHeld = true;
            }
            else
            {
                m_PlayerContext.JumpHeld = false;
            }

            m_PlayerContext.HorizontalInputDir = m_InputManager.FrameInput.Direction;
        }
    
        private void UpdateCoyoteTime(float deltaTime)
        {
            if (m_PlayerContext.Falling)
            {
                m_PlayerContext.CoyoteTime -= deltaTime;
            
                if (m_PlayerContext.CoyoteTime <= 0f)
                {
                    m_PlayerContext.CoyoteTime = 0f;
                }
            }
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            StoreCurrentVelocity();
            
            m_MovementRequests = UpdateAbilities(fixedDeltaTime);
            
            Vector2 newFrameVelocity = m_AbilityResolver.ResolveMovement(ref m_MovementRequests, m_PlayerContext);
            
            ApplyGravity(ref newFrameVelocity);
            
            UpdateCollisions(ref newFrameVelocity);
            
            ApplyPendingSnap();
        
            ApplyMovement(newFrameVelocity);
        }

        private ref readonly PlayerMovementRequest[] UpdateAbilities(float fixedDeltaTime)
        {
            m_MovementRequests[1] = m_PlayerHorizontalMove.OnFixedUpdate(fixedDeltaTime);
            m_MovementRequests[2] = m_PlayerJump.OnFixedUpdate(fixedDeltaTime);
            
            return ref m_MovementRequests;
        }
        
        private void UpdateCollisions(ref Vector2 newFrameVelocity)
        {
            m_PlayerContext.CollisionPattern = 0;
            m_PlayerContext.PredictedVelocity = newFrameVelocity;
            
            m_PlayerContext.CollisionContext.Ground = m_CollisionDetection.GroundCheck();
            m_PlayerContext.CollisionContext.Ceiling = m_CollisionDetection.CeilingCheck();
            m_PlayerContext.CollisionContext.WallLeft = m_CollisionDetection.LeftWallCheck();
            m_PlayerContext.CollisionContext.WallRight = m_CollisionDetection.RightWallCheck();
        
            m_PlayerContext.CollisionPattern |= m_PlayerContext.CollisionContext.Ground.HitPattern;
            m_PlayerContext.CollisionPattern |= m_PlayerContext.CollisionContext.Ceiling.HitPattern;
            m_PlayerContext.CollisionPattern |= m_PlayerContext.CollisionContext.WallLeft.HitPattern;
            m_PlayerContext.CollisionPattern |= m_PlayerContext.CollisionContext.WallRight.HitPattern;

            if (!m_PlayerContext.Grounded && m_PlayerContext.CollisionContext.Ground && !m_PlayerContext.Jumping)
            {
                GroundTouch();
            
                m_PlayerContext.LastGround = m_PlayerContext.CollisionContext.Ground.CollidedTransform;
            
                RequestSnap(m_PlayerContext.LastGround, SnapDirection.Ground, m_PlayerContext.CollisionContext.Ground.Distance);
            
                newFrameVelocity.y = 0f;
            
                PlayerGrounded?.Invoke(m_PlayerContext);
            }
            // we are no longer on the ground (fall or jump)
            else if (m_PlayerContext.Grounded && !m_PlayerContext.CollisionContext.Ground)
            {
                m_PlayerContext.Grounded = false;
                m_PlayerContext.LastGround = null;
                m_PlayerContext.TimeLeftTheGround = m_InputManager.FrameInput.Time;
                m_PlayerContext.CoyoteTime = m_PlayerControllerConfiguration.CoyoteTime;
            }
            
            // handle horizontal collisions
            if (m_PlayerContext.CurrentVelocity.x != 0)
            {
                float currentPlayerDir = Mathf.Sign(m_PlayerContext.CurrentVelocity.x);
                
                if (HorizontalCollision(currentPlayerDir))
                {
                    newFrameVelocity.x = 0f;
                }
            }
        }
        
        private bool HorizontalCollision(float direction)
        { 
            // moving left
            if (direction < 0)
            {
                ref readonly CollisionDetectionResult leftWallCheck = ref m_PlayerContext.CollisionContext.WallLeft;

                if (leftWallCheck)
                {
                    RequestSnap(leftWallCheck.CollidedTransform, SnapDirection.WallLeft, leftWallCheck.Distance);
                    
                    return true;
                }
            }
            
            // moving right
            if (direction > 0)
            {
                ref readonly CollisionDetectionResult rightWallCheck = ref m_PlayerContext.CollisionContext.WallRight;

                if (rightWallCheck)
                {
                    RequestSnap(rightWallCheck.CollidedTransform, SnapDirection.WallRight, rightWallCheck.Distance);

                    return true;
                }
            }

            return false;
        }
        
        // player touched the ground
        private void GroundTouch()
        {
            m_PlayerContext.Grounded = true;
            m_PlayerContext.Falling = false;
            m_PlayerContext.TimeLeftTheGround = 0;
            m_PlayerContext.AvailableJumps = m_PlayerControllerConfiguration.MaxJumps;
        }
    
        private void ApplyGravity(ref Vector2 predictedVelocity)
        {

            if (m_PlayerContext.Jumping) return;
            
            if (m_PlayerContext.Grounded && m_PlayerContext.CurrentVelocity.y <= 0f)
            {
                predictedVelocity.y = 0f;

                return;
            }
            
            float airGravity = m_PlayerControllerConfiguration.FallAcceleration;
            float maxFallSpeed = m_PlayerControllerConfiguration.MaxFallSpeed;
                
            float targetWithGravity = Mathf.MoveTowards(
                m_PlayerContext.CurrentVelocity.y,
                -maxFallSpeed,
                airGravity * Time.fixedDeltaTime);
            
            float gravityDelta = targetWithGravity - m_PlayerContext.CurrentVelocity.y;

            predictedVelocity.y += gravityDelta;
        }
    
        private void ApplyMovement(Vector2 finalFrameVelocity)
        {
            m_Rigidbody.linearVelocity = finalFrameVelocity;
        }
    
        private void StoreCurrentVelocity()
        {
            m_PlayerContext.CurrentVelocity = m_Rigidbody.linearVelocity;
        }

        private void SetUpdate(bool updateEnabled)
        {
            EnableUpdate = updateEnabled;
            EnableFixedUpdate = updateEnabled;
        }
        
        private void RequestSnap(Transform target, SnapDirection direction, float distance)
        {
            m_PlayerContext.SnapRequest.Target = target; 
            m_PlayerContext.SnapRequest.Direction = direction;
            m_PlayerContext.SnapRequest.Distance = distance;
        }
    
        private void ApplyPendingSnap()
        {
            var snap = m_PlayerContext.SnapRequest;
    
            switch (snap.Direction)
            {
                case SnapDirection.Ground:
                    m_CollisionDetection.SnapToGround(snap.Distance);
                    break;

                case SnapDirection.Ceiling:
                    m_CollisionDetection.SnapToCeiling(m_BoxCollider, snap.Target);
                    break;

                case SnapDirection.WallLeft:
                    m_CollisionDetection.SnapToWall(Vector2.right, snap.Distance);
                    break;

                case SnapDirection.WallRight:
                    m_CollisionDetection.SnapToWall(Vector2.left, snap.Distance);
                    break;
            }
        
            m_PlayerContext.SnapRequest.Clear();
        }
        
        public void FlipX(bool facingRight)
        {
            if (facingRight)
            {
                Vector3 rotation = transform.rotation.eulerAngles.Replace(y: 0f);
                transform.localRotation = Quaternion.Euler(rotation);
            }
            else
            {
                Vector3 rotation = transform.rotation.eulerAngles.Replace(y: 180f);
                transform.localRotation = Quaternion.Euler(rotation);
            }

            m_PlayerContext.FacingRight = facingRight;
        }

        public void RaisePlayerFallingEvent()
        {
            PlayerFalling?.Invoke(m_PlayerContext);
        }

        public void RaiseJumpEvent()
        {
            PlayerJumped?.Invoke(m_PlayerContext);
        }
        
        private void OnDestroy()
        {
            SetUpdate(false);
        
            m_UpdateManager.RemoveFromUpdate(this);
            m_UpdateManager.RemoveFromFixedUpdate(this);
        }
    } 
}