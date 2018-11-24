using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public enum CelestialType : ushort
    {
        Invalid = 0,
        Planet = 1,
        Moon = 2,
        Asteroid = 4,
        Ship = 8,
        Unidentified = 16,

        All = ushort.MaxValue
    }

    public float Scale
    {
        get
        {
            return m_Scale;
        }

        set
        {
            if ( value > m_MaximumScaleMultiplier )
            {
                value = m_MaximumScaleMultiplier;

                // Create some sort of visual to indicate body was capped at Max scale?
            }

            m_Scale = value;

            float celestialScale = m_InitialScale * value;
            transform.localScale = new Vector3( celestialScale, celestialScale, celestialScale );
        }
    }

    public float Radius
    {
        get
        {
            return m_RadiusInKM;
        }
    }

    // This is the radius in game units
    public float CelestialRadius
    {
        get
        {
            return m_RadiusInKM / GlobalConstants.CelestialUnit;
        }
    }

    public float Velocity
    {
        get
        {
            return m_VelocityInKMS;
        }
    }

    public bool Orbit
    {
        get
        {
            return m_HasOrbit;
        }
    }

    public bool GetIsVisible() { return GetComponent<Renderer>().isVisible; }

    public virtual CelestialType GetCelestialType() { return CelestialType.Invalid; }

    public static CelestialBody Create( FileInfo file )
    {
        CelestialBodyLoader loader = new CelestialBodyLoader();

        if ( loader.Load( file ) )
        {
            UnityEngine.Object prefab = Resources.Load( loader.m_PrefabDataPath, typeof( GameObject ) );

            if ( null != prefab )
            {
                GameObject gameObject = UnityEngine.Object.Instantiate( prefab ) as GameObject;

                if ( null != gameObject )
                {
                    gameObject.name = loader.m_Name;

                    if( loader.m_Type == "Planet" )
                    {
                        CelestialPlanetPhysical newPlanet = gameObject.AddComponent<CelestialPlanetPhysical>();

                        if ( null != newPlanet )
                        {
                            if ( newPlanet.Initialize( loader ) )
                            {
                                return newPlanet;
                            }
                        }
                    }
                    else if ( loader.m_Type == "VirtualPlanet" )
                    {
                        CelestialPlanetVirtual newPlanet = gameObject.AddComponent<CelestialPlanetVirtual>();

                        if ( null != newPlanet )
                        {
                            if ( newPlanet.Initialize( loader ) )
                            {
                                return newPlanet;
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    public static List<CelestialBody> GetClosestBodiesToPosition( List<CelestialBody> inputList, int maxResults, CelestialBody.CelestialType type, Vector3 position )
    {
        List<CelestialBody> resultsList = new List<CelestialBody>();
        List<float> distanceList = new List<float>();

        if ( maxResults > 0 )
        {
            foreach ( CelestialBody body in inputList )
            {
                if ( ( body.GetCelestialType() & type ) != CelestialBody.CelestialType.Invalid )
                {
                    float distance = ( body.transform.position - position ).sqrMagnitude;

                    int index = 0;
                    for ( ; index < distanceList.Count; ++index )
                    {
                        if ( distance < distanceList[ index ] )
                        {
                            break;
                        }
                    }
                    distanceList.Insert( index, distance );
                    resultsList.Insert( index, body );
                }
            }

            if ( maxResults < resultsList.Count )
            {
                resultsList.RemoveRange( maxResults, resultsList.Count - maxResults );
            }
        }

        return resultsList;
    }

    public uint GetCelestialID() { return m_CelestialID; }

    public virtual bool Initialize( CelestialBodyLoader loader )
    {
        // Maximum Scale( optional)
        List<int> intList = null;
        if ( loader.GetData( m_MaximumScaleLabel, ref intList ) )
        {
            if ( intList.Count != 1 )
            {
                Debug.LogError( "Error" );
                return false;
            }
            if ( 1 > intList[ 0 ] )
            {
                Debug.LogError( "Error" );
                return false;
            }
            m_MaximumScaleMultiplier = (uint)intList[ 0 ];
        }

        if ( loader.m_Radius <= 0.0 )
        {
            Debug.LogError( "Error" );
            return false;
        }
        m_RadiusInKM = loader.m_Radius;

        // Has Orbit (optional)
        List<bool> boolList = null;
        if ( loader.GetData( m_OrbitFlagLabel, ref boolList ) )
        {
            if ( boolList.Count != 1 )
            {
                Debug.LogError( "Error" );
                return false;
            }
            m_HasOrbit = boolList[ 0 ];
        }

        // TODO: This is a sphere shaped calculation so will need to support other types eventually?
        // Initial scale is in game diameter (2 * radius)
        m_InitialScale = CelestialRadius * 2;

        Scale = 1;

        return true;
    }

    #region Private Interface

    private void Awake()
    {
        m_CelestialID = m_CelestialIDGenerator++;
    }

    private float m_Scale = 1.0f;

    // The base scale at multiplier value 1
    private float m_InitialScale = 1.0f;

    private uint m_MaximumScaleMultiplier = 1;

    private float m_RadiusInKM = 0;
    private float m_VelocityInKMS = 0.0f;

    private uint m_CelestialID = 0;

    private uint m_VisualUnitsScale = 100;

    private bool m_HasOrbit = false;

    private const string m_MaximumScaleLabel = "MaximumScale";
    private const string m_OrbitFlagLabel = "Orbit";

    private static UInt32 m_CelestialIDGenerator = 1;

    #endregion
}
