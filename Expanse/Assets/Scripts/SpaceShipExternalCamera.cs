using UnityEngine;

public class SpaceShipExternalCamera : MonoBehaviour
{
    public uint m_MaxZoom = 200;
    public Vector2 m_PanRange = new Vector2( 90.0f, 90.0f );
    //public Vector2 m_PanCenter = new Vector2( 0.0f, 0.0f );

    public uint Zoom { get; set; }

    public Vector2 Pan
    {
        get
        {
            return new Vector2( m_Pan.x, m_Pan.y );
        }

        set
        {
            Debug.Log( "Camera (" + name + ") Pan(" + value.ToString() + ")" );

            // Transfer the pan x value
            m_Pan.x += value.x;
            m_Pan.x = Mathf.Min( m_Pan.x, m_MaxPanAngles.x );
            m_Pan.x = Mathf.Max( m_Pan.x, m_MinPanAngles.x );

            // Transfer the pan y value
            m_Pan.y += value.y;
            m_Pan.y = Mathf.Min( m_Pan.y, m_MaxPanAngles.y );
            m_Pan.y = Mathf.Max( m_Pan.y, m_MinPanAngles.y );

            UpdateOrientation( m_Pan.x, m_Pan.y );
        }
    }

    public void Reset()
    {
        m_Pan = m_PanBase;

        UpdateOrientation( m_PanBase.x, m_PanBase.y );
    }

    private void Awake()
    {
        // Set the default orientation
        m_PanBase = transform.localRotation.eulerAngles;

        // Create a Vec3 copy of the pan range (Vec3 <= Vec2)
        Vector3 panRange = m_PanRange;

        m_MinPanAngles = ( m_PanBase - panRange );
        m_MaxPanAngles = ( m_PanBase + panRange );

        Debug.Log("Camera (" + name + ") Orientation(" + m_PanBase.ToString() + ") Min(" + m_MinPanAngles.ToString() + ") Max(" + m_MaxPanAngles.ToString() + ")" );

        Reset();
    }

    private void Update()
    {
    }

    private void UpdateOrientation(float x, float y)
    {
        Debug.Log( "Camera (" + name + ") UpdateOrientation(" + x.ToString() + ", " + y.ToString() + ")" );

        //transform.Rotate( new Vector3( x, y, 0 ) );
        //float X = transform.rotation.eulerAngles.x;
        //float Y = transform.rotation.eulerAngles.y;
        //transform.rotation = Quaternion.Euler( X, Y, 0 );
        transform.localRotation = Quaternion.Euler( x, y, 0 );
    }

    // The base (default) local orientation of the camera
    private Vector3 m_PanBase;

    // The current local orientation of the camera
    private Vector3 m_Pan = new Vector3();

    // The minimum boundary local angles
    private Vector3 m_MinPanAngles;

    // The maximum boundary local angles
    private Vector3 m_MaxPanAngles;
}
