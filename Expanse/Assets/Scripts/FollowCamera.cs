
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform Target;

    public float SmoothSpeed = 3.0f;

    public Vector3 Offset;

    private void FixedUpdate()
    {
        Vector3 desiredPosition = Target.position + Target.forward * 200.0f;

        Vector3 smoothedPosition = Vector3.Lerp( transform.position, desiredPosition, SmoothSpeed * Time.deltaTime );
        transform.position = smoothedPosition;

        transform.LookAt( Target );
    }
}
