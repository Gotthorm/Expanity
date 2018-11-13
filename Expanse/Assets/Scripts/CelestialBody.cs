using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    public enum CelestialType : UInt16
    {
        Invalid = 0,
        Planet = 1,
        Moon = 2,
        Asteroid = 4,
        Ship = 8,
        Unidentified = 16
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

    public void SetCamera( CelestialCamera camera )
    {
        m_Camera = camera;
    }

    public bool GetIsVisible() { return GetComponent<Renderer>().isVisible; }

    public virtual CelestialType GetCelestialType() { return CelestialType.Invalid; }

    public static CelestialBody Create( FileInfo file )
    {
        //Debug.Log( configPath );

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
                        CelestialPlanet newPlanet = gameObject.AddComponent<CelestialPlanet>();

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

        //Debug.LogError( "Failed to create celestial body: " + name );

        return null;
    }

    public UInt32 GetCelestialID() { return m_CelestialID; }

    public virtual bool Initialize( CelestialBodyLoader loader )
    {
        List<int> intList = null;
        if ( false == loader.GetData( m_MaximumScaleLabel, ref intList ) )
        {
            Debug.LogError( "Error" );
            return false;
        }
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

    #region Private Interface

    private void Awake()
    {
        m_CelestialID = m_CelestialIDGenerator++;
    }

    private float m_Scale = 1.0f;

    // The base scale at multiplier value 1
    private float m_InitialScale = 1.0f;

    private uint m_MaximumScaleMultiplier = 1000;

    private float m_RadiusInKM = 0;
    private float m_VelocityInKMS = 0.0f;

    private UInt32 m_CelestialID = 0;

    private uint m_VisualUnitsScale = 100;

    private const string m_MaximumScaleLabel = "MaximumScale";

    private static CelestialCamera m_Camera = null;
    private static UInt32 m_CelestialIDGenerator = 1;

    #endregion
}
