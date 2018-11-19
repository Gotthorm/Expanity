using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CelestialBodyClickable : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    public void OnPointerClick( PointerEventData eventData )
    {
        if ( eventData.clickCount == 2 )
        {
            Debug.Log( "CelestialBodyClickable double click: " + eventData.pointerCurrentRaycast.gameObject.name );

            Camera.main.GetComponent<CelestialCamera>().SetTargetedObject( this.gameObject.GetComponent<CelestialBody>() );
        }
        else if ( eventData.clickCount == 1 )
        {
            Debug.Log( "CelestialBodyClickable single click: " + eventData.pointerCurrentRaycast.gameObject.name );

            //Select();

            Camera.main.GetComponent<CelestialCamera>().SetSelectedObject( this.gameObject.GetComponent<CelestialBody>(), false );
        }
    }

    public void OnPointerDown( PointerEventData eventData )
    {
        Camera.main.GetComponent<CelestialCamera>().DisableClickMissDetectionForThisFrame();
    }
}
