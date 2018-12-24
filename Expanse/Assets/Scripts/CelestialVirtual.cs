using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( CelestialClickable ) )]

public class CelestialVirtual : CelestialBody
{
    public uint ParentID
    {
        get { return m_ParentID; }
    }

    public static CelestialVirtual Create( CelestialBody celestialBody )
    {
        string name = celestialBody.name;
        double radius = celestialBody.Radius;

        GameObject gameObject = null;
        CelestialVirtual celestialVirtual = null;

        if ( celestialBody.Type == CelestialType.Ship )
        {
            gameObject = new GameObject( name );

            celestialVirtual = gameObject.AddComponent<CelestialVirtual>();

            celestialVirtual.m_CelestialType = celestialBody.Type;
            celestialVirtual.m_ParentID = celestialBody.ID;
            celestialVirtual.m_RadiusInKM = celestialBody.Radius;

            celestialVirtual.m_InitialScale = 1;

            celestialVirtual.m_HasOrbit = false;

            celestialVirtual.m_MaximumScaleMultiplier = 1U;
            celestialVirtual.Scale = 1;
        }
        else
        {
            string prefabPath = CelestialBodyLoader.PrefabPath + celestialBody.name + "_Virtual";
            UnityEngine.Object prefab = Resources.Load( prefabPath, typeof( GameObject ) );

            if ( null != prefab )
            {
                gameObject = UnityEngine.Object.Instantiate( prefab ) as GameObject;
            }

            if ( null != gameObject )
            {
                gameObject.name = name;

                celestialVirtual = gameObject.AddComponent<CelestialVirtual>();

                celestialVirtual.m_CelestialType = celestialBody.Type;
                celestialVirtual.m_ParentID = celestialBody.ID;
                celestialVirtual.m_RadiusInKM = celestialBody.Radius;

                // TODO: This is a sphere shaped calculation so will need to support other types eventually?
                // Initial scale is in game diameter (2 * radius)
                celestialVirtual.m_InitialScale = celestialVirtual.CelestialRadius * 2;

                celestialVirtual.m_HasOrbit = celestialBody.Orbit;

                celestialVirtual.m_MaximumScaleMultiplier = ( name != "Sol" ) ? 100U : 10U;
                celestialVirtual.Scale = 100;
            }
        }

        return celestialVirtual;
    }

    protected override void SetPosition( CelestialVector3 position )
    {
        transform.position = (Vector3)( position / GlobalConstants.CelestialUnit );
    }

    private uint m_ParentID = 0;
}
