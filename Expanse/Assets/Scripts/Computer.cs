using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Computer
{
    // Starts at 0 velocity and ends at 0 velocity
    public TimeSpan TravelTime( float km, float gravity )
    {
        TimeSpan total = CalculateBurnTime( km * 0.5f, gravity );

        return total + total;
    }

    public TimeSpan CalculateBurnTime( float km, float gravity )
    {
        double seconds = Math.Sqrt( ( 2000.0f * km ) / ( gravity * 9.8f ) );

        return new TimeSpan( 0, 0, (int)seconds );
    }
}
