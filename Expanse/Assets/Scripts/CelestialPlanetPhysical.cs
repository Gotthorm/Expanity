using System;
using System.Collections.Generic;
using UnityEngine;

public class CelestialPlanetPhysical : CelestialPlanet
{
    public void UpdatePosition( double julianDate )
    {
        double radiusVector;
        double eclipticalLongitude;
        double eclipticLatitude;

        PlanetPositionUtility.GetHeliocentricEclipticalCoordinates( m_MeanEquinoxData, julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );

        CelestialVector3 position = GetPositionFromHeliocentricEclipticalCoordinates( radiusVector, eclipticalLongitude, eclipticLatitude );

        SetPosition( position );
    }

    public List<Vector3> GetOrbit( double currentJulianDate, double resolution )
    {
        // One day in JD is equal to 1.0

        // Calculating the orbital period around Sol
        // orbitalPeriodInYears = Sqrt( averageAU * averageAU * averageAU );
        double averageAUFromSun = m_MeanEquinoxData[ (int)PlanetPositionUtility.OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ][ 0 ];

        double orbitalPeriodInDays = Math.Sqrt( Math.Pow( averageAUFromSun, 3 ) ) * 365;

        double julianDaysPerPosition = ( orbitalPeriodInDays / resolution );

        List<Vector3> orbit = new List<Vector3>();

        // Start at 1/2 orbit in the past
        double julianDate = currentJulianDate - ( orbitalPeriodInDays * 0.5 );

        double radiusVector;
        double eclipticalLongitude;
        double eclipticLatitude;

        PlanetPositionUtility.GetHeliocentricEclipticalCoordinates( m_MeanEquinoxData, julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );

        // In case a planet makes it in here without an orbit, abort
        if ( radiusVector != 0.0f )
        {
            double initialRotation = eclipticalLongitude;
            double difference = 0.0;
            bool closing = false;

            while ( true )
            {
                PlanetPositionUtility.GetHeliocentricEclipticalCoordinates( m_MeanEquinoxData, julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );
                double newDifference = Math.Abs( initialRotation - eclipticalLongitude );

                if ( closing )
                {
                    if ( newDifference > difference )
                    {
                        break;
                    }
                }
                else if ( newDifference < difference )
                {
                    closing = true;
                }

                difference = newDifference;

                CelestialVector3 position = GetPositionFromHeliocentricEclipticalCoordinates( radiusVector, eclipticalLongitude, eclipticLatitude );

                orbit.Add( (Vector3)( position / GlobalConstants.CelestialUnit ) );

                julianDate += julianDaysPerPosition;
            }
        }

        return orbit;
    }

    public float GetAverageOrbitDistance()
    {
        return m_MeanEquinoxData[ (int)PlanetPositionUtility.OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ][ 0 ] * ( GlobalConstants.AstronomicalUnit / GlobalConstants.CelestialUnit);
    }

    public override bool Initialize( CelestialBodyLoader loader )
    {
        if ( base.Initialize( loader ) )
        {
            List<float> floatList = null;

            if( false == loader.GetData( m_MeanLongitudeLabel, ref floatList ) )
            {
                Debug.LogError("Error");
                return false;
            }
            m_MeanEquinoxData[ (int)PlanetPositionUtility.OrbitalElements.MEAN_LONGITUDE_OF_PLANET ].AddRange( floatList );

            if ( false == loader.GetData( m_SemiMajorAxisOfOrbitLabel, ref floatList ) )
            {
                Debug.LogError( "Error" );
                return false;
            }
            m_MeanEquinoxData[ (int)PlanetPositionUtility.OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ].AddRange( floatList );

            if ( false == loader.GetData( m_EccentricityOfOrbitLabel, ref floatList ) )
            {
                Debug.LogError( "Error" );
                return false;
            }
            m_MeanEquinoxData[ (int)PlanetPositionUtility.OrbitalElements.ECCENTRICITY_OF_THE_ORBIT ].AddRange( floatList );

            if ( false == loader.GetData( m_InclinationOnPlaneOfEclipticLabel, ref floatList ) )
            {
                Debug.LogError( "Error" );
                return false;
            }
            m_MeanEquinoxData[ (int)PlanetPositionUtility.OrbitalElements.INCLINATION_ON_PLANE_OF_ECLIPTIC ].AddRange( floatList );

            // ArgumentOfPerihelion (optional)
            if ( loader.GetData( m_ArgumentOfPerihelionLabel, ref floatList ) )
            {
                m_MeanEquinoxData[ (int)PlanetPositionUtility.OrbitalElements.ARGUMENT_OF_PERIHELION ].AddRange( floatList );
            }

            // LongitudeOfAscendingNode (optional)
            if ( loader.GetData( m_LongitudeOfAscendingNodeLabel, ref floatList ) )
            {
                m_MeanEquinoxData[ (int)PlanetPositionUtility.OrbitalElements.LONGITUDE_OF_ASCENDING_NODE ].AddRange( floatList );
            }

            return true;
        }

        return false;
    }

    protected override void SetPosition( CelestialVector3 position )
    {
        // TODO: Apply dynamic scalar instead of celestial units
        transform.localPosition = (Vector3)( position / GlobalConstants.CelestialUnit );
    }

    #region Private Interface

