using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionDetection : ILogable
{
    private PlayerContext m_PlayerContext;
    private Transform m_PlayerTransform;
    
    private ICollisionDetectionStrategy m_GroundCheck;
    private ICollisionDetectionStrategy m_RightWallCheck;
    private ICollisionDetectionStrategy m_LeftWallCheck;
    private ICollisionDetectionStrategy m_CeilingCheck;
    
    private CollisionDetectionResult m_DefaultCachedResult;
    
    private bool m_EnableCollisionDetection;
    public bool EnableLogging { get; set; }
    public bool EnableRaysDebugging { get; set; }
    public Transform transform => m_PlayerTransform;
    

    public PlayerCollisionDetection(PlayerController playerController, PlayerContext context, bool enableLogging, bool enableDebugging)
    {
        m_PlayerTransform = playerController.transform;
        m_PlayerContext = context;
        EnableRaysDebugging = enableDebugging;
        EnableLogging = enableLogging;
        
        InitializeChecks();

        m_DefaultCachedResult = new CollisionDetectionResult()
        {
            CollidedTransform = null,
            HitPattern = 0,
            Distance = 0,
            Collided = false
        };
        
        m_EnableCollisionDetection = true;
    }

    private void InitializeChecks()
    {
        m_GroundCheck = new GroundThreeRays(m_PlayerContext, EnableRaysDebugging);
        m_RightWallCheck = new WallRightThreeRays(m_PlayerContext, EnableRaysDebugging);
        m_LeftWallCheck = new WallLeftThreeRays(m_PlayerContext, EnableRaysDebugging);
        m_CeilingCheck = new CeilingThreeRays(m_PlayerContext, EnableRaysDebugging);
    }

    public ref readonly CollisionDetectionResult GroundCheck()
    {
        if (m_EnableCollisionDetection)
        {
            return ref m_GroundCheck.Calculate();
        }

        return ref m_DefaultCachedResult;
    }
    
    public ref readonly CollisionDetectionResult LeftWallCheck()
    {
        if (m_EnableCollisionDetection)
        {
            return ref m_LeftWallCheck.Calculate();
        }

        return ref m_DefaultCachedResult;
    }
    
    public ref readonly CollisionDetectionResult RightWallCheck()
    {
        if (m_EnableCollisionDetection)
        {
            return ref m_RightWallCheck.Calculate();
        }

        return ref m_DefaultCachedResult;
    }
    
    public ref readonly CollisionDetectionResult CeilingCheck()
    {
        if (m_EnableCollisionDetection)
        {
            return ref m_CeilingCheck.Calculate();
        }

        return ref m_DefaultCachedResult;
    }

    public bool SnapToGround(BoxCollider2D collider, Transform playerContextLastGround)
    {
        float playerHalfSize = collider.bounds.extents.y;
        float groundHalfSize = playerContextLastGround.localScale.y * 0.5f;
        
        float playerOrigin = transform.position.y - playerHalfSize;
        float groundOrigin = playerContextLastGround.position.y + groundHalfSize;
        
        float actualPlayerGroundDistance = playerOrigin - groundOrigin;

        if (Mathf.Abs(actualPlayerGroundDistance) > 0.001f)
        {
            transform.position = transform.position.Add(y : -actualPlayerGroundDistance);
            return true;
        }

        return false;
    }
    
    public bool SnapToCeiling(BoxCollider2D collider, Transform playerContextLastGround)
    {
        float playerHalfSize = collider.bounds.extents.y;
        float ceilingHalfSize = playerContextLastGround.localScale.y * 0.5f;
        
        float playerOrigin = transform.position.y + playerHalfSize;
        float ceilingOrigin = playerContextLastGround.position.y - ceilingHalfSize;
        
        float actualPlayerCeilingDistance = ceilingOrigin - playerOrigin;

        if (actualPlayerCeilingDistance > 0.001f)
        {
            transform.position = transform.position.Add(y : actualPlayerCeilingDistance);
            return true;
        }

        return false;
    }
    
    public void EnableCollisionDetection(bool enable)
    {
        m_EnableCollisionDetection = enable;
    }
}
