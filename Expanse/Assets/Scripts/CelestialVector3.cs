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

    public CelestialVector3 Normalized()
    {
        double length = Length();

        if ( 0 < length )
        {
            return new CelestialVector3( x / length, y / length, z / length );
        }
        else
        {
            return new CelestialVector3();
        }
    }

    //    Dot Product
    //V1.x* V2.x + V1.y* V2.y + V1.z* V2.z

    //Cross Product
    //cx = aybz− azby
    //cy = azbx− axbz
    //cz = axby− aybx

    public static double Dot( CelestialVector3 a, CelestialVector3 b )
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static CelestialVector3 Cross( CelestialVector3 a, CelestialVector3 b )
    {
        double cx = a.y * b.z - a.z * b.y;
        double cy = a.z * b.x - a.x * b.z;
        double cz = a.x * b.y - a.y * b.x;

        return new CelestialVector3( cx, cy, cz );
    }

    public static explicit operator Vector3( CelestialVector3 v )
    {
        return new Vector3( (float)v.x, (float)v.y, (float)v.z );
    }
}
