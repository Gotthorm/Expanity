using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using UnityEngine;

// Class for loading the thruster control system configuration data which will define one or more thruster systems.
// The main purpose of this class is to abstract where the data comes from and what format it resides in.
//
// The current data format is XML which describes thruster systems for a single ship.
// Each maneuver system has a name which identifies the axis of control and it also has a pair of opposed thruster groups.
// Each pair are referred to as "ying" and "yang"
//  
// The validation rules are:
// 1) It must be well formed XML
// 
// 2) Thruster system names must be unique within the entire file.
// 3) Thruster names must be unique within each thruster controller.
//
// There is no check whether the thruster game objects exist or whether the thruster system names are valid (exist in the ship).
// These will be checked when the loaded data is registered with the control system.

public class ThrusterSystemLoader : XmlLoader
{
    #region Public Interface

    // Given a relative file path to an XML file, load the file as thruster system config data.
    // This will reset an of thre current state info.
    // Returns true is successful
    public bool Load( string filePath )
    {
        // Clear any existing data
        Reset();

        return InternalLoad( filePath );
    }

    public abstract class ThrusterSystemRecord
    {
        public string Name { get { return m_Name; } }

        public ReadOnlyCollection<string> Ying { get { return m_Ying.AsReadOnly(); } }

        public ReadOnlyCollection<string> Yang { get { return m_Yang.AsReadOnly(); } }

        protected string m_Name;
        protected List<string> m_Ying = null;
        protected List<string> m_Yang = null;
    }

    public abstract class ManeuveringSystemRecord
    {
        public string Name { get { return m_Name; } }

        public int Count { get { return m_ThrusterControls.Count; } }

        public ThrusterSystemRecord GetThrusterSystem( int index )
        {
            if( index >= 0 && index < Count )
            {
                return m_ThrusterControls[ index ];
            }

            return null;
        }

        protected string m_Name;
        protected List<ThrusterSystemRecord> m_ThrusterControls = null;
    }

    public abstract class PropulsionSystemRecord
    {
        public string Name { get { return m_Name; } }

        public ReadOnlyCollection<string> Engines { get { return m_Engines.AsReadOnly(); } }

        protected string m_Name = "Main";
        protected List<string> m_Engines = new List<string>();
    }

    public ReadOnlyCollection<ManeuveringSystemRecord> ManeuveringSystems { get { return m_ManeuveringSystems.AsReadOnly(); } }

    public PropulsionSystemRecord PropulsionSystems { get { return m_PropulsionSystems; } }

    #endregion

    #region Private Interface

