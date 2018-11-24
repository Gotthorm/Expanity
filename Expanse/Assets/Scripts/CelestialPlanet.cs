using System;
using System.Collections.Generic;
using UnityEngine;

public class CelestialPlanet : CelestialBody
{
    public override CelestialType GetCelestialType() { return CelestialType.Planet; }

    public override bool Initialize( CelestialBodyLoader loader )
    {
        if ( base.Initialize( loader ) )
        {
            return true;
        }

        return false;
    }
}
