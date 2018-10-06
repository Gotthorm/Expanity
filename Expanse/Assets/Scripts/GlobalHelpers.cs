using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalHelpers
{
    // Clamps the given rotation value to the given max so that the following is satisfied: 0 <= value <= maxRotation
    public static double RotationClamp( double value, double maxRotation )
    {
        if ( maxRotation > 0.0 )
        {
            while ( value < 0 )
            {
                value += maxRotation;
            }
            while ( value > maxRotation )
            {
                value -= maxRotation;
            }
        }
        else
        {
            Debug.LogError( "The given maxRotation must be a positive value!" );
        }

        return value;
    }
    public static float RotationClamp( float value, float maxRotation )
    {
        if ( maxRotation > 0.0f )
        {
            while ( value < 0 )
            {
                value += maxRotation;
            }
            while ( value > maxRotation )
            {
                value -= maxRotation;
            }
        }
        else
        {
            Debug.LogError( "The given maxRotation must be a positive value!" );
        }

        return value;
    }

    // Solves a polynomial of the form: X0 + (X1 * t^1) + (X2 + t^2) + .... 
    public static double Polynomial( double t, List<float> valueList )
    {
        double result = 0.0;

        for ( int index = valueList.Count - 1; index >= 0; --index )
        {
            result += valueList[ index ];

            if ( index > 0 )
            {
                result *= t;
            }
        }

        return result;
    }

    public static string MakeSpaceDistanceString( float gameDistance )
    {
        string distanceString;

        if ( gameDistance >= GlobalConstants.AUThreshold )
        {
            gameDistance /= GlobalConstants.AUThreshold;
            distanceString = gameDistance.ToString( ".00" ) + " au" + Environment.NewLine;
        }
        else
        {
            gameDistance *= GlobalConstants.CelestialUnit;
            distanceString = gameDistance.ToString( "n0" ) + " km" + Environment.NewLine;
        }

        return distanceString;
    }
}
