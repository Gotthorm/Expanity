using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Parent is a toggle button that needs its label color to match the current state color

public class ToggleTextColorHelper : MonoBehaviour
{
    private void Awake()
    {
        Toggle toggleScript = GetComponent<Toggle>();

        if ( toggleScript != null )
        {
            Image[] images = GetComponentsInChildren<Image>();

            foreach ( Image image in images )
            {
                if ( image.name == "Background" )
                {
                    m_Disabled = image.color;
                }
                else if ( image.name == "Checkmark" )
                {
                    m_Enabled = image.color;
                }
            }

            m_Text = GetComponentInChildren<Text>();
        }
    }

    // Use this for initialization
    private void Start()
    {
        UpdateTextColor();
    }
	
	// Update is called once per frame
	private void Update()
    {
        UpdateTextColor();
    }

    private void UpdateTextColor()
    {
        if ( m_Text != null )
        {
            Toggle toggleScript = GetComponent<Toggle>();

            if ( toggleScript != null )
            {
                m_Text.color = toggleScript.isOn ? m_Enabled : m_Disabled;
            }
        }
    }

    private Text m_Text = null;
    private Color m_Enabled = Color.white;
    private Color m_Disabled = Color.gray;
}
