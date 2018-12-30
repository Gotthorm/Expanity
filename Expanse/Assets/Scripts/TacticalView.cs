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
            if( celestialHUD.GetOwner().ID == celestialBody.ID )
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
            // Setup the view camera to a default position
            CelestialBody body = m_VirtualManager.GetCelestialBody( "Earth" );
            if ( body )
            {
                m_ViewCamera.SetTargetedObject( body );
            }

            // Find all of the planets
            List<CelestialBody> planets = m_VirtualManager.GetCelestialBodies( CelestialBody.CelestialType.All );
            foreach ( CelestialBody celestialBody in planets )
            {
                // Create HUD element
                CelestialBodyHUD celestialBodyHUD = CreateHUDElement( celestialBody );

                m_HUDList.Add( celestialBodyHUD );

                // Set the celestial body as interactable
                CelestialClickable clickableObject = celestialBody.gameObject.GetComponent<CelestialClickable>();
                if ( clickableObject != null )
                {
                    clickableObject.SetSelected = ClickSelected;
                    clickableObject.SetTargeted = ClickTargeted;
                    clickableObject.DisableClickMiss = ClickDisableMiss;
                    clickableObject.MouseDrag = ClickDrag;
                }

                // Create orbit
                if ( celestialBody.Orbit )
                {
                    CelestialOrbit orbit = CelestialOrbit.Create( celestialBody as CelestialVirtual );
                    orbit.transform.SetParent( this.gameObject.transform );
                    m_CelestialOrbits.Add( orbit );
                }
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

                // Setup the HUD objects to notify us when clicked
                CelestialClickable[] clickableGUIObjects = newHUD.GetComponentsInChildren<CelestialClickable>();

                foreach ( CelestialClickable clickableGUIObject in clickableGUIObjects )
                {
                    //clickableGUIObject.ParentToNotify = this.gameObject;
                    //clickableGUIObject.myDelegate = ChildHUDClicked;
                    clickableGUIObject.SetSelected      = ClickSelected;
                    clickableGUIObject.SetTargeted      = ClickTargeted;
                    clickableGUIObject.DisableClickMiss = ClickDisableMiss;
                    clickableGUIObject.MouseDrag        = ClickDrag;
                }

                newHUD.SetCamera(m_ViewCamera.GetComponent<Camera>());
                //celestialBody.Unselect();
            }
        }

        return newHUD;
    }

    private void UpdateHUDElement( CelestialBodyHUD celestialBodyHUD, bool active )
    {
        if ( celestialBodyHUD.GetIsVisible() && active )
        {
            celestialBodyHUD.gameObject.SetActive( true );
        }
        else
        {
            celestialBodyHUD.gameObject.SetActive( false );
        }

        CelestialBody celestialBody = celestialBodyHUD.GetOwner();

        if( celestialBody != null )
        {
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
                m_ProximityPanel.SelectProximityObject( m_SelectedBodyHUD.GetOwner().ID );
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
        //m_Camera.DisableClickMissDetectionForThisFrame();
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
