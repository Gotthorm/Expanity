using System;
using System.Collections.Generic;
using UnityEngine;

public class CelestialMoon : CelestialPlanetoid
{
    public override bool Initialize( CelestialBodyLoader loader )
    {
        if ( base.Initialize( loader ) )
        {
            m_CelestialType = CelestialType.Moon;

            return true;
        }

        return false;
    }

    public override void GetHeliocentricEclipticalCoordinates( double julianDate, out double radiusVector, out double eclipticalLongitude, out double eclipticLatitude )
    {
        PlanetPositionUtility.GetLunaHeliocentricEclipticalCoordinates( julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );
    }

    public override double GetOrbitalPeriod()
    {
        // Calculating the orbital period around Earth
        return 27;
    }
}
