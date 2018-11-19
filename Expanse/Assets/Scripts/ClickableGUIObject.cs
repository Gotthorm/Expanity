using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableGUIObject : MonoBehaviour, IPointerClickHandler
{
    public GameObject ParentToNotify = null;

    public delegate void MyDelegate( GameObject eventOwner );
    public MyDelegate myDelegate = null;

    public void OnPointerClick( PointerEventData eventData )
    {
        Debug.Log( "ClickableGUIObject: " + eventData.pointerCurrentRaycast.gameObject.name );

        myDelegate?.Invoke( eventData.pointerCurrentRaycast.gameObject );
    }
}
