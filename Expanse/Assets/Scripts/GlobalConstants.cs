using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalConstants
{
    // The units in space will be 150,000km
    public static uint CelestialUnit = 150000;

    // The "real" space unit used for distances is the distance from the earth to the sun
    public static uint AstronomicalUnit = 149600000;

    public static float AUThreshold = GlobalConstants.AstronomicalUnit / GlobalConstants.CelestialUnit;

    public static double MaxDegrees = 360.0;
    public static double MaxRadians = ( 2.0 * Math.PI );

    public static double DegreesToRadians = ( Math.PI / 180.0 );
    public static double RadiansToDegrees = ( 180.0 / Math.PI );

    public static int SecondsIn24Hours = ( 24 * 60 * 60 );
}
