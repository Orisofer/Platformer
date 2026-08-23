using UnityEngine;

public static class ExtensionsVector3
{
    public static Vector3 Add(this Vector3 vec3, float? x = null, float? y = null, float? z = null)
    {
        float addX = x ?? 0;
        float addY = y ?? 0;
        float addZ = z ?? 0;
        
        vec3.x += addX;
        vec3.y += addY;
        vec3.z += addZ;
        
        return vec3;
    }
    
    public static Vector3 Replace(this Vector3 vec3, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(x ?? vec3.x,y ?? vec3.y, z ?? vec3.z);
    }
}
