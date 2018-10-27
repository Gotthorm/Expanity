using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class CelestialBodyLoader : XmlLoader
{
    public string m_Name = "";
    public uint m_Radius = 0;
    public uint m_MaxScale = 0;
    public bool m_Orbit = true;
    public List<float> m_MeanLongitude = new List<float>();
    public List<float> m_SemiMajorAxisOfOrbit = new List<float>();
    public List<float> m_EccentricityOfOrbit = new List<float>();
    public List<float> m_InclinationOnPlaneOfEcliptic = new List<float>();
    public List<float> m_ArgumentOfPerihelion = new List<float>();
    public List<float> m_LongitudeOfAscendingNode = new List<float>();
    public List<float> m_MeanAnomaly = new List<float>();
    public string m_PrefabDataPath = "";

    public bool Load( string filePath )
    {
        // Clear any existing data
        Reset();

        if( InternalLoad( filePath ) )
        {
            // Verify that everything was read correctly
            if( 0 == m_Name.Length )
            {
                Debug.LogError( "Error" );
            }
            else if ( m_Radius == 0 )
            {
                Debug.LogError( "Error" );
            }
            else if ( m_MaxScale == 0 )
            {
                Debug.LogError( "Error" );
            }
            else if ( m_MeanLongitude.Count == 0 )
            {
                Debug.LogError( "Error" );
            }
            else if ( m_SemiMajorAxisOfOrbit.Count == 0 )
            {
                Debug.LogError( "Error" );
            }
            else if ( m_EccentricityOfOrbit.Count == 0 )
            {
                Debug.LogError( "Error" );
            }
            else if ( m_InclinationOnPlaneOfEcliptic.Count == 0 )
            {
                Debug.LogError( "Error" );
            }
            else if ( m_Name != "Earth" && m_Name != "Sol" && m_ArgumentOfPerihelion.Count == 0 )
            {
                Debug.LogError( "Error" );
            }
            else if ( m_Name != "Earth" && m_Name != "Sol" && m_LongitudeOfAscendingNode.Count == 0 )
            {
                Debug.LogError( "Error" );
            }
            else if( 0 == m_PrefabDataPath.Length )
            {
                Debug.LogError( "Error" );
            }
            else
            {
                return true;
            }
        }

        Debug.LogError( "CelestialBodyLoader failed to load: " + filePath );

        return false;
    }

    // Implementation of XmlLoader interface 
    protected override bool Push( XmlReader reader )
    {
        bool results = false;

        string elementName = reader.Name;
        string elementValue = reader.GetAttribute( "value" );

        switch( reader.Name )
        {
            case m_PlanetLabel:
                {
                    if ( 0 < elementValue.Length && 0 == m_Name.Length )
                    {
                        m_Name = elementValue;
                        results = true;
                    }
                    ++m_StackSize;
                }
                break;
            case m_GeometryPrefabLabel:
                {
                    if( 0 == m_PrefabDataPath.Length )
                    {
                        //string dataPath = Application.dataPath + "/Resources/" + elementValue + ".prefab";
                        string dataPath = elementValue;
                        //if ( File.Exists( dataPath ) )
                        {
                            m_PrefabDataPath = elementValue;
                            results = true;
                        }
                        //else
                        //{
                        //    Debug.LogError("Missing prefab: " + dataPath);
                        //}
                    }
                }
                break;
            case m_RadiusLabel:
                {
                    if ( 0 == m_Radius )
                    {
                        results = uint.TryParse( elementValue, out m_Radius );
                    }
                }
                break;
            case m_MaximumScaleLabel:
                {
                    if( 0 == m_MaxScale )
                    {
                        results = uint.TryParse( elementValue, out m_MaxScale );
                    }
                }
                break;
            case m_MeanLongitudeLabel:
                {
                    results = ProcessFloats( elementValue, m_MeanLongitude );
                }
                break;
            case m_SemiMajorAxisOfOrbitLabel:
                {
                    results = ProcessFloats( elementValue, m_SemiMajorAxisOfOrbit );
                }
                break;
            case m_EccentricityOfOrbitLabel:
                {
                    results = ProcessFloats( elementValue, m_EccentricityOfOrbit );
                }
                break;
            case m_InclinationOnPlaneOfEclipticLabel:
                {
                    results = ProcessFloats( elementValue, m_InclinationOnPlaneOfEcliptic );
                }
                break;
            case m_ArgumentOfPerihelionLabel:
                {
                    results = ProcessFloats( elementValue, m_ArgumentOfPerihelion );
                }
                break;
            case m_LongitudeOfAscendingNodeLabel:
                {
                    results = ProcessFloats( elementValue, m_LongitudeOfAscendingNode );
                }
                break;
            case m_MeanAnomalyLabel:
                {
                    results = ProcessFloats( elementValue, m_MeanAnomaly );
                }
                break;
            case m_OrbitFlagLabel:
                {
                    results = bool.TryParse( elementValue, out m_Orbit );
                }
                break;

            default:
                {
                    Debug.LogError( "Invalid token: " + reader.Name );
                }
                break;
        }

        return results;
    }

    // Implementation of XmlLoader interface 
    protected override bool Pop( XmlReader reader )
    {
        --m_StackSize;

        return true;
    }

    // Implementation of XmlLoader interface 
    protected override bool WellFormed()
    {
        return m_StackSize == 0;
    }

    private void Reset()
    {
        //m_CelestialBody = null;
        m_Name = "";
        m_Radius = 0;
        m_MaxScale = 0;
        m_MeanLongitude = new List<float>();
        m_SemiMajorAxisOfOrbit = new List<float>();
        m_EccentricityOfOrbit = new List<float>();
        m_InclinationOnPlaneOfEcliptic = new List<float>();
        m_ArgumentOfPerihelion = new List<float>();
        m_LongitudeOfAscendingNode = new List<float>();
        //m_GameObjectInstance = null;
        m_PrefabDataPath = "";
        m_StackSize = 0;
        m_Orbit = true;
    }

    // Given an input string, attempt to convert it to a list of float values
    // Given float list must be empty
    private bool ProcessFloats( string inputString, List<float> data )
    {
        bool results = false;

        if ( 0 == data.Count )
        {
            string[] valueStringList = inputString.Split( ' ' );

            if ( 0 < valueStringList.Length )
            {
                results = true;

                foreach ( string valueString in valueStringList )
                {
                    float value;
                    if ( float.TryParse( valueString, out value ) )
                    {
                        data.Add( value );
                    }
                    else
                    {
                        results = false;
                    }
                }
            }
        }

        return results;
    }

    private uint m_StackSize = 0;

    private const string m_PlanetLabel = "Planet";
    private const string m_GeometryPrefabLabel = "GeometryPrefab";
    private const string m_RadiusLabel = "Radius";
    private const string m_MaximumScaleLabel = "MaximumScale";
    private const string m_MeanLongitudeLabel = "MeanLongitude";
    private const string m_SemiMajorAxisOfOrbitLabel = "SemiMajorAxisOfOrbit";
    private const string m_EccentricityOfOrbitLabel = "EccentricityOfOrbit";
    private const string m_InclinationOnPlaneOfEclipticLabel = "InclinationOnPlaneOfEcliptic";
    private const string m_ArgumentOfPerihelionLabel = "ArgumentOfPerihelion";
    private const string m_LongitudeOfAscendingNodeLabel = "LongitudeOfAscendingNode";
    private const string m_MeanAnomalyLabel = "MeanAnomaly";
    private const string m_OrbitFlagLabel = "Orbit";
}
