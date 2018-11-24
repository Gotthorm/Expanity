using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A vector3 implementation using doubles instead of floats
// Adding functionality as needed.

[System.Serializable]
public class CelestialVector3
{
    public double x;
    public double y;
    public double z;

    public CelestialVector3()
    {
        x = 0;
        y = 0;
        z = 0;
    }

    public CelestialVector3( double _x, double _y, double _z )
    {
        x = _x;
        y = _y;
        z = _z;
    }

    public static CelestialVector3 operator +( CelestialVector3 a, CelestialVector3 b )
    {
        return new CelestialVector3( a.x + b.x, a.y + b.y, a.z + b.z );
    }

    public static CelestialVector3 operator -( CelestialVector3 a, CelestialVector3 b )
    {
        return new CelestialVector3( a.x - b.x, a.y - b.y, a.z - b.z );
    }

    public static CelestialVector3 operator /( CelestialVector3 a, double b )
    {
        return new CelestialVector3( a.x / b, a.y / b, a.z / b );
    }

    public static CelestialVector3 operator *( CelestialVector3 a, double b )
    {
        return new CelestialVector3( a.x * b, a.y * b, a.z * b );
    }

    public double Length()
    {
        return System.Math.Sqrt( ( x * x ) + ( y * y ) + ( z * z ) );
    }

    public static implicit operator Vector3( CelestialVector3 v )
    {
        return new Vector3( (float)v.x, (float)v.y, (float)v.z );
    }
}
