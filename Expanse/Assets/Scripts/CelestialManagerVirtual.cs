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
    public bool Init( Transform parentViewTransform )
    {
        if ( m_Initialized == false )
        {
            // For each planet we will try to create a virtual version
            List<CelestialBody> celestialBodies = CelestialManagerPhysical.Instance.GetCelestialBodies( CelestialBody.CelestialType.Planet );

            foreach ( CelestialBody celestialBody in celestialBodies )
            {
                CelestialVirtual virtualBody = CelestialVirtual.Create( celestialBody );

                m_CelestialBodies.Add( virtualBody.ID, virtualBody );

                virtualBody.transform.parent = parentViewTransform;

                UpdatePosition( virtualBody );
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

            CelestialVirtual closestPlanet = closestBody as CelestialVirtual;

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
        CelestialVirtual planet = body as CelestialVirtual;

        if ( null != planet )
        {
            CelestialBody celestialBody = CelestialManagerPhysical.Instance.GetCelestialBody( planet.ParentID );

            if ( null != celestialBody )
            {
                // Set the virtual position to match the real planet's
                planet.Position = celestialBody.Position;
            }
            else
            {
                Debug.LogWarning( "Unable to update position of " + body.name );
            }
        }
    }

    private bool m_AutoScale = false;
}
