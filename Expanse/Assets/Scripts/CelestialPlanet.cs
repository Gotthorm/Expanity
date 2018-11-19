using System;
using System.Collections.Generic;
using UnityEngine;

public class CelestialPlanet : CelestialBody
{
    public override CelestialType GetCelestialType() { return CelestialType.Planet; }

    public void UpdatePosition( double julianDate )
    {
        double radiusVector;
        double eclipticalLongitude;
        double eclipticLatitude;

        PlanetPosition.GetHeliocentricEclipticalCoordinates( m_MeanEquinoxData, julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );

        transform.position = GetPositionFromHeliocentricEclipticalCoordinates( radiusVector, eclipticalLongitude, eclipticLatitude );
    }

    public List<Vector3> GetOrbit( double currentJulianDate, double resolution )
    {
        // One day in JD is equal to 1.0

        // Calculating the orbital period around Sol
        // orbitalPeriodInYears = Sqrt( averageAU * averageAU * averageAU );
        double averageAUFromSun = m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ][ 0 ];

        double orbitalPeriodInDays = Math.Sqrt( Math.Pow( averageAUFromSun, 3 ) ) * 365;

        double julianDaysPerPosition = ( orbitalPeriodInDays / resolution );

        List<Vector3> orbit = new List<Vector3>();

        // Start at 1/2 orbit in the past
        double julianDate = currentJulianDate - ( orbitalPeriodInDays * 0.5 );

        double radiusVector;
        double eclipticalLongitude;
        double eclipticLatitude;

        PlanetPosition.GetHeliocentricEclipticalCoordinates( m_MeanEquinoxData, julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );

        // In case a planet makes it in here without an orbit, abort
        if ( radiusVector != 0.0f )
        {
            double initialRotation = eclipticalLongitude;
            double difference = 0.0;
            bool closing = false;

            while ( true )
            {
                PlanetPosition.GetHeliocentricEclipticalCoordinates( m_MeanEquinoxData, julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );
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

                Vector3 position = GetPositionFromHeliocentricEclipticalCoordinates( radiusVector, eclipticalLongitude, eclipticLatitude );

                orbit.Add( position );

                julianDate += julianDaysPerPosition;
            }
        }

        return orbit;
    }

    public float GetAverageOrbitDistance()
    {
        return m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ][ 0 ] * ( GlobalConstants.AstronomicalUnit / GlobalConstants.CelestialUnit);
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
            m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.MEAN_LONGITUDE_OF_PLANET ].AddRange( floatList );

            if ( false == loader.GetData( m_SemiMajorAxisOfOrbitLabel, ref floatList ) )
            {
                Debug.LogError( "Error" );
                return false;
            }
            m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ].AddRange( floatList );

            if ( false == loader.GetData( m_EccentricityOfOrbitLabel, ref floatList ) )
            {
                Debug.LogError( "Error" );
                return false;
            }
            m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.ECCENTRICITY_OF_THE_ORBIT ].AddRange( floatList );

            if ( false == loader.GetData( m_InclinationOnPlaneOfEclipticLabel, ref floatList ) )
            {
                Debug.LogError( "Error" );
                return false;
            }
            m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.INCLINATION_ON_PLANE_OF_ECLIPTIC ].AddRange( floatList );

            // ArgumentOfPerihelion (optional)
            if ( loader.GetData( m_ArgumentOfPerihelionLabel, ref floatList ) )
            {
                m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.ARGUMENT_OF_PERIHELION ].AddRange( floatList );
            }

            // LongitudeOfAscendingNode (optional)
            if ( loader.GetData( m_LongitudeOfAscendingNodeLabel, ref floatList ) )
            {
                m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.LONGITUDE_OF_ASCENDING_NODE ].AddRange( floatList );
            }

            return true;
        }

        return false;
    }

    // real only
    public Vector3 GetPositionFromHeliocentricEclipticalCoordinates( double radiusVector, double eclipticalLongitude, double eclipticLatitude )
    {
        Vector3 position = new Vector3( (float)( radiusVector * ( GlobalConstants.AstronomicalUnit / (double)GlobalConstants.CelestialUnit ) ), 0, 0 );

        Quaternion rotation = Quaternion.Euler( 0, -(float)eclipticalLongitude, -(float)eclipticLatitude );

        return rotation * position;
    }

    #region Private Interface

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
