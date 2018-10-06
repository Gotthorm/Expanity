using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityControlCamera : MonoBehaviour
{
    public Transform Target;

    public float Distance = 1000.0f;

    public float InterpolationSpeed = 1.0f;

    public float X
    {
        get
        {
            return m_CurrentRotationX;
        }
        set
        {
            // We ignore external rotation control while interpolating
            if ( m_InterpolationTimer >= 1.0f )
            {
                m_CurrentRotationX = value;
                while ( m_CurrentRotationX < -360.0f )
                {
                    m_CurrentRotationX += 360.0f;
                }
                while ( m_CurrentRotationX > 360.0f )
                {
                    m_CurrentRotationX -= 360.0f;
                }
            }
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
            // We ignore external rotation control while interpolating
            if ( m_InterpolationTimer >= 1.0f )
            {
                m_CurrentRotationY = value;
                while ( m_CurrentRotationY < -360.0f )
                {
                    m_CurrentRotationY += 360.0f;
                }
                while ( m_CurrentRotationY > 360.0f )
                {
                    m_CurrentRotationY -= 360.0f;
                }
            }
        }
    }

    // Setting the target to null frees the camera from orbit mode, enabling strafe 
    // Setting the target to a celectial body or ship puts the camera in orbit mode
    public void SetTarget( Transform target )
    {
        // When a new non-null target is set, interpolate to the new position and rotation
        if( null != target )
        {
            m_InterpolationTimer = 0.0f;

            Vector3 relativePos = target.position - transform.position;
            m_TargetRotation = Quaternion.LookRotation( relativePos );
            m_CurrentRotation = transform.rotation;
            Distance = ( target.position - transform.position ).magnitude;

            // TEMP HACK
            //transform.rotation = Quaternion.Euler( tempTransform.eulerAngles.y, tempTransform.eulerAngles.x, 0 );
            //m_CurrentRotationX = transform.rotation.eulerAngles.x;
            //m_CurrentRotationY = transform.rotation.eulerAngles.y;
        }
        else
        {
            // Ensure we interrupt any active interpolation
            m_InterpolationTimer = 1.0f;
        }
        // Update current "from position" and "from orientation" to reset any current interpolation.

        Target = target;
    }

    // Use this for initialization
    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        m_CurrentRotationX = angles.y;
        m_CurrentRotationY = angles.x;
    }

    private void Update()
    {
        // Right mouse button drags become rotations or strafes
        // Mouse wheel is always zoom
        if ( null != Target )
        {
            if ( 1.0f > m_InterpolationTimer )
            {
                float interpolationStep = Time.deltaTime * InterpolationSpeed;
                m_InterpolationTimer += interpolationStep;

                transform.rotation = Quaternion.Slerp( m_CurrentRotation, m_TargetRotation, m_InterpolationTimer );

                //Debug.Log( "Interpolated X from " + m_CurrentRotationX.ToString() + " to " + transform.rotation.eulerAngles.y.ToString() );
                //Debug.Log( "Interpolated Y from " + m_CurrentRotationY.ToString() + " to " + transform.rotation.eulerAngles.x.ToString() );

                m_CurrentRotationX = transform.rotation.eulerAngles.y;
                m_CurrentRotationY = transform.rotation.eulerAngles.x;

                if ( 1.0f <= m_InterpolationTimer )
                {
                    m_InterpolationTimer = 1.0f;
                }
            }
            else
            {
                transform.rotation = Quaternion.Euler( m_CurrentRotationY, m_CurrentRotationX, 0 );

                Vector3 negDistance = new Vector3( 0.0f, 0.0f, -Distance );
                Vector3 position = transform.rotation * negDistance + Target.position;

                transform.rotation = transform.rotation;
                transform.position = position;

                transform.LookAt( Target );
            }
        }
        else
        {
            transform.rotation = Quaternion.Euler( m_CurrentRotationY, m_CurrentRotationX, 0 );
        }
    }

    private float m_InterpolationTimer = 1.0f;

    private Quaternion m_TargetRotation;
    private Quaternion m_CurrentRotation;

    private float m_CurrentRotationX = 0.0f;
    private float m_CurrentRotationY = 0.0f;
}
