using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CelestialTime : MonoBehaviour
{
    public Text m_ActualText = null;
    public InputField m_CurrentText = null;

    public double Actual
    {
        get
        {
            return m_Actual;
        }
    }

    public double Current
    {
        get
        {
            return m_Current;
        }
    }

    public static CelestialTime Instance
    {
        get
        {
            return m_Instance;
        }
    }

    public void IncreaseCurrentTimePress()
    {
        m_Synced = false;
        m_Increasing = true;
        m_Decreasing = false;
        m_Current += m_IncrementUnit;
        m_PressDuration = 0.0f;
    }

    public void IncreaseCurrentTimeRelease()
    {
        m_Increasing = false;
    }

    public void DecreaseCurrentTimePress()
    {
        m_Synced = false;
        m_Decreasing = true;
        m_Increasing = false;
        m_Current -= m_IncrementUnit;
        m_PressDuration = 0.0f;
    }

    public void DecreaseCurrentTimeRelease()
    {
        m_Decreasing = false;
    }

    public void SyncCurrentClicked()
    {
        m_Synced = true;
    }

    private void Awake()
    {
        m_Instance = this;
    }

    private void Update()
    {
        if ( m_Increasing || m_Decreasing )
        {
            m_PressDuration += Time.deltaTime;

            if( m_PressDuration > 1.0f )
            {
                m_Current += ( m_Increasing ) ? m_IncrementUnit : -m_IncrementUnit;
            }
        }

        m_Actual = PlanetPositionUtility.GetJulianDate( System.DateTime.Now );

        if( m_ActualText != null )
        {
            m_ActualText.text = m_Actual.ToString(".00000");
        }

        if( m_Synced )
        {
            m_Current = m_Actual;
        }

        if ( m_CurrentText != null )
        {
            m_CurrentText.text = m_Current.ToString( ".00000" );
        }
    }

    private static CelestialTime m_Instance = null;

    private double m_Actual = 0.0;
    private double m_Current = 0.0;
    private bool m_Synced = true;
    private bool m_Increasing = false;
    private bool m_Decreasing = false;
    private double m_IncrementUnit = 0.00001;
    private float m_PressDuration = 0.0f;
}
