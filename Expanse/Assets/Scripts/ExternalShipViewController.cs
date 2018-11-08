﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExternalShipViewController : MonoBehaviour
{
    public ExternalShipView m_DefaultView = null;

    public Text m_LabelName = null;

    [Tooltip( "A mouse speed scalar for how fast the camera will rotate" )]
    public float m_RotateSpeed = 3.5f;

    public static ExternalShipViewController GetInstance() { return m_Instance; }

    public void SetView( ExternalShipView view )
    {
        if ( m_View != view )
        {
            if ( m_View != null )
            {
                m_View.Select( false );
            }

            m_View = view;
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
    }

    private void Awake()
    {
        m_Instance = this;

        SetView( m_DefaultView );
    }

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if ( MouseScreenCheck() )
        {
            if ( m_DefaultView != null )
            {
                SpaceShipExternalCamera camera = m_View.m_ViewCamera;

                if ( camera != null )
                {
                    if ( Input.GetMouseButton( 1 ) )
                    {
                        float mouseX = Input.GetAxis( "Mouse X" ) * m_RotateSpeed;
                        float mouseY = Input.GetAxis( "Mouse Y" ) * -m_RotateSpeed;

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
                            //camera.Pan = new Vector2( m_MouseBase.y - mouseY, m_MouseBase.x - mouseX );
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
                }
            }

        }
        //bool active = GetComponentInParent<ScreenPanel>().Enabled;

        //m_ViewCamera.GetComponent<Camera>().enabled = active;
        /*
         *
        if ( MouseScreenCheck() )
        {
            // Dragging motions are only active with the right mouse button depressed
            if ( Input.GetMouseButton( 1 ) )
            {
                if ( Cursor.lockState != CursorLockMode.Locked )
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                if ( Cursor.visible )
                {
                    Cursor.visible = false;
                }

                float mouseX = Input.GetAxis( "Mouse X" ) * m_RotateSpeed;
                float mouseY = Input.GetAxis( "Mouse Y" ) * -m_RotateSpeed;

                if ( m_Target == null )
                {
                    UpdateFreeView( mouseX, mouseY );
                    transform.Rotate( new Vector3( mouseY, mouseX, 0 ) );
                    float X = transform.rotation.eulerAngles.x;
                    float Y = transform.rotation.eulerAngles.y;
                    transform.rotation = Quaternion.Euler( X, Y, 0 );
                }
            }
            else
            {
                if ( Cursor.lockState == CursorLockMode.Locked )
                {
                    Cursor.lockState = CursorLockMode.None;
                }
                if( Cursor.visible == false )
                {
                    Cursor.visible = true;
                }
            }
         */
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
        float mouseX = Input.GetAxis( "Mouse X" ) * m_RotateSpeed;
        float mouseY = Input.GetAxis( "Mouse Y" ) * -m_RotateSpeed;

        // Add the new rotation delta to the current pan value
        camera.Pan += new Vector2( mouseY, mouseX );
    }

    private ExternalShipView m_View = null;
    //private SpaceShipExternalCamera m_ViewCamera = null;
    private static ExternalShipViewController m_Instance = null;
    private Vector2 m_MouseBase = new Vector2();
}
