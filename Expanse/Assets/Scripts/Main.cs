using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [Tooltip( "This must be assigned in the editor" )]
    public Canvas m_Canvas = null;

    [Tooltip( "This must be assigned in the editor" )]
    public SpaceShip m_SpaceShip = null;

    [Tooltip( "Name of celestial body that spaceship's initial position is relative to" )]
    public string m_BasePositionBodyName = "";

    [Tooltip( "Spaceship's initial position is relative to the defined celestial body" )]
    public CelestialVector3 m_Position = new CelestialVector3();

    [Tooltip( "Initial light source to be attached to Sol" )]
    public Light m_MainLight = null;

    // Use this for initialization
    void Awake()
    {
        // This must be done first.  With no celestial objects, space is EMPTY!!
        {
            // Initialization cannot fail but will log warnings for any data it fails to load
            // All planets loaded will start with a base view position of origin
            CelestialManagerPhysical.Instance.Init( Application.dataPath + "/StreamingAssets/Config/CelestialBodies/" );

            if( m_MainLight != null )
            {
                CelestialBody sol = CelestialManagerPhysical.Instance.GetCelestialBody( "Sol" );
                if( sol != null )
                {
                    m_MainLight.transform.parent = sol.transform;
                }
            }

            m_SpaceShip.Init();

            CelestialBody baseBody = CelestialManagerPhysical.Instance.GetCelestialBody( m_BasePositionBodyName );

            CelestialVector3 initialPosition = new CelestialVector3();

            if ( null != baseBody )
            {
                CelestialVector3 offset = m_Position + ( m_Position.Normalized() * baseBody.Radius );

                initialPosition = baseBody.Position - offset;
            }

            m_SpaceShip.Position = initialPosition;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        CelestialVector3 basePosition = new CelestialVector3();

        if( null != m_SpaceShip )
        {
            basePosition = m_SpaceShip.Position;
        }

        // Update the position of all celestial bodies relative to the main ship's position
        CelestialManagerPhysical.Instance.Update( basePosition );
    }
}
