using UnityEngine;

public class SpaceShipExternalCamera : MonoBehaviour
{
    public uint m_MaxZoom = 200;
    public Vector2 m_PanRange = new Vector2( 90.0f, 90.0f );
    public Vector2 m_PanCenter = new Vector2( 0.0f, 0.0f );

    public uint Zoom { get; set; }
    public Vector2 Pan { get; set; }

    public void Reset()
    {
    }

    private void Start()
    {
    }

    private void Update()
    {
    }
}
