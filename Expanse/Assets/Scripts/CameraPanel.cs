using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanel : MonoBehaviour
{
    public GameObject m_ParentPanel = null;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		if( m_ParentPanel != null )
        {
            // Extract what screen space the parent panel occupies and convert it to viewport dimensions
            RectTransform rectTransform = m_ParentPanel.GetComponent<RectTransform>();

            var worldCorners = new Vector3[ 4 ];
            rectTransform.GetWorldCorners( worldCorners );

            Rect result = new Rect(
              worldCorners[ 0 ].x,
              worldCorners[ 0 ].y,
              worldCorners[ 2 ].x - worldCorners[ 0 ].x,
              worldCorners[ 2 ].y - worldCorners[ 0 ].y );

            float width = result.width / Screen.width;
            float height = result.height / Screen.height;
            float x = worldCorners[ 0 ].x / Screen.width;
            float y = worldCorners[ 0 ].y / Screen.height;

            Camera camera = GetComponent<Camera>();
            //camera.rect = new Rect( x, y, width, height );
            camera.aspect = result.width / result.height;
        }
	}
}
