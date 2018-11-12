using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

// A planetary position reference for accuracy
// http://www.theplanetstoday.com/

public class CelestialManager : MonoBehaviour
{
    public Canvas m_Canvas = null;

    public CelestialCamera m_Camera = null;

    public float m_CloseRange = 1e-05f;
    public float m_FarRange = 200.0f;

    // Scale control will call this to perform auto scale updates
    public void GetSelectedBody()
    {

    }

    public CelestialBody GetCelestialBody( string name )
    {
        foreach ( KeyValuePair<UInt32, CelestialBody> body in m_CelestialBodies )
        {
            if ( body.Value.name.ToLower() == name.ToLower() )
            {
                return body.Value;
            }
        }

        return null;
    }

    public CelestialBody GetCelestialBody( UInt32 celestialID )
    {
        CelestialBody body;
        if( m_CelestialBodies.TryGetValue(celestialID, out body) )
        {
            return body;
        }

        return null;
    }

    public int GetPlanetCount() { return m_Planets.Count; }

    public CelestialPlanet GetPlanet( int planetIndex )
    {
        if(planetIndex < m_Planets.Count)
        {
            return m_Planets[ planetIndex ];
        }

        return null;
    }

    // Called to set the celestial scale of all bodies
    public void SetScale( float scale )
    {
        foreach( KeyValuePair<UInt32, CelestialBody> celestialBody in m_CelestialBodies )
        {
            celestialBody.Value.Scale = scale;
        }
    }

    public void SetAutoScale( bool enabled )
    {
        if( enabled == false )
        {
            SetScale( 1.0f );
        }
        m_AutoScale = enabled;
    }

    public List<CelestialBody> GetClosestBodies( int count, CelestialBody.CelestialType types, Vector3 position )
    {
        List<CelestialBody> bodyList = new List<CelestialBody>();
        List<float> distanceList = new List<float>();

        if ( count > 0 )
        {
            foreach ( KeyValuePair<UInt32, CelestialBody> body in m_CelestialBodies )
            {
                if ( ( body.Value.GetCelestialType() & types ) != CelestialBody.CelestialType.Invalid )
                {
                    float distance = ( body.Value.transform.position - position ).sqrMagnitude;

                    int index = 0;
                    for ( ; index < distanceList.Count; ++index )
                    {
                        if ( distance < distanceList[ index ] )
                        {
                            break;
                        }
                    }
                    distanceList.Insert( index, distance );
                    bodyList.Insert( index, body.Value );
                }
            }

            if ( count < bodyList.Count )
            {
                bodyList.RemoveRange( count, bodyList.Count - count );
            }
        }

        return bodyList;
    }

    // Use this for initialization
    public void Init ()
    {
        DirectoryInfo info = new DirectoryInfo( Application.dataPath + "/StreamingAssets/Config/CelestialBodies/" );
        FileInfo[] fileInfo = info.GetFiles( "*.xml");

        foreach ( FileInfo file in fileInfo )
        {
            // Load
            CelestialBody newBody = CelestialBody.Create( file );

            if ( newBody != null )
            {
                m_CelestialBodies.Add( newBody.GetCelestialID(), newBody );

                // Specific processing and storage based on celestial type
                if ( newBody is CelestialPlanet )
                {
                    m_Planets.Add( newBody as CelestialPlanet );
                }

                Debug.Log( file );
            }
        }

        // Calculate the JD corresponding to 1976 - July - 20, 12:00 UT.
        //DateTime desiredTime = new DateTime( 1976, 7, 20, 12, 0, 0 );
        //DateTime desiredTime = new DateTime( 2018, 2, 26, 12, 0, 0 );
        //DateTime desiredTime = new DateTime( 2000, 1, 1, 0, 0, 0 );
        DateTime desiredTime = DateTime.Now;

        double julianDate = PlanetPosition.GetJulianDate( desiredTime );

        foreach( CelestialPlanet planet in m_Planets )
        {
            planet.UpdatePosition( julianDate );
        }
    }

    public CelestialCamera SetActiveCamera( CelestialCamera camera )
    {
        CelestialCamera oldCamera = m_Camera;

        m_Camera = camera;

        if( null != m_Camera )
        {
            // Setup the start position of the camera as opposite Mars
            // and a 20 degree inclination, looking towards Sol
            CelestialBody mars = GetCelestialBody( "Mars" );

            // Since Sol is at origin, the Mars' position is essentially the vector Sol => Mar
            Vector3 position = ( mars != null ) ? mars.gameObject.transform.position : new Vector3( 100, 200, 100 );

            // Determine the position above Mars that would be 20 degrees
            float height = (float)( Math.Tan( 20 * GlobalConstants.DegreesToRadians ) * position.magnitude );

            position.y += height;

            m_Camera.transform.position = Quaternion.AngleAxis( 180, Vector3.up ) * position;
            m_Camera.transform.LookAt( Vector3.zero );
        }
        else
        {
            // Camera must be setup in the editor
            Debug.LogError( "No camera was assigned to Celestial Manager" );
        }

        return oldCamera;
    }

    public static CelestialManager CreateInstance()
    {
        if ( null == m_Instance )
        {
            GameObject gameObject = new GameObject();

            if ( null != gameObject )
            {
                gameObject.name = "Celestial Manager";

                m_Instance = gameObject.AddComponent<CelestialManager>();

                if( null != m_Instance )
                {
                    m_Instance.Init();
                }
            }
        }

        return m_Instance;
    }

    public static CelestialManager GetInstance() { return m_Instance;  }

    private void Update()
    {
        if( m_AutoScale && m_Camera != null )
        {
            // Find the closest body to the camera and adjust the scale
            float closestDistance = float.MaxValue;
            foreach ( KeyValuePair<UInt32, CelestialBody> celestialBody in m_CelestialBodies )
            {
                Vector3 vector = m_Camera.transform.position - celestialBody.Value.transform.position;

                if( vector.magnitude < closestDistance )
                {
                    closestDistance = vector.magnitude;
                }               
            }

            if( closestDistance < float.MaxValue )
            {
                if( closestDistance >= m_FarRange )
                {
                    SetScale( 1000.0f );
                }
                else if( closestDistance <= m_CloseRange )
                {
                    SetScale( 1.0f );
                }
                else
                {
                    float rangeScalar = ( closestDistance - m_CloseRange ) / ( m_FarRange - m_CloseRange );
                    SetScale( Math.Max(1.0f, rangeScalar * 1000) );
                }
            }
        }
    }

    private Dictionary<UInt32, CelestialBody> m_CelestialBodies = new Dictionary<UInt32, CelestialBody>();
    private List<CelestialPlanet> m_Planets = new List<CelestialPlanet>();

    private bool m_AutoScale = false;

    private static CelestialManager m_Instance = null;
}
