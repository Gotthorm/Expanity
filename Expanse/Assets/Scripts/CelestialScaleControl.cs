using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CelestialScaleControl : MonoBehaviour
{
    public string AutoEnabledString = "Auto";
    public string AutoDisabledString = "Manual";

    public Sprite AutoEnabledImage = null;
    public Sprite AutoDisabledImage = null;

    public Color AutoEnabledColor = new Color( 15/255.0f, 104/255.0f, 225/255.0f ); // 0x0F68E1
    public Color AutoDisabledColor = Color.white;

    public Text m_ToggleAutoButtonText = null;
    public Image m_ToggleAutoButtonImage = null;
    public Text m_ScaleValueText = null;
    public Text m_ScaleLabelText = null;
    public Image m_SliderButtonImage = null;
    public Image m_SliderFillImage = null;

    public bool m_StartEnabled = false;

    public CelestialManager m_CelestialManager = null;

    public void ToggleAutoScale()
    {
        Debug.Log( "Auto Scale Toggled" );

        m_AutoEnabled = !m_AutoEnabled;

        string buttonText = m_AutoEnabled ? AutoEnabledString : AutoDisabledString;
        m_ToggleAutoButtonText.text = buttonText;

        //Sprite buttonImage = AutoEnabled ? AutoEnabledImage : AutoDisabledImage;
        //m_ToggleAutoButtonImage.sprite = buttonImage;

        Color controlColor = m_AutoEnabled ? AutoEnabledColor : AutoDisabledColor;
        m_ToggleAutoButtonText.color = controlColor;
        m_ToggleAutoButtonImage.color = controlColor;

        EnableSlider( !m_AutoEnabled );

        //m_CelestialManager.SetAutoScale( m_AutoEnabled );
    }

    // Called directly from the scale slider control
    //public void UpdateScale( float newScale )
    //{
    //    m_ScaleValueText.text = newScale.ToString();
    //    m_Scale = newScale;
    //    m_CelestialManager.SetScale( m_Scale );
    //}

    // Use this for initialization
    private void Start()
    {
        if ( m_ToggleAutoButtonText == null )
        {
            Debug.LogError( "Toggle auto button text was not configfured!" );
        }
        if ( m_ToggleAutoButtonImage == null )
        {
            Debug.LogError( "Toggle auto button image was not configfured!" );
        }
        if ( m_ScaleValueText == null )
        {
            Debug.LogError( "Scale value text was not configfured!" );
        }
        if( m_ScaleLabelText  == null )
        {
            Debug.LogError( "Scale value label was not configfured!" );
        }
        if ( m_SliderButtonImage == null )
        {
            Debug.LogError( "Slider button image was not configfured!" );
        }
        if ( m_SliderFillImage == null )
        {
            Debug.LogError( "Slider fill image was not configfured!" );
        }
        if ( m_CelestialManager == null )
        {
            Debug.LogError( "Celestial manager was not configfured!" );
        }

        m_ScaleValueText.color = AutoEnabledColor;
        m_ScaleLabelText.color = AutoEnabledColor;

        EnableSlider( true );

        //m_CelestialManager.SetScale( m_Scale );

        if ( m_StartEnabled )
        {
            ToggleAutoScale();
        }
    }

    private void EnableSlider( bool enable )
    {
        Color sliderColor = enable ? AutoEnabledColor : AutoDisabledColor;
        m_SliderButtonImage.color = sliderColor;
        m_SliderFillImage.color = sliderColor;

        Slider sliderControl = GetComponentInChildren<Slider>();
        sliderControl.enabled = enable;
    }

    private bool m_AutoEnabled = false;

    private float m_Scale = 1.0f;
}
