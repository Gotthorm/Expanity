using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SplitPanel : MonoBehaviour, /*IBeginDragHandler,*/ IDragHandler, IEndDragHandler
{
    public GameObject m_LeftPanel = null;

    public GameObject m_RightPanel = null;

    // Percent of the panel that the left portion will occupy
    [Tooltip( "Percent (0.0 <=> 1.0) of the panel that the left child panel will occupy" )]
    public float m_LeftPortion = 0.3f;

    public float m_DragHotZone = 0.1f;

    public float m_DragDirectionTolerance = 0.9f;

    public float m_TransitionSpeed = 1.0f;

    private void OnValidate()
    {
        m_LeftPortion = Mathf.Min( 1.0f, Mathf.Max( 0.0f, m_LeftPortion ) );
        m_DragHotZone = Mathf.Min( 1.0f, Mathf.Max( 0.0f, m_DragHotZone ) );
        m_DragDirectionTolerance = Mathf.Min( 1.0f, Mathf.Max( 0.0f, m_DragDirectionTolerance ) );
    }

    private void Start()
    {
        m_CurrentPortion = m_LeftPortion;

        foreach ( Transform child in m_LeftPanel.transform )
        {
            m_ChildList.Add( child.gameObject );
        }
    }

    // Update is called once per frame
    private void Update ()
    {
		if( m_LeftPanel != null && m_RightPanel != null )
        {
            switch( m_State )
            {
                case State.TRANSITION_TO_MIN:
                    DecrementTransition();
                    break;
                case State.TRANSITION_TO_MAX:
                    IncrementTransition();
                    break;
                case State.MINIMIZED:
                case State.MAXIMIZED:
                default:
                    break;
            };

            UpdateProportions();
        }
	}

    private void IncrementTransition()
    {
        m_CurrentPortion += ( m_TransitionSpeed * Time.deltaTime );

        if( m_CurrentPortion >= m_LeftPortion )
        {
            m_CurrentPortion = m_LeftPortion;
            m_State = State.MAXIMIZED;

            EnablePanelChildren( true );
        }
    }

    private void DecrementTransition()
    {
        m_CurrentPortion -= ( m_TransitionSpeed * Time.deltaTime );

        if ( m_CurrentPortion <= 0.0f )
        {
            m_CurrentPortion = 0.0f;
            m_State = State.MINIMIZED;
        }
    }

    private void EnablePanelChildren( bool enable )
    {
        foreach ( GameObject gameObject in m_ChildList )
        {
            gameObject.SetActive( enable );
        }
    }

    private void UpdateProportions()
    {
        RectTransform rectTransform = this.GetComponent<RectTransform>();

        // Get the current dimensions
        float parentWidth = rectTransform.rect.width;

        // Determine the current correct width for the left child
        float leftWidth = parentWidth * m_CurrentPortion;

        // Update the current left child proportion
        UpdateLeftChild( parentWidth - leftWidth );

        // Update the current right child proportion
        UpdateRightChild( leftWidth );
    }

    private void UpdateLeftChild( float width )
    {
        RectTransform rectTransform = m_LeftPanel.GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2( -width, 0.0f );

        rectTransform.offsetMin = new Vector2( 0.0f, 0.0f );
        rectTransform.offsetMax = new Vector2( -width, 0.0f );
    }

    private void UpdateRightChild( float width )
    {
        RectTransform rectTransform = m_RightPanel.GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2( -width, 0.0f );

        rectTransform.offsetMin = new Vector2( width, 0.0f );
        rectTransform.offsetMax = new Vector2( 0.0f, 0.0f );
    }

    private bool GetOpenSwipe()
    {
        return false;
    }

    private bool GetCloseSwipe()
    {
        return false;
    }

    //public void OnBeginDrag( PointerEventData eventData )
    //{
    //    //Debug.Log( "OnBeginDrag" );
    //}

    public void OnDrag( PointerEventData eventData )
    {
    }

    public void OnEndDrag( PointerEventData eventData )
    {
        //Debug.Log( "Press position + " + eventData.pressPosition );
        //Vector2 dragVectorDirection = ( eventData.position - eventData.pressPosition ).normalized;
        //Debug.Log( "norm + " + dragVectorDirection );

        // So the idea is, if the start position is within a certain region on the right child panel
        // and the swipe is long enough then:
        // The swipe direction is left and the left child panel is open
        //     Close the left panel to the closed proportion
        //
        // The swipe direction if right and the left panel is closed
        //     Open the left panel to the correct proportion
        //

        bool dragConsumed = false;

        if ( m_State == State.MINIMIZED || m_State == State.MAXIMIZED )
        {
            Vector2 dragVectorDirection = ( eventData.position - eventData.pressPosition ).normalized;

            // Was the start position inside of the hot zone?
            RectTransform rectTransform = m_RightPanel.GetComponent<RectTransform>();

            Rect rect = RectTransformToScreenSpace( rectTransform );
            rect.width *= m_DragHotZone;

            if ( rect.Contains( eventData.pressPosition ) )
            {
                // Check the swipe direction
                Vector2 correctDirection = ( m_State == State.MINIMIZED ) ? Vector2.right : Vector2.left;

                if ( Vector2.Dot( correctDirection, dragVectorDirection ) > m_DragDirectionTolerance )
                {
                    if( m_State == State.MINIMIZED )
                    {
                        m_State = State.TRANSITION_TO_MAX;
                        Debug.Log( "Successfully swiped open" );
                    }
                    else
                    {
                        m_State = State.TRANSITION_TO_MIN;
                        EnablePanelChildren( false );
                        Debug.Log( "Successfully swiped closed" );
                    }

                    dragConsumed = true;
                }
            }
        }

        if( false == dragConsumed )
        {
            ScreenManager parentPanelViewer = GetComponentInParent<ScreenManager>();
            if ( parentPanelViewer != null )
            {
                GetComponentInParent<ScreenManager>().SendMessage( "OnEndDrag", eventData );
            }
        }
    }

    private Rect RectTransformToScreenSpace( RectTransform transform )
    {
        Vector2 size = Vector2.Scale( transform.rect.size, transform.lossyScale );
        return new Rect( (Vector2)transform.position - ( size * 0.5f ), size );
    }

    private enum State { MINIMIZED, TRANSITION_TO_MIN, TRANSITION_TO_MAX, MAXIMIZED };

    private State m_State = State.MAXIMIZED;
    
    private float m_CurrentPortion = 0.0f;

    private List<GameObject> m_ChildList = new List<GameObject>();
}
