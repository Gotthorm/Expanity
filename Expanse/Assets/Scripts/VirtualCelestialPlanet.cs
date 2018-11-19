using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( CelestialClickable ) )]

public class VirtualCelestialPlanet : CelestialBody
{
    //?
    public override CelestialType GetCelestialType() { return CelestialType.Planet; }

    public override bool Initialize( CelestialBodyLoader loader )
    {
        if ( base.Initialize( loader ) )
        {
            return true;
        }

        return false;
    }

    public uint ParentPlanetID { get; set; }
    
    #region Private Interface

    //private void ClickSelected( GameObject eventOwner )
    //{
    //    //Debug.Log( "Object click selected: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
    //    m_Camera.SetSelectedObject( this, false );

    //    // Notify the panel?
    //}

    //private void ClickTargeted( GameObject eventOwner )
    //{
    //    Debug.Log( "Object click targeted: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
    //    m_Camera.SetTargetedObject( this );

    //    // Notify the panel?
    //}

    //private void ClickDisableMiss( GameObject eventOwner )
    //{
    //    Debug.Log( "Object click disable miss: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
    //    m_Camera.DisableClickMissDetectionForThisFrame();
    //}

    //private void ClickDrag( GameObject eventOwner )
    //{
    //    Debug.Log( "Object click drag: " + eventOwner.name + " on celestial body: " + this.gameObject.name );
    //    m_Camera.DragObject( this );
    //}

    #endregion
}
