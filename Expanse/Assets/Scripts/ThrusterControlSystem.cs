using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class ThrusterControlSystem
{
    #region Public Interface

    public bool Initialize( string shipName, Rigidbody parent, List<Thruster> thrusters )
    {
        m_Parent = parent;

        m_AxisCount = Enum.GetNames( typeof( ManeuveringAxisID ) ).Length;

        string filePath = Application.dataPath + "/StreamingAssets/Config/" + shipName + "/ThrusterControlSystemConfig.xml";

        ThrusterSystemLoader thrusterSystemLoader = new ThrusterSystemLoader();

        if ( false == thrusterSystemLoader.Load( filePath ) )
        {
            Debug.LogError( "Failed to load thruster system data for the ship <" + shipName + ">" );
            return false;
        }

        // Register each thruster system
        foreach( ThrusterSystemLoader.ManeuveringSystemRecord maneuverSystemSetRecord in thrusterSystemLoader.ManeuveringSystems )
        {
            if( false == RegisterThrusterSystemSet( thrusters, maneuverSystemSetRecord.Name, maneuverSystemSetRecord ) )
            {
                Debug.LogWarning( "Failed to register thruster system set <" + maneuverSystemSetRecord.Name + ">" );
                return false;
            }
        }

        foreach( string engineName in thrusterSystemLoader.PropulsionSystems.Engines )
        {
            Thruster engineThruster = null;
            if ( RegisterEngineThruster( thrusters, engineName, ref engineThruster ) )
            {
                m_EngineThrusters.Add( engineThruster );
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public enum ManeuveringAxisID
    {
        PITCH = 0,
        ROLL,
        YAW,
    };

    public void BurnManeuveringSet( ManeuveringAxisID axisID, float power )
    {
        if ( m_ThrusterSystemSets.Count > m_CurrentThrusterSet )
        {
            ThrusterSystem currentThrusterSystem = null;

            int thrusterIndex = (int)axisID;

            currentThrusterSystem = m_ThrusterSystemSets[ m_CurrentThrusterSet ][ (int)axisID ];

            if( null != currentThrusterSystem )
            {
                List<Thruster> thrusterList = null;

                if( power >= 0.0f )
                {
                    thrusterList = currentThrusterSystem.GetYingThrusters();
                }
                else
                {
                    thrusterList = currentThrusterSystem.GetYangThrusters();
                    power = -power;
                }

                foreach ( Thruster thruster in thrusterList )
                {
                    thruster.Burn( power );
                }
            }
        }
    }

    public uint ManeuveringSetCount { get; }

    public uint ActiveManeuveringSet { get; set; }

    public bool SetActiveManeuveringSet( uint setID )
    {
        if( setID < ManeuveringSetCount )
        {
            ActiveManeuveringSet = setID;
            return true;
        }

        return false;
    }

    //// Update is called once per frame
    //public void Update()
    //{
    //    // Mouse wheel can traverse control systems
    //    // Each control system can represent a set of thruster pairs
    //    // The shift can can toggle between set members 1 and 2
    //    // For example, the pilot uses the left and right mouse buttons to pivot the ship left and right.
    //    // By holding the shift button, the same mouse input causes the ship to pivot up and down
    //    // Using the mouse wheel can change to the next system that may control the ship roll, etc.

    //    if ( null != m_Parent )
    //    {
    //        if ( m_ThrusterSystemSets.Count > m_CurrentThrusterSet )
    //        {
    //            ThrusterSystem currentThrusterSystem = null;

    //            if( Input.GetKey( KeyCode.LeftAlt ) )
    //            {
    //                currentThrusterSystem = m_ThrusterSystemSets[ m_CurrentThrusterSet ].Item2;
    //            }
    //            else if ( Input.GetKey( KeyCode.LeftControl ) )
    //            {
    //                currentThrusterSystem = m_ThrusterSystemSets[ m_CurrentThrusterSet ].Item3;
    //            }
    //            else
    //            {
    //                currentThrusterSystem = m_ThrusterSystemSets[ m_CurrentThrusterSet ].Item1;
    //            }

    //            // TODO: A sustained press on a button will increase power exponentially
    //            // This will allow us to support small adjustments by tapping, and heavy pushes by holding the button down

    //            if ( Input.GetMouseButton( 0 ) )
    //            {
    //                // Ying
    //                foreach ( Thruster thruster in currentThrusterSystem.GetYingThrusters() )
    //                {
    //                    thruster.Burn( 1.0f );
    //                }

    //                //Debug.Log( "Pressed left click." );
    //            }

    //            if ( Input.GetMouseButton( 1 ) )
    //            {
    //                // Yang
    //                foreach ( Thruster thruster in currentThrusterSystem.GetYangThrusters() )
    //                {
    //                    thruster.Burn( 1.0f );
    //                }

    //                //Debug.Log( "Pressed right click." );
    //            }
    //        }

    //        if( Input.GetKey( KeyCode.Space ) )
    //        {
    //            //foreach ( Thruster thruster in m_ThrusterSystemSets[ m_CurrentThrusterSet ].Item1.GetYingThrusters() )
    //            foreach ( Thruster thruster in m_EngineThrusters )
    //            {
    //                thruster.Burn( 1.0f );
    //            }
    //        }
    //    }
    //}

    #endregion

    #region Private Interface

    private bool CollectThrusterObjects( List<Thruster> globalThrusterList, ReadOnlyCollection<string> thrusterNames, List<Thruster> shipThrusterList )
    {
        foreach ( string thrusterName in thrusterNames )
        {
            foreach ( Thruster thruster in globalThrusterList )
            {
                if ( thruster.gameObject.name == thrusterName )
                {
                    shipThrusterList.Add( thruster );
                    break;
                }
            }
        }

        return true;
    }

    private bool RegisterThrusterSystemSet( List<Thruster> thrusters, string thrusterSetName, ThrusterSystemLoader.ManeuveringSystemRecord maneuveringSystemRecord )
    {
        List<ThrusterSystem> thrusterSystemList = new List<ThrusterSystem>();

        for ( int index = 0; index < maneuveringSystemRecord.Count; ++index )
        {
            ThrusterSystemLoader.ThrusterSystemRecord thrusterSystemRecord = maneuveringSystemRecord.GetThrusterSystem( index );

            if ( null != thrusterSystemRecord )
            {
                ThrusterSystem thrusterSystem = null;
                if ( RegisterThrusterSystem( thrusters, thrusterSystemRecord.Name, thrusterSystemRecord.Ying, thrusterSystemRecord.Yang, ref thrusterSystem ) )
                {
                    thrusterSystemList.Add( thrusterSystem );
                }
            }
        }

        if ( thrusterSystemList.Count == maneuveringSystemRecord.Count )
        {
            m_ThrusterSystemSets.Add( thrusterSystemList );

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool RegisterThrusterSystem( List<Thruster> thrusters, string thrusterName, ReadOnlyCollection<string> yingNames, ReadOnlyCollection<string> yangNames, ref ThrusterSystem newThrusterSystem )
    {
        // We expect every thruster name found in the ying and yang collections to be the name of an actual thruster game object
        List<Thruster> yingThrusters = new List<Thruster>();

        if ( false == CollectThrusterObjects( thrusters, yingNames, yingThrusters ) )
        {
            return false;
        }

        List<Thruster> yangThrusters = new List<Thruster>();

        if ( false == CollectThrusterObjects( thrusters, yangNames, yangThrusters ) )
        {
            return false;
        }

        newThrusterSystem = new ThrusterSystem( yingThrusters, yangThrusters );
        //m_ThrusterSystems.Add( new ThrusterSystem( yingThrusters, yangThrusters ) );

        //Debug.Log( "Registered thruster <" + thrusterName + "> at index " + (m_ThrusterSystems.Count - 1).ToString() );

        return true;
    }

    private bool RegisterEngineThruster( List<Thruster> globalThrusterList, string thrusterName, ref Thruster newThruster )
    {
        foreach ( Thruster thruster in globalThrusterList )
        {
            if ( thruster.gameObject.name == thrusterName )
            {
                newThruster = thruster;
                return true;
            }
        }
        return false;
    }

    // The control system must be registered with the ship that it controls
    private Rigidbody m_Parent = null;

    private int m_CurrentThrusterSet = 0;

    // This is the collection of active thruster systems
    private List<List<ThrusterSystem>> m_ThrusterSystemSets = new List<List<ThrusterSystem>>();

    private List<Thruster> m_EngineThrusters = new List<Thruster>();

    private int m_AxisCount;

    #endregion
}

