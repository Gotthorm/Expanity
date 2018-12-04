using System.Collections.Generic;
using UnityEngine;

public abstract class CelestialManager
{
    public CelestialBody GetCelestialBody( string name )
    {
        if ( 0 < name.Length )
        {
            foreach ( KeyValuePair<uint, CelestialBody> celestialBody in m_CelestialBodies )
            {
                if ( celestialBody.Value.name.ToLower() == name.ToLower() )
                {
                    return celestialBody.Value;
                }
            }
        }

        return null;
    }

    public CelestialBody GetCelestialBody( uint celestialID )
    {
        CelestialBody body;
        if ( m_CelestialBodies.TryGetValue( celestialID, out body ) )
        {
            return body;
        }

        return null;
    }

    public List<CelestialBody> GetCelestialBodies( CelestialBody.CelestialType celestialType )
    {
        List<CelestialBody> celestialBodyList = new List<CelestialBody>();

        foreach ( KeyValuePair<uint, CelestialBody> celestialBody in m_CelestialBodies )
        {
            if( CelestialBody.CelestialType.Invalid != ( celestialBody.Value.Type & celestialType ) )
            {
                celestialBodyList.Add( celestialBody.Value );
            }
        }

        return celestialBodyList;
    }

    public CelestialBody GetClosestCelestialBody( CelestialBody.CelestialType celestialType, Vector3 position )
    {
        List<CelestialBody> celestialBodyList = GetClosestCelestialBodies( celestialType, 1, position );

        return ( 0 < celestialBodyList.Count ) ? celestialBodyList[ 0 ] : null;
    }

    public List<CelestialBody> GetClosestCelestialBodies( CelestialBody.CelestialType celestialType, int maxResults, Vector3 position )
    {
        List<CelestialBody> resultsList = new List<CelestialBody>();
        List<float> distanceList = new List<float>();

        if ( maxResults > 0 )
        {
            foreach ( KeyValuePair<uint, CelestialBody> celestialBody in m_CelestialBodies )
            {
                if ( ( celestialBody.Value.Type & celestialType ) != CelestialBody.CelestialType.Invalid )
                {
                    float distance = ( celestialBody.Value.transform.position - position ).sqrMagnitude;

                    int index = 0;
                    for ( ; index < distanceList.Count; ++index )
                    {
                        if ( distance < distanceList[ index ] )
                        {
                            break;
                        }
                    }
                    distanceList.Insert( index, distance );
                    resultsList.Insert( index, celestialBody.Value );
                }
            }

            if ( maxResults < resultsList.Count )
            {
                resultsList.RemoveRange( maxResults, resultsList.Count - maxResults );
            }
        }

        return resultsList;
    }

    // Need this?
    //public int GetCelestialCount() { return m_CelestialBodies.Count; }

    protected bool m_Initialized = false;

    protected Dictionary<uint, CelestialBody> m_CelestialBodies = new Dictionary<uint, CelestialBody>();
}

