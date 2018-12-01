using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class SpaceShip : MonoBehaviour
{
    #region Public Interface

    public CelestialVector3 Position { get; set; }

    public bool Init()
    {
        // TODO: Will load config data, initialize systems, etc.
        return true;
    }

    #endregion

    #region Private Interface

    // Use this for initialization
    //private void Start ()
    //{
    //    // Find all of the thrusters in this ship
    //    List<Thruster> thrusterList = new List<Thruster>();
    //    CollectThrusters( this.gameObject, thrusterList );

    //    //GameObject gameObject = new GameObject( "Control System", typeof( ControlSystem ) );
    //    //gameObject.transform.SetParent( this.gameObject.transform );

    //    m_ControlSystem = gameObject.GetComponent<ControlSystem>();
    //    if ( m_ControlSystem != null )
    //    {
    //        m_ControlSystem.Initialize( this, thrusterList );
    //    }
    //}

    // Update is called once per frame
    private void Update ()
    {
        //m_ControlSystem.Update();
    }

    // Helper method for collecting all of the thruster game objects attached to this ship
    private void CollectThrusters( GameObject gameObject, List<Thruster> thrustersList )
    {
        Thruster childThruster = gameObject.GetComponent<Thruster>();

        if ( null != childThruster )
        {
            // Found a thruster
            thrustersList.Add( childThruster );
        }

        foreach ( Transform child in gameObject.transform )
        {
            CollectThrusters( child.gameObject, thrustersList );
        }
    }

    // The control system that encapsulates all controls
    private ControlSystem m_ControlSystem = null;

    #endregion
}
