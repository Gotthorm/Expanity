using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExternalShipViewController : MonoBehaviour
{
    public static ExternalShipViewController GetInstance() { return m_Instance; }

    public void SetView( ExternalShipView view )
    {
        if( m_View != view )
        {
            if( m_View != null )
            {
                m_View.Select( false );
            }

            m_View = view;
        }

        //m_ViewCamera = view.m_ViewCamera;

        RawImage currentRawImage = GetComponent<RawImage>();

        if( currentRawImage != null )
        {
            RawImage newRawImage = view.GetComponent<RawImage>();

            if( newRawImage != null )
            {
                currentRawImage.texture = newRawImage.texture;
            }
        }
    }

    private void Awake()
    {
        m_Instance = this;
    }

    // Use this for initialization
    private void Start ()
    {
	}
	
	// Update is called once per frame
	private void Update ()
    {
        //bool active = GetComponentInParent<ScreenPanel>().Enabled;

        //m_ViewCamera.GetComponent<Camera>().enabled = active;
    }

    private ExternalShipView m_View = null;
    //private SpaceShipExternalCamera m_ViewCamera = null;
    private static ExternalShipViewController m_Instance = null;
}
