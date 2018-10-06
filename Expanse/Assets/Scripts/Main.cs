using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public Canvas m_Canvas = null;
    public CelestialCamera m_Camera = null;

    // Use this for initialization
    void Awake()
    {
        CelestialManager celestialManager = CelestialManager.CreateInstance();

        celestialManager.SetActiveCamera( m_Camera );

        //GameObject gameObject = new GameObject();

        //if ( null != gameObject )
        //{
        //    gameObject.name = "Celestial Manager";

        //    m_CelestialManager = gameObject.AddComponent<CelestialManager>();

        //    m_CelestialManager.m_Camera = m_Camera;
        //    m_CelestialManager.m_FarRange = 200.0f;
        //    m_CelestialManager.m_CloseRange = 1e-05f;
        //    m_CelestialManager.m_Canvas = m_Canvas;

        //    m_CelestialManager.Init();

        //    ProximityControlPanel proximityControlPanel = GameObject.FindObjectOfType<ProximityControlPanel>();
        //    if( null != proximityControlPanel )
        //    {
        //        proximityControlPanel.m_CelestialManager = m_CelestialManager;
        //    }
        //}
    }

    // Update is called once per frame
    void Update ()
    {
    }

    //private CelestialManager m_CelestialManager = null;
}
