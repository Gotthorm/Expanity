using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialShipIcon : CelestialIcon
{
    public GameObject m_SideIcon = null;
    public GameObject m_FrontIcon = null;
    public GameObject m_BackIcon = null;

    public override void UpdateState( CelestialBody owner, Camera camera )
    {
        base.UpdateState( owner, camera );

        Vector3 differenceVector = camera.transform.position - owner.transform.position;
        differenceVector.Normalize();

        float dotProduct = Vector3.Dot( owner.transform.forward, differenceVector );

        if ( dotProduct > 0.9f )
        {
            SetIcon( m_BackIcon, 0.0f );
        }
        else if ( dotProduct < -0.9f )
        {
            SetIcon( m_FrontIcon, 0.0f );
        }
        else
        {
            Vector3 crossProduct = Vector3.Cross( owner.transform.forward, camera.transform.forward );
            Vector2 crossProduct2D = crossProduct;
            bool sign = ( Vector2.Dot( crossProduct2D, Vector2.up ) >= 0.0f ) ? false : true;

            Vector3 screenPos = Vector3.ProjectOnPlane( owner.transform.forward, differenceVector );

            Vector2 screenPos2D = screenPos;

            if ( dotProduct >= 0.0f )
            {
                screenPos2D.x = -screenPos2D.x;
            }
            screenPos2D.Normalize();

            float angle = Vector2.Angle( screenPos2D, Vector2.up );
            SetIcon( m_SideIcon, sign ? -angle : angle );
        }
    }

    private void Awake()
    {
        SetIcon( m_FrontIcon, 0.0f );
    }

    private void SetIcon( GameObject icon, float rotation )
    {
        m_SideIcon.SetActive( false );
        m_FrontIcon.SetActive( false );
        m_BackIcon.SetActive( false );

        m_Icon = icon;
        m_Icon.SetActive( true );

        m_Icon.transform.eulerAngles = new Vector3( 0, 0, rotation );
    }
}
