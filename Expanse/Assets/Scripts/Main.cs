using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [Tooltip( "This must be assigned in the editor" )]
    public Canvas m_Canvas = null;

    [Tooltip( "This must be assigned in the editor" )]
    public CelestialCamera m_Camera = null;

    // Use this for initialization
    void Awake()
    {
        // This must be done first.  With no celestial objects, space is EMPTY!!
        {
            // Initialization cannot fail but will log warnings for any data it fails to load
            CelestialManager.Instance.Init( Application.dataPath + "/StreamingAssets/Config/CelestialBodies/" );
        }
    }

    // Update is called once per frame
    void Update ()
    {
    }
}
