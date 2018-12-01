using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [Tooltip( "This must be assigned in the editor" )]
    public Canvas m_Canvas = null;

    [Tooltip( "Name of celestial body that spaceship's initial position is relative to" )]
    public string m_BasePositionBodyName = "";

    [Tooltip( "Spaceship's initial position is relative to the defined celestial body" )]
    public CelestialVector3 m_Position = new CelestialVector3();

    [Tooltip( "Initial light source to be attached to Sol" )]
    public Light m_MainLight = null;

    [Tooltip( "Parent UI object containing thumbnail views for main ship" )]
    public GameObject m_UICameraViewsParent = null;

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

            CelestialBody rocinante = CelestialManagerPhysical.Instance.GetCelestialBody( "Rocinante" );
            if ( rocinante != null )
            {
                m_SpaceShip = rocinante as CelestialShip;

                CelestialBody baseBody = CelestialManagerPhysical.Instance.GetCelestialBody( m_BasePositionBodyName );

                if ( null != baseBody && null != m_SpaceShip )
                {
                    CelestialVector3 offset = m_Position + ( m_Position.Normalized() * baseBody.Radius );

                    m_SpaceShip.Position = baseBody.Position - offset;
                }

                // Connect cameras
                if( m_UICameraViewsParent != null )
                {
                    ExternalShipView[] viewList = m_UICameraViewsParent.GetComponentsInChildren<ExternalShipView>();

                    foreach( ExternalShipView view in viewList )
                    {
                        // Does ship have this view?
                        SpaceShipExternalCamera camera = m_SpaceShip.GetExternalCamera( view.name );

                        if ( camera != null )
                        {
                            view.m_ViewCamera = camera;
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    private void Update ()
    {
        CelestialVector3 basePosition = new CelestialVector3();

        if( null != m_SpaceShip )
        {
            basePosition = m_SpaceShip.Position;
        }

        // Update the position of all celestial bodies relative to the main ship's position
        CelestialManagerPhysical.Instance.Update( basePosition );
    }

    private CelestialShip m_SpaceShip = null;
}
