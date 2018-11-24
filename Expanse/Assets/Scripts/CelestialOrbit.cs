using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( LineRenderer ) )]

public class CelestialOrbit : MonoBehaviour
{
    public float m_MinWidth = 0.001f;
    public float m_MaxWidth = 60.0f;
    public float m_MinRange = 1.0f;
    public float m_MaxRange = 30000.0f;

    public static CelestialOrbit Create( CelestialPlanetVirtual celestialPlanet )
    {
        CelestialOrbit orbit = null;

        GameObject gameObject = new GameObject();

        if ( null != gameObject )
        {
            gameObject.name = celestialPlanet.name + "_Orbit";

            orbit = gameObject.AddComponent<CelestialOrbit>();

            orbit.m_CelestialPlanet = celestialPlanet;

            int layerID = LayerMask.NameToLayer( "Virtual Universe" );
            if( layerID != -1 )
            {
                gameObject.layer = layerID;
            }
        }

        return orbit;
	}

    private void Awake()
    {
        m_LineRenderer = GetComponent<LineRenderer>();

        m_LineRenderer.loop = true;
        m_LineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        m_LineRenderer.receiveShadows = false;
        m_LineRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        m_LineRenderer.alignment = LineAlignment.View;

        Material yourMaterial = (Material)Resources.Load( "Materials/Orbit", typeof( Material ) );
        m_LineRenderer.material = yourMaterial;
    }

    // Update is called once per frame
    private void LateUpdate ()
    {
        if ( null != m_CelestialPlanet )
        {
            Camera camera = Camera.main;
            if ( camera != null )
            {
                Vector3 cameraPosition = camera.transform.position;

                if ( m_Rebuild )
                {
                    CelestialBody celestialBody = CelestialManager.Instance.GetCelestialBody( m_CelestialPlanet.ParentPlanetID );
                    CelestialPlanetPhysical celestialPlanet = celestialBody as CelestialPlanetPhysical;
                    m_OrbitPositions = celestialPlanet.GetOrbit( PlanetPositionUtility.GetJulianDate( DateTime.Now ), m_ResolutionScale ).ToArray();

                    m_LineRenderer.positionCount = m_OrbitPositions.Length;

                    m_LineRenderer.SetPositions( m_OrbitPositions );

                    m_Rebuild = false;
                }

                if ( m_OrbitPositions.Length > 0 )
                {
                    Vector3 closestApproximateOrbitPosition = GetClosestApproximateOrbitPosition( cameraPosition, m_OrbitPositions );

                    float distance = ( closestApproximateOrbitPosition - cameraPosition ).magnitude;

                    //Debug.Log( "CameraPosition(" + cameraPosition.ToString(".00") + ") ClosestPointOnOrbit(" + m_OrbitPositions[ closestPositionIndex ].ToString(".00") + ") Distance(" + distance.ToString(".00") + ")" );

                    float lineWidth = Mathf.Min( m_MaxWidth, Mathf.Max( m_MinWidth, ( ( distance - m_MinRange ) / m_MaxRange ) * m_MaxWidth ) );

                    m_LineRenderer.widthMultiplier = lineWidth;
                    m_LineRenderer.startWidth = lineWidth;
                    m_LineRenderer.endWidth = lineWidth;
                }
            }
        }
    }

    private Vector3 GetClosestApproximateOrbitPosition( Vector3 targetPosition, Vector3[] positionArray )
    {
        int closestPositionIndex = FindClosestPositionIndex( targetPosition, positionArray );

        // Find the closest neighbor and generate an approximate position between them that represents the closest position in the orbit
        int lower = closestPositionIndex - 1;
        if( lower < 0 )
        {
            lower += m_OrbitPositions.Length;
        }

        int higher = closestPositionIndex + 1;
        if( higher >= m_OrbitPositions.Length )
        {
            higher = 0;
        }

        Vector3 lowerVector = targetPosition - m_OrbitPositions[ lower ];
        Vector3 higherVector = targetPosition - m_OrbitPositions[ higher ];

        int closestNeighborIndex = ( lowerVector.sqrMagnitude <= higherVector.sqrMagnitude ) ? lower : higher;
        Vector3 closestNeighborVector = ( lowerVector.sqrMagnitude <= higherVector.sqrMagnitude ) ? lowerVector : higherVector;

        Vector3 baseVector = m_OrbitPositions[ closestPositionIndex ] - m_OrbitPositions[ closestNeighborIndex ];

        float vectorLength = Vector3.Dot( baseVector.normalized, closestNeighborVector );

        Vector3 approximatePosition = vectorLength * baseVector.normalized + m_OrbitPositions[ closestNeighborIndex ];

        return approximatePosition;
    }

    private int FindClosestPositionIndex( Vector3 targetPosition, Vector3[] positionArray )
    {
        int length = m_OrbitPositions.Length;
        int startIndex = 0;
        int endIndex = length - 1;

        // This search can be inaccurate when the target index is approximately opposite index 0 (for example 100 on a 200 element orbit)
        // So we perform one quick test to possible eliminate half the search circle
        float slowStart = ( targetPosition - m_OrbitPositions[ startIndex ] ).sqrMagnitude;
        float quickStart = ( targetPosition - m_OrbitPositions[ ( length + 1 ) / 2 ] ).sqrMagnitude;

        if ( quickStart < slowStart )
        {
            float percentageDiff = ( quickStart / slowStart );

            if ( percentageDiff < 0.9f )
            {
                int jump = ( length / 4 );
                startIndex += jump;
                endIndex -= jump;
                length -= ( jump * 2 );
            }
        }

        while ( length > 1 )
        {
            float startDistance = ( targetPosition - m_OrbitPositions[ startIndex ] ).sqrMagnitude;
            float endDistance = ( targetPosition - m_OrbitPositions[ endIndex ] ).sqrMagnitude;

            if ( startDistance < endDistance )
            {
                length = ( length + 1 ) / 2;
                endIndex = startIndex + length - 1;
            }
            else
            {
                length = ( length / 2 );
                startIndex = endIndex - length + 1;
            }
        }

        return startIndex;
    }

    // Calculating the orbital period around the sun
    // orbitalPeriodInYears = Sqr( averageAU * averageAU * averageAU );

    private CelestialPlanetVirtual m_CelestialPlanet = null;

    private LineRenderer m_LineRenderer = null;

    private Vector3[] m_OrbitPositions = null;

    //private Camera m_Camera = null;

    private bool m_Rebuild = true;
    private static double m_ResolutionScale = 200;
}
