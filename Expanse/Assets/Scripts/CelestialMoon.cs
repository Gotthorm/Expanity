using System;
using System.Collections.Generic;
using UnityEngine;

public class CelestialMoon : CelestialBody
{
    public override bool Initialize( CelestialBodyLoader loader )
    {
        if ( base.Initialize( loader ) )
        {
            //List<float> floatList = null;

            m_CelestialType = CelestialType.Moon;

            return true;
        }

        return false;
    }

    public override void UpdatePosition( double julianDate )
    {
        double radiusVector;
        double eclipticalLongitude;
        double eclipticLatitude;

        PlanetPositionUtility.GetLunaHeliocentricEclipticalCoordinates( julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );

        CelestialVector3 position = PlanetPositionUtility.GetPositionFromHeliocentricEclipticalCoordinates( radiusVector, eclipticalLongitude, eclipticLatitude );

        LocalPosition = position;

        if ( OrbitParentID != 0 )
        {
            CelestialBody parentBody = CelestialManagerPhysical.Instance.GetCelestialBody( OrbitParentID );

            if ( parentBody != null )
            {
                position += parentBody.Position;
            }
        }

        Position = position;

        m_RadiusVector = Radius;
    }

    public override List<Vector3> GetOrbit( double currentJulianDate, double resolution )
    {
        // One day in JD is equal to 1.0

        // Calculating the orbital period around Earth
        double orbitalPeriodInDays = 27;

        double julianDaysPerPosition = ( orbitalPeriodInDays / resolution );

        List<Vector3> orbit = new List<Vector3>();

        // Start at 1/2 orbit in the past
        double julianDate = currentJulianDate - ( orbitalPeriodInDays * 0.5 );

        double radiusVector;
        double eclipticalLongitude;
        double eclipticLatitude;

        PlanetPositionUtility.GetLunaHeliocentricEclipticalCoordinates( julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );

        // In case a planet makes it in here without an orbit, abort
        if ( radiusVector != 0.0f )
        {
            double initialRotation = eclipticalLongitude;
            double difference = 0.0;
            bool closing = false;

            while ( true )
            {
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

                CelestialVector3 position = PlanetPositionUtility.GetPositionFromHeliocentricEclipticalCoordinates( radiusVector, eclipticalLongitude, eclipticLatitude );

                orbit.Add( (Vector3)( position / GlobalConstants.CelestialUnit ) );

                julianDate += julianDaysPerPosition;

                PlanetPositionUtility.GetLunaHeliocentricEclipticalCoordinates( julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );
            }
        }

        return orbit;
    }

    #region Private Interface

    //private List<List<float>> m_MeanEquinoxData = new List<List<float>>()
    //{
    //    new List<float>(), // MEAN_LONGITUDE_OF_PLANET
    //    new List<float>(), // SEMI_MAJOR_AXIS_OF_ORBIT
    //    new List<float>(), // ECCENTRICITY_OF_THE_ORBIT
    //    new List<float>(), // INCLINATION_ON_PLANE_OF_ECLIPTIC
    //    new List<float>(), // ARGUMENT_OF_PERIHELION
    //    new List<float>(), // LONGITUDE_OF_ASCENDING_NODE
    //    //new List<float>(), // MEAN_ANOMALY
    //};

    //private const string m_MeanLongitudeLabel = "MeanLongitude";
    //private const string m_SemiMajorAxisOfOrbitLabel = "SemiMajorAxisOfOrbit";
    //private const string m_EccentricityOfOrbitLabel = "EccentricityOfOrbit";
    //private const string m_InclinationOnPlaneOfEclipticLabel = "InclinationOnPlaneOfEcliptic";
    //private const string m_ArgumentOfPerihelionLabel = "ArgumentOfPerihelion";
    //private const string m_LongitudeOfAscendingNodeLabel = "LongitudeOfAscendingNode";
    //private const string m_MeanAnomalyLabel = "MeanAnomaly";

    double m_RadiusVector = 0.0;

    #endregion
}
