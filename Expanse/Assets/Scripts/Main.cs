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
    public string m_PositionBase = "";

    [Tooltip( "Spaceship's initial position is relative to the defined celestial body" )]
    public CelestialVector3 m_Position = new CelestialVector3();

    // Use this for initialization
    void Awake()
    {
        // This must be done first.  With no celestial objects, space is EMPTY!!
        {
            // Initialization cannot fail but will log warnings for any data it fails to load
            // All planets loaded will start with a base view position of origin
            CelestialManagerPhysical.Instance.Init( Application.dataPath + "/StreamingAssets/Config/CelestialBodies/" );

            m_SpaceShip.Init();

            CelestialBody earth = CelestialManagerPhysical.Instance.GetCelestialBody( m_PositionBase );

            CelestialVector3 initialPosition = new CelestialVector3();

            if ( null != earth )
            {
                CelestialVector3 offset = m_Position + ( m_Position.Normalized() * earth.Radius );

                initialPosition = earth.Position + offset;
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
