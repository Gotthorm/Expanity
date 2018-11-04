using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExternalShipView : MonoBehaviour, IPointerClickHandler
{
    public SpaceShipExternalCamera m_ViewCamera = null;

    public Sprite m_Unselected = null;
    public Sprite m_Selected = null;

    public void OnPointerClick( PointerEventData eventData )
    {
        Debug.Log( "Object click selected: " + eventData.pointerCurrentRaycast.gameObject.name );

        ExternalShipViewController.GetInstance().SetView( this );
        Select( true );
    }

    public void Select(bool select)
    {
        Sprite newSprite = select ? m_Selected : m_Unselected;

        Image mask = GetComponentInChildren<Image>();
        mask.sprite = newSprite;
    }
}
