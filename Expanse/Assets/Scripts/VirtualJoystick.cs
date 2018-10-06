using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent( typeof( Image ) )]

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Vector3 InputDirection { set; get; }

    private void Start()
    {
        //GameObject joystickBackgroundObject = transform.GetChild( 0 ).gameObject;

        bgImg = transform.GetChild( 0 ).gameObject.GetComponent<Image>();
        jsImg = transform.GetChild( 0 ).GetChild( 0 ).gameObject.GetComponent<Image>();

        //bgImg = GetComponent<Image>();
        //jsImg = GetComponentsInChildren<Image>()[1];
        InputDirection = Vector3.zero;
    }

    //EventSystems interfaces
    public virtual void OnDrag(PointerEventData ped)
    {
        Vector2 pos = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle( bgImg.rectTransform, ped.position, ped.pressEventCamera, out pos ))
        {
            pos.x = (pos.x / bgImg.rectTransform.sizeDelta.x);
            pos.y = (pos.y / bgImg.rectTransform.sizeDelta.y);

            // Assumes a centered image
            float x = pos.x * 2;
            float y = pos.y * 2;

            InputDirection = new Vector3(x, 0, y);
            InputDirection = (InputDirection.magnitude > 1) ? InputDirection.normalized : InputDirection;

            jsImg.rectTransform.anchoredPosition = new Vector3(InputDirection.x * (bgImg.rectTransform.sizeDelta.x / 3), InputDirection.z * (bgImg.rectTransform.sizeDelta.y / 3));
        }
    }

    public virtual void OnPointerDown(PointerEventData ped)
    {
        OnDrag(ped);
    }

    public virtual void OnPointerUp(PointerEventData ped)
    {
        InputDirection = Vector3.zero;
        jsImg.rectTransform.anchoredPosition = Vector3.zero;
    }

    private Image bgImg;
    private Image jsImg;
}
