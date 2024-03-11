using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportLineRender : MonoBehaviour
{

    [SerializeField] private LineRenderer _lineRendererPrefab;


    public Material material1;
    //public Material material2;

    //transportation textures
    public Material trainMat;
    public Material tramMat;

    private Dictionary<string, LineData> lines = new Dictionary<string, LineData>();

    public List<LineRenderer> renderedLines = new List<LineRenderer>();



    public void newLineInstance(Vector2 start, Vector2 end, Color colour, TransportType transportType, string lineCode)
    {
        LineRenderer lineRenderer = Instantiate(_lineRendererPrefab, Vector3.zero, Quaternion.identity);

        lineRenderer.SetPosition(0, new Vector3(start.x, start.y, 0));
        lineRenderer.SetPosition(1, new Vector3(end.x, end.y, 0));

        // Change colour of line
        lineRenderer.startColor = colour;
        lineRenderer.endColor = colour;

        // Calculate the line length and adjust texture scale accordingly
        float lineLength = Vector3.Distance(start, end);
        lineRenderer.material.mainTextureScale = new Vector2(lineLength / lineRenderer.startWidth, 1);

        switch (transportType)
        {
            case TransportType.Bus:
                // Set material specifics for Bus
                break;
            case TransportType.Tram:
                lineRenderer.material = Instantiate(tramMat);
                break;
            case TransportType.Train:
                lineRenderer.material = Instantiate(trainMat);
                break;
        }

        lineRenderer.name = lineCode;

        LineData lineData = new LineData(transportType, lineCode)
        {
            LineRenderer = lineRenderer,
            Positions = new List<Vector3> { new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0) }
        };
        lines[lineCode] = lineData; // Adds a new entry or updates an existing one
    }


    public void AddPointToLine(Vector3 newPoint, string lineCode)
    {
        if (lines.TryGetValue(lineCode, out LineData lineData))
        {
            lineData.Positions.Add(newPoint);
            UpdateLineRenderer(lineCode);
        }
        else
        {
            Debug.LogError("Line with code " + lineCode + " not found.");
        }
    }

    public void UpdateLineRenderer(string lineCode)
    {
        if (lines.TryGetValue(lineCode, out LineData lineData))
        {
            lineData.LineRenderer.positionCount = lineData.Positions.Count;
            lineData.LineRenderer.SetPositions(lineData.Positions.ToArray());
        }
    }

    public void RemoveLastPointFromLine(string lineCode)
    {
        if (lines.TryGetValue(lineCode, out LineData lineData))
        {
            // Check if there are any points to remove
            if (lineData.Positions.Count > 0)
            {
                // Remove the last point
                lineData.Positions.RemoveAt(lineData.Positions.Count - 1);
                // Update the LineRenderer to reflect the change
                UpdateLineRenderer(lineCode);
            }
            else
            {
                Debug.LogWarning("Line with code " + lineCode + " has no points to remove.");
            }
        }
        else
        {
            Debug.LogError("Line with code " + lineCode + " not found.");
        }
    }

    public void DeleteCurrentLine(string currentLineCode)
    {
        Debug.Log("At least i am being called");
        if (string.IsNullOrEmpty(currentLineCode))
        {
            Debug.LogWarning("No current line code provided.");
            return;
        }

        // Check if the current line exists in the dictionary
        if (lines.TryGetValue(currentLineCode, out LineData currentLineData))
        {
            // Destroy the GameObject of the LineRenderer associated with the current line
            if (currentLineData.LineRenderer != null)
            {
                Destroy(currentLineData.LineRenderer.gameObject);
            }

            // Remove the current line from the dictionary
            lines.Remove(currentLineCode);

            if (currentLineData.PathNodes != null)
            {
                currentLineData.PathNodes.Clear();
            }
            if (currentLineData.PathNodesAnchors != null)
            {
                currentLineData.PathNodesAnchors.Clear();
            }

            renderedLines.RemoveAll(lineRenderer => lineRenderer.name == currentLineCode);

            Debug.Log($"Current line {currentLineCode} deleted.");
        }
        else
        {
            Debug.LogWarning($"Line with code {currentLineCode} not found.");
        }

        
    }
}
