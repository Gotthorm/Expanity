using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalShipView : MonoBehaviour
{
    public Camera m_ViewCamera = null;

	// Use this for initialization
	private void Start ()
    {
	}
	
	// Update is called once per frame
	private void Update ()
    {
        bool active = GetComponentInParent<ScreenPanel>().Enabled;

        m_ViewCamera.GetComponent<Camera>().enabled = active;
    }

    private void ClickSelected( GameObject eventOwner )
    {
        Debug.Log( "Object click selected: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
    }

    private void ClickTargeted( GameObject eventOwner )
    {
        Debug.Log( "Object click targeted: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        //m_Camera.SetTargetedObject( this );

        // Notify the panel?
    }

    private void ClickDisableMiss( GameObject eventOwner )
    {
        Debug.Log( "Object click disable miss: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        //m_Camera.DisableClickMissDetectionForThisFrame();
    }

    private void ClickDrag( GameObject eventOwner )
    {
        Debug.Log( "Object click drag: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        //m_Camera.DragObject( this );
    }
}
