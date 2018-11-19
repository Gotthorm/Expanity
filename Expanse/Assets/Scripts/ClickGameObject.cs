using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickGameObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    public void OnPointerClick( PointerEventData eventData )
    {
        if ( eventData.clickCount == 2 )
        {
            Debug.Log( "ClickGameObject double click: " + eventData.pointerCurrentRaycast.gameObject.name );

            //Camera.main.GetComponent<CelestialCamera>().SetTargetedObject( this );
        }
        else if ( eventData.clickCount == 1 )
        {
            Debug.Log( "ClickGameObject single click: " + eventData.pointerCurrentRaycast.gameObject.name );

            //Select();

            //Camera.main.GetComponent<CelestialCamera>().SetSelectedObject( this );
        }
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        Camera.main.GetComponent<CelestialCamera>().DisableClickMissDetectionForThisFrame();
    }

    public void Select()
    {
        SetSelected( true );
    }

    public void Unselect()
    {
        SetSelected( false );
    }

    private void SetSelected( bool selected )
    {
        Color currentColor = selected ? Color.yellow : Color.white;

        gameObject.GetComponent<MeshRenderer>().material.color = currentColor;
    }
}
