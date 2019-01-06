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
        CelestialBodyLoader.PrefabPath = "Prefabs/CelestialBodies/";

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

                // Connect cameras to the spaceship
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

    // This script has a -100 script execution order to ensure its "Start" and "Update" are executed before all others.
    // This is set in: Edit => Project Settings => Script Execution Order

    private void Start()
    {
        // Ensure all of the celestial bodies have their initial positions set
        CelestialManagerPhysical.Instance.Update();

        m_BaseBody = CelestialManagerPhysical.Instance.GetCelestialBody( m_BasePositionBodyName );

        UpdateShipPosition();
    }

    // Update is called once per frame
    private void Update ()
    {
        CelestialManagerPhysical.Instance.Update();

        UpdateShipPosition();

        // Update the position of all celestial bodies relative to the main ship's position
        CelestialManagerPhysical.Instance.UpdateDynamicScale( m_BasePosition );
    }

    private void UpdateShipPosition()
    {
        // Update the spaceship's position
        if ( null != m_BaseBody && null != m_SpaceShip )
        {
            CelestialVector3 offset = m_Position + ( m_Position.Normalized() * m_BaseBody.Radius );

            m_SpaceShip.Position = m_BaseBody.Position - offset;

            m_BasePosition = m_SpaceShip.Position;
        }
    }

    // This is a temporary global reference to the main ship that represents the point of view of the player.
    private CelestialShip m_SpaceShip = null;

    // This is a hack for the ship to base its relative position from.
    // This will be removed once the ship has its own positional update implemented.
    private CelestialBody m_BaseBody = null;

    // 
    private CelestialVector3 m_BasePosition = new CelestialVector3();
}
