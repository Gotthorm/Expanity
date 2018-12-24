using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CelestialIcon : MonoBehaviour
{
    public Color Color
    {
        set
        {
            if ( m_Icon != null )
            {
                Image image = m_Icon.GetComponent<Image>();
                if ( image != null )
                {
                    image.color = value;
                }
            }
        }
    }

    public virtual void UpdateState( CelestialBody owner, Camera camera )
    {
    }

    protected GameObject m_Icon = null;
}
