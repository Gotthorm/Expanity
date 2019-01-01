using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProximityControlPanel : MonoBehaviour
{
    //public Camera m_Camera = null;
    public bool m_AutoDefault = true;
    public TacticalView m_TacticalView = null;
    public List<ProximityObject> m_TextBoxList = new List<ProximityObject>();

    // Called directly from the Auto Scale Button
    public void ToggleAutoScale()
    {
        m_AutoDefault = !m_AutoDefault;

        if ( m_TacticalView != null )
        {
            m_TacticalView.SetAutoScale( m_AutoDefault );
        }
    }

    public void ToggleFilter_Planets()
    {
        m_CelestialFilter ^= CelestialBody.CelestialType.Planet;
    }
    public void ToggleFilter_Moons()
    {
        m_CelestialFilter ^= CelestialBody.CelestialType.Moon;
    }
    public void ToggleFilter_Asteroids()
    {
        m_CelestialFilter ^= CelestialBody.CelestialType.Asteroid;
    }
    public void ToggleFilter_Ships()
    {
        m_CelestialFilter ^= CelestialBody.CelestialType.Ship;
    }
    public void ToggleFilter_Unidentified()
    {
        m_CelestialFilter ^= CelestialBody.CelestialType.Unidentified;
    }

    public void SelectProximityObject( ProximityObject proximityObject, bool lookAtTarget )
    {
        // Deselect old object
        if ( m_SelectedProximityObject != null )
        {
            m_SelectedProximityObject.SetSelected( false );
            m_SelectedCelestialID = 0;
        }

        // Select new object
        m_SelectedProximityObject = proximityObject;

        if ( m_SelectedProximityObject != null )
        {
            m_SelectedProximityObject.SetSelected( true );
            m_SelectedCelestialID = m_SelectedProximityObject.GetCelestialID();

            //CelestialBody body = CelestialManager.GetInstance().GetCelestialBody( m_SelectedCelestialID );
            if ( null != m_TacticalView )
            {
                m_TacticalView.SetSelected( m_SelectedCelestialID, lookAtTarget );
            }
        }
    }

    public void SelectProximityObject( uint celestialID )
    {
        ProximityObject proximityObject = null;

        foreach ( ProximityObject proximityObjectCandidate in m_TextBoxList )
        {
            if( proximityObjectCandidate.GetCelestialID() == celestialID)
            {
                proximityObject = proximityObjectCandidate;
                break;
            }
        }

        SelectProximityObject( proximityObject, false );
    }

    public void TargetProximityObject( ProximityObject proximityObject )
    {
        // Target the object
        if ( m_SelectedProximityObject != null )
        {
            //CelestialBody body = CelestialManager.GetInstance().GetCelestialBody( m_SelectedProximityObject.GetCelestialID() );

            if ( null != m_TacticalView )
            {
                m_TacticalView.SetTarget( m_SelectedProximityObject.GetCelestialID() );
                //CelestialManager.GetInstance().m_Camera.SetTargetedObject( body );
            }
        }
    }

    public bool CelestialTypeIsActive( CelestialBody.CelestialType type )
    {
        return ( ( type & m_CelestialFilter ) != CelestialBody.CelestialType.Invalid );
    }

    private void Awake()
    {
        foreach ( ProximityObject proximityObject in m_TextBoxList )
        {
            m_CurrentCelestialBodies.Add( proximityObject, null );
        }
    }

    // Use this for initialization
    private void Start()
    {
        if ( m_TacticalView != null )
        {
            m_TacticalView.SetAutoScale( m_AutoDefault );
        }
    }
	
	// Update is called once per frame
	private void Update()
    {
        if ( m_TacticalView != null )
        {
            Vector3 cameraPosition = m_TacticalView.GetCameraPosition();

            int entryCount = m_TextBoxList.Count;

            List<CelestialBody> bodyList = m_TacticalView.GetClosestBodies( entryCount, m_CelestialFilter, cameraPosition );

            for ( int index = 1; index <= entryCount; ++index )
            {
                ProximityObject proximityObject = m_TextBoxList[ index - 1 ];

                CelestialBody body = ( bodyList.Count >= index ) ? bodyList[ index - 1 ] : null;
                bool selected = ( m_SelectedCelestialID != 0 && proximityObject.GetCelestialID() == m_SelectedCelestialID ) ? true : false;

                proximityObject.Set( body, cameraPosition, selected );
                m_CurrentCelestialBodies[ proximityObject ] = body;
            }
        }
	}

    private ProximityObject m_SelectedProximityObject = null;
    private uint m_SelectedCelestialID = 0;
    private CelestialBody.CelestialType m_CelestialFilter = CelestialBody.CelestialType.Invalid;
    private Dictionary<ProximityObject, CelestialBody> m_CurrentCelestialBodies = new Dictionary<ProximityObject, CelestialBody>();
}
