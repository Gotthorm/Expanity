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
        CelestialManager.GetInstance().SetAutoScale( m_AutoDefault );
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
            m_SelectedCelestialBody = 0;
        }

        // Select new object
        m_SelectedProximityObject = proximityObject;

        if ( m_SelectedProximityObject != null )
        {
            m_SelectedProximityObject.SetSelected( true );
            m_SelectedCelestialBody = m_SelectedProximityObject.GetCelestialID();

            CelestialBody body = CelestialManager.GetInstance().GetCelestialBody( m_SelectedCelestialBody );
            CelestialManager.GetInstance().m_Camera.SetSelectedObject( body, lookAtTarget );

            if( m_TacticalView != null )
            {
                m_TacticalView.SetSelectedCelestial( body.GetCelestialID() );
            }
        }
    }

    public void SelectProximityObject( UInt32 celestialID )
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
            CelestialBody body = CelestialManager.GetInstance().GetCelestialBody( m_SelectedProximityObject.GetCelestialID() );
            CelestialManager.GetInstance().m_Camera.SetTargetedObject( body );
        }
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
        CelestialManager celestialManager = CelestialManager.GetInstance();

        if ( celestialManager != null )
        {
            celestialManager.SetAutoScale( m_AutoDefault );
        }
        else
        {
            Debug.LogError( "Celestial Manager does not exist!" );
        }
    }
	
	// Update is called once per frame
	private void Update()
    {
        Vector3 cameraPosition = CelestialManager.GetInstance().m_Camera.transform.position;

        int entryCount = m_TextBoxList.Count;

        List<CelestialBody> bodyList = CelestialManager.GetInstance().GetClosestBodies( entryCount, m_CelestialFilter, cameraPosition );

        for ( int index = 1; index <= entryCount; ++index )
        {
            ProximityObject proximityObject = m_TextBoxList[ index - 1 ];

            CelestialBody body = ( bodyList.Count >= index ) ? bodyList[ index - 1 ] : null;
            bool selected = ( m_SelectedCelestialBody != 0 && proximityObject.GetCelestialID() == m_SelectedCelestialBody ) ? true : false;

            proximityObject.Set( body, cameraPosition, selected );
            m_CurrentCelestialBodies[ proximityObject ] = body;
        }
	}

    private ProximityObject m_SelectedProximityObject = null;
    private UInt32 m_SelectedCelestialBody = 0;
    private CelestialBody.CelestialType m_CelestialFilter = CelestialBody.CelestialType.Invalid;
    private Dictionary<ProximityObject, CelestialBody> m_CurrentCelestialBodies = new Dictionary<ProximityObject, CelestialBody>();
}
