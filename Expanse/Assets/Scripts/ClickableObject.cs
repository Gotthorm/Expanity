using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour
{
    //public ProximityControlCamera ParentCamera;

    private void OnMouseDown()
    {
        if ( Input.GetMouseButtonDown( 0 ) )
        {
            Debug.Log( "Clickable Object: " + this.gameObject.name );

            // Set this object as active
            //if ( null != ParentCamera )
            //{
            //    ParentCamera.SetTarget( this.transform );
            //}
        }
    }
}
