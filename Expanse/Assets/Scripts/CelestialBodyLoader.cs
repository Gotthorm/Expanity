using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class CelestialBodyLoader : XmlLoader
{
    public string m_Name = "";
    public string m_Type = "";
    public float m_Radius = 0.0f;
    public string m_PrefabDataPath = "";

    public bool Load( string filePath )
    {
        // Clear any existing data
        Reset();

        if( InternalLoad( filePath ) )
        {
            // Verify that everything was read correctly
            return Verify();
        }

        Debug.LogError( "CelestialBodyLoader failed to load: " + filePath );

        return false;
    }

    public bool Load( FileInfo fileInfo )
    {
        // Clear any existing data
        Reset();

        if ( InternalLoad( fileInfo ) )
        {
            // Verify that everything was read correctly
            return Verify();
        }

        Debug.LogError( "CelestialBodyLoader failed to load: " + fileInfo.Name );

        return false;
    }

    public bool GetData( string dataName, ref List<int> dataList )
    {
        if( m_IntegerData.ContainsKey(dataName) )
        {
            dataList = m_IntegerData[ dataName ];
            return true;
        }

        return false;
    }

    public bool GetData( string dataName, ref List<bool> dataList )
    {
        if ( m_BooleanData.ContainsKey( dataName ) )
        {
            dataList = m_BooleanData[ dataName ];
            return true;
        }

        return false;
    }

    public bool GetData( string dataName, ref List<float> dataList )
    {
        if ( m_FloatData.ContainsKey( dataName ) )
        {
            dataList = m_FloatData[ dataName ];
            return true;
        }

        return false;
    }

    public bool GetData( string dataName, ref string dataString )
    {
        if( m_StringData.ContainsKey( dataName ) )
        {
            dataString = m_StringData[ dataName ];
            return true;
        }

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
            case m_CelestialBodyLabel:
                {
                    if ( 0 == m_StackSize )
                    {
                        results = true;
                    }
                    ++m_StackSize;
                }
                break;
            case m_NameLabel:
                {
                    if ( 0 < elementValue.Length && 0 == m_Name.Length )
                    {
                        m_Name = elementValue;
                        results = true;
                    }
                }
                break;
            case m_TypeLabel:
                {
                    if ( 0 < elementValue.Length && 0 == m_Type.Length )
                    {
                        m_Type = elementValue;
                        results = true;
                    }
                }
                break;
            case m_RadiusLabel:
                {
                    if ( 0 < elementValue.Length && 0.0 == m_Radius )
                    {
                        float value;
                        if ( float.TryParse( elementValue, out value ) )
                        {
                            m_Radius = value;
                            results = true;
                        }
                    }
                }
                break;
            case m_GeometryPrefabLabel:
                {
                    if( 0 < elementValue.Length && 0 == m_PrefabDataPath.Length )
                    {
                        m_PrefabDataPath = elementValue;
                        results = true;
                    }
                }
                break;
            default:
                {
                    if ( m_StackSize > 0 )
                    {
                        ValueDataType dataType = GetDataType( elementValue );

                        switch ( dataType )
                        {
                            case ValueDataType.BOOL:
                                {
                                    results = AddAsBooleanData( reader.Name, elementValue );
                                }
                                break;
                            case ValueDataType.INT:
                                {
                                    results = AddAsIntegerData( reader.Name, elementValue );
                                }
                                break;
                            case ValueDataType.FLOAT:
                                {
                                    results = AddAsFloatData( reader.Name, elementValue );
                                }
                                break;
                            case ValueDataType.STRING:
                                {
                                    results = AddAsStringData( reader.Name, elementValue );
                                }
                                break;
                            default:
                                {
                                    Debug.LogError( "Invalid value for token: " + reader.Name );
                                }
                                break;
                        }
                    }
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

    private enum ValueDataType { INVALID, BOOL, INT, FLOAT, STRING }

    private void Reset()
    {
        //m_CelestialBody = null;
        m_Name = "";
        m_Type = "";
        m_Radius = 0.0f;
        m_PrefabDataPath = "";

        m_BooleanData.Clear();
        m_FloatData.Clear();
        m_IntegerData.Clear();
        m_StringData.Clear();
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

    private bool Verify()
    {
        if ( 0 == m_Name.Length )
        {
            Debug.LogError( "Error" );
        }
        else if ( 0 == m_Type.Length )
        {
            Debug.LogError( "Error" );
        }
        else if ( 0 == m_PrefabDataPath.Length )
        {
            Debug.LogError( "Error" );
        }
        else if ( 0.0 == m_Radius )
        {
            Debug.LogError( "Error" );
        }
        else
        {
            return true;
        }

        return false;
    }

    private ValueDataType GetDataType(string inputString)
    {
        if ( inputString.Length > 0 )
        {
            if ( TestAsBooleans( inputString ) )
            {
                return ValueDataType.BOOL;
            }
            else if ( TestAsIntegers( inputString ) )
            {
                return ValueDataType.INT;
            }
            else if ( TestAsFloats( inputString ) )
            {
                return ValueDataType.FLOAT;
            }
            else
            {
                return ValueDataType.STRING;
            }
        }

        return ValueDataType.INVALID;
    }

    private bool TestAsBooleans( string inputString )
    {
        bool results = false;

        string[] valueStringList = inputString.Split( ' ' );

        if ( 0 < valueStringList.Length )
        {
            results = true;

            foreach ( string valueString in valueStringList )
            {
                bool value;
                if ( false == bool.TryParse( valueString, out value ) )
                {
                    results = false;
                    break;
                }
            }
        }

        return results;
    }

    private bool TestAsIntegers( string inputString )
    {
        bool results = false;

        string[] valueStringList = inputString.Split( ' ' );

        if ( 0 < valueStringList.Length )
        {
            results = true;

            foreach ( string valueString in valueStringList )
            {
                int value;
                if ( false == int.TryParse( valueString, out value ) )
                {
                    results = false;
                    break;
                }
            }
        }

        return results;
    }

    private bool TestAsFloats( string inputString )
    {
        bool results = false;

        string[] valueStringList = inputString.Split( ' ' );

        if ( 0 < valueStringList.Length )
        {
            results = true;

            foreach ( string valueString in valueStringList )
            {
                float value;
                if ( false == float.TryParse( valueString, out value ) )
                {
                    results = false;
                    break;
                }
            }
        }

        return results;
    }

    private bool AddAsBooleanData( string booleanName, string booleanString )
    {
        List<bool> dataList = new List<bool>();

        // Name must be unique
        if ( m_BooleanData.ContainsKey( booleanName ) )
        {
            Debug.LogError( "Name of boolean data (" + booleanName + ") is not unique!" );
        }
        else
        {
            string[] valueStringList = booleanString.Split( ' ' );

            foreach ( string valueString in valueStringList )
            {
                bool value;

                if ( false == bool.TryParse( valueString, out value ) )
                {
                    Debug.LogError( "Found invalid token in boolean data (" + booleanName + ")!" );

                    return false;
                }
                else
                {
                    dataList.Add( value );
                }
            }

            if ( dataList.Count > 0 )
            {
                m_BooleanData[ booleanName ] = dataList;

                return true;
            }
        }

        return false;
    }

    private bool AddAsIntegerData( string integerName, string integerString )
    {
        List<int> dataList = new List<int>();

        // Name must be unique
        if ( m_IntegerData.ContainsKey( integerName ) )
        {
            Debug.LogError( "Name of integer data (" + integerName + ") is not unique!" );
        }
        else
        {
            string[] valueStringList = integerString.Split( ' ' );

            foreach ( string valueString in valueStringList )
            {
                int value;

                if ( false == int.TryParse( valueString, out value ) )
                {
                    Debug.LogError( "Found invalid token in integer data (" + integerName + ")!" );

                    break;
                }
                else
                {
                    dataList.Add( value );
                }
            }

            if ( dataList.Count > 0 )
            {
                m_IntegerData[ integerName ] = dataList;

                return true;
            }
        }

        return false;
    }

    private bool AddAsFloatData( string floatName, string floatString )
    {
        List<float> dataList = new List<float>();

        // Name must be unique
        if ( m_FloatData.ContainsKey( floatName ) )
        {
            Debug.LogError( "Name of float data (" + floatName + ") is not unique!" );
        }
        else
        {
            string[] valueStringList = floatString.Split( ' ' );

            foreach ( string valueString in valueStringList )
            {
                float value;

                if ( false == float.TryParse( valueString, out value ) )
                {
                    Debug.LogError( "Found invalid token in float data (" + floatName + ")!" );

                    break;
                }
                else
                {
                    dataList.Add( value );
                }
            }

            if ( dataList.Count > 0 )
            {
                m_FloatData[ floatName ] = dataList;

                return true;
            }
        }

        return false;
    }

    private bool AddAsStringData( string stringName, string stringString )
    {
        // Name must be unique
        if ( m_FloatData.ContainsKey( stringName ) )
        {
            Debug.LogError( "Name of string data (" + stringName + ") is not unique!" );

            return false;
        }
        else
        {
            m_StringData[ stringName ] = stringString;

            return true;
        }
    }

    private uint m_StackSize = 0;

    private Dictionary<string, List<bool>> m_BooleanData = new Dictionary<string, List<bool>>();
    private Dictionary<string, List<int>> m_IntegerData = new Dictionary<string, List<int>>();
    private Dictionary<string, List<float>> m_FloatData = new Dictionary<string, List<float>>();
    private Dictionary<string, string> m_StringData = new Dictionary<string, string>();

    private const string m_CelestialBodyLabel = "CelestialBody";
    private const string m_NameLabel = "Name";
    private const string m_TypeLabel = "Type";
    private const string m_RadiusLabel = "Radius";
    private const string m_GeometryPrefabLabel = "GeometryPrefab";
}
