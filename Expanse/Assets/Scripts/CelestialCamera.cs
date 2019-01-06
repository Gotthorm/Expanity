
#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

using UnityEngine;

public class CelestialCamera : MonoBehaviour
{
    [Tooltip( "A mouse speed scalar for how fast the camera will rotate" )]
    public float m_RotateSpeed = 3.5f;

    public float m_MoveSpeed = 5.0f;

    public float m_DragSpeed = 20.0f;

    [Tooltip( "The time in seconds it takes to rotate until facing a new target with movement" )]
    public float m_RotationTimeWithMovement = 2.5f;

    [Tooltip( "The time in seconds it takes to rotate until facing a new target without movement" )]
    public float m_RotationTimeWithoutMovement = 1.5f;

    [Tooltip( "The time in seconds it takes to move to a new target position" )]
    public float m_TransitionTranslationTime = 2.4f;

    [Tooltip( "The time in seconds to delay when initially selecting a target" )]
    public float m_SelectionRotationDelay = 0.2f;

    public float m_TargetPositionInclination = 20.0f;
    public float m_TargetPositionScale = 8.0f;

    public void SetSelectedObject( CelestialBody selectedGameObject, bool lookAtTarget )
    {
        if ( m_SelectedGameObject != selectedGameObject )
        {
            if ( m_SelectedGameObject != null )
            {
                //m_SelectedGameObject.Unselect();

                SetTarget( null, true );
            }

            m_SelectedGameObject = selectedGameObject;

            m_LookingAtTarget = false;
        }

        if ( m_SelectedGameObject != null )
        {
            //m_SelectedGameObject.Select();

            if ( lookAtTarget && m_LookingAtTarget != true )
            {
                // Look at but do not move
                SetTarget( selectedGameObject, false );

                m_LookingAtTarget = true;
            }
        }

        m_ClickMissDetected = false;
    }

    public void SetTargetedObject( CelestialBody targetedGameObject )
    {
        if ( null != targetedGameObject )
        {
            SetSelectedObject( targetedGameObject, true );
            SetTarget( targetedGameObject, true );
        }
        else
        {
            SetTarget( null, true );
        }
     }

    public void DisableClickMissDetectionForThisFrame()
    {
        Debug.Log( "DisableClickMissDetectionForThisFrame" );
        m_ClickMissDisabled = true;
    }

    public void DragObject( CelestialBody dragGameObject )
    {
        float mouseX = Input.GetAxis( "Mouse X" ) * -m_DragSpeed;
        float mouseY = Input.GetAxis( "Mouse Y" ) * -m_DragSpeed;

        // World position of dragged object
        Vector3 targetPosition = dragGameObject.transform.position;

        // Vector from the dragged object to the camera
        Camera camera = GetComponent<Camera>();
        Vector3 directionVector = targetPosition - camera.transform.position;

        Vector3 targetScreenPosition = camera.WorldToScreenPoint( targetPosition );
        targetScreenPosition.x += mouseX;
        targetScreenPosition.y += mouseY;
        Vector3 newTargetPosition = camera.ScreenToWorldPoint( targetScreenPosition );

        Vector3 worldPositionOffset = newTargetPosition - targetPosition;

        this.transform.position += worldPositionOffset;
    }

    private void Update()
    {
        // Test for an unselect event
        if ( Input.GetMouseButtonUp( 0 ) && m_ClickMissDetected )
        {
            Debug.Log( "Unselect Object" );
            SetSelectedObject( null, false );
        }

        // If the camera is still in transition then update it
        // Notice we do not allow any manual control while transitioning
        if ( m_TransitionRotationTime > m_InterpolationRotationTimer || m_TransitionTranslationTime > m_InterpolationTranslationTimer )
        {
            UpdateTargetedTransition();
        }
        // Test that the mouse cursor is in our window
        else
        {
            if ( null != m_Target )
            {
                UpdateTargetedView();
            }

            if ( MouseScreenCheck() )
            {
                // Dragging motions are only active with the right mouse button depressed
                if ( Input.GetMouseButton( 1 ) )
                {
                    //Debug.LogWarning("Mouse right clicked in view panel");

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
                    }
                    else
                    {
                        float distance = 0.0f;
                        if ( Input.GetKey( KeyCode.W ) )
                        {
                            distance -= m_MoveSpeed;
                        }
                        if ( Input.GetKey( KeyCode.S ) )
                        {
                            distance += m_MoveSpeed;
                        }

                        UpdateTargetedView( mouseX, mouseY, distance );
                    }
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

                    // Detect a mouse down miss
                    if ( Input.GetMouseButtonDown( 0 ) )
                    {
                        if ( m_ClickMissDisabled == false )
                        {
                            Debug.Log( "Click Miss Detected" );
                            m_ClickMissDetected = true;
                        }
                        else
                        {
                            Debug.Log( "Click Miss bypassed" );
                        }
                    }
                    else if ( m_Target != null )
                    {
                        float mouseWheelValue = Input.GetAxis( "Mouse ScrollWheel" );
                        if ( 0.0f != mouseWheelValue )
                        {
                            float distance = mouseWheelValue * -m_RotationDistance;
                            UpdateTargetedView( 0, 0, distance );
                        }
                    }
                    {
                        // We want to calculate a final drag distanced that is auto scaled by the current view.
                        // Therefore if you clicked on a planet very far away, the drag scalar would be huge
                        // I think I need to determine the closest visible object to the camera as base.

                        // Left mouse button drag
                        //Debug.Log("Left mouse button drag");

                        //float mouseX = Input.GetAxis( "Mouse X" ) * -m_DragSpeed;
                        //float mouseY = Input.GetAxis( "Mouse Y" ) * -m_DragSpeed;

                        //// Multiply mouseX by camera right vector
                        //Vector3 offsetX = this.transform.right * mouseX;

                        //// Multiply mouseY by camera up vector
                        //Vector3 offsetY = this.transform.up * mouseY;

                        //this.transform.position += (offsetX + offsetY);
                    }
                }
                m_ClickMissDisabled = false;
            }
        }