    private CelestialVector3 GetPositionFromHeliocentricEclipticalCoordinates( double radiusVector, double eclipticalLongitude, double eclipticLatitude )
    {
        // radiusVector is in AU units

        // If we were using only floats then all we would need to do here is this:
        // *Notice* how we need to scale by astronomical units to keep data in range
        // Vector3 position = new Vector3( (float)( radiusVector * GlobalConstants.AstronomicalUnit ), 0, 0 );
        // Quaternion rotation = Quaternion.Euler( 0, -(float)eclipticalLongitude, -(float)eclipticLatitude );
        // return position * rotation;

        // Instead we are using doubles so we need to do this manually
        // We do not need to scale the data using this method

        CelestialVector3 position = new CelestialVector3( ( radiusVector * GlobalConstants.AstronomicalUnit ), 0, 0 );

        Quaternion rotation = Quaternion.Euler( 0, -(float)eclipticalLongitude, -(float)eclipticLatitude );

        // Algorithm for converting Euler angles to a quaternion
        //public void rotate( double heading, double attitude, double bank )
        //{
        //    // Assuming the angles are in radians.
        //    double c1 = Math.cos( heading / 2 );
        //    double s1 = Math.sin( heading / 2 );
        //    double c2 = Math.cos( attitude / 2 );
        //    double s2 = Math.sin( attitude / 2 );
        //    double c3 = Math.cos( bank / 2 );
        //    double s3 = Math.sin( bank / 2 );
        //    double c1c2 = c1 * c2;
        //    double s1s2 = s1 * s2;
        //    w = c1c2 * c3 - s1s2 * s3;
        //    x = c1c2 * s3 + s1s2 * c3;
        //    y = s1 * c2 * c3 + c1 * s2 * s3;
        //    z = c1 * s2 * c3 - s1 * c2 * s3;
        //}
        
        // Algorithm for rotation a vector by a quaternion
        //void rotate_vector_by_quaternion(const Vector3&v, const Quaternion&q, Vector3 & vprime)
        //{
        //    // Extract the vector part of the quaternion
        //    Vector3 u( q.x, q.y, q.z);
        //    
        //    // Extract the scalar part of the quaternion
        //    float s = q.w;
        //    
        //    // Do the math
        //    vprime = 2.0f * dot( u, v ) * u
        //          + ( s * s - dot( u, u ) ) * v
        //          + 2.0f * s * cross( u, v );
        //}

        // The algorithm can be simplified to this

        double halfDegreesToRadians = GlobalConstants.DegreesToRadians / 2;

        double c1 = Math.Cos( -eclipticalLongitude * halfDegreesToRadians );
        double s1 = Math.Sin( -eclipticalLongitude * halfDegreesToRadians );
        double c2 = Math.Cos( -eclipticLatitude * halfDegreesToRadians );
        double s2 = Math.Sin( -eclipticLatitude * halfDegreesToRadians );

        double w = c1 * c2;

        CelestialVector3 u = new CelestialVector3( s1 * s2, s1 * c2, c1 * s2 );

        CelestialVector3 part1 = u * ( 2.0 * CelestialVector3.Dot( u, position ) );
        CelestialVector3 part2 = position * ( w * w - CelestialVector3.Dot( u, u ) );
        CelestialVector3 part3 = CelestialVector3.Cross( u, position ) * ( 2.0 * w );

        return part1 + part2 + part3;
    }

    //private void ClickSelected( GameObject eventOwner )
    //{
    //    //Debug.Log( "Object click selected: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
    //    m_Camera.SetSelectedObject( this, false );

    //    // Notify the panel?
    //}

    //private void ClickTargeted( GameObject eventOwner )
    //{
    //    Debug.Log( "Object click targeted: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
    //    m_Camera.SetTargetedObject( this );

    //    // Notify the panel?
    //}

    //private void ClickDisableMiss( GameObject eventOwner )
    //{
    //    Debug.Log( "Object click disable miss: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
    //    m_Camera.DisableClickMissDetectionForThisFrame();
    //}

    //private void ClickDrag( GameObject eventOwner )
    //{
    //    Debug.Log( "Object click drag: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
    //    m_Camera.DragObject( this );
    //}

    private List<List<float>> m_MeanEquinoxData = new List<List<float>>()
    {
        new List<float>(), // MEAN_LONGITUDE_OF_PLANET
        new List<float>(), // SEMI_MAJOR_AXIS_OF_ORBIT
        new List<float>(), // ECCENTRICITY_OF_THE_ORBIT
        new List<float>(), // INCLINATION_ON_PLANE_OF_ECLIPTIC
        new List<float>(), // ARGUMENT_OF_PERIHELION
        new List<float>(), // LONGITUDE_OF_ASCENDING_NODE
        //new List<float>(), // MEAN_ANOMALY
    };

    //private static CelestialCamera m_Camera = null;

    private const string m_MeanLongitudeLabel = "MeanLongitude";
    private const string m_SemiMajorAxisOfOrbitLabel = "SemiMajorAxisOfOrbit";
    private const string m_EccentricityOfOrbitLabel = "EccentricityOfOrbit";
    private const string m_InclinationOnPlaneOfEclipticLabel = "InclinationOnPlaneOfEcliptic";
    private const string m_ArgumentOfPerihelionLabel = "ArgumentOfPerihelion";
    private const string m_LongitudeOfAscendingNodeLabel = "LongitudeOfAscendingNode";
    private const string m_MeanAnomalyLabel = "MeanAnomaly";

    #endregion
}