    // When a new element is encountered it is pushed onto the current state
    protected override bool Push( XmlReader reader )
    {
        string elementName = reader.Name;
        string token = reader.GetAttribute( "token" );

        bool results = false;

        switch( elementName )
        {
            case m_ThrusterControlSystemLabel:
                {
                    if ( ControlState.INVALID == m_State )
                    {
                        int versionNumber;
                        if ( int.TryParse( token, out versionNumber ) && m_Version == versionNumber )
                        {
                            m_State = ControlState.CONFIG_START;
                            results = true;
                        }
                        else
                        {
                            Debug.LogError( "Version mismatch!  Loader expected <" + m_Version + "> but read <" + versionNumber + ">" );
                        }
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_ManueveringSystemLabel:
                {
                    if ( ( ControlState.CONFIG_START == m_State || ControlState.MANEUVERING_END == m_State || ControlState.PROPULSION_END == m_State ) && null == m_CurrentManeuveringSystem )
                    {
                        // Validate the system name
                        List<string> maneuveringSystemNames;
                        if ( m_ManeuveringSystemNames.TryGetValue( token, out maneuveringSystemNames ) )
                        {
                            // Do not allow duplicate thruster system set names
                            if ( false == m_ManeuveringSystems.Exists( thrusterControlSet => ( thrusterControlSet.Name == token ) ) )
                            {
                                m_State = ControlState.MANEUVERING_START;
                                m_CurrentManeuveringSystem = new ManeuveringSystem( token );
                                m_CurrentManeuveringSystem.m_ValidSystemNames = maneuveringSystemNames;
                                results = true;
                            }
                            else
                            { 
                                Debug.LogError( "Encountered duplicate maneuvering system <" + token + ">" );
                            }
                        }
                        else
                        {
                            Debug.LogError( "Encountered invalid maneuvering system <" + token + ">" );
                        }
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_ThrusterControlLabel:
                {
                    if ( ( ControlState.MANEUVERING_START == m_State || ControlState.THRUSTER_END == m_State ) && null != m_CurrentManeuveringSystem && null == m_CurrentThrusterControl )
                    {
                        // Validate the control name
                        if (    ( "Rotation" == m_CurrentManeuveringSystem.Name && m_CurrentManeuveringSystem.m_ValidSystemNames.Contains( token ) ) ||
                                ( "Lateral" == m_CurrentManeuveringSystem.Name  && m_CurrentManeuveringSystem.m_ValidSystemNames.Contains( token ) ) )
                        {
                            if ( ManeuveringSystem.MaxCount > m_CurrentManeuveringSystem.ThrusterControls.Count )
                            {
                                // Do not allow duplicate thruster system names
                                if ( false == m_CurrentManeuveringSystem.ThrusterControls.Exists( thrusterControl => ( thrusterControl.Name == token ) ) )
                                {
                                    m_State = ControlState.THRUSTER_START;
                                    m_CurrentThrusterControl = new ThrusterControlRecord( token );
                                    results = true;
                                }
                                else
                                {
                                    Debug.LogError( "Encountered duplicate thruster system name <" + token + ">" );
                                }
                            }
                            else
                            {
                                Debug.LogError( "Encountered too many thruster controls for <" + m_CurrentManeuveringSystem.Name + ">" );
                            }
                        }
                        else
                        {
                            Debug.LogError( "Invalid thruster control name <" + m_ThrusterControlLabel + "> found in maneuvering system <" + m_CurrentManeuveringSystem.Name + ">" );
                        }
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_YingLabel:
                {
                    if ( ControlState.THRUSTER_START == m_State && null != m_CurrentThrusterControl && null == m_CurrentThrusterControl.OpenSide )
                    {
                        if ( m_CurrentThrusterControl.OpenYing() )
                        {
                            results = true;
                        }
                        else
                        {
                            Debug.LogError( "Failed to open Ying in thruster control <" + m_CurrentThrusterControl.Name + ">" );
                        }
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_YangLabel:
                {
                    if ( ControlState.THRUSTER_START == m_State && null != m_CurrentThrusterControl && null == m_CurrentThrusterControl.OpenSide )
                    {
                        if ( m_CurrentThrusterControl.OpenYang() )
                        {
                            results = true;
                        }
                        else
                        {
                            Debug.LogError( "Failed to open Yang in thruster control <" + m_CurrentThrusterControl.Name + ">" );
                        }
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_ThrusterLabel:
                {
                    if ( ControlState.THRUSTER_START == m_State && null != m_CurrentThrusterControl && null != m_CurrentThrusterControl.OpenSide )
                    {
                        List<string> openSide = m_CurrentThrusterControl.OpenSide;
                        if ( 0 < token.Length && false == openSide.Contains( token ) )
                        {
                            openSide.Add( token );
                            results = true;
                        }
                        else
                        {
                            Debug.LogError( "Invalid thruster name <" + token + ">" );
                        }
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_PropulsionSystemLabel:
                {
                    if( ( ControlState.CONFIG_START == m_State || ControlState.MANEUVERING_END == m_State ) && 0 == m_PropulsionSystems.Engines.Count )
                    {
                        m_State = ControlState.PROPULSION_START;
                        results = true;
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_EngineLabel:
                {
                    if( ControlState.PROPULSION_START == m_State )
                    {
                        m_PropulsionSystems.AddEngine( token );
                        results = true;
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            default:
                Debug.LogError( "Encountered unhandled element <" + elementName + "> when loading thruster systems!" );
                break;
        }

        return results;
    }

    // When an end element is encountered it is popped off the current state
    protected override bool Pop( XmlReader reader )
    {
        string elementName = reader.Name;

        bool results = false;

        switch( elementName )
        {
            case m_ThrusterControlSystemLabel:
                {
                    if ( ControlState.MANEUVERING_END == m_State || ControlState.PROPULSION_END == m_State || ControlState.CONFIG_START == m_State )
                    {
                        m_State = ControlState.VALID;
                        results = true;
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_ManueveringSystemLabel:
                {
                    if ( ( ControlState.THRUSTER_END == m_State || ControlState.MANEUVERING_START == m_State ) && null == m_CurrentThrusterControl && null != m_CurrentManeuveringSystem )
                    {
                        if ( ManeuveringSystem.MaxCount == m_CurrentManeuveringSystem.ThrusterControls.Count )
                        {
                            m_State = ControlState.MANEUVERING_END;
                            m_ManeuveringSystems.Add( m_CurrentManeuveringSystem );
                            m_CurrentManeuveringSystem = null;
                            m_CurrentThrusterControl = null;
                            results = true;
                        }
                        else
                        {
                            Debug.LogError( "Was expecting " + ManeuveringSystem.MaxCount + " thruster controls in <" + m_CurrentManeuveringSystem.Name + "> but only found " + m_CurrentManeuveringSystem.ThrusterControls.Count );
                        }
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_ThrusterControlLabel:
                {
                    if ( ControlState.THRUSTER_START == m_State && null != m_CurrentThrusterControl && null != m_CurrentManeuveringSystem )
                    {
                        m_State = ControlState.THRUSTER_END;
                        m_CurrentManeuveringSystem.ThrusterControls.Add( m_CurrentThrusterControl );
                        m_CurrentThrusterControl = null;
                        results = true;
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_YingLabel:
                {
                    if ( ControlState.THRUSTER_START == m_State && null != m_CurrentThrusterControl && null != m_CurrentManeuveringSystem )
                    {
                        results = m_CurrentThrusterControl.CloseYing();
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_YangLabel:
                {
                    if ( ControlState.THRUSTER_START == m_State && null != m_CurrentThrusterControl && null != m_CurrentManeuveringSystem )
                    {
                        results = m_CurrentThrusterControl.CloseYang();
                    }
                    else
                    {
                        Debug.LogError( "Unexpected token <" + elementName + ">" );
                    }
                }
                break;
            case m_ThrusterLabel:
                {
                    // These need not exist
                    results = true;
                }
                break;
            case m_PropulsionSystemLabel:
                {
                    if ( ControlState.PROPULSION_START == m_State )
                    {
                        m_State = ControlState.PROPULSION_END;
                        results = true;
                    }
                }
                break;
            case m_EngineLabel:
                {
                    // These need not exist
                    results = true;
                }
                break;
            default:
                Debug.LogError( "Encountered unhandled element <" + elementName + "> when loading thruster systems!" );
                break;
        }

        return results;
    }

    protected override bool WellFormed()
    {
        return ( ControlState.VALID == m_State );
    }

    private void Reset()
    {
        m_State = ControlState.INVALID;
        m_CurrentThrusterControl = null;
        m_CurrentManeuveringSystem = null;
        m_ManeuveringSystems.Clear();
    }

    // A helper class that encapsulates a single thruster control
    private class ThrusterControlRecord : ThrusterSystemRecord
    {
        #region Public Interface

        public ThrusterControlRecord( string name )
        {
            m_Name = name;
        }

        public bool OpenYing()
        {
            if( null == m_OpenSide && null == m_Ying )
            {
                m_Ying = new List<string>();
                m_OpenSide = m_Ying;

                return true;
            }

            return false;
        }

        public bool CloseYing()
        {
            if( null != m_OpenSide && m_OpenSide == m_Ying )
            {
                m_OpenSide = null;

                return true;
            }

            return false;
        }

        public bool OpenYang()
        {
            if ( null == m_OpenSide && null == m_Yang )
            {
                m_Yang = new List<string>();
                m_OpenSide = m_Yang;

                return true;
            }

            return false;
        }

        public bool CloseYang()
        {
            if ( null != m_OpenSide && m_OpenSide == m_Yang )
            {
                m_OpenSide = null;

                return true;
            }

            return false;
        }

        public List<string> OpenSide { get { return m_OpenSide; } }
 
        #endregion

        #region Private Interface

        private List<string> m_OpenSide = null;

        #endregion
    }

    private class ManeuveringSystem : ManeuveringSystemRecord
    {
        public ManeuveringSystem( string name )
        {
            m_Name = name;

            // We expect there to be three thruster controls per set
            m_ThrusterControls = new List<ThrusterSystemRecord>( MaxCount );
        }

        public List<ThrusterSystemRecord> ThrusterControls { get { return m_ThrusterControls; } }

        public List<string> m_ValidSystemNames = null;

        public static int MaxCount = 3;
    }

    private class PropulsionSystem : PropulsionSystemRecord
    {
        public void AddEngine( string locationName )
        {
            m_Engines.Add( locationName );
        }
    }

    private enum ControlState
    {
        INVALID,
        CONFIG_START,
        MANEUVERING_START,
        THRUSTER_START,
        THRUSTER_END,
        MANEUVERING_END,
        PROPULSION_START,
        PROPULSION_END,
        VALID
    }

    private ControlState m_State = ControlState.INVALID;

    private ThrusterControlRecord m_CurrentThrusterControl = null;
    private ManeuveringSystem m_CurrentManeuveringSystem = null;

    private List<ManeuveringSystemRecord> m_ManeuveringSystems = new List<ManeuveringSystemRecord>();
    private PropulsionSystem m_PropulsionSystems = new PropulsionSystem();

    // Which version of PropulsionSystemsConfig is supported by this class
    private int m_Version = 2;

    private const string m_ThrusterControlSystemLabel = "ThrusterControlSystem";
    private const string m_ManueveringSystemLabel = "ManueveringSystem";
    private const string m_PropulsionSystemLabel = "PropulsionSystem";
    private const string m_ThrusterControlLabel = "ThrusterControl";
    private const string m_YingLabel = "Ying";
    private const string m_YangLabel = "Yang";
    private const string m_ThrusterLabel = "Thruster";
    private const string m_EngineLabel = "Engine";

    private readonly Dictionary<string, List<string>> m_ManeuveringSystemNames = new Dictionary<string, List<string>> {
        { "Rotation", new List<string> { "Roll", "Pitch", "Yaw" } },
        { "Lateral", new List<string> { "ForwardBackward", "LeftRight", "UpDown" } } };

    #endregion
}
