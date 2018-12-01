using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    [Tooltip( "A mouse speed scalar for how fast the camera will move" )]
    public float m_MoveSpeed = 5.0f;

    [Tooltip( "A mouse speed scalar for how fast the camera will rotate" )]
    public float m_RotateSpeed = 3.5f;

    private void Update()
    {
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

            UpdateFreeView( mouseX, mouseY );
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

    private void UpdateFreeView( float mouseX, float mouseY )
    {
        transform.Rotate( new Vector3( mouseY, mouseX, 0 ) );
        float X = transform.rotation.eulerAngles.x;
        float Y = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler( X, Y, 0 );

        if ( Input.GetKey( KeyCode.D ) )
        {
            transform.position = transform.position + ( transform.right * m_MoveSpeed );
        }
        if ( Input.GetKey( KeyCode.W ) )
        {
            transform.position = transform.position + ( transform.forward * m_MoveSpeed );
        }
        if ( Input.GetKey( KeyCode.S ) )
        {
            transform.position = transform.position + ( transform.forward * -m_MoveSpeed );
        }
        if ( Input.GetKey( KeyCode.A ) )
        {
            transform.position = transform.position + ( transform.right * -m_MoveSpeed );
        }
    }
}
