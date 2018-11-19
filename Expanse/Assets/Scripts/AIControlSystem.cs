using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class AIControlSystem : ControlSystem
{
    public override bool GetInput(out float inputX, out float inputY, out float inputZ)
    {
        // Used for adding a bit of spin to the ship
        //if ( m_Timer > 0.0f )
        //{
        //    inputX = 1.0f;
        //    inputY = 0.0f;
        //    inputZ = 0.0f;

        //    m_Timer -= Time.deltaTime;
        //}
        //else
        //{
        inputX = 0.0f;
        inputY = 0.0f;
        inputZ = 0.0f;
        //}

        return true;
    }

    private float m_Timer = 1.0f;
}
