using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualManager : CelestialManagerBase
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
    public bool Init( Transform parent )
    {
        if ( m_Initialized == false )
        {
            string configPath = CelestialManager.Instance.GetConfigPath();

            // For each planet we will try to create a virtual version
            List<CelestialBody> realCelestialPlanets = CelestialManager.Instance.GetCelestialBodies( CelestialBody.CelestialType.Planet );

            foreach ( CelestialBody realCelestialBody in realCelestialPlanets )
            {
                CelestialPlanet realCelestialPlanet = realCelestialBody as CelestialPlanet;

                if ( null != realCelestialPlanet )
                {
                    string prefabPath = configPath + "../VirtualCelestialBodies/" + realCelestialPlanet.name + "_Virtual.xml";

                    FileInfo file = new FileInfo( prefabPath );

                    if ( file.Exists )
                    {
                        CelestialBody virtualCelestialBody = CelestialBody.Create( file );

                        if ( virtualCelestialBody != null )
                        {
                            m_CelestialBodies.Add( virtualCelestialBody.GetCelestialID(), virtualCelestialBody );

                            VirtualCelestialPlanet virtualCelestialPlanet = virtualCelestialBody as VirtualCelestialPlanet;

                            if ( null != virtualCelestialPlanet )
                            {
                                virtualCelestialPlanet.transform.parent = parent;

                                virtualCelestialPlanet.ParentPlanetID = realCelestialPlanet.GetCelestialID();

                                virtualCelestialPlanet.Scale = 1000;

                                UpdatePosition( virtualCelestialPlanet );
                            }
                            else
                            {
                                Debug.LogError( "Virtual Manager failed to add valid VirtualCelestialPlanet: " + file.FullName );
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning( "Virtual Manager failed to find: " + file.FullName );
                    }
                }
                else
                {
                    Debug.LogError( "Encountered invalid planet object: " + realCelestialBody.name );
                }
            }

            m_Initialized = true;
        }

        return m_Initialized;
    }

    public void Update( CelestialCamera camera )
    {
        foreach( KeyValuePair<uint, CelestialBody> celestialBody in m_CelestialBodies )
        {
            UpdatePosition( celestialBody.Value );
        }

        // Find the closest body to the camera and adjust the scale
        if ( m_AutoScale && camera != null )
        {
            CelestialBody closestBody = GetClosestCelestialBody( CelestialBody.CelestialType.Planet, camera.transform.position );

            VirtualCelestialPlanet closestPlanet = closestBody as VirtualCelestialPlanet;

            if ( null != closestPlanet )
            {
                float distance = ( camera.transform.position - closestPlanet.transform.position ).magnitude;

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
        CelestialBody celestialBody = null;

        VirtualCelestialPlanet planet = body as VirtualCelestialPlanet;

        if ( null != planet )
        {
            celestialBody = CelestialManager.Instance.GetCelestialBody( planet.ParentPlanetID );
        }

        if ( null != celestialBody )
        {
            // Set the virtual position to match the real planet's
            planet.transform.position = celestialBody.transform.position;
        }
        else
        {
            Debug.LogWarning( "Unable to update position of " + body .name );
        }
    }

    private bool m_AutoScale = false;
}
