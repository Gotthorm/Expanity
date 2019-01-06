using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class CelestialManagerVirtual : CelestialManager
{
    public float m_CloseRange = 1e-05f;
    public float m_FarRange = 1000.0f;

    public void SetAutoScale( bool enabled )
    {
        if( enabled == false )
        {
            SetScale( 1.0f );
        }
        m_AutoScale = enabled;
    }

    // Called to set the celestial scale of all bodies
    public void SetScale( float scale )
    {
        foreach ( KeyValuePair<uint, CelestialBody> celestialBody in m_CelestialBodies )
        {
            celestialBody.Value.Scale = scale;
        }
    }

    // Use this for initialization
    public bool Init()
    {
        if( m_Initialized == false )
        {
            GameObject celestialBodyParent = new GameObject( "Virtual Celestial Bodies" );

            Dictionary<string, uint> celestialBodyIds = new Dictionary<string, uint>();

            // For each celestial body we will try to create a virtual version
            List<CelestialBody> celestialPhysicalBodies = CelestialManagerPhysical.Instance.GetCelestialBodies( CelestialBody.CelestialType.All );

            foreach( CelestialBody celestialPhysicalBody in celestialPhysicalBodies )
            {
                CelestialVirtual virtualBody = CelestialVirtual.Create( celestialPhysicalBody );

                m_CelestialBodies.Add( virtualBody.CelestialID, virtualBody );
                celestialBodyIds.Add( virtualBody.name, virtualBody.CelestialID );

                // For organization we parent all bodies to a dummy object
                virtualBody.transform.parent = celestialBodyParent.transform;

                UpdatePosition( virtualBody );
            }

            // Set up the celestial hierarchy now that all bodies have been instantiated
            List<CelestialBody> bodies = GetCelestialBodies( CelestialBody.CelestialType.All );

            foreach ( CelestialBody body in bodies )
            {
                if ( body.OrbitParentName.Length > 0 )
                {
                    uint bodyID;
                    if ( celestialBodyIds.TryGetValue( body.OrbitParentName, out bodyID ) )
                    {
                        CelestialBody parentBody;
                        if ( m_CelestialBodies.TryGetValue( bodyID, out parentBody ) )
                        {
                            body.OrbitParentID = parentBody.CelestialID;
                        }
                        else
                        {
                            Debug.LogError( "Failed to find orbit parent: " + bodyID.ToString() );
                        }
                    }
                    else
                    {
                        Debug.LogError( "Failed to find orbit parent: " + body.OrbitParentName );
                    }
                }
            }

            m_Initialized = true;
        }

        return m_Initialized;
    }

    public void Update( CelestialCamera celestialCamera )
    {
        foreach( KeyValuePair<uint, CelestialBody> celestialBodyRecord in m_CelestialBodies )
        {
            CelestialBody celestialBody = celestialBodyRecord.Value;

            UpdatePosition( celestialBody );
        }

        // Find the closest body to the camera and adjust the scale
        if ( m_AutoScale && celestialCamera != null )
        {
            CelestialBody closestBody = GetClosestCelestialBody( CelestialBody.CelestialType.Planet, celestialCamera.transform.position );

            CelestialVirtual closestPlanet = closestBody as CelestialVirtual;

            if ( null != closestPlanet )
            {
                float distance = ( celestialCamera.transform.position - closestPlanet.transform.position ).magnitude;

                if ( distance < float.MaxValue )
                {
                    if ( distance >= m_FarRange )
                    {
                        SetScale( 1000.0f );
                    }
                    else if ( distance <= m_CloseRange )
                    {
                        SetScale( 1.0f );
                    }
                    else
                    {
                        float rangeScalar = ( distance - m_CloseRange ) / ( m_FarRange - m_CloseRange );
                        SetScale( Math.Max( 1.0f, rangeScalar * 1000 ) );
                    }
                }
            }
        }
    }

    private void UpdatePosition( CelestialBody body )
    {
        CelestialVirtual virtualBody = body as CelestialVirtual;

        if ( null != virtualBody )
        {
            CelestialBody celestialBody = CelestialManagerPhysical.Instance.GetCelestialBody( virtualBody.OwnerID );

            if ( null != celestialBody )
            {
                CelestialPlanetoid planetoid = celestialBody as CelestialPlanetoid;

                if ( null != planetoid )
                {
                    CelestialVector3 position = planetoid.CalculatePosition( CelestialTime.Instance.Current );

                    virtualBody.LocalPosition = position;

                    if ( body.OrbitParentID != 0 )
                    {
                        CelestialBody parentBody = GetCelestialBody( body.OrbitParentID );

                        if ( parentBody != null )
                        {
                            position += parentBody.Position;
                        }
                    }

                    virtualBody.Position = position;
                }
                else
                {
                    // Set the virtual position to match the real planet's
                    virtualBody.LocalPosition = celestialBody.LocalPosition;
                    virtualBody.Position = celestialBody.Position;
                }
            }
            else
            {
                Debug.LogWarning( "Unable to update position of " + body.name );
            }
        }
    }

    private bool m_AutoScale = false;
}