        // Update our distance to the closest celestial body
        // This is used by the auto scale
    }

    private void SetTarget( CelestialBody targetCelestial, bool allowMovement )
    {
        if ( targetCelestial != null )
        {
            // Cache the target transform
            m_Target = allowMovement ? targetCelestial.transform : null;

            Vector3 targetPosition;
            Vector3 originalTargetPosition = targetCelestial.transform.position;

            float rotationTimerStart = 0.0f;

            if ( allowMovement )
            {
                // Determine the current radius of the target 
                float currentTargetRadius = targetCelestial.CelestialRadius * targetCelestial.Scale;

                if( targetCelestial.Type == CelestialBody.CelestialType.Ship )
                {
                    currentTargetRadius = 0.01f;
                }

                // Based on the target's current radius we will calculate an "ideal" viewing position of the target
                targetPosition = originalTargetPosition;
                Vector3 vectorTargetToSource = transform.position - targetPosition;

                // Since we will apply an inclination, flatten the y
                vectorTargetToSource.y = 0.0f;

                // Scale and cache the current distance from the target object 
                m_RotationDistance = currentTargetRadius * m_TargetPositionScale;

                // Now we adjust the target distance by the tunable "RotationDistance"
                // With this our target position will be the desired view distance from the target
                targetPosition += ( vectorTargetToSource.normalized * m_RotationDistance );

                // Apply the inclination
                float height = (float)( System.Math.Tan( m_TargetPositionInclination * GlobalConstants.DegreesToRadians ) * m_RotationDistance );
                targetPosition.y += height;

                // Our target position may now have an incorrect distance and could be corrected here
                // When we applied the y above our view dustance mlikely changed

                // Cache the start and end positions for the position slerp
                m_SlerpPositionStart = transform.position;
                m_SlerpPositionEnd = targetPosition;

                // Kick of the position slerp
                m_InterpolationTranslationTimer = 0.0f;

                // Set the duration of the rotation slerp
                m_TransitionRotationTime = m_RotationTimeWithMovement;
            }
            else
            {
                targetPosition = transform.position;

                // Set the duration of the rotation slerp
                m_TransitionRotationTime = m_RotationTimeWithoutMovement;

                // Set the start of the rotation slerp to have a delay
                rotationTimerStart -= m_SelectionRotationDelay;
            }

            // Our target view will be from the camera position towards the target object
            // This will get updated as the camera moves
            m_TargetDirection = originalTargetPosition - targetPosition;
            m_TargetRotation = Quaternion.LookRotation( m_TargetDirection.normalized );
            m_CurrentRotation = transform.rotation;
            
            // Kick of the rotation slerp
            m_InterpolationRotationTimer = rotationTimerStart;
        }
        else
        {
            m_InterpolationRotationTimer = float.MaxValue;
            m_InterpolationTranslationTimer = float.MaxValue;
            m_Target = null;
        }
    }

    private void UpdateTargetedTransition()
    {
        float interpolationStep = Time.deltaTime;

        //bool positionChanged = false;

        if ( m_InterpolationTranslationTimer < m_TransitionTranslationTime )
        {
            m_InterpolationTranslationTimer += interpolationStep;

            // A progress percent from [0 <=> 1]
            float translationInterpolationProgress = m_InterpolationTranslationTimer / m_TransitionTranslationTime;

            //transform.position = m_SlerpPositionBase + Vector3.Slerp( m_SlerpPositionStart, m_SlerpPositionEnd, translationInterpolationProgress );
            //transform.position = Vector3.Slerp( transform.position, m_TargetPosition, translationInterpolationProgress );
            //transform.position = Vector3.Lerp( m_SlerpPositionStart, m_SlerpPositionEnd, translationInterpolationProgress );
            transform.position = Vector3.Lerp( m_SlerpPositionStart, m_SlerpPositionEnd, Mathf.SmoothStep(0, 1, translationInterpolationProgress ) );

            if ( 1.0f <= translationInterpolationProgress )
            {
                m_InterpolationTranslationTimer = float.MaxValue;
            }

            //positionChanged = true;
        }

        if ( m_InterpolationRotationTimer < m_TransitionRotationTime )
        {
            m_InterpolationRotationTimer += interpolationStep;

            // Do not actually start the slerp until the timer is greater than or equal to zero
            // This allows us to support having a delay which is used by SetSelectedObject 
            if ( m_InterpolationRotationTimer >= 0.0f )
            {
                // A progress percent from [0 <=> 1]
                float rotationInterpolationProgress = m_InterpolationRotationTimer / m_TransitionRotationTime;

                transform.rotation = Quaternion.Slerp( m_CurrentRotation, m_TargetRotation, Mathf.SmoothStep( 0, 1, rotationInterpolationProgress ) );

                //m_CurrentRotationX = transform.rotation.eulerAngles.y;
                //m_CurrentRotationY = transform.rotation.eulerAngles.x;

                if ( 1.0f <= rotationInterpolationProgress )
                {
                    m_InterpolationRotationTimer = float.MaxValue;
                }
            }
        }

#if false
        if( positionChanged )
        {
            if ( m_InterpolationRotationTimer >= m_TransitionRotationTime )
            {
                transform.LookAt( m_Target );
            }

            //Vector3 currentDirection = transform.forward;

            //float dotProduct = Vector3.Dot( m_TargetDirection.normalized, currentDirection.normalized );

            //Debug.Log( "DotProduct:" + dotProduct.ToString() );
         }
#endif
    }

    private void UpdateTargetedView( float mouseX, float mouseY, float distanceDelta )
    {
        m_RotationDistance += distanceDelta;

        // Clamp this shit?
        float X = transform.rotation.eulerAngles.y;
        float Y = transform.rotation.eulerAngles.x;

        Quaternion rotation = Quaternion.Euler( Y + mouseY, X + mouseX, 0 );

        Vector3 negDistance = new Vector3( 0.0f, 0.0f, -m_RotationDistance );
        Vector3 position = rotation * negDistance + m_Target.position;

        //transform.rotation = rotation;
        transform.position = position;

        transform.LookAt( m_Target );
    }

    private void UpdateTargetedView()
    {
        Quaternion rotation = transform.rotation;

        Vector3 negDistance = new Vector3( 0.0f, 0.0f, -m_RotationDistance );
        Vector3 position = rotation * negDistance + m_Target.position;

        transform.position = position;

        transform.LookAt( m_Target );
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

    private bool MouseScreenCheck()
    {
        CameraPanel viewPanel = GetComponent<CameraPanel>();
        if( viewPanel != null )
        {
            RectTransform rectTransform = viewPanel.m_ParentPanel.GetComponent<RectTransform>();

            if( rectTransform != null )
            {
                if ( RectTransformUtility.RectangleContainsScreenPoint( rectTransform, Input.mousePosition, null ) )
                {
                    return true;
                }
            }
        }

// Old method left for reference
// This used the entire screen.  The new method above uses only the actual view panel
//        if ( Input.mousePosition.x >= 0 && Input.mousePosition.y >= 0 )
//        {
//#if UNITY_EDITOR
//            if ( Input.mousePosition.x < Handles.GetMainGameViewSize().x - 1 && Input.mousePosition.y < Handles.GetMainGameViewSize().y - 1 )
//#else
//            if ( Input.mousePosition.x < Screen.width - 1 && Input.mousePosition.y < Screen.height - 1 )
//#endif
//            {
//                return true;
//            }
//        }

        return false;
    }

    private CelestialBody m_SelectedGameObject = null;

    private bool m_ClickMissDetected = false;
    private bool m_ClickMissDisabled = false;

    private float m_RotationDistance = 1000.0f;

    private float m_InterpolationRotationTimer = float.MaxValue;
    private float m_InterpolationTranslationTimer = float.MaxValue;

    private Quaternion m_TargetRotation;
    private Quaternion m_CurrentRotation;
    private Vector3 m_TargetDirection;

    //private Vector3 m_TargetPosition;
    //private Vector3 m_CurrentPosition;

    //private Vector3 m_SlerpPositionBase;
    private Vector3 m_SlerpPositionStart;
    private Vector3 m_SlerpPositionEnd;

    private Transform m_Target = null;

    private float m_TransitionRotationTime = 0.0f;

    private bool m_LookingAtTarget = false;

    //private float m_CurrentEulerRotationX = 0.0f;
    //private float m_CurrentEulerRotationY = 0.0f;
}
