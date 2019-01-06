using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CelestialPlanetoid : CelestialBody
{
    public abstract void GetHeliocentricEclipticalCoordinates( double julianDate, out double radiusVector, out double eclipticalLongitude, out double eclipticLatitude );

    public abstract double GetOrbitalPeriod();

    public virtual CelestialVector3 CalculatePosition( double julianDate )
    {
        double radiusVector;
        double eclipticalLongitude;
        double eclipticLatitude;

        GetHeliocentricEclipticalCoordinates( julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );

        return PlanetPositionUtility.GetPositionFromHeliocentricEclipticalCoordinates( radiusVector, eclipticalLongitude, eclipticLatitude );
    }

    public override List<Vector3> GetOrbit( double currentJulianDate, double resolution )
    {
        double orbitalPeriodInDays = GetOrbitalPeriod();

        double julianDaysPerPosition = ( orbitalPeriodInDays / resolution );

        List<Vector3> orbit = new List<Vector3>();

        // Start at 1/2 orbit in the past
        double julianDate = currentJulianDate - ( orbitalPeriodInDays * 0.5 );

        double radiusVector;
        double eclipticalLongitude;
        double eclipticLatitude;

        GetHeliocentricEclipticalCoordinates( julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );

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

                GetHeliocentricEclipticalCoordinates( julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );
            }
        }

        return orbit;
    }
}
