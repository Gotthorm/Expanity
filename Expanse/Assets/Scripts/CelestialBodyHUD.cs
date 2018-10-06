using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CelestialBodyHUD : MonoBehaviour
{
    // Offset the anchoring position in the object's local space
    public Vector3 m_LocalOffset;

    // Offset the anchoring position in canvas space
    public Vector3 m_ScreenOffset;

    // Cached references to the child objects
    public Text m_TitleLabel = null;
    public Text m_InfoLabel = null;
    public Text m_InfoText = null;
    public Image m_PointerImage = null;

    public void SetOwner( CelestialBody owner )
    {
        m_ObjectToFollow = owner.transform;
    }

    //public void SetParent( Canvas parent )
    //{
    //    //m_ParentCanvas = parent;
    //}

    //public void SetFartyPants( GameObject parentPanel )
    //{
    //    //m_FartyPants = parentPanel;
    //}

    public void SetCamera( Camera camera )
    {
        m_Camera = camera;
    }

    public void UpdateDisplayScale( uint scale )
    {

    }

    public void UpdateDisplayDistance( uint distance )
    {

    }

    // Use this for initialization
    private void Start ()
    {
        if ( null != transform.parent )
        {
            m_ParentRectTransform = transform.parent.GetComponent<RectTransform>();
            //transform.SetParent( m_ParentCanvas.transform, false );

            //transform.SetParent( m_FartyPants.transform );
        }
        else
        {
            Debug.LogError( "Parent canvas not set for UI Anchor" );
        }

        if ( null != m_ObjectToFollow )
        {
            Text textObject = this.GetComponentInChildren<Text>();
            textObject.text = m_ObjectToFollow.gameObject.name;
        }
        else
        {
            Debug.LogError( "Object to follow not set for UI Anchor" );
        }
    }

    private void LateUpdate()
    {
        if ( null != m_ParentRectTransform && null != m_ObjectToFollow && m_Camera != null )
        {
            // Translate our anchored position into world space.
            Vector3 worldPoint = m_ObjectToFollow.TransformPoint( m_LocalOffset );

            // Translate the world position into viewport space.
            Vector3 viewportPoint = m_Camera.WorldToViewportPoint( worldPoint );

            // Convert the viewport to account for the camera viewport not occupying the entire canvas
            viewportPoint.x = viewportPoint.x * m_Camera.rect.width + m_Camera.rect.x;
            viewportPoint.y = viewportPoint.y * m_Camera.rect.height + m_Camera.rect.y;

            // Canvas local coordinates are relative to its center, 
            // so we offset by half. We also discard the depth.
            viewportPoint -= 0.5f * Vector3.one;
            viewportPoint.z = 0.0f;

            // Scale our position by the canvas size, 
            // so we line up regardless of resolution & canvas scaling.
            Rect rect = m_ParentRectTransform.rect;

            viewportPoint.x *= rect.width;
            viewportPoint.y *= rect.height;

            // Add the canvas space offset and apply the new position.
            transform.localPosition = viewportPoint + m_ScreenOffset;
        }
    }

    // Both of these references must be set to a non null value before the first update (start)
    private Transform m_ObjectToFollow = null;
    //private Canvas m_ParentCanvas = null;
    private Camera m_Camera = null;

    private RectTransform m_ParentRectTransform = null;
}
