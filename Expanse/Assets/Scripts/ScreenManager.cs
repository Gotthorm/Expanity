using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Design thought:
// Each child panel should be sized to match the canvas

//[ExecuteInEditMode]
[RequireComponent( typeof( Image ) )]
public class ScreenManager : MonoBehaviour, IDragHandler/*, IBeginDragHandler*/, IEndDragHandler
{
    public float m_TransitionSpeed = 3.0f;

    public float m_DragDirectionTolerance = 0.5f;

    public List<ScreenPanel> m_PanelList = new List<ScreenPanel>();

    public void OnDrag( PointerEventData eventData )
    {
        //Debug.Log( "Drag" );
    }

    //public void OnBeginDrag( PointerEventData eventData )
    //{
    //}

    public void OnEndDrag( PointerEventData eventData )
    {
        Debug.Log( "PanelViewer : End drag" );

        if ( m_TargetIndex == m_CurrentIndex )
        {
            Vector2 dragVectorDirection = ( eventData.position - eventData.pressPosition ).normalized;

            // Check the swipe direction
            float dragDirectionTest = Vector2.Dot( Vector2.up, dragVectorDirection );

            if ( dragDirectionTest > m_DragDirectionTolerance )
            {
                Debug.Log( "Swipe up" );

                // Drag Up
                if ( m_CurrentIndex > 0 )
                {
                    --m_TargetIndex;
                }
            }
            else if ( dragDirectionTest < -m_DragDirectionTolerance )
            {
                Debug.Log( "Swipe down" );

                // Drag Down
                if ( m_CurrentIndex < m_PanelList.Count - 1 )
                {
                    ++m_TargetIndex;
                }
            }

            // TODO : Disable rendering on current panel?
        }
    }

    // Use this for initialization
    private void Start()
    {
        InitializeStack();
    }

    // Update is called once per frame
    private void Update()
    {
//#if UNITY_EDITOR
//        // This code should only be called from the editor while it is not playing
//        // It is meant to auto format any panel added to the canvas panel list
//        if ( UnityEditor.EditorApplication.isPlaying == false )
//        {
//            InitializeStack();

//            // Do not call the actual update code below when in edit mode
//            return;
//        }
//#endif

        UpdateStack();
    }

    // Call this whenever the contents of the panel list has changed
    private void InitializeStack()
    {
        // Do not call this from Awake, it is too early to obtain valid data from the canvas
        Debug.Log( "InitializeStack:" + System.DateTime.Now.ToString() );

        Canvas canvas = GetComponentInParent<Canvas>();

        // TODO: If this fails it should be an error
        if ( canvas != null )
        {
            // Check if there is a canvas scaler being used
            CanvasScaler canvasScaler = GetComponentInParent<CanvasScaler>();

            // When there is a canvas scaler we need to extract the post scaled dimensions from it.
            // Otherwise we can just read the dimensions directly from the canvas rect.
            RectTransform canvasRectTransform = ( canvasScaler != null ) ? canvasScaler.GetComponent<RectTransform>() : canvas.GetComponent<RectTransform>();

            // TODO: If this fails it should be an error
            if ( canvasRectTransform != null )
            {
                m_PanelWidth = canvasRectTransform.rect.width;
                m_PanelHeight = canvasRectTransform.rect.height;

                RectTransform rectTransform;

                int index = 0;
                foreach ( ScreenPanel screenPanel in m_PanelList )
                {
                    rectTransform = screenPanel.GetComponent<RectTransform>();
                    if ( rectTransform != null )
                    {
                        // Set the anchor info
                        rectTransform.anchorMin = new Vector2( 0, 0 );
                        rectTransform.anchorMax = new Vector2( 0, 0 );
                        rectTransform.pivot = new Vector2( 0, 0 );
                        rectTransform.anchoredPosition = new Vector2( 0, m_PanelHeight * index );
                        ++index;

                        // Set the dimensions
                        rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, m_PanelHeight );
                        rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, m_PanelWidth );
                    }
                }

                // Set up this parent panel based on the number of children
                rectTransform = GetComponent<RectTransform>();
                if ( rectTransform != null )
                {
                    // Set the anchor info
                    rectTransform.anchorMin = new Vector2( 0, 0 );
                    rectTransform.anchorMax = new Vector2( 0, 0 );
                    rectTransform.pivot = new Vector2( 0, 0 );
                    rectTransform.anchoredPosition = new Vector2( 0, 0 );

                    // Set the dimensions
                    rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, m_PanelHeight * index );
                    rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, m_PanelWidth );
                }


                // Reset the stack to the default position
                m_CurrentIndex = 0;
                m_TargetIndex = 0;
                m_TransitionValue = 0.0f;
            }
        }
    }

    private void UpdateStack()
    {
        // When transitioning
        if ( m_TransitionValue != 0.0f || m_CurrentIndex != m_TargetIndex )
        {
            m_TransitionValue += m_TransitionSpeed * Time.deltaTime;

            float transitionOffset = 0.0f;

            if ( m_TransitionValue >= 1.0f )
            {
                m_TransitionValue = 0.0f;
                m_CurrentIndex = m_TargetIndex;

                // TODO : Enable rendering on new target panel?
            }
            else
            {
                transitionOffset = ( m_CurrentIndex < m_TargetIndex ) ? -m_TransitionValue : m_TransitionValue;
            }

            int index = 0;
            foreach ( ScreenPanel screenPanel in m_PanelList )
            {
                RectTransform rectTransform = screenPanel.GetComponent<RectTransform>();
                if ( rectTransform != null )
                {
                    // Set the anchor info
                    //rectTransform.anchorMin = new Vector2( 0, 0 );
                    //rectTransform.anchorMax = new Vector2( 0, 0 );
                    //rectTransform.pivot = new Vector2( 0, 0 );

                    float newIndex = System.Convert.ToSingle( index - m_CurrentIndex );

                    rectTransform.anchoredPosition = new Vector2( 0, m_PanelHeight * ( newIndex + transitionOffset ) );
                    ++index;

                    // Set the dimensions
                    //rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, m_PanelHeight );
                    //rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, panelWidth );
                }
            }
        }
    }

    private float m_PanelHeight = 0;
    private float m_PanelWidth = 0;

    private int m_CurrentIndex = 0;
    public int m_TargetIndex = 0;
    private float m_TransitionValue = 0.0f;
}