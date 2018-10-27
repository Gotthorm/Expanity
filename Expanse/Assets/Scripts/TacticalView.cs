using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalView : MonoBehaviour
{
    public CelestialCamera m_ViewCamera = null;
    public ProximityControlPanel m_ProximityPanel = null;

    public void SetSelectedCelestial(uint celestialID )
    {
        foreach(CelestialBodyHUD celestialHUD in m_HUDList)
        {
            if(celestialHUD.GetOwner().GetCelestialID() == celestialID)
            {
                SelectHUD( celestialHUD, false );
                return;
            }
        }
    }

	// Use this for initialization
	private void Start ()
    {
        // Find all of the planets
        CelestialManager celestialManager = CelestialManager.GetInstance();

        if ( celestialManager != null )
        {
            int planetCount = celestialManager.GetPlanetCount();
            for ( int planetIndex = 0; planetIndex < planetCount; ++planetIndex )
            {
                CelestialBody celestialBody = celestialManager.GetPlanet( planetIndex );

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
                    CelestialOrbit orbit = CelestialOrbit.Create( celestialBody );
                    orbit.transform.SetParent( this.gameObject.transform );
                    m_CelestialOrbits.Add( orbit );
                }
            }
        }
        else
        {
            Debug.LogError( "CelestialManager is unavailable!" );
        }
	}
	
	// Update is called once per frame
	private void Update ()
    {
        foreach ( CelestialBodyHUD celestialBodyHUD in m_HUDList )
        {
            UpdateHUDElement( celestialBodyHUD );
        }
    }

    private CelestialBodyHUD CreateHUDElement( CelestialBody celestialBody )
    {
        CelestialBodyHUD newHUD = null;

        // Load the HUD
        UnityEngine.Object prefab = Resources.Load( "Prefabs/Celestial HUD", typeof( GameObject ) );

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

    private void UpdateHUDElement( CelestialBodyHUD celestialBodyHUD )
    {
        CelestialBody celestialBody = celestialBodyHUD.GetOwner();

        if ( celestialBody.GetIsVisible() )
        {
            celestialBodyHUD.gameObject.SetActive( true );
        }
        else
        {
            celestialBodyHUD.gameObject.SetActive( false );
        }

        Vector3 distanceVector = m_ViewCamera.transform.position - this.transform.position;
        float distance = distanceVector.magnitude;

        string distanceString = GlobalHelpers.MakeSpaceDistanceString( distance );
        //string infoText = m_Category + Environment.NewLine;
        string infoText = System.Environment.NewLine;
        infoText += celestialBody.Radius.ToString() + " km" + System.Environment.NewLine;
        infoText += distanceString;
        infoText += celestialBody.Velocity.ToString() + " km/s" + System.Environment.NewLine;
        infoText += celestialBody.Scale.ToString( ".#" ) + "X" + System.Environment.NewLine;

        celestialBodyHUD.m_InfoText.text = infoText;
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
                m_ProximityPanel.SelectProximityObject( m_SelectedBodyHUD.GetOwner().GetCelestialID() );
            }
        }
    }

    private void ClickSelected( GameObject eventOwner )
    {
        Debug.Log( "Object click selected: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        
        if(eventOwner.transform.parent != null)
        {
            CelestialBodyHUD celestialBodyHUD = eventOwner.transform.parent.GetComponent<CelestialBodyHUD>();

            SelectHUD( celestialBodyHUD, true );
        }
    }

    private void ClickTargeted( GameObject eventOwner )
    {
        Debug.Log( "Object click targeted: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        //m_Camera.SetTargetedObject( this );

        // Notify the panel?
    }

    private void ClickDisableMiss( GameObject eventOwner )
    {
        Debug.Log( "Object click disable miss: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        //m_Camera.DisableClickMissDetectionForThisFrame();
    }

    private void ClickDrag( GameObject eventOwner )
    {
        Debug.Log( "Object click drag: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        //m_Camera.DragObject( this );
    }

    //private void SetSelected
    private CelestialBodyHUD m_SelectedBodyHUD = null;

    private List<CelestialBodyHUD> m_HUDList = new List<CelestialBodyHUD>();
    private List<CelestialOrbit> m_CelestialOrbits = new List<CelestialOrbit>();
}
