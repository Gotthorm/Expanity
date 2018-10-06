using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ProximityObject : MonoBehaviour, IPointerClickHandler
{
    public ProximityControlPanel m_ParentControlPanel = null;
    public Text m_NameTextField = null;
    public Text m_DistanceTextField = null;
    public Byte m_SelectedAlpha = 0x80;

    public void Set( CelestialBody celestialBody, bool selected )
    {
        string distanceString = "";
        string name = "";

        if( celestialBody != null )
        {
            name = celestialBody.name;

            if ( m_ParentControlPanel != null && m_ParentControlPanel.m_Camera != null )
            {
                float distance = ( m_ParentControlPanel.m_Camera.transform.position - celestialBody.transform.position ).magnitude;
                distanceString = GlobalHelpers.MakeSpaceDistanceString( distance );
            }
        }
        if ( m_NameTextField != null )
        {
            m_NameTextField.text = name;
        }
        if ( m_DistanceTextField != null )
        {
            m_DistanceTextField.text = distanceString;
        }

        m_CelestialBody = celestialBody;

        SetSelected( selected );
    }

    public void SetSelected( bool selected )
    {
        Image image = GetComponent<Image>();

        if( image != null )
        {
            Color newColor = image.color;
            newColor.a = selected ? m_SelectedAlpha : 0x00;
            image.color = newColor;
        }
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        if ( eventData.clickCount == 2 )
        {
            //Debug.Log( "ProximityObject double click: " + m_NameTextField.text );

            if ( m_ParentControlPanel != null )
            {
                m_ParentControlPanel.TargetProximityObject( this );
            }
            //SetTargeted?.Invoke( eventData.pointerCurrentRaycast.gameObject );
        }
        else if ( eventData.clickCount == 1 )
        {
            //Debug.Log( "ProximityObject single click: " + m_NameTextField.text );

            if( m_ParentControlPanel != null )
            {
                m_ParentControlPanel.SelectProximityObject( this );
            }
            //SetSelected?.Invoke( eventData.pointerCurrentRaycast.gameObject );
        }

    }

    public UInt32 GetCelestialID() { return ( m_CelestialBody != null ) ? m_CelestialBody.GetCelestialID() : 0; }

    private CelestialBody m_CelestialBody = null;
}
