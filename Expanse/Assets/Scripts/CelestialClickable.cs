using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CelestialClickable : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IDragHandler
{
    public delegate void CallbackDelegate( GameObject eventOwner );
    public CallbackDelegate SetSelected = null;
    public CallbackDelegate SetTargeted = null;
    public CallbackDelegate DisableClickMiss = null;
    public CallbackDelegate MouseDrag = null;

    public void OnPointerClick( PointerEventData eventData )
    {
        if ( eventData.clickCount == 2 )
        {
            //Debug.Log( "ClickableGameObject double click: " + eventData.pointerCurrentRaycast.gameObject.name );

            SetTargeted?.Invoke( eventData.pointerCurrentRaycast.gameObject );
        }
        else if ( eventData.clickCount == 1 )
        {
            //Debug.Log( "ClickableGameObject single click: " + eventData.pointerCurrentRaycast.gameObject.name );

            SetSelected?.Invoke( eventData.pointerCurrentRaycast.gameObject );
        }
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        DisableClickMiss?.Invoke( eventData.pointerCurrentRaycast.gameObject );
    }

    public void OnDrag( PointerEventData eventData )
    {
        MouseDrag?.Invoke( this.gameObject );
    }
}
