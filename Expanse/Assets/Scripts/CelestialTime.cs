using UnityEngine;
using UnityEngine.UI;
using System;

public class CelestialTime : MonoBehaviour
{
    public Text m_CurrentTimeText = null;
    public Text m_VirtualTimeText = null;

    public Text m_CurrentJulianDateText = null;
    public Text m_VirtualJulianDateText = null;

    public double Actual
    {
        get
        {
            return m_CurrentJulianDate;
        }
    }

    public double Current
    {
        get
        {
            return m_VirtualJulianDate;
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
        m_VirtualTime = new DateTime( m_VirtualTime.Ticks + m_TimeIncrementUnit );
        m_PressDuration = 0.0f;
        Debug.Log( m_VirtualTime.ToLongTimeString() );
    }

    public void IncreaseCurrentTimeRelease()
    {
        m_Increasing = false;
        m_CurrentTimeIncrement = m_MinCurrentTimeIncrement;
    }

    public void DecreaseCurrentTimePress()
    {
        m_Synced = false;
        m_Decreasing = true;
        m_Increasing = false;
        m_VirtualTime = new DateTime( m_VirtualTime.Ticks - m_TimeIncrementUnit );
        m_PressDuration = 0.0f;
    }

    public void DecreaseCurrentTimeRelease()
    {
        m_Decreasing = false;
        m_CurrentTimeIncrement = m_MinCurrentTimeIncrement;
    }

    public void SyncCurrentClicked()
    {
        m_Synced = true;
    }

    private void Awake()
    {
        m_Instance = this;
        m_CurrentTime = DateTime.Now;
        m_VirtualTime = m_CurrentTime;
        m_CurrentJulianDate = PlanetPositionUtility.GetJulianDate( m_CurrentTime );
        m_VirtualJulianDate = m_CurrentJulianDate;
    }

    private void Update()
    {
        if ( m_Increasing || m_Decreasing )
        {
            m_PressDuration += Time.deltaTime;

            // Treat press like a single click until the button is held more than a certain duration
            if( m_PressDuration > 0.5f )
            {
                m_CurrentTimeIncrement += (long)(100 * Time.deltaTime);

                if ( m_CurrentTimeIncrement > m_MaxCurrentTimeIncrement )
                {
                    m_CurrentTimeIncrement = m_MaxCurrentTimeIncrement;
                }

                long timeIncrement = m_TimeIncrementUnit * ( m_CurrentTimeIncrement * m_CurrentTimeIncrement );

                m_VirtualTime = new DateTime( m_VirtualTime.Ticks + ( (m_Increasing) ? timeIncrement : -timeIncrement ) );

                m_VirtualJulianDate = PlanetPositionUtility.GetJulianDate( m_VirtualTime );
            }
        }

        m_CurrentTime = DateTime.Now;
        m_CurrentJulianDate = PlanetPositionUtility.GetJulianDate( m_CurrentTime );

        if( m_CurrentJulianDateText != null )
        {
            m_CurrentJulianDateText.text = m_CurrentJulianDate.ToString(".00000");
        }

        if( m_CurrentTimeText != null )
        {
            m_CurrentTimeText.text = GetTimeString( m_CurrentTime );
        }

        if( m_Synced )
        {
            m_VirtualJulianDate = m_CurrentJulianDate;
            m_VirtualTime = m_CurrentTime;
        }

        if ( m_VirtualJulianDateText != null )
        {
            m_VirtualJulianDateText.text = m_VirtualJulianDate.ToString( ".00000" );
        }

        if( m_VirtualTimeText != null )
        {
            m_VirtualTimeText.text = GetTimeString( m_VirtualTime );
        }
    }

    private string GetTimeString( DateTime dateTime )
    {
        // 2015/12/12 12:34:26 PM
        string results = string.Format( "{0}/{1}/{2} ", dateTime.Year.ToString(), dateTime.Month.ToString("00"), dateTime.Day.ToString("00") );
        results += string.Format( "{0}:{1}:{2} PM", dateTime.Hour.ToString("00"), dateTime.Minute.ToString("00"), dateTime.Second.ToString("00") );
        return results;
    }

    private static CelestialTime m_Instance = null;

    private double m_CurrentJulianDate = 0.0;
    private double m_VirtualJulianDate = 0.0;
    private bool m_Synced = true;
    private bool m_Increasing = false;
    private bool m_Decreasing = false;
    private float m_PressDuration = 0.0f;

    private DateTime m_CurrentTime;
    private DateTime m_VirtualTime;
    private const long m_MinCurrentTimeIncrement = 1;
    private const long m_MaxCurrentTimeIncrement = 4000;
    private const long m_TimeIncrementUnit = 10000000; // One second
    private long m_CurrentTimeIncrement = m_MinCurrentTimeIncrement;
}
