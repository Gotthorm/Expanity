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
    public CelestialIcon m_Icon = null;

    public void SetOwner( CelestialBody owner )
    {
        m_Owner = owner;
    }

    public CelestialBody GetOwner() { return m_Owner; }

    public void SetCamera( Camera camera )
    {
        m_Camera = camera;
    }

    public void SetSelected( bool selected )
    {
        Color currentColor = selected ? Color.yellow : Color.red;

        m_TitleLabel.color = currentColor;
        m_Icon.Color = currentColor;
        m_InfoLabel.color = currentColor;
        m_InfoText.color = currentColor;

        m_InfoLabel.enabled = selected;
        m_InfoText.enabled = selected;
    }

    public bool GetIsVisible()
    {
        if ( null != m_Owner && m_Camera != null )
        {
            // Translate our anchored position into world space.
            Vector3 worldPoint = m_Owner.transform.TransformPoint( m_LocalOffset );

            // Translate the world position into viewport space.
            Vector3 viewportPoint = m_Camera.WorldToViewportPoint( worldPoint );

            return (viewportPoint.z >= 0);
        }

        return false;
    }

    // Use this for initialization
    private void Start ()
    {
        if ( null != transform.parent )
        {
            m_ParentRectTransform = transform.parent.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError( "Parent canvas not set for UI Anchor" );
        }

        if ( null != m_Owner )
        {
            Text textObject = this.GetComponentInChildren<Text>();
            textObject.text = m_Owner.transform.gameObject.name;
        }
        else
        {
            Debug.LogError( "Object to follow not set for UI Anchor" );
        }

        SetSelected( false );
    }

    private void LateUpdate()
    {
        if ( null != m_ParentRectTransform && null != m_Owner && m_Camera != null )
        {
            // Translate our anchored position into world space.
            Vector3 worldPoint = m_Owner.transform.TransformPoint( m_LocalOffset );

            // Translate the world position into viewport space.
            Vector3 viewportPoint = m_Camera.WorldToViewportPoint( worldPoint );

            // Convert the viewport to account for the camera viewport not occupying the entire canvas
            viewportPoint.x = viewportPoint.x * m_Camera.rect.width + m_Camera.rect.x;
            viewportPoint.y = viewportPoint.y * m_Camera.rect.height + m_Camera.rect.y;

            //bool visible = viewportPoint.z >= 0;

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

            m_Icon.UpdateState( m_Owner, m_Camera );

            //// If the celestial body is visible, ensure the HUD element is active and updated
            //// Otherwise disable it 
            //if ( visible )
            //{
            //    gameObject.SetActive( true );
            //}
            //else
            //{
            //    gameObject.SetActive( false );
            //}
        }
    }

    // Both of these references must be set to a non null value before the first update (start)
    private CelestialBody m_Owner = null;
    private Camera m_Camera = null;

    private RectTransform m_ParentRectTransform = null;
}
