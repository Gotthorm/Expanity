using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ControlSystem : MonoBehaviour
{
    public bool Initialize( SpaceShip ship, List<Thruster> thrusters )
    {
        string shipName = ship.Name;

        Rigidbody shipRigidBody = ship.GetComponent<Rigidbody>();

        if ( m_ThrusterControlSystem.Initialize( shipName, shipRigidBody, thrusters ) )
        {
            m_Initialized = true;
        }
        else
        {
            Debug.LogError( "Failed to initialize the control system for the ship <" + shipName + ">" );
        }

        return m_Initialized;
    }

    public virtual bool GetInput( out float inputX, out float inputY, out float inputZ )
    {
        inputX = 0.0f;
        inputY = 0.0f;
        inputZ = 0.0f;

        return true;
    }

    private void Update()
    {
        if ( m_Initialized )
        {
            float inputX;
            float inputY;
            float inputZ;

            if ( GetInput( out inputX, out inputY, out inputZ ) )
            {
                m_ThrusterControlSystem.BurnManeuveringSet( ThrusterControlSystem.ManeuveringAxisID.PITCH, inputX );
                m_ThrusterControlSystem.BurnManeuveringSet( ThrusterControlSystem.ManeuveringAxisID.YAW, inputY );
                m_ThrusterControlSystem.BurnManeuveringSet( ThrusterControlSystem.ManeuveringAxisID.ROLL, inputZ );
            }
        }
    }

    // The thruster control system
    private ThrusterControlSystem m_ThrusterControlSystem = new ThrusterControlSystem();

    private bool m_Initialized = false;
}

//[RequireComponent( typeof( Canvas ) )]
//[RequireComponent( typeof( GraphicRaycaster ) )]
//[RequireComponent( typeof( CanvasScaler ) )]

//public class ControlSystem : MonoBehaviour
//{
//    public bool Initialize( SpaceShip ship, List<Thruster> thrusters )
//    {
//        string shipName = ship.Name;

//        Rigidbody shipRigidBody = ship.GetComponent<Rigidbody>();

//        if ( m_ThrusterControlSystem.Initialize( shipName, shipRigidBody, thrusters ) )
//        {
//            // Set up canvas
//            m_Canvas = this.GetComponent<Canvas>();
//            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;

//            // Set up virtual joystick control
//            CreateVirtualJoystickControl();

//            // Setup proximity control
//            CreateProximityControl( ship.transform );

//            m_Initialized = true;
//        }
//        else
//        {
//            Debug.LogError( "Failed to initialize the control system for the ship <" + shipName + ">" );
//        }

//        return m_Initialized;
//    }

//    private bool CreateProximityControl( Transform target )
//    {
//        Object prefab = Resources.Load( "Prefabs/ProximityControl", typeof( GameObject ) );

//        if ( null != prefab )
//        {
//            GameObject instance = Instantiate( prefab ) as GameObject;

//            if ( null != prefab )
//            {
//                instance.name = "Proximity Control";
//                instance.transform.SetParent( this.GetComponent<Canvas>().transform );

//                m_ProximityControl = instance.GetComponent<ProximityControl>();

//                m_ProximityControl.Camera.Target = target;

//                return true;
//            }
//        }

//        return false;
//    }

//    private bool CreateVirtualJoystickControl()
//    {
//        Object prefab = Resources.Load( "Prefabs/VirtualJoystick", typeof( GameObject ) );

//        if ( null != prefab )
//        {
//            GameObject instance = Instantiate( prefab ) as GameObject;

//            if ( null != instance )
//            {
//                instance.name = "Virtual Joystick";
//                instance.transform.SetParent( this.GetComponent<Canvas>().transform );

//                RectTransform rectTransform = instance.GetComponent<Image>().rectTransform;
//                rectTransform.anchorMin = new Vector2( 1, 0 );
//                rectTransform.anchorMax = new Vector2( 1, 0 );
//                rectTransform.pivot = new Vector2( 1, 0 );
//                rectTransform.anchoredPosition = new Vector2( 0, 0 );

//                m_VirtualJoystick = instance.GetComponent<VirtualJoystick>();

//                return true;
//            }
//        }

//        return false;
//    }

//    private void Update()
//    {
//        if ( m_Initialized )
//        {
//            if( null != m_VirtualJoystick )
//            {
//                m_ThrusterControlSystem.BurnManeuveringSet( ThrusterControlSystem.ManeuveringAxisID.PITCH, m_VirtualJoystick.InputDirection.x );
//                m_ThrusterControlSystem.BurnManeuveringSet( ThrusterControlSystem.ManeuveringAxisID.YAW, m_VirtualJoystick.InputDirection.y );
//                m_ThrusterControlSystem.BurnManeuveringSet( ThrusterControlSystem.ManeuveringAxisID.ROLL, m_VirtualJoystick.InputDirection.z );
//            }



//            // Mouse wheel can traverse control systems
//            // Each control system can represent a set of thruster pairs
//            // The shift can can toggle between set members 1 and 2
//            // For example, the pilot uses the left and right mouse buttons to pivot the ship left and right.
//            // By holding the shift button, the same mouse input causes the ship to pivot up and down
//            // Using the mouse wheel can change to the next system that may control the ship roll, etc.

//            // TODO: A sustained press on a button will increase power exponentially
//            // This will allow us to support small adjustments by tapping, and heavy pushes by holding the button down

//            //if ( Input.GetMouseButton( 0 ) )
//            //{
//            //    m_ThrusterControlSystem.BurnManeuveringSet( ThrusterControlSystem.ManeuveringAxisID.PITCH, 1.0f );
//            //    // Ying
//            //    //foreach ( Thruster thruster in currentThrusterSystem.GetYingThrusters() )
//            //    //{
//            //    //    thruster.Burn( 1.0f );
//            //    //}

//            //    //Debug.Log( "Pressed left click." );
//            //}

//            //if ( Input.GetMouseButton( 1 ) )
//            //{
//            //    m_ThrusterControlSystem.BurnManeuveringSet( ThrusterControlSystem.ManeuveringAxisID.PITCH, -1.0f );
//            //    // Yang
//            //    //foreach ( Thruster thruster in currentThrusterSystem.GetYangThrusters() )
//            //    //{
//            //    //    thruster.Burn( 1.0f );
//            //    //}

//            //    //Debug.Log( "Pressed right click." );
//            //}

//            //if ( Input.GetKey( KeyCode.Space ) )
//            //{
//            //    //foreach ( Thruster thruster in m_ThrusterSystemSets[ m_CurrentThrusterSet ].Item1.GetYingThrusters() )
//            //    foreach ( Thruster thruster in m_EngineThrusters )
//            //    {
//            //        thruster.Burn( 1.0f );
//            //    }
//            //}
//        }
//    }

//    // The thruster control system
//    private ThrusterControlSystem m_ThrusterControlSystem = new ThrusterControlSystem();

//    private Canvas m_Canvas = null;
//    private VirtualJoystick m_VirtualJoystick = null;
//    private ProximityControl m_ProximityControl = null;

//    private bool m_Initialized = false;
//}
