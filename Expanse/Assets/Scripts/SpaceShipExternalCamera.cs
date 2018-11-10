using UnityEngine;

public class SpaceShipExternalCamera : MonoBehaviour
{
    public uint m_MaxZoom = 200;
    public Vector2 m_PanRange = new Vector2( 90.0f, 90.0f );
    //public Vector2 m_PanCenter = new Vector2( 0.0f, 0.0f );

    public uint GetZoom()
    {
        return m_ZoomCurrent;
    }

    public void ChangeZoom( int zoomDelta )
    {
        int newZoom = (int)m_ZoomCurrent + zoomDelta;

        if( newZoom <= 1 )
        {
            newZoom = 1;
        }
        else if ( newZoom > (int)m_MaxZoom )
        {
            newZoom = (int)m_MaxZoom;
        }

        UpdateZoom( newZoom );
    }

    public Vector2 Pan
    {
        get
        {
            return new Vector2( m_PanCurrent.x, m_PanCurrent.y );
        }

        set
        {
            Debug.Log( "Camera (" + name + ") Pan(" + value.ToString() + ")" );

            // Transfer the pan x value
            m_PanCurrent.x += value.x;
            m_PanCurrent.x = Mathf.Min( m_PanCurrent.x, m_MaxPanAngles.x );
            m_PanCurrent.x = Mathf.Max( m_PanCurrent.x, m_MinPanAngles.x );

            // Transfer the pan y value
            m_PanCurrent.y += value.y;
            m_PanCurrent.y = Mathf.Min( m_PanCurrent.y, m_MaxPanAngles.y );
            m_PanCurrent.y = Mathf.Max( m_PanCurrent.y, m_MinPanAngles.y );

            UpdateOrientation( m_PanCurrent.x, m_PanCurrent.y );
        }
    }

    public void Reset()
    {
        m_PanCurrent = m_PanBase;

        UpdateOrientation( m_PanBase.x, m_PanBase.y );

        UpdateZoom( 1 );
    }

    private void Awake()
    {
        // Set the default orientation
        m_PanBase = transform.localRotation.eulerAngles;

        // Create a Vec3 copy of the pan range (Vec3 <= Vec2)
        Vector3 panRange = m_PanRange;

        m_MinPanAngles = ( m_PanBase - panRange );
        m_MaxPanAngles = ( m_PanBase + panRange );

        Camera camera = GetComponent<Camera>();
        if( camera != null )
        {
            m_FieldOfViewBase = camera.fieldOfView;
        }

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

    private void UpdateZoom(int zoom)
    {
        m_ZoomCurrent = (uint)zoom;

        Camera camera = GetComponent<Camera>();
        if( camera != null )
        {
            camera.fieldOfView = m_FieldOfViewBase / m_ZoomCurrent;
        }
    }

    // The base (default) local orientation of the camera
    private Vector3 m_PanBase;

    // The current local orientation of the camera
    private Vector3 m_PanCurrent = new Vector3();

    // The minimum boundary local angles
    private Vector3 m_MinPanAngles;

    // The maximum boundary local angles
    private Vector3 m_MaxPanAngles;

    private float m_FieldOfViewBase = 60.0f;

    private uint m_ZoomCurrent = 1;
}
