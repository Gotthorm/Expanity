using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialPlanetIcon : CelestialIcon
{
    public GameObject m_PointerIcon = null;

    private void Awake()
    {
        m_Icon = m_PointerIcon;
        m_Icon.SetActive( true );
    }
}
