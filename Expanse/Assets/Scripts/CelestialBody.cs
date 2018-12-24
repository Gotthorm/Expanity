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
            }

            m_Scale = value;

            float celestialScale = m_InitialScale * value;
            transform.localScale = new Vector3( celestialScale, celestialScale, celestialScale );
        }
    }

    public double Radius
    {
        get
        {
            return m_RadiusInKM;
        }
    }

    public bool Orbit
    {
        get
        {
            return m_HasOrbit;
        }
    }

    public CelestialVector3 Position
    {
        get
        {
            return m_PositionInKM;
        }

        set
        {
            SetPosition( value );
        }
    }

    // This is the radius in game units
    public float CelestialRadius
    {
        get
        {
            return (float)m_RadiusInKM / GlobalConstants.CelestialUnit;
        }
    }

    public float Velocity
    {
        get
        {
            return m_VelocityInKMS;
        }
    }

    public uint ID
    {
        get { return m_CelestialID; }
    }

    public CelestialType Type
    {
        get { return m_CelestialType; }
    }

    public bool GetIsVisible() { return GetComponent<Renderer>().isVisible; }

    public static CelestialBody Create( FileInfo file )
    {
        CelestialBodyLoader loader = new CelestialBodyLoader();

        if ( loader.Load( file ) )
        {
            UnityEngine.Object prefab = Resources.Load( CelestialBodyLoader.PrefabPath + loader.m_Name, typeof( GameObject ) );

            if ( null != prefab )
            {
                GameObject gameObject = UnityEngine.Object.Instantiate( prefab ) as GameObject;

                if ( null != gameObject )
                {
                    gameObject.name = loader.m_Name;

                    if( loader.m_Type == "Planet" )
                    {
                        CelestialPlanet newPlanet = gameObject.AddComponent<CelestialPlanet>();

                        if ( null != newPlanet )
                        {
                            if ( newPlanet.Initialize( loader ) )
                            {
                                return newPlanet;
                            }
                        }
                    }
                    else if ( loader.m_Type == "Ship" )
                    {
                        CelestialShip newShip = gameObject.AddComponent<CelestialShip>();

                        if ( null != newShip )
                        {
                            if ( newShip.Initialize( loader ) )
                            {
                                return newShip;
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
                if ( ( body.Type & type ) != CelestialBody.CelestialType.Invalid )
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

    public virtual bool Initialize( CelestialBodyLoader loader )
    {
        // Has Orbit (optional)
        string boolList = null;
        if ( loader.GetData( m_OrbitFlagLabel, ref boolList ) )
        {
            m_HasOrbit = boolList.Length > 0;
        }

        if ( loader.m_Radius <= 0.0 )
        {
            Debug.LogError( "Error" );
            return false;
        }
        m_RadiusInKM = loader.m_Radius;

        // TODO: This is a sphere shaped calculation so will need to support other types eventually?
        // Initial scale is in game diameter (2 * radius)
        m_InitialScale = CelestialRadius * 2;

        Scale = 1;

        return true;
    }

    #region Protected Interface

    // This is the position in space using real units
    protected virtual void SetPosition( CelestialVector3 position )
    {
        m_PositionInKM = position;
        transform.localPosition = (Vector3)position;
    }

    protected CelestialType m_CelestialType = CelestialType.Invalid;

    protected bool m_HasOrbit = false;

    protected double m_RadiusInKM = 0;

    // The base scale at multiplier value 1
    protected float m_InitialScale = 1.0f;

    protected uint m_MaximumScaleMultiplier = 1;

    #endregion

    #region Private Interface

    private void Awake()
    {
        m_CelestialID = m_CelestialIDGenerator++;
    }

    private float m_Scale = 1.0f;

    private float m_VelocityInKMS = 0.0f;

    private CelestialVector3 m_PositionInKM = new CelestialVector3( 0.0, 0.0, 0.0 );

    private uint m_CelestialID = 0;

    //private uint m_VisualUnitsScale = 100;

    private const string m_OrbitFlagLabel = "Orbit";

    private static UInt32 m_CelestialIDGenerator = 1;

    #endregion
}
