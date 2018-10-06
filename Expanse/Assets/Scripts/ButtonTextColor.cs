using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTextColor : Button
{
    protected override void Awake()
    {
        m_Pressed = colors.pressedColor;
        m_Released = colors.normalColor;

        m_Text = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    private void Update()
    {
        if ( m_Text != null )
        {
            Button buttonScript = GetComponent<Button>();

            if ( buttonScript != null )
            {
                m_Text.color = IsPressed() ? m_Pressed : m_Released;
            }
        }
    }

    private Text m_Text = null;
    private Color m_Pressed = Color.gray;
    private Color m_Released = Color.white;
}
