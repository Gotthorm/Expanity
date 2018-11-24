﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class PlanetPositionUtility
{
    //public enum Planet
    //{
    //    MERCURY,
    //    VENUS,
    //    EARTH,
    //    MARS,
    //    JUPITER,
    //    SATURN,
    //    URANUS,
    //    NEPTUNE,
    //    PLUTO,
    //}

    public enum OrbitalElements
    {
        MEAN_LONGITUDE_OF_PLANET,           // L = mean longitude of the planet 
        SEMI_MAJOR_AXIS_OF_ORBIT,           // a = semimajor axis of the orbit (this element is a constant for each planet) 
        ECCENTRICITY_OF_THE_ORBIT,          // e = eccentricity of the orbit 
        INCLINATION_ON_PLANE_OF_ECLIPTIC,   // i = inclination on the plane of the ecliptic
        ARGUMENT_OF_PERIHELION,             // w = argument of perihelion
        LONGITUDE_OF_ASCENDING_NODE,        // W = longitude of ascending node
        MEAN_ANOMALY,                       // M = Mean anomaly (used only for Earth)
    }

    public static double GetJulianDate( DateTime dateTime )
    {
        int year = dateTime.Year;

        int month = dateTime.Month;
        if( month < 3 )
        {
            year -= 1;
            month += 12;
        }

        double day = dateTime.Day;
        double dayFraction = dateTime.Second + ( dateTime.Minute + dateTime.Hour * 60 ) * 60;
        dayFraction /= GlobalConstants.SecondsIn24Hours;
        day += dayFraction;

        int b = 0;

        // If the date is equal to or after 1582-Oct-15 it is in the gregorian calendar
        if ( dateTime > new DateTime( 1582, 10, 15 ) )
        { 
            // Adjust for the gregorian calendar
            int a = year / 100;
            b = 2 - a + ( a / 4 );
        }

        return (int)( 365.25 * year ) + (int)( 30.6001 * ( month + 1 ) ) + day + 1720994.5 + b;
    }

    public static void GetHeliocentricEclipticalCoordinates( List<List<float>> meanEquinoxData, double julianDate, out double radiusVector, out double eclipticalLongitude, out double eclipticLatitude )
    {
        // Calculate time in Julian centuries of 36525 ephemeris days from the epoch 1900 January 0.5 ET
        double t = ( julianDate - 2415020.0 ) / 36525.0;

        // Mean longitude of the planet in degrees
        double meanLongitude = GlobalHelpers.RotationClamp( GlobalHelpers.Polynomial( t, meanEquinoxData[ (int)OrbitalElements.MEAN_LONGITUDE_OF_PLANET ] ), GlobalConstants.MaxDegrees );

        // Semi major axis of the orbit( this element is a constant for each planet)
        double semiMajorAxis = GlobalHelpers.Polynomial( t, meanEquinoxData[ (int)OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ] );

        // Eccentricity of the orbit
        double eccentricityOfOrbit = GlobalHelpers.Polynomial( t, meanEquinoxData[ (int)OrbitalElements.ECCENTRICITY_OF_THE_ORBIT ] );

        // Inclination on the plane of the ecliptic in radians
        double inclinationOnPlaneOfEcliptic = GlobalHelpers.RotationClamp( GlobalHelpers.Polynomial( t, meanEquinoxData[ (int)OrbitalElements.INCLINATION_ON_PLANE_OF_ECLIPTIC ] ), GlobalConstants.MaxDegrees ) * GlobalConstants.DegreesToRadians;

        // Argument of perihelion in degrees
        double argumentOfPerihelion = GlobalHelpers.RotationClamp( GlobalHelpers.Polynomial( t, meanEquinoxData[ (int)OrbitalElements.ARGUMENT_OF_PERIHELION ] ), GlobalConstants.MaxDegrees );

        // Longitude of ascending node in degrees
        double longitudeOfAscendingNode = GlobalHelpers.RotationClamp( GlobalHelpers.Polynomial( t, meanEquinoxData[ (int)OrbitalElements.LONGITUDE_OF_ASCENDING_NODE ] ), GlobalConstants.MaxDegrees );

        double longitudeOfPerihelion = argumentOfPerihelion + longitudeOfAscendingNode;
        double meanAnomaly = GlobalHelpers.RotationClamp( meanLongitude - longitudeOfPerihelion, GlobalConstants.MaxDegrees );

        // Calculate the heliocentric ecliptical coordinates

        // Determine eccentric anomaly
        double E = SolveEccentricAnomaly( meanAnomaly * GlobalConstants.DegreesToRadians, eccentricityOfOrbit );

        // Determine true anomaly in degrees
        // tan(v/2) = [ (1 + eccentricityOfOrbit) / (1 – eccentricityOfOrbit) ]1/2 × tan(E/2)
        // v is true anomaly
        double trueAnomaly = GlobalHelpers.RotationClamp( 2.0 * GlobalConstants.RadiansToDegrees * Math.Atan( Math.Pow( ( 1.0 + eccentricityOfOrbit ) / ( 1.0 - eccentricityOfOrbit ), 0.5 ) * Math.Tan( E * 0.5 ) ), GlobalConstants.MaxDegrees );

        // Determine argument of latitude in radians
        // u = meanLongitude + v – M – W
        // u is argument of latitude
        double argumentOfLatitude = GlobalHelpers.RotationClamp( ( meanLongitude + trueAnomaly - meanAnomaly - longitudeOfAscendingNode ), GlobalConstants.MaxDegrees ) * GlobalConstants.DegreesToRadians;

        // Radius vector of planet in AU
        // r = semiMajorAxis × ( 1 – eccentricityOfOrbit × cos E ) OR r = semiMajorAxis × ( 1 – e2 ) / ( 1 + eccentricityOfOrbit × cos n )
        radiusVector = semiMajorAxis * ( 1.0 - eccentricityOfOrbit * Math.Cos( E ) );

        // Determine ecliptical longitude in degrees
        // l is ecliptical longitude
        // The ecliptical longitude l can be deduced from (l – W), which is given by tan( l – W ) = cos inclinationOnPlaneOfEcliptic × tan u
        // To deal with the correct quadrants we use instead: tan(l – W) = cos inclinationOnPlaneOfEcliptic × sin u / cos u and use ATan2 on the RHS to resolve l
        eclipticalLongitude = Math.Atan2( Math.Cos( inclinationOnPlaneOfEcliptic ) * Math.Sin( argumentOfLatitude ), Math.Cos( argumentOfLatitude ) ) * GlobalConstants.RadiansToDegrees + longitudeOfAscendingNode;

        // Determine ecliptic latitude in degrees
        // b is ecliptic latitude
        // sin b = sin u × sin inclinationOnPlaneOfEcliptic
        eclipticLatitude = Math.Asin( Math.Sin( argumentOfLatitude ) * Math.Sin( inclinationOnPlaneOfEcliptic ) ) * GlobalConstants.RadiansToDegrees;
    }

    // Using the formula: E = M + eccentricityOfOrbit * Sin E
    // E is the eccentric anomaly that must be solved.
    // M is the mean anomaly.
    // e is the eccentricity of the orbit
    private static double SolveEccentricAnomaly( double meanAnomaly, double eccentricityOfOrbit )
    {
        // E = M + e * Sin E

        double startRadians = 0.0;
        double endRadians = GlobalConstants.MaxRadians; // 360 degrees

        // The iteration count is just a guess at what is "good enough"
        for ( int loop = 0; loop < 30; ++loop )
        {
            SplitRangeForEccentricAnomaly( meanAnomaly, eccentricityOfOrbit, ref startRadians, ref endRadians );
        }

        return startRadians;
    }

    // Using the formula: E = M + e * Sin E
    // E is the eccentric anomaly that must be solved.
    // M is the mean anomaly.
    // e is the eccentricity of the orbit
    // Given start and end degree values, determine which half of the range the correct value resides
    // and update the given range bounds with the correct half range.
    private static void SplitRangeForEccentricAnomaly( double meanAnomaly, double eccentricityOfOrbit, ref double startRadians, ref double endRadians )
    {
        double middle = startRadians + ( endRadians - startRadians ) * 0.5;

        double startDiff = Math.Abs( startRadians - ( Math.Sin( startRadians ) * eccentricityOfOrbit + meanAnomaly ) );
        double endDiff = Math.Abs( endRadians - ( Math.Sin( endRadians ) * eccentricityOfOrbit + meanAnomaly ) );

        if( startDiff < endDiff )
        {
            endRadians = middle;
        }
        else if( startDiff > endDiff )
        {
            startRadians = middle;
        }
        else
        {
            // Edge case (start == 0 && end == 360)?
            if( middle > ( Math.Sin( middle ) * eccentricityOfOrbit + meanAnomaly ) )
            {
                endRadians = middle;
            }
            else
            {
                startRadians = middle;
            }
        }
    }
}