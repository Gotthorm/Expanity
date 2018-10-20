using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// TODO: REfactor to be dynamically loaded?
//[RequireComponent( typeof( SphereCollider ) )]
//[RequireComponent( typeof( MeshFilter ) )]
[RequireComponent( typeof( CelestialClickable ) )]

public class CelestialBody : MonoBehaviour
{
    //public Canvas ParentCanvas = null;

    //public Camera ParentCamera = null;

    [Tooltip( "The maximum scalar multiplier that can be applied to this body" )]
    public uint m_MaximumScaleMultiplier = 1000;

    public enum CelestialType : UInt16
    {
        Invalid = 0,
        Planet = 1,
        Moon = 2,
        Asteroid = 4,
        Ship = 8,
        Unidentified = 16
    }

    //public void Select()
    //{
    //    SetSelected( true );
    //}

    //public void Unselect()
    //{
    //    SetSelected( false );
    //}

    public float Scale
    {
        get
        {
            return m_Scale;
        }

        set
        {
            if ( value > m_MaximumScaleMultiplier )
            {
                value = m_MaximumScaleMultiplier;

                // Create some sort of visual to indicate body was capped at Max scale?
            }

            m_Scale = value;

            float celestialScale = m_InitialScale * value;
            transform.localScale = new Vector3( celestialScale, celestialScale, celestialScale );

            //UpdateInfoText();
        }
    }

    public float Radius
    {
        get
        {
            return m_RadiusInKM;
        }
    }

    public float CelestialRadius
    {
        get
        {
            return m_RadiusInKM / (float)GlobalConstants.CelestialUnit;
        }
    }

    public float Velocity
    {
        get
        {
            return m_VelocityInKMS;
        }
    }

    public void SetCamera( CelestialCamera camera )
    {
        m_Camera = camera;
        //if ( null != m_HUDElement )
        //{
        //    m_HUDElement.SetCamera( camera.GetComponent<Camera>() );
        //}
    }

    public bool GetIsVisible() { return GetComponent<Renderer>().isVisible; }

    public void UpdateCameraPosition( Vector3 position )
    {
        Vector3 distanceVector = position - this.transform.position;
        m_Distance = distanceVector.magnitude;

        // If the celestial body is visible, ensure the HUD element is active and updated
        // Otherwise disable it 
        //if ( null != m_HUDElement )
        //{
        //    if ( GetComponent<Renderer>().isVisible )
        //    {
        //        m_HUDElement.gameObject.SetActive( true );
        //    }
        //    else
        //    {
        //        m_HUDElement.gameObject.SetActive( false );
        //    }

        //    UpdateInfoText();
        //}
    }

    public Vector3 GetPosition( double julianDate )
    {
        double radiusVector;
        double eclipticalLongitude;
        double eclipticLatitude;

        PlanetPosition.GetHeliocentricEclipticalCoordinates( m_MeanEquinoxData, julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );

        return GetPositionFromHeliocentricEclipticalCoordinates( radiusVector, eclipticalLongitude, eclipticLatitude );
    }

    public List<Vector3> GetOrbit( double currentJulianDate, double resolution )
    {
        // One day in JD is equal to 1.0

        // Calculating the orbital period around Sol
        // orbitalPeriodInYears = Sqrt( averageAU * averageAU * averageAU );
        double averageAUFromSun = m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ][ 0 ];

        double orbitalPeriodInDays = Math.Sqrt( Math.Pow( averageAUFromSun, 3 ) ) * 365;

        double julianDaysPerPosition = ( orbitalPeriodInDays / resolution );

        List<Vector3> orbit = new List<Vector3>();

        // Start at 1/2 orbit in the past
        double julianDate = currentJulianDate - ( orbitalPeriodInDays * 0.5 );

        double radiusVector;
        double eclipticalLongitude;
        double eclipticLatitude;

        PlanetPosition.GetHeliocentricEclipticalCoordinates( m_MeanEquinoxData, julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );

        double initialRotation = eclipticalLongitude;
        double difference = 0.0;
        bool closing = false;

        while( true )
        {
            PlanetPosition.GetHeliocentricEclipticalCoordinates( m_MeanEquinoxData, julianDate, out radiusVector, out eclipticalLongitude, out eclipticLatitude );
            double newDifference = Math.Abs( initialRotation - eclipticalLongitude );

            if( closing )
            {
                if( newDifference > difference )
                {
                    break;
                }
            }
            else if( newDifference < difference )
            {
                closing = true;
            }

            difference = newDifference;

            Vector3 position = GetPositionFromHeliocentricEclipticalCoordinates( radiusVector, eclipticalLongitude, eclipticLatitude );

            orbit.Add( position );

            julianDate += julianDaysPerPosition;
        }

        return orbit;
    }

    public float GetAverageOrbitDistance()
    {
        return m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ][ 0 ] * ( GlobalConstants.AstronomicalUnit / GlobalConstants.CelestialUnit);
    }

    public CelestialType GetCelestialType() { return CelestialType.Planet; }

    public static CelestialBody Create( string name, double julianDate, Canvas canvas )
    {
        string configPath = Application.dataPath + "/StreamingAssets/Config/CelestialBodies/" + name + ".xml";

        Debug.Log( configPath );

        CelestialBodyLoader loader = new CelestialBodyLoader();

        if ( loader.Load( configPath ) )
        {
            UnityEngine.Object prefab = Resources.Load( loader.m_PrefabDataPath, typeof( GameObject ) );

            if ( null != prefab )
            {
                GameObject gameObject = UnityEngine.Object.Instantiate( prefab ) as GameObject;

                if ( null != gameObject )
                {
                    gameObject.name = loader.m_Name;

                    CelestialBody newPlanet = gameObject.AddComponent<CelestialBody>();

                    if ( null != newPlanet )
                    {
                        newPlanet.m_MaximumScaleMultiplier = loader.m_MaxScale;
                        newPlanet.m_RadiusInKM = loader.m_Radius;

                        newPlanet.m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.MEAN_LONGITUDE_OF_PLANET ].AddRange( loader.m_MeanLongitude );
                        newPlanet.m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.SEMI_MAJOR_AXIS_OF_ORBIT ].AddRange( loader.m_SemiMajorAxisOfOrbit );
                        newPlanet.m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.ECCENTRICITY_OF_THE_ORBIT ].AddRange( loader.m_EccentricityOfOrbit );
                        newPlanet.m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.INCLINATION_ON_PLANE_OF_ECLIPTIC ].AddRange( loader.m_InclinationOnPlaneOfEcliptic );
                        newPlanet.m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.ARGUMENT_OF_PERIHELION ].AddRange( loader.m_ArgumentOfPerihelion );
                        newPlanet.m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.LONGITUDE_OF_ASCENDING_NODE ].AddRange( loader.m_LongitudeOfAscendingNode );
                        newPlanet.m_MeanEquinoxData[ (int)PlanetPosition.OrbitalElements.MEAN_ANOMALY ].AddRange( loader.m_LongitudeOfAscendingNode );

                        newPlanet.Initialize( canvas );

                        newPlanet.transform.position = newPlanet.GetPosition( julianDate );

                        //newPlanet.GetOrbit( julianDate, 50 );
                    }

                    return newPlanet;
                }
            }
        }

        Debug.LogError( "Failed to create celestial body: " + name );

        return null;
    }

    //public static void SetCamera( CelestialCamera camera ) { m_Camera = camera; }

    public Vector3 GetPositionFromHeliocentricEclipticalCoordinates( double radiusVector, double eclipticalLongitude, double eclipticLatitude )
    {
        Vector3 position = new Vector3( (float)( radiusVector * ( GlobalConstants.AstronomicalUnit / (double)GlobalConstants.CelestialUnit ) ), 0, 0 );

        Quaternion rotation = Quaternion.Euler( 0, -(float)eclipticalLongitude, -(float)eclipticLatitude );

        return rotation * position;
    }

    public UInt32 GetCelestialID() { return m_CelestialID; }

    #region Private Interface

    private void Awake()
    {
        m_CelestialID = m_CelestialIDGenerator++;
    }

    private void Initialize( Canvas canvas )
    {
        if ( 0 < m_RadiusInKM )
        {
            float celectialScale = m_RadiusInKM / (float)GlobalConstants.CelestialUnit;

            m_InitialScale = celectialScale / m_VisualUnitsScale;
        }
        else
        {
            Debug.LogError( "Radius not defined for celestial object: " + this.name );
        }

        Scale = 1;

        // Load the HUD
        //UnityEngine.Object prefab = Resources.Load( "Prefabs/Celestial HUD", typeof( GameObject ) );

        //if ( null != prefab )
        //{
        //    GameObject hudObject = Instantiate( prefab ) as GameObject;

        //    m_HUDElement = hudObject.GetComponent<CelestialBodyHUD>();

        //    if ( null != prefab )
        //    {
        //        m_HUDElement.name = this.name + " HUD";
        //        m_HUDElement.transform.localScale = new Vector3( 1, 1, 1 ); // Should check why this was necessary

        //        GameObject parentPanel = GameObject.Find( "View Panel" );

        //        if ( null != parentPanel )
        //        {
        //            m_HUDElement.transform.SetParent( parentPanel.transform );
        //        }
        //        else
        //        {
        //            Debug.LogError( "CelestialBody has no parent!" );
        //        }

        //        //CelestialBodyHUD celestialHUD = m_HUDElement.GetComponent<CelestialBodyHUD>();

        //        //celestialHUD.SetOwner( this );
        //        m_HUDElement.SetOwner( this );

        //        //celestialHUD.SetParent( canvas );
        //        //m_HUDElement.SetParent( canvas );

        //        //m_HUDElement.SetCamera(m_Camera.GetComponent<Camera>());

        //        //GameObject fartyPants = GameObject.Find( "View Panel" );

        //        //m_HUDElement.SetFartyPants( fartyPants );

        //        // Setup the HUD objects to notify us when clicked
        //        CelestialClickable[] clickableGUIObjects = m_HUDElement.GetComponentsInChildren<CelestialClickable>();

        //        foreach ( CelestialClickable clickableGUIObject in clickableGUIObjects )
        //        {
        //            //clickableGUIObject.ParentToNotify = this.gameObject;
        //            //clickableGUIObject.myDelegate = ChildHUDClicked;
        //            clickableGUIObject.SetSelected = ClickSelected;
        //            clickableGUIObject.SetTargeted = ClickTargeted;
        //            clickableGUIObject.DisableClickMiss = ClickDisableMiss;
        //            clickableGUIObject.MouseDrag = ClickDrag;
        //        }

        //        Unselect();
        //    }
        //    else
        //    {
        //        // error
        //    }
        //}

        //CelestialClickable clickableObject = this.gameObject.GetComponent<CelestialClickable>();
        //if ( clickableObject != null )
        //{
        //    clickableObject.SetSelected = ClickSelected;
        //    clickableObject.SetTargeted = ClickTargeted;
        //    clickableObject.DisableClickMiss = ClickDisableMiss;
        //    clickableObject.MouseDrag = ClickDrag;
        //}
    }

    private void ClickSelected( GameObject eventOwner )
    {
        //Debug.Log( "Object click selected: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        m_Camera.SetSelectedObject( this, false );

        // Notify the panel?
    }

    private void ClickTargeted( GameObject eventOwner )
    {
        Debug.Log( "Object click targeted: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        m_Camera.SetTargetedObject( this );

        // Notify the panel?
    }

    private void ClickDisableMiss( GameObject eventOwner )
    {
        Debug.Log( "Object click disable miss: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        m_Camera.DisableClickMissDetectionForThisFrame();
    }

    private void ClickDrag( GameObject eventOwner )
    {
        Debug.Log( "Object click drag: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
        m_Camera.DragObject( this );
    }

    //private void ChildHUDClicked( GameObject eventOwner )
    //{
    //    Debug.Log( "Child object clicked: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
    //    SetSelected( true );
    //}

    //private void SetSelected( bool selected )
    //{
        //Color currentColor = selected ? Color.yellow : Color.red;

        //m_HUDElement.m_TitleLabel.color = currentColor;
        //m_HUDElement.m_PointerImage.color = currentColor;
        //m_HUDElement.m_InfoLabel.color = currentColor;
        //m_HUDElement.m_InfoText.color = currentColor;

        //m_HUDElement.m_InfoLabel.enabled = selected;
        //m_HUDElement.m_InfoText.enabled = selected;
    //}

    //private void UpdateInfoText()
    //{
    //    if ( null != m_HUDElement )
    //    {
    //        string distanceString = GlobalHelpers.MakeSpaceDistanceString( m_Distance );
    //        string infoText = m_Category + Environment.NewLine;
    //        infoText += m_RadiusInKM.ToString() + " km" + Environment.NewLine;
    //        infoText += distanceString;
    //        infoText += m_VelocityInKMS.ToString() + " km/s" + Environment.NewLine;
    //        infoText += m_Scale.ToString( ".#" ) + "X" + Environment.NewLine;

    //        m_HUDElement.m_InfoText.text = infoText;
    //    }
    //}

    //private CelestialBodyHUD m_HUDElement = null;

    private string m_Category = "";
    private float m_Distance = 0;
    private float m_Scale = 1.0f;

    // The base scale at multiplier value 1
    private float m_InitialScale = 1.0f;

    private uint m_VisualUnitsScale = 100;
    private uint m_RadiusInKM = 0;
    private float m_VelocityInKMS = 0.0f;

    private List<List<float>> m_MeanEquinoxData = new List<List<float>>()
    {
        new List<float>(), // MEAN_LONGITUDE_OF_PLANET
        new List<float>(), // SEMI_MAJOR_AXIS_OF_ORBIT
        new List<float>(), // ECCENTRICITY_OF_THE_ORBIT
        new List<float>(), // INCLINATION_ON_PLANE_OF_ECLIPTIC
        new List<float>(), // ARGUMENT_OF_PERIHELION
        new List<float>(), // LONGITUDE_OF_ASCENDING_NODE
        new List<float>(), // MEAN_ANOMALY
    };

    private UInt32 m_CelestialID = 0;

    private static CelestialCamera m_Camera = null;
    private static UInt32 m_CelestialIDGenerator = 1;

    #endregion
}
