using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterSystem
{
    public ThrusterSystem( List<Thruster> ying, List<Thruster> yang )
    {
        m_Ying = ying;
        m_Yang = yang;
    }

    public void Invert() { m_Invert = !m_Invert; }

    public List<Thruster> GetYingThrusters() { return m_Invert ? m_Yang : m_Ying; }
    public List<Thruster> GetYangThrusters() { return m_Invert ? m_Ying : m_Yang; }

    // Invert Ying/Yang
    private bool m_Invert = false;

    // The thruster collections
    private List<Thruster> m_Ying = null;
    private List<Thruster> m_Yang = null;
}
