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
    //public string GetConfigPath() { return m_ConfigPath; }

    // Use this for initialization
    public void Init( string configPath )
    {
        if ( m_Initialized == false )
        {
            //m_ConfigPath = configPath;

            DirectoryInfo info = new DirectoryInfo( configPath );
            FileInfo[] fileInfo = info.GetFiles( "*.xml" );

            GameObject celestialBodyParent = new GameObject( "Celestial Bodies" );

            foreach ( FileInfo file in fileInfo )
            {
                // Load
                CelestialBody newBody = CelestialBody.Create( file );

                if ( newBody != null )
                {
                    // Temp hack to keep everything easier to see for now
                    //float celestialScale = 100.0f;
                    //newBody.transform.localScale = new Vector3( celestialScale, celestialScale, celestialScale );
                    if ( newBody.transform.parent == null )
                    {
                        newBody.transform.parent = celestialBodyParent.transform;
                    }

                    m_CelestialBodies.Add( newBody.ID, newBody );

                    Debug.Log( file );
                }
                else
                {
                    Debug.LogWarning( "Celestial Manager failed to load: " + file.FullName );
                }
            }

            // Calculate the JD corresponding to 1976 - July - 20, 12:00 UT.
            //DateTime desiredTimeTest = new DateTime( 1976, 7, 20, 12, 0, 0 );
            //double julianDateTest = PlanetPositionUtility.GetJulianDate( desiredTimeTest );

            // Calculate the JD corresponding to 1968 - December - 12, 12:00 UT.
            //DateTime desiredTime = new DateTime( 1968, 12, 24, 10, 0, 0 );

            //DateTime desiredTime = new DateTime( 2018, 2, 26, 12, 0, 0 );
            //DateTime desiredTime = new DateTime( 2000, 1, 1, 0, 0, 0 );
            DateTime desiredTime = DateTime.Now;

            double julianDate = PlanetPositionUtility.GetJulianDate( desiredTime );

            List<CelestialBody> planets = GetCelestialBodies( CelestialBody.CelestialType.Planet );

            foreach ( CelestialBody body in planets )
            {
                CelestialPlanet planet = body as CelestialPlanet;

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
        foreach( KeyValuePair<uint, CelestialBody> celestialBodyRecord in m_CelestialBodies )
        {
            CelestialBody celestialBody = celestialBodyRecord.Value;

            CelestialVector3 position = new CelestialVector3();
            double scale;
            GetScaledPosition( basePosition, celestialBody.Position, out position, out scale );
            float radius = (float)( scale * celestialBody.Radius );
            float diameter = 2.0f * radius;
            celestialBody.transform.localScale = new Vector3( diameter, diameter, diameter );

            celestialBody.transform.localPosition = (Vector3)position;
        }
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

    // Give the planets position in real data and get back the scaled version
    private void GetScaledPosition( CelestialVector3 basePosition, CelestialVector3 celestialPosition, out CelestialVector3 scaledPosition, out double scale )
    {
        scale = 1.0;

        scaledPosition = celestialPosition - basePosition;

        double distance = scaledPosition.Length();

        double _cosfov2 = 1.0 - System.Math.Cos( m_FieldOfView * 0.5 );

        double f = m_FarClipPlane * _cosfov2;
        double p = 0.75 * f;

        if ( distance > p )
        {
            double s = 1.0 - System.Math.Exp( -p / ( distance - p ) );
            double dist = p + ( f - p ) * s;
            scale = dist / distance;
            scaledPosition = scaledPosition / distance * dist;
        }
    }

    //private string m_ConfigPath = "";

    private float m_FieldOfView = 60.0f;
    private float m_FarClipPlane = 3000.0f;

    private static CelestialManagerPhysical m_Instance = null;
}
