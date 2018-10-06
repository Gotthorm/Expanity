
using UnityEngine;

// http://blog.theknightsofunity.com/implementing-minimap-unity/
// https://www.youtube.com/watch?v=Tjl8jP5Nuvc&t=171s
// https://www.youtube.com/watch?v=SlTkBe4YNbo

public class OrbitCamera : MonoBehaviour
{
    public Transform Target;

    public float Distance = 1000.0f;

    public float X
    {
        get
        {
            return m_CurrentRotationX;
        }
        set
        {
            m_CurrentRotationX = GlobalHelpers.RotationClamp( value, 360.0f );
        }
    }

    public float Y
    {
        get
        {
            return m_CurrentRotationY;
        }
        set
        {
            m_CurrentRotationY = GlobalHelpers.RotationClamp( value, 360.0f );
        }
    }

    // Use this for initialization
    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        m_CurrentRotationX = angles.y;
        m_CurrentRotationY = angles.x;
    }

    private void FixedUpdate()
    {
        Quaternion rotation = Quaternion.Euler( m_CurrentRotationY, m_CurrentRotationX, 0 );

        Vector3 negDistance = new Vector3( 0.0f, 0.0f, -Distance );
        Vector3 position = rotation * negDistance + Target.position;

        transform.rotation = rotation;
        transform.position = position;

        transform.LookAt( Target );
    }

    private float m_CurrentRotationX = 0.0f;
    private float m_CurrentRotationY = 0.0f;

    //void LateUpdate()
    //{
    //    if ( Target )
    //    {
    //        x += Input.GetAxis( "Mouse X" ) * xSpeed * distance * 0.02f;
    //        y -= Input.GetAxis( "Mouse Y" ) * ySpeed * 0.02f;

    //        y = ClampAngle( y, yMinLimit, yMaxLimit );

    //        Quaternion rotation = Quaternion.Euler( y, x, 0 );

    //        Vector3 negDistance = new Vector3( 0.0f, 0.0f, -Distance );
    //        Vector3 position = rotation * negDistance + Target.position;

    //        transform.rotation = rotation;
    //        transform.position = position;
    //    }
    //}

    //public static float ClampAngle( float angle, float min, float max )
    //{
    //    if ( angle < -360F )
    //        angle += 360F;
    //    if ( angle > 360F )
    //        angle -= 360F;
    //    return Mathf.Clamp( angle, min, max );
    //}
}
