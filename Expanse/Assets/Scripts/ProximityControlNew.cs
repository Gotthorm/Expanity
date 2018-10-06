using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProximityControlNew : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler/*, IPointerClickHandler*/ //, IEventSystemHandler
{
    public ProximityControlCamera ProximityCamera = null;

    public delegate void CameraPositionUpdated( Vector3 cameraPosition );
    public CameraPositionUpdated m_CameraPositionCallback = null;

    public static void SelectCelestialObject( CelestialBody body )
    {
        if( null != m_Instance && null != body )
        {
            if( null != m_Instance.m_SelectedCelestialBody )
            {
                m_Instance.m_SelectedCelestialBody.Unselect();
            }

            m_Instance.m_SelectedCelestialBody = body;

            m_Instance.ProximityCamera.SetTarget( body.transform );
        }
    }

    public Canvas GetCanvas()
    {
        return this.GetComponentInParent<Canvas>();
    }

    private void Awake()
    {
        m_Instance = this;
    }

    private void Start()
    {
        BroadcastCameraPositionUpdate();
    }

    // Update is called once per frame
    private void Update()
    {
        //if ( m_MousePointerActive )
        {
            float mouseWheelValue = Input.GetAxis( "Mouse ScrollWheel" );

            if ( 0.0f != mouseWheelValue )
            {
                if ( null != this.ProximityCamera )
                {
                    float currentRange = ProximityCamera.Distance;
                    float currentChange = currentRange * mouseWheelValue;
                    ProximityCamera.Distance -= currentChange;
                    BroadcastCameraPositionUpdate();
                }
            }
        }
    }

    public void OnBeginDrag( PointerEventData eventData )
    {
        // Hide mouse pointer?
        //throw new System.NotImplementedException();
    }

    public void OnDrag( PointerEventData eventData )
    {
        if ( eventData.button == PointerEventData.InputButton.Right )
        {
            Vector2 deltaXY = eventData.delta;

            float x = ProximityCamera.X + deltaXY.x;
            float y = ProximityCamera.Y - deltaXY.y;

            ProximityCamera.X = x;
            ProximityCamera.Y = y;

            BroadcastCameraPositionUpdate();
        }
    }

    public void OnEndDrag( PointerEventData eventData )
    {
        // Unhide mouse pointer?
        //throw new System.NotImplementedException();
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        //m_MousePointerActive = true;
        //Debug.Log( "Mouse Activated" );
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        if ( eventData.pointerCurrentRaycast.gameObject == null )
        {
            //m_MousePointerActive = false;
            //Debug.Log( "Mouse Deactivated" );
        }
    }

    //public void OnPointerClick( PointerEventData eventData )
    //{
    //    // OnClick code goes here ...
    //    Debug.Log( "Stomach gurgling!" );

    //    Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
    //    RaycastHit hit;

    //    //if ( Physics.Raycast( ray, out hit ) )
    //    //{
    //    //    var sellTemp = hit.transform.gameObject;//the hitted gameobject is stored

    //    //    ProximityCamera.SetTarget( sellTemp.transform );

    //    //    Debug.Log( "Not anymore!" );
    //    //}
    //    //else
    //    if ( false == Physics.Raycast( ray, out hit ) )
    //    {
    //        Debug.Log( "Stomach gurgling!" );

    //        ProximityCamera.SetTarget( null );

    //        if ( null != m_SelectedCelestialBody )
    //        {
    //            m_SelectedCelestialBody.Unselect();
    //            m_SelectedCelestialBody = null;
    //        }
    //    }
    //}

    private void BroadcastCameraPositionUpdate()
    {
        Vector3 cameraPosition = ProximityCamera ? ProximityCamera.transform.position : Vector3.zero;

        m_CameraPositionCallback?.Invoke( cameraPosition );
    }

    //private bool m_MousePointerActive = false;

    private CelestialBody m_SelectedCelestialBody = null;

    private static ProximityControlNew m_Instance = null;
}
