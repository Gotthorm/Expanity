using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalView : MonoBehaviour
{
    public ProximityControlPanel m_ProximityPanel = null;

    public void SetSelected( CelestialBody celestialBody, bool lookAtTarget )
    {
        if ( null != celestialBody && null != m_ViewCamera )
        {
            m_ViewCamera.SetSelectedObject( celestialBody, lookAtTarget );
        }

        foreach ( CelestialBodyHUD celestialHUD in m_HUDList)
        {
            if( celestialHUD.GetOwner().CelestialID == celestialBody.CelestialID )
            {
                SelectHUD( celestialHUD, false );
                break;
            }
        }
    }

    public void SetSelected( uint celestialBodyID, bool lookAtTarget )
    {
        CelestialBody celestialBody = m_VirtualManager.GetCelestialBody( celestialBodyID );

        SetSelected( celestialBody, lookAtTarget );
    }

    public void SetTarget( CelestialBody celestialBody )
    {
        if( null != celestialBody && null != m_ViewCamera )
        {
            m_ViewCamera.SetTargetedObject( celestialBody );
        }
    }

    public void SetTarget( uint celestialBodyID )
    {
        CelestialBody celestialBody = m_VirtualManager.GetCelestialBody( celestialBodyID );

        SetTarget( celestialBody );
    }

    public void SetAutoScale( bool enabled )
    {
        m_VirtualManager.SetAutoScale( enabled );
    }

    public Vector3 GetCameraPosition()
    {
        return m_ViewCamera.transform.position;
    }

    public List<CelestialBody> GetClosestBodies( int maxEntries, CelestialBody.CelestialType celestialFilter, Vector3 position )
    {
        return m_VirtualManager.GetClosestCelestialBodies( celestialFilter, maxEntries, position );
    }

    private void Awake()
    {
        // Find my camera
        m_ViewCamera = GetComponentInChildren<CelestialCamera>();

        if( null == m_ViewCamera )
        {
            Debug.LogError("TacticalView requires a child CelestialCamera!");
        }
    }

    // Use this for initialization
    private void Start ()
    {
        if ( m_VirtualManager.Init() )
        {
            // Find all of the planets
            List<CelestialBody> planets = m_VirtualManager.GetCelestialBodies( CelestialBody.CelestialType.All );
            foreach ( CelestialBody celestialBody in planets )
            {
                // Create HUD element
                CelestialBodyHUD celestialBodyHUD = CreateHUDElement( celestialBody );

                m_HUDList.Add( celestialBodyHUD );

                // Create orbit

                // Only virtual bodies have visual orbits
                CelestialVirtual celestialVirtual = celestialBody as CelestialVirtual;

                if ( celestialBody.HasOrbit && null != celestialVirtual )
                {
                    CelestialBody virtualParent = m_VirtualManager.GetCelestialBody( celestialBody.OrbitParentID );

                    // The orbit needs the id of the physical owner and a reference to the virtual parent
                    CelestialOrbit orbit = CelestialOrbit.Create( celestialVirtual.OwnerID, virtualParent );

                    // Use the same grouping parent as the virtual owner
                    orbit.transform.SetParent( celestialBody.transform.parent );

                    m_CelestialOrbits.Add( orbit );
                }
            }

            // Setup the HUD objects to notify us when clicked
            CelestialClickable[] clickableGUIObjects = GetComponentsInChildren<CelestialClickable>();

            foreach ( CelestialClickable clickableGUIObject in clickableGUIObjects )
            {
                if ( clickableGUIObject.m_EnableClick )
                {
                    clickableGUIObject.SetSelected = ClickSelected;
                    clickableGUIObject.SetTargeted = ClickTargeted;
                }
                clickableGUIObject.DisableClickMiss = ClickDisableMiss;

                if ( clickableGUIObject.m_EnableDrag )
                {
                    clickableGUIObject.MouseDrag = ClickDrag;
                }
            }

            // Set default filter
            if ( m_ProximityPanel != null )
            {
                m_ProximityPanel.ToggleFilter_Planets();
            }

            // Set default scale
            SetAutoScale( false );

            // Setup the view camera to a default position
            CelestialBody body = m_VirtualManager.GetCelestialBody( "Earth" );
            if ( body )
            {
                m_ViewCamera.SetTargetedObject( body );
            }
        }
        else
        {
            Debug.LogError( "VirtualManager failed to initialize!" );
        }
	}
	
	// Update is called once per frame
	private void Update ()
    {
        // Update virtual celestial bodies
        m_VirtualManager.Update( m_ViewCamera );

        bool active = GetComponentInParent<ScreenPanel>().Enabled;

        m_ViewCamera.GetComponent<Camera>().enabled = active;

        foreach ( CelestialBodyHUD celestialBodyHUD in m_HUDList )
        {
            UpdateHUDElement( celestialBodyHUD, active );
        }
    }

    private CelestialBodyHUD CreateHUDElement( CelestialBody celestialBody )
    {
        CelestialBodyHUD newHUD = null;

        // Load the HUD
        UnityEngine.Object prefab = null;

        switch( celestialBody.Type )
        {
            case CelestialBody.CelestialType.Ship:
                {
                    prefab = Resources.Load( "Prefabs/Celestial Ship HUD", typeof( GameObject ) );
                }
                break;
            case CelestialBody.CelestialType.Planet:
            default:
                {
                    prefab = Resources.Load( "Prefabs/Celestial Planet HUD", typeof( GameObject ) );
                }
                break;
        }

        if ( null != prefab )
        {
            GameObject hudObject = Instantiate( prefab ) as GameObject;

            newHUD = hudObject.GetComponent<CelestialBodyHUD>();

            if ( null != newHUD )
            {
                newHUD.name = celestialBody.name + " HUD";
                newHUD.transform.localScale = new Vector3( 1, 1, 1 ); // Should check why this was necessary

                newHUD.transform.SetParent( this.gameObject.transform );

                newHUD.SetOwner( celestialBody );

                newHUD.SetCamera(m_ViewCamera.GetComponent<Camera>());
            }
        }

        return newHUD;
    }

    private void UpdateHUDElement( CelestialBodyHUD celestialBodyHUD, bool active )
    {
        bool isActive = false;

        CelestialBody celestialBody = celestialBodyHUD.GetOwner();

        if ( celestialBody != null )
        {

            // Is HUD element's parent control active AND is it in the current view?
            if ( celestialBodyHUD.GetIsVisible() && active )
            {
                // Is HUD element's type active in the proximity control?
                if ( m_ProximityPanel.CelestialTypeIsActive( celestialBody.Type ) )
                {
                    isActive = true;
                }
            }

            Vector3 distanceVector = m_ViewCamera.transform.position - celestialBody.transform.position;
            float distance = distanceVector.magnitude;

            string distanceString = GlobalHelpers.MakeSpaceDistanceString( distance );
            string infoText = "";

            switch ( celestialBody.Type )
            {
                case CelestialBody.CelestialType.Ship:
                    {
                        infoText = "Ship" + System.Environment.NewLine;
                        infoText += "Corvette" + System.Environment.NewLine;
                        infoText += distanceString;
                        infoText += celestialBody.Velocity.ToString() + " km/s" + System.Environment.NewLine;
                    }
                    break;
                case CelestialBody.CelestialType.Planet:
                default:
                    {
                        infoText = "Planet" + System.Environment.NewLine;
                        infoText += celestialBody.Radius.ToString() + " km" + System.Environment.NewLine;
                        infoText += distanceString;
                        infoText += celestialBody.Velocity.ToString() + " km/s" + System.Environment.NewLine;
                        infoText += celestialBody.Scale.ToString( ".#" ) + "X" + System.Environment.NewLine;
                    }
                    break;
            }
            celestialBodyHUD.m_InfoText.text = infoText;
        }

        celestialBodyHUD.gameObject.SetActive( isActive );
    }

    private void SelectHUD(CelestialBodyHUD celestialBodyHUD, bool notifyProximityPanel)
    {
        if ( celestialBodyHUD != null )
        {
            CelestialBody celestialBody = celestialBodyHUD.GetOwner();

            if ( celestialBody != null )
            {
                m_ViewCamera.SetSelectedObject( celestialBody, false );
            }

            if ( m_SelectedBodyHUD != null && m_SelectedBodyHUD != celestialBodyHUD )
            {
                m_SelectedBodyHUD.SetSelected( false );
            }
            m_SelectedBodyHUD = celestialBodyHUD;
            m_SelectedBodyHUD.SetSelected( true );

            if ( m_ProximityPanel != null && notifyProximityPanel )
            {
                m_ProximityPanel.SelectProximityObject( m_SelectedBodyHUD.GetOwner().CelestialID );
            }
        }
    }

    // CelestialClickable.OnPointerClick => this.ClickSelected
    private void ClickSelected( GameObject eventOwner )
    {
        Debug.Log( "TacticalView: Object click selected: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        
        if(eventOwner.transform.parent != null)
        {
            CelestialBodyHUD celestialBodyHUD = eventOwner.transform.parent.GetComponent<CelestialBodyHUD>();

            SelectHUD( celestialBodyHUD, true );
        }
    }

    private void ClickTargeted( GameObject eventOwner )
    {
        Debug.Log( "TacticalView: Object click targeted: " + eventOwner.name + " on celestial body: " + this.gameObject.name );

        CelestialBodyHUD parent = eventOwner.transform.parent.GetComponent<CelestialBodyHUD>();
        if(null != parent )
        {
            CelestialBody body = parent.GetOwner();
            if( null != body )
            {
                SetTarget( body );
            }
        }
    }

    private void ClickDisableMiss( GameObject eventOwner )
    {
        Debug.Log( "TacticalView: Object click disable miss: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        m_ViewCamera.DisableClickMissDetectionForThisFrame();
    }

    private void ClickDrag( GameObject eventOwner )
    {
        Debug.Log( "Object click drag: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        //m_Camera.DragObject( this );
    }

    private CelestialBodyHUD m_SelectedBodyHUD = null;

    private CelestialCamera m_ViewCamera = null;

    private CelestialManagerVirtual m_VirtualManager = new CelestialManagerVirtual();

    private List<CelestialBodyHUD> m_HUDList = new List<CelestialBodyHUD>();
    private List<CelestialOrbit> m_CelestialOrbits = new List<CelestialOrbit>();
}
