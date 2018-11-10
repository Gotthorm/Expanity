using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExternalShipViewController : MonoBehaviour
{
    public ExternalShipView m_DefaultView = null;

    public Text m_LabelName = null;
    public Text m_LabelZoom = null;

    [Tooltip( "A mouse speed scalar for how fast the camera will pan" )]
    public float m_PanSpeed = 3.5f;

    [Tooltip( "A mouse wheel scalar for how fast the camera will zoom" )]
    public float m_ZoomSpeed = 20.0f;

    public void ResetCamera()
    {
        if( m_CurrentView != null )
        {
            if( m_CurrentView.m_ViewCamera != null )
            {
                m_CurrentView.m_ViewCamera.Reset();
            }
        }
    }

    public static ExternalShipViewController GetInstance() { return m_Instance; }

    public void SetView( ExternalShipView view )
    {
        if ( m_CurrentView != view )
        {
            if ( m_CurrentView != null )
            {
                m_CurrentView.Select( false );
            }

            m_CurrentView = view;
        }

        //m_ViewCamera = view.m_ViewCamera;

        RawImage currentRawImage = GetComponent<RawImage>();

        if ( currentRawImage != null )
        {
            RawImage newRawImage = view.GetComponent<RawImage>();

            if ( newRawImage != null )
            {
                currentRawImage.texture = newRawImage.texture;
            }
        }

        if ( m_LabelName != null )
        {
            m_LabelName.text = view.GetLabel();
        }

        if ( m_LabelZoom != null )
        {
            SpaceShipExternalCamera camera = m_CurrentView.m_ViewCamera;

            if ( camera != null )
            {
                m_LabelZoom.text = "Zoom: x" + camera.GetZoom().ToString();
            }
        }
    }

    private void Awake()
    {
        m_Instance = this;

        SetView( m_DefaultView );
    }

    // Update is called once per frame
    private void Update()
    {
        if ( MouseScreenCheck() )
        {
            if ( m_DefaultView != null )
            {
                SpaceShipExternalCamera camera = m_CurrentView.m_ViewCamera;

                if ( camera != null )
                {
                    // Pan control
                    if ( Input.GetMouseButton( 1 ) )
                    {
                        float mouseX = Input.GetAxis( "Mouse X" ) * m_PanSpeed;
                        float mouseY = Input.GetAxis( "Mouse Y" ) * -m_PanSpeed;

                        if ( Cursor.lockState != CursorLockMode.Locked )
                        {
                            Cursor.lockState = CursorLockMode.Locked;

                            if ( Cursor.visible )
                            {
                                Cursor.visible = false;
                            }
                        }
                        else
                        {
                            // Add the new rotation delta to the current pan value
                            camera.Pan = new Vector2( mouseY, mouseX );

                            //UpdateFreeView( camera );
                        }

                        m_MouseBase = new Vector2( mouseX, mouseY );
                    }
                    else
                    {
                        if ( Cursor.lockState == CursorLockMode.Locked )
                        {
                            Cursor.lockState = CursorLockMode.None;
                        }
                        if ( Cursor.visible == false )
                        {
                            Cursor.visible = true;
                        }
                    }

                    // Zoom control
                    uint currentZoom = camera.GetZoom();

                    float mouseWheelValue = Input.GetAxis( "Mouse ScrollWheel" );
                    if ( 0.0f != mouseWheelValue )
                    {
                        int zoomChange = (int)(mouseWheelValue * m_ZoomSpeed);

                        camera.ChangeZoom(zoomChange);

                        currentZoom = camera.GetZoom();
                    }

                    if( m_OldZoom != currentZoom )
                    {
                        m_LabelZoom.text = "Zoom: x" + currentZoom.ToString();

                        m_OldZoom = currentZoom;
                    }
                }
            }

        }
    }

    private bool MouseScreenCheck()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        if ( rectTransform != null )
        {
            if ( RectTransformUtility.RectangleContainsScreenPoint( rectTransform, Input.mousePosition, null ) )
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateFreeView( SpaceShipExternalCamera camera )
    {
        float mouseX = Input.GetAxis( "Mouse X" ) * m_PanSpeed;
        float mouseY = Input.GetAxis( "Mouse Y" ) * -m_PanSpeed;

        // Add the new rotation delta to the current pan value
        camera.Pan += new Vector2( mouseY, mouseX );
    }

    private ExternalShipView m_CurrentView = null;
    private static ExternalShipViewController m_Instance = null;
    private Vector2 m_MouseBase = new Vector2();
    private uint m_OldZoom = 0;
}
