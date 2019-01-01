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

            Dictionary<string, uint> celestialBodyIds = new Dictionary<string, uint>();

            DirectoryInfo info = new DirectoryInfo( configPath );
            FileInfo[] fileInfo = info.GetFiles( "*.xml" );

            foreach ( FileInfo file in fileInfo )
            {
                // Load
                CelestialBody newBody = CelestialBody.Create( file );

                if ( newBody != null )
                {
                    // Temp hack to keep everything easier to see for now
                    //float celestialScale = 100.0f;
                    //newBody.transform.localScale = new Vector3( celestialScale, celestialScale, celestialScale );
 
                    m_CelestialBodies.Add( newBody.CelestialID, newBody );
                    celestialBodyIds.Add( newBody.name, newBody.CelestialID );

                    Debug.Log( file );
                }
                else
                {
                    Debug.LogWarning( "Celestial Manager failed to load: " + file.FullName );
                }
            }

            GameObject celestialBodyParent = new GameObject( "Celestial Bodies" );

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

                // For organization we parent all bodies to a dummy object
                body.transform.parent = celestialBodyParent.transform;
            }

            UpdatePositions();

            m_Initialized = true;
        }
    }

    public void UpdatePositions()
    {
        // Calculate the JD corresponding to 1976 - July - 20, 12:00 UT.
        // DateTime desiredTime = new DateTime( 1976, 7, 20, 12, 0, 0 );

        // Calculate the JD corresponding to 1968 - December - 12, 12:00 UT.
        // DateTime desiredTime = new DateTime( 1968, 12, 24, 10, 0, 0 );

        // Calculate the JD corresponding to now
        DateTime desiredTime = DateTime.Now;

        double julianDate = PlanetPositionUtility.GetJulianDate( desiredTime );

        List<CelestialBody> bodies = GetCelestialBodies( CelestialBody.CelestialType.Planet | CelestialBody.CelestialType.Moon );

        foreach ( CelestialBody body in bodies )
        {
            body.UpdatePosition( julianDate );
        }
    }

    public void UpdateDynamicScale( CelestialVector3 basePosition )
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
