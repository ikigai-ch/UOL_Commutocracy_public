using System.Collections;
using UnityEngine;

public class LineAnimations : MonoBehaviour
{
    public GameObject lineRendererObj;
    public Vector3[] linePositions; // Assign this array in the inspector or from another script
    public float drawSpeed = 2f; // Speed at which the line is drawn
    private LineRenderer lineRenderer;

    public GameObject lineRendererObj2;
    public Vector3[] linePositions2; // Assign this array in the inspector or from another script
    private LineRenderer lineRenderer2;
    public float drawSpeed2 = 1f; // Speed at which the line is drawn

    public GameObject lineRendererObj3;
    public Vector3[] linePositions3; // Assign this array in the inspector or from another script
    private LineRenderer lineRenderer3;
    public float drawSpeed3 = 0.5f; // Speed at which the line is drawn

    void Start()
    {
        lineRenderer = lineRendererObj.GetComponent<LineRenderer>();
        lineRenderer2 = lineRendererObj2.GetComponent<LineRenderer>();
        lineRenderer3 = lineRendererObj3.GetComponent<LineRenderer>();

        StartCoroutine(AnimateLine(linePositions, lineRenderer, drawSpeed));
        StartCoroutine(AnimateLine(linePositions2, lineRenderer2, drawSpeed2));
        StartCoroutine(AnimateLine(linePositions3, lineRenderer3, drawSpeed3));

    }

    IEnumerator AnimateLine(Vector3[] linePositions, LineRenderer lineRenderer, float drawSpeed)
    {
        float segmentDuration;

        for (int i = 0; i < linePositions.Length - 1; i++)
        {
            float startTime = Time.time;
            segmentDuration = (linePositions[i + 1] - linePositions[i]).magnitude / drawSpeed;
            float endTime = startTime + segmentDuration;

            lineRenderer.positionCount = i + 1;
            lineRenderer.SetPosition(i, linePositions[i]);

            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / segmentDuration;
                Vector3 currentPoint = Vector3.Lerp(linePositions[i], linePositions[i + 1], t);
                lineRenderer.positionCount = i + 2; // Make sure the line renderer knows how many positions to draw
                lineRenderer.SetPosition(i + 1, currentPoint);
                yield return null;
            }
            // Ensure that the segment ends precisely at the next point
            lineRenderer.SetPosition(i + 1, linePositions[i + 1]);
        }
    }

}

