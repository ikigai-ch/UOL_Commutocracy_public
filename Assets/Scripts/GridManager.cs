using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class DistrictNode
{
    public string Name;
    public Vector2Int Coordinates; // Added for heuristic calculation
    public List<TransportEdge> Edges;
    public float Cost; // Total cost from the start node
    public float Heuristic; // Heuristic cost to the goal
    public DistrictNode Parent; // Parent node in the path

    public DistrictNode(string name, Vector2Int coordinates)
    {
        Name = name;
        Coordinates = coordinates;
        Edges = new List<TransportEdge>();
        Cost = float.MaxValue;
        Heuristic = 0;
        Parent = null;
    }
}

public class TransportEdge
{
    public DistrictNode Destination;
    public float Weight; // Metric for pathfinding, e.g., distance, time
   

    public TransportEdge(DistrictNode destination, float weight)
    {
        Destination = destination;
        Weight = weight;
    }
}

//////////////////// CLASS FOR TRANSPORT /////////////////////////////

public class DistrictNodeTransport
{
    public string Name;
    public Vector2Int Coordinates; // Added for heuristic calculation
    public List<TransportEdgeTransport> Edges;
    public float Cost; // Total cost from the start node
    public float Heuristic; // Heuristic cost to the goal
    public DistrictNodeTransport Parent; // Parent node in the path
    public TransportType TransportType;

    public DistrictNodeTransport(string name, Vector2Int coordinates, TransportType transportType)
    {
        Name = name;
        Coordinates = coordinates;
        Edges = new List<TransportEdgeTransport>();
        Cost = float.MaxValue;
        Heuristic = 0;
        Parent = null;
        TransportType = transportType;
    }
}

public class TransportEdgeTransport
{
    public DistrictNodeTransport Destination;
    public float Weight; // Metric for pathfinding, e.g., distance, time
    public TransportType TransportType;


    public TransportEdgeTransport(DistrictNodeTransport destination, float weight, TransportType transportType)
    {
        Destination = destination;
        Weight = weight;
        TransportType = transportType;
    }
}

//////////////////////////////////////////////////////////////////////

//https://www.youtube.com/watch?v=kkAjpQAM-jE
public class GridManager : MonoBehaviour
{
    [SerializeField] public int _width, _height;

    [SerializeField] public int districtNumberX, districtNumberY;

    [SerializeField] private District _districtPrefab;
    [SerializeField] private GameObject _stationPrefab;


    [SerializeField] private LineRenderer _lineRendererPrefab;
    private LineRenderer lineRenderer;
    private List<Vector3> linePositions = new List<Vector3>();
    private string currentLineName;

    private Dictionary<string, LineData> lines = new Dictionary<string, LineData>();


    [SerializeField] private Transform _cam;


    //list of rendered lines
    public List<LineRenderer> renderedLines = new List<LineRenderer>();
    //checks how many lines are in the scene. Is added to the information
    private int lineCount = 0;


    public Material material1;
    //public Material material2;


    //transportation textures
    public Material trainMat;
    public Material tramMat;

    //store node positions
    List<Vector2> nodePositions = new List<Vector2>();

    //District List
    public List<string> districtNames = new List<string>();


    // Graph related fields
    private Dictionary<string, DistrictNode> districtGraph = new Dictionary<string, DistrictNode>();
    private Dictionary<string, DistrictNodeTransport> districtGraphTransport = new Dictionary<string, DistrictNodeTransport>();


    //int that decides which district should have the main station (District {firstIdNr} {secondIdNr}
    public int firstIdNr;
    public int secondIdNr;

    //int that decides which district should have the motorway exit (District {firstIdNr} {secondIdNr}
    public int motorDistrictIdX;
    int[] possibleDistrictsX = { 0, 3};
    public int motorDistrictIdY;


