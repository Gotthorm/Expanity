using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnchor : MonoBehaviour
{
    // Assign this to the object you want the health bar to track:
    public Transform ObjectToFollow;

    // This lets us tweak the anchoring position in the object's local space
    // eg. if you want the bar to appear above the unit's head.
    public Vector3 LocalOffset;

    // This lets us tweak the anchoring position in our canvas space
    // eg. if we want the UI to sit off to the right on our screen.
    public Vector3 ScreenOffset;

    // Cached reference to the canvas containing this object.
    // We'll use this to position it correctly
    public Canvas ParentCanvas;

    // Cache a reference to our parent canvas, so we don't repeatedly search for it.
    void Start()
    {
        //_myCanvas = ParentCanvas.GetComponent<RectTransform>();

        if ( null != ParentCanvas )
        {
            m_CanvasRectTransform = ParentCanvas.GetComponent<RectTransform>();
            transform.SetParent( ParentCanvas.transform, false );
        }
        else
        {
            Debug.LogError( "Parent canvas not set for UI Anchor" );
        }

        if ( null != ObjectToFollow )
        {
            //Debug.Log( ObjectToFollow.gameObject.name );
            Text textObject = this.GetComponentInChildren<Text>();
            textObject.text = ObjectToFollow.gameObject.name;
        }
        else
        {
            Debug.LogError( "Object to follow not set for UI Anchor" );
        }
    }

    // Use LateUpdate to apply the UI follow after all movement & animation
    // for the frame has been applied, so we don't lag behind the unit.
    void LateUpdate()
    {
        if ( null != m_CanvasRectTransform && null != ObjectToFollow )
        {
            // Translate our anchored position into world space.
            Vector3 worldPoint = ObjectToFollow.TransformPoint( LocalOffset );

            // Translate the world position into viewport space.
            Vector3 viewportPoint = Camera.main.WorldToViewportPoint( worldPoint );

            // Canvas local coordinates are relative to its center, 
            // so we offset by half. We also discard the depth.
            viewportPoint -= 0.5f * Vector3.one;
            viewportPoint.z = 0.0f;

            // Scale our position by the canvas size, 
            // so we line up regardless of resolution & canvas scaling.
            Rect rect = m_CanvasRectTransform.rect;
            viewportPoint.x *= rect.width;
            viewportPoint.y *= rect.height;

            // Add the canvas space offset and apply the new position.
            transform.localPosition = viewportPoint + ScreenOffset;
        }
    }

    private RectTransform m_CanvasRectTransform = null;
}
