using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleTextContentHelper : MonoBehaviour
{
    public string m_EnabledText;
    public string m_DisabledText;

	// Use this for initialization
	void Awake ()
    {
        Toggle toggleScript = GetComponent<Toggle>();

        if ( toggleScript != null )
        {
            m_Text = GetComponentInChildren<Text>();
        }
    }

    // Use this for initialization
    private void Start()
    {
        UpdateText();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if ( m_Text != null )
        {
            Toggle toggleScript = GetComponent<Toggle>();

            m_Text.text = toggleScript.isOn ? m_EnabledText : m_DisabledText;
        }
    }

    private Text m_Text = null;
}