    // Start is called before the first frame update
    void Start()
    {

        //This line randomly decides which district should have a main station
        //Source: https://docs.unity3d.com/ScriptReference/Random.Range.html
        firstIdNr = Random.Range(0, districtNumberX - 1);
        secondIdNr = Random.Range(0, districtNumberY - 1);


        //This line randomly decides which district should have a motorway exit
        //Source: https://forum.unity.com/threads/picking-random-number-from-list.633685/
        int randomIndex = Random.Range(0, possibleDistrictsX.Length);
        motorDistrictIdX = possibleDistrictsX[randomIndex];
        motorDistrictIdY = Random.Range(0, districtNumberY - 1);


        GenerateGrid();
        GenerateStations();
        ConnectStations();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateGrid()
    {
        for(int x = 0; x < districtNumberX; x++)
        {
            for(int y = 0; y < districtNumberY; y++)
            {
                //spawn/instantiate  district
                var spawnedDistrict = Instantiate(_districtPrefab, new Vector3(x * _width / districtNumberX,
                                                                                y * _height / districtNumberY),
                                                                                Quaternion.identity);
                //https://forum.unity.com/threads/instantiate-scale.79830/
                //Transform buttonObject = spawnedDistrict.transform.Find("DistrictButton2");
                //Transform textObject = spawnedDistrict.transform.Find("Text (TMP)");

                //Debug.Log(buttonObject.transform.localScale);



                //Vector3 textSize = textObject.transform.GetChild(0).GetChild(0).localScale;
               // Debug.Log(buttonObject.transform.localScale);

                spawnedDistrict.transform.localScale = new Vector3(_width/districtNumberX, _height/districtNumberY); // change its local scale


                //Reset size
                //buttonObject.transform.localScale = buttonSize;
                //textObject.transform.localScale = textSize;
              

                spawnedDistrict.name = $"District {x} {y}";

                //Add distric names to a list
                districtNames.Add(spawnedDistrict.name);

                //Debug.Log(buttonObject.transform.localScale);

                // Determine which material to apply
                if ((x + y) % 2 == 0)
                {
                    // Apply the first material
                    spawnedDistrict.GetComponent<Renderer>().material = material1;
                }
                else
                {
                    // Apply the second material
                    spawnedDistrict.GetComponent<Renderer>().material = material1;
                }
                AddDistrictNode(spawnedDistrict.name, x, y);
                AddDistrictNodeTransport(spawnedDistrict.name, x, y, TransportType.Foot);
                //in later iteration add some randomness

                //insert it into the tables 
            }
        }
        //_cam.transform.position = new Vector3((float)_width / 2 - 3f, (float)_height / 2 - 2.0f, -10);
    }

    void GenerateStations()
    {
        for (int x = 0; x < districtNumberX; x++)
        {
            for (int y = 0; y < districtNumberY; y++)
            {
                //spawn/instantiate  district
                var spawnedStation = Instantiate(_stationPrefab, new Vector3((x * _width / districtNumberX),
                                                                                (y * _height / districtNumberY - 0.5f)),
                                                                                Quaternion.identity);

                //Rotate the station
                spawnedStation.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -15.0f));

                //https://forum.unity.com/threads/instantiate-scale.79830/
                //spawnedDistrict.transform.localScale = new Vector3(_width / districtNumberX, _height / districtNumberY); // change its local scale
                spawnedStation.name = $"Station {x} {y}";
                Vector2 nodePos = new Vector2(spawnedStation.transform.position.x, spawnedStation.transform.position.y); // X and Y positions
                
                nodePositions.Add(nodePos);
               
                //Debug.Log(nodePositions[nodePositions.Count - 1]);


                //in later iteration add some randomness 
            }
        }
        

    }

    void ConnectStations()
    {
        for (int x = 0; x < districtNumberX; x++)
        {
            for (int y = 0; y < districtNumberY; y++)
            {
                int currentIndex = x * districtNumberY + y;
                string currentDistrict = $"District {x} {y}";

                // Connect to right neighbor
                if (x < districtNumberX - 1)
                {
                    int rightNeighborIndex = (x + 1) * districtNumberY + y;
                    //CreateLineBetweenStations(nodePositions[currentIndex], nodePositions[rightNeighborIndex], Color.black);

                    string rightNeighbor = $"District {x + 1} {y}";
                    AddEdgeBetweenDistricts(currentDistrict, rightNeighbor, 1);
                    AddEdgeBetweenDistrictsTransport(currentDistrict, rightNeighbor, TransportType.Foot, 3f);
                }

                // Connect to top neighbor
                if (y < districtNumberY - 1)
                {
                    int topNeighborIndex = x * districtNumberY + (y + 1);
                    //CreateLineBetweenStations(nodePositions[currentIndex], nodePositions[topNeighborIndex], Color.black);

                    string topNeighbor = $"District {x} {y + 1}";
                    AddEdgeBetweenDistricts(currentDistrict, topNeighbor, 1);
                    AddEdgeBetweenDistrictsTransport(currentDistrict, topNeighbor, TransportType.Foot, 3f);
                }

                // Connect to top-right diagonal neighbor
                if (x < districtNumberX - 1 && y < districtNumberY - 1)
                {
                    int diagonalNeighborIndex = (x + 1) * districtNumberY + (y + 1);
                    //CreateLineBetweenStations(nodePositions[currentIndex], nodePositions[diagonalNeighborIndex], Color.black);

                    string diagonalNeighbor = $"District {x + 1} {y + 1}";
                    AddEdgeBetweenDistricts(currentDistrict, diagonalNeighbor, 1);
                    AddEdgeBetweenDistrictsTransport(currentDistrict, diagonalNeighbor, TransportType.Foot, 3f);
                }
                if (x > 0 && y < districtNumberY - 1)
                {
                    int topLeftNeighborIndex = (x - 1) * districtNumberY + (y + 1);
                    //CreateLineBetweenStations(nodePositions[currentIndex], nodePositions[topLeftNeighborIndex], Color.black);

                    string topLeftNeighbor = $"District {x - 1} {y + 1}";
                    AddEdgeBetweenDistricts(currentDistrict, topLeftNeighbor, 1f);
                    AddEdgeBetweenDistrictsTransport(currentDistrict, topLeftNeighbor, TransportType.Foot, 3f);
                }


            }
        }
    }


    private void UpdateLineRenderer(string lineCode)
    {
        if (lines.TryGetValue(lineCode, out LineData lineData))
        {
            lineData.LineRenderer.positionCount = lineData.Positions.Count;
            lineData.LineRenderer.SetPositions(lineData.Positions.ToArray());
        }
    }

    public void CreateLineBetweenStations(Vector2 start, Vector2 end, Color colour, TransportType transportType)
    {
        lineRenderer.positionCount = linePositions.Count;
        lineRenderer.SetPositions(linePositions.ToArray());

        //Change colour of line
        lineRenderer.startColor = colour;
        lineRenderer.endColor = colour;

        switch (transportType)
        {
            case TransportType.Bus:

                break;
            case TransportType.Tram:
                lineRenderer.material = tramMat;
                break;
            case TransportType.Train:
                lineRenderer.material = trainMat;
                break;
        }



        //if transport line add
        renderedLines.Add(lineRenderer);

    }



    void AddDistrictNode(string districtName, int x, int y)
    {
        if (!districtGraph.ContainsKey(districtName))
        {
            districtGraph.Add(districtName, new DistrictNode(districtName, new Vector2Int(x, y)));
        }
    }

    void AddEdgeBetweenDistricts(string fromDistrict, string toDistrict, float weight) //undirected grpah
    {
        if (districtGraph.ContainsKey(fromDistrict) && districtGraph.ContainsKey(toDistrict))
        {
            //edge from 'fromDistrict' to 'toDistrict'
            districtGraph[fromDistrict].Edges.Add(new TransportEdge(districtGraph[toDistrict], weight));

            //reverse edge from 'toDistrict' to 'fromDistrict'
            districtGraph[toDistrict].Edges.Add(new TransportEdge(districtGraph[fromDistrict], weight));
        }
    }

    private float GetHeuristicCost(DistrictNode node, DistrictNode goal)
    {
        return Mathf.Abs(node.Coordinates.x - goal.Coordinates.x)
               + Mathf.Abs(node.Coordinates.y - goal.Coordinates.y);
    }


    //https://www.redblobgames.com/pathfinding/a-star/introduction.html
    public List<DistrictNode> AStarPathfinding(DistrictNode start, DistrictNode goal)
    {
        var openList = new List<DistrictNode> { start };
        var closedList = new HashSet<DistrictNode>();
        start.Cost = 0;

        while (openList.Count > 0)
        {
            var current = openList[0];
            foreach (var node in openList)
            {
                if (node.Cost + node.Heuristic < current.Cost + current.Heuristic)
                {
                    current = node;
                }
            }

            if (current == goal)
            {
                return ConstructPath(current);
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (var edge in current.Edges)
            {
                var neighbor = edge.Destination;
                if (closedList.Contains(neighbor))
                {
                    continue;
                }

                var tentativeCost = current.Cost + edge.Weight;
                if (tentativeCost < neighbor.Cost)
                {
                    neighbor.Parent = current;
                    neighbor.Cost = tentativeCost;
                    neighbor.Heuristic = GetHeuristicCost(neighbor, goal);

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return new List<DistrictNode>(); // Path not found
    }

    private List<DistrictNode> ConstructPath(DistrictNode node)
    {
        var path = new List<DistrictNode>();
        while (node != null)
        {
            path.Add(node);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }

    public void ResetGraph()
    {
        foreach (var node in districtGraph.Values)
        {
            node.Cost = float.MaxValue;
            node.Heuristic = 0;
            node.Parent = null;
        }
    }

    public float FindAndDisplayPath(string startDistrictName, string goalDistrictName)
    {
        ResetGraph();
        float cost = 0;
        //Debug.Log("Finding path from " + startDistrictName + " to " + goalDistrictName);

        if (districtGraph.ContainsKey(startDistrictName) && districtGraph.ContainsKey(goalDistrictName))
        {
            var startNode = districtGraph[startDistrictName];
            var goalNode = districtGraph[goalDistrictName];
            //Debug.Log("Start Node: " + startNode.Name);
            //Debug.Log("Goal Node: " + goalNode.Name);
            var path = AStarPathfinding(startNode, goalNode);

            if (path == null || path.Count == 0)
            {
                //Debug.Log("Start Node: " + startNode.Name);
                //Debug.Log("Goal Node: " + goalNode.Name);
                //Debug.LogError("Pathfinding failed or returned an empty path.");
                return -1; // Indicate an error
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                var currentNode = path[i];
                var nextNode = path[i + 1];

                var edge = currentNode.Edges.Find(e => e.Destination == nextNode);
                if (edge != null)
                {
                    cost += edge.Weight;
                    //Debug.Log("Path Step: " + currentNode.Name + " to " + nextNode.Name + " with cost " + edge.Weight);
                }
            }
        }
        else
        {
            Debug.LogError("Start or goal district not found in the graph.");
            return -1; // Indicate an error
        }

        //Debug.Log("Total path cost: " + cost);
        return cost;
    }




    /////////////////////////////////////// PATH FINDING FOR PUBLIC TRANSPORT ////////////////////////////////

    public void AddDistrictNodeTransport(string districtName, int x, int y, TransportType transportType)
    {
        if (!districtGraphTransport.ContainsKey(districtName))
        {
            districtGraphTransport.Add(districtName, new DistrictNodeTransport(districtName, new Vector2Int(x, y), transportType));
        }
    }


    public void AddEdgeBetweenDistrictsTransport(string fromDistrict, string toDistrict, TransportType transportType, float baseWeight)
    {
        float weight = baseWeight;

        switch (transportType)
        {
            case TransportType.Bus:
                weight = 2f;
                break;
            case TransportType.Tram:
                weight = 1.5f;
                break;
            case TransportType.Train:
                weight = 1f;
                break;
        }

        // Check for existing edge with same destination and transport type
        var existingEdge = districtGraphTransport[fromDistrict].Edges
            .FirstOrDefault(e => e.Destination.Name == toDistrict && e.TransportType == transportType);

        if (existingEdge == null)
        {
            // If no such edge exists, add a new edge
            districtGraphTransport[fromDistrict].Edges.Add(new TransportEdgeTransport(districtGraphTransport[toDistrict], weight, transportType));
            districtGraphTransport[toDistrict].Edges.Add(new TransportEdgeTransport(districtGraphTransport[fromDistrict], weight, transportType));
        }

        //Debug.Log($"Adding edge from {fromDistrict} to {toDistrict} with weight {weight} and type {transportType}");


    }


    private float GetHeuristicCostTransport(DistrictNodeTransport node, DistrictNodeTransport goal)
    {
        return Mathf.Abs(node.Coordinates.x - goal.Coordinates.x)
               + Mathf.Abs(node.Coordinates.y - goal.Coordinates.y);
    }

    public void PrepareStartNodeTransportType(DistrictNodeTransport startNode)
    {
        
        var lowestWeightEdge = startNode.Edges.OrderBy(e => e.Weight).FirstOrDefault();
        if (lowestWeightEdge != null)
        {
            // Set the start node's transport type to that of the best edge
            startNode.TransportType = lowestWeightEdge.TransportType;
        }
    }

    public List<DistrictNodeTransport> AStarPathfindingTransport(DistrictNodeTransport start, DistrictNodeTransport goal)
    {
        // Prepare start node by setting an efficient initial transport type
        PrepareStartNodeTransportType(start);

        const float SwitchingCost = 1.0f; // Cost for changing lines
        var openList = new List<DistrictNodeTransport> { start };
        var closedList = new HashSet<DistrictNodeTransport>();
        start.Cost = 0;
        start.Heuristic = GetHeuristicCostTransport(start, goal);

        while (openList.Count > 0)
        {
            var current = openList.OrderBy(node => node.Cost + node.Heuristic).First();

            if (current == goal)
            {
                return ConstructPathTransport(current);
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (var edge in current.Edges)
            {

                var neighbor = edge.Destination;

                float tentativeCost = current.Cost + edge.Weight;
                // Add extra cost for changing lines
                if (current.TransportType != edge.TransportType)
                {
                    tentativeCost += SwitchingCost; // Apply the switching cost
                }

                if (!closedList.Contains(neighbor) && tentativeCost < neighbor.Cost)
                {
                    neighbor.Parent = current;
                    neighbor.Cost = tentativeCost;
                    neighbor.Heuristic = GetHeuristicCostTransport(neighbor, goal);
                    neighbor.TransportType = edge.TransportType; // Update the neighbor's transport type to the current edge's transport type

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
                //Debug.Log($"Evaluating edge from {current.Name} to {neighbor.Name} with weight {edge.Weight} and type {edge.TransportType}. Tentative cost: {tentativeCost}");
                //Debug.Log($"From {current.Name} (Type: {current.TransportType}) to {neighbor.Name} (Type: {edge.TransportType}), Weight: {edge.Weight}, TentativeCost: {tentativeCost}");

            }
        }
        return new List<DistrictNodeTransport>(); // Return an empty path if none found
    }


    private List<DistrictNodeTransport> ConstructPathTransport(DistrictNodeTransport node)
    {
        var path = new List<DistrictNodeTransport>();
        while (node != null)
        {
            path.Add(node);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }


    public float FindAndDisplayPathTransport(string startDistrictName, string goalDistrictName)
    {
        ResetGraphTransport();
        float cost = 0;

        if (!districtGraphTransport.ContainsKey(startDistrictName) || !districtGraphTransport.ContainsKey(goalDistrictName))
        {
            Debug.LogError($"District not found in the graph.");
            return -1; // Indicate an error
        }

        var startNode = districtGraphTransport[startDistrictName];
        var goalNode = districtGraphTransport[goalDistrictName];
        var path = AStarPathfindingTransport(startNode, goalNode);

        if (path.Count == 0)
        {
            Debug.LogError("Pathfinding failed or returned an empty path.");
            return -1; // Indicate an error
        }

        // Iterate through the path to calculate the total cost
        for (int i = 0; i < path.Count - 1; i++)
        {
            var currentNode = path[i];
            var nextNode = path[i + 1];
            // Find the edge used to reach nextNode from currentNode
            var edge = currentNode.Edges.Find(e => e.Destination == nextNode && e.TransportType == nextNode.TransportType);
            if (edge != null)
            {
                cost += edge.Weight;
                //Debug.Log($"Final Path Step: {currentNode.Name} to {nextNode.Name}, Weight: {edge.Weight}, TransportType: {edge.TransportType}, Accumulated Cost: {cost}");
            }
        }
        return cost;
    }

    public void ResetGraphTransport()
    {
        foreach (var node in districtGraphTransport.Values)
        {
            node.Cost = float.MaxValue;
            node.Heuristic = 0;
            node.Parent = null;
        }
    }


    public string GetDistrictNameFromPosition(Vector2 position)
    {
        // Calculate the district indices from the position
        int xIndex = Mathf.FloorToInt(position.x / (_width / districtNumberX));
        int yIndex = Mathf.FloorToInt(position.y / (_height / districtNumberY));

        return $"District {xIndex} {yIndex}";
    }

    public string GetStationNameFromPosition(Vector2 position)
    {
        // Calculate the district indices from the position
        int xIndex = Mathf.FloorToInt(position.x / (_width / districtNumberX));
        int yIndex = Mathf.FloorToInt(position.y / (_height / districtNumberY));

        return $"Station {xIndex} {yIndex}";
    }
}
