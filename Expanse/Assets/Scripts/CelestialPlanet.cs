using System;
using System.Collections.Generic;
using UnityEngine;

public class CelestialPlanet : CelestialPlanetoid
{
    public override bool Initialize( CelestialBodyLoader loader )
    {
        if ( base.Initialize( loader ) )
        {
            List<float> floatList = null;

            if ( false == loader.GetData( m_MeanLongitudeLabel, ref floatList ) )
            {
                Debug.LogError( "Error" );
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

            m_CelestialType = CelestialType.Planet;

            return true;
        }

        return false;
    }

    public override void GetHeliocentricEclipticalCoordinates( double julianDate, out double radiusVector, out double eclipticalLongitude, out double eclipticLatitude )
    {
        PlanetPositionUtility.GetHeliocentricEclipticalCoordinates( m_MeanEquinoxData, julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );
    }

    public override void UpdatePosition( double julianDate )
    {
        CelestialVector3 position = CalculatePosition( julianDate );

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
    }

    public override double GetOrbitalPeriod()
    {
        // Calculating the orbital period around Sol
        // orbitalPeriodInYears = Sqrt( averageAU * averageAU * averageAU );
        double averageAUFromSun = m_MeanEquinoxData[ (int)PlanetPositionUtility.OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ][ 0 ];

        return Math.Sqrt( Math.Pow( averageAUFromSun, 3 ) ) * 365;
    }

    #region Private Interface

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

    private const string m_MeanLongitudeLabel = "MeanLongitude";
    private const string m_SemiMajorAxisOfOrbitLabel = "SemiMajorAxisOfOrbit";
    private const string m_EccentricityOfOrbitLabel = "EccentricityOfOrbit";
    private const string m_InclinationOnPlaneOfEclipticLabel = "InclinationOnPlaneOfEcliptic";
    private const string m_ArgumentOfPerihelionLabel = "ArgumentOfPerihelion";
    private const string m_LongitudeOfAscendingNodeLabel = "LongitudeOfAscendingNode";
    private const string m_MeanAnomalyLabel = "MeanAnomaly";

    #endregion
}
