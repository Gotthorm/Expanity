using System;
using System.Collections.Generic;
using UnityEngine;

public class CelestialShip : CelestialBody
{
    public override CelestialType GetCelestialType() { return CelestialType.Ship; }

    public override bool Initialize( CelestialBodyLoader loader )
    {
        if ( base.Initialize( loader ) )
        {
            return true;
        }

        return false;
    }

    public SpaceShipExternalCamera GetExternalCamera( string name )
    {
        SpaceShipExternalCamera camera = null;

        if( m_ExternalCameras.TryGetValue( name.GetHashCode(), out camera ) )
        {
            return camera;
        }

        return null;
    }

    private void Awake()
    {
        // Find all of the thrusters in this ship
        List<Thruster> thrusterList = new List<Thruster>();
        CollectThrusters( this.gameObject, thrusterList );

        //GameObject gameObject = new GameObject( "Control System", typeof( ControlSystem ) );
        //gameObject.transform.SetParent( this.gameObject.transform );

        m_ControlSystem = gameObject.GetComponent<ControlSystem>();
        if ( m_ControlSystem != null )
        {
            m_ControlSystem.Initialize( this, thrusterList );
        }

        CollectCameras();
    }

    // Helper method for collecting all of the thruster game objects attached to this ship
    private void CollectThrusters( GameObject gameObject, List<Thruster> thrustersList )
    {
        Thruster childThruster = gameObject.GetComponent<Thruster>();

        if ( null != childThruster )
        {
            // Found a thruster
            thrustersList.Add( childThruster );
        }

        foreach ( Transform child in gameObject.transform )
        {
            CollectThrusters( child.gameObject, thrustersList );
        }
    }

    private void CollectCameras()
    {
        SpaceShipExternalCamera[] cameraList = this.gameObject.GetComponentsInChildren<SpaceShipExternalCamera>();

        foreach( SpaceShipExternalCamera camera in cameraList )
        {
            m_ExternalCameras.Add( camera.name.GetHashCode(), camera );
        }
    }

    // The control system that encapsulates all controls
    private ControlSystem m_ControlSystem = null;

    private Dictionary<int, SpaceShipExternalCamera> m_ExternalCameras = new Dictionary<int, SpaceShipExternalCamera>();
}
