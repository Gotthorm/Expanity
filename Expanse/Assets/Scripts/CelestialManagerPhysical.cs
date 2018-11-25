using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

// A planetary position reference for accuracy
// http://www.theplanetstoday.com/

public class CelestialManagerPhysical : CelestialManager
{
    public string GetConfigPath() { return m_ConfigPath; }

    // Use this for initialization
    public void Init( string configPath )
    {
        if ( m_Initialized == false )
        {
            m_ConfigPath = configPath;

            DirectoryInfo info = new DirectoryInfo( m_ConfigPath );
            FileInfo[] fileInfo = info.GetFiles( "*.xml" );

            foreach ( FileInfo file in fileInfo )
            {
                // Load
                CelestialBody newBody = CelestialBody.Create( file );

                if ( newBody != null )
                {
                    // Temp hack to keep everything easier to see for now
                    float celestialScale = 100.0f;
                    newBody.transform.localScale = new Vector3( celestialScale, celestialScale, celestialScale );

                    m_CelestialBodies.Add( newBody.GetCelestialID(), newBody );

                    Debug.Log( file );
                }
                else
                {
                    Debug.LogWarning( "Celestial Manager failed to load: " + file.FullName );
                }
            }

            // Calculate the JD corresponding to 1976 - July - 20, 12:00 UT.
            //DateTime desiredTime = new DateTime( 1976, 7, 20, 12, 0, 0 );
            //DateTime desiredTime = new DateTime( 2018, 2, 26, 12, 0, 0 );
            //DateTime desiredTime = new DateTime( 2000, 1, 1, 0, 0, 0 );
            DateTime desiredTime = DateTime.Now;

            double julianDate = PlanetPositionUtility.GetJulianDate( desiredTime );

            List<CelestialBody> planets = GetCelestialBodies( CelestialBody.CelestialType.Planet );

            foreach ( CelestialBody body in planets )
            {
                CelestialPlanetPhysical planet = body as CelestialPlanetPhysical;

                if ( null != planet )
                {
                    planet.UpdatePosition( julianDate );
                }
            }

            m_Initialized = true;
        }
    }

    public void Update( CelestialVector3 basePosition )
    {
        // Update the dynamic scalar values of all Celestial Bodies using given base position
    }

    public static CelestialManagerPhysical Instance
    {
        get
        {
            if( null == m_Instance )
            {
                m_Instance = new CelestialManagerPhysical();
            }
            return m_Instance;
        }
    }

    private string m_ConfigPath = "";

    private static CelestialManagerPhysical m_Instance = null;
}
