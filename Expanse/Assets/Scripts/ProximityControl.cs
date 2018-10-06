using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProximityControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public OrbitCamera Camera = null;

	// Update is called once per frame
	void Update ()
    {
        if ( m_MousePointerActive )
        {
            float mouseWheelValue = Input.GetAxis( "Mouse ScrollWheel" );

            if ( 0.0f != mouseWheelValue )
            {
                if( null != this.Camera )
                {
                    float currentRange = Camera.Distance;
                    float currentChange = currentRange * mouseWheelValue;
                    Camera.Distance += currentChange;
                }
            }
        }
    }

    public void OnBeginDrag( PointerEventData eventData )
    {
        // Hide mouse pointer?
        //throw new System.NotImplementedException();
    }

    public void OnDrag( PointerEventData eventData )
    {
        Vector2 deltaXY = eventData.delta;

        float x = Camera.X + deltaXY.x;
        float y = Camera.Y - deltaXY.y;

        Camera.X = x;
        Camera.Y = y;
    }

    public void OnEndDrag( PointerEventData eventData )
    {
        // Unhide mouse pointer?
        //throw new System.NotImplementedException();
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        m_MousePointerActive = true;
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        m_MousePointerActive = false;
    }

    private bool m_MousePointerActive = false;
}
