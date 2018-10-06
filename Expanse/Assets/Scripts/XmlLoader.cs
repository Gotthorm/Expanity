using System.IO;
using System.Xml;
using UnityEngine;

public abstract class XmlLoader
{
    protected bool InternalLoad( string filePath )
    {
        bool results = false;

        if ( File.Exists( filePath ) )
        {
            using ( XmlReader reader = XmlReader.Create( filePath ) )
            {
                results = true;

                while ( results && reader.Read() )
                {
                    switch ( reader.NodeType )
                    {
                        case XmlNodeType.Element:
                            results &= Push( reader );
                            break;
                        case XmlNodeType.EndElement:
                            results &= Pop( reader );
                            break;
                        default:
                            break;
                    }
                }

                results &= WellFormed();
            }
        }
        else
        {
            Debug.LogError( "Was expecting to find: " + filePath );
        }

        return results;
    }

    protected abstract bool Push( XmlReader reader );

    protected abstract bool Pop( XmlReader reader );

    protected abstract bool WellFormed();
}
