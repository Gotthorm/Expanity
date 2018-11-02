using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenPanel : MonoBehaviour
{
    public bool Enabled { get; set; }

	// Use this for initialization
	private void Awake ()
    {
        Enabled = false;
	}
}
