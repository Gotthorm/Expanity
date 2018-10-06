using UnityEngine;
using System.Collections;

public class Thruster : MonoBehaviour
{
    // Tunables

    [Tooltip( "The parent ship which must contain a rigidbody" )]
    public GameObject ParentShip;

    [Tooltip( "blah blah" )]
    public Class m_Class;

    // Burn the thruster at the given power level (0.0 - 1.0)
    public void Burn( float power )
    {
        m_ParentRigidBody.AddForceAtPosition( transform.forward * -m_CurrentThrust * power, transform.position, ForceMode.Force );
    }

    public enum Class
    {
        MANEUVERING,
        PROPULSION
    }

    private void Start()
    {
        if ( null != ParentShip )
        {
            m_ParentRigidBody = ParentShip.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError( this.name +" thruster was not properly connected to a ship!" );
        }
    }

    private void OnValidate()
    {
        // Validate the set parent ship
        if ( null != ParentShip )
        {
            if ( null == ParentShip.GetComponent<Rigidbody>() )
            {
                ParentShip = null;
            }
        }

        m_CurrentThrust = m_MaximumThrust[ (int)m_Class ];
    }

    // The parent structure this thruster is attached to
    private Rigidbody m_ParentRigidBody = null;

    // Defined by the thruster type
    private float[] m_MaximumThrust = { 0.001f, 100.0f };
    private float m_CurrentThrust = 0.0f;
}
