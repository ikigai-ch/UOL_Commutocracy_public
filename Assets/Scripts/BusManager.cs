using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
//using static UnityEditor.PlayerSettings;


public enum TransportType
{
    Bus,
    Tram,
    Train,
    Foot
}


public class LineData
{
    public TransportType TransportType { get; set; }
    //public int LineId { get; set; }
    public string LineName { get; set; } 
    public List<Vector2> PathNodes { get; set; }
    public List<Vector2> PathNodesAnchors { get; set; }
    public GameObject Vehicle { get; set; }

    // Properties for line rendering
    public LineRenderer LineRenderer { get; set; }
    public List<Vector3> Positions { get; set; } 

    // Constructor
    public LineData(TransportType transportType, string lineName) 
    {
        TransportType = transportType;
        //LineId = lineId;
        LineName = lineName; 
        PathNodes = new List<Vector2>();
        PathNodesAnchors = new List<Vector2>();
        Vehicle = null;
        Positions = new List<Vector3>(); 
    }

    public override string ToString()
    {
        var pathNodes = string.Join(", ", PathNodes.Select(p => $"({p.x}, {p.y})"));
        var pathNodesAnchors = string.Join(", ", PathNodesAnchors.Select(p => $"({p.x}, {p.y})"));
        var positions = string.Join(", ", Positions.Select(p => $"({p.x}, {p.y}, {p.z})"));
        return $"TransportType: {TransportType}, LineName: {LineName}, PathNodes: [{pathNodes}], PathNodesAnchors: [{pathNodesAnchors}], Positions: [{positions}], Vehicle: {(Vehicle != null ? Vehicle.name : "null")}";
    }
}

public class Anchor
{
    public bool IsOccupied { get; set; }
    public Color AnchorColor { get; set; }

    public Anchor(Color color)
    {
        IsOccupied = false;
        AnchorColor = color;
    }
}

public class BusManager : MonoBehaviour
{

    DatabaseManager dbManager;
    [SerializeField] public GameObject grid;
    GridManager gridManager;
    ContentGeneration contentGeneration;
    TransportLineRender transportLineRender;
    GameManager gameManager;

    EditTranpsortationLines editTranpsortationLines;
    public GameObject editLines;

    private bool BusButtonsCreated = false;
    private bool lineIsbeingEdited = false;


    Coroutine movementCoroutine;


    //station Prefabs
    [SerializeField] private GameObject _busStationPrefab;
    [SerializeField] private GameObject _tramStationPrefab;
    [SerializeField] private GameObject _trainStationPrefab;


    [SerializeField] private GameObject _addStationPrefab;
    [SerializeField] private GameObject _deleteStationPrefab;

    [SerializeField] private GameObject _busStationIconPrefab;
    [SerializeField] private GameObject _tramStationIconPrefab;
    [SerializeField] private GameObject _trainStationIconPrefab;

    //[SerializeField] private GameObject _editLineButtonPrefab;

    //takes the current path
    private List<Vector2> currentPathNodes = new List<Vector2>();

    //takes the current paths ancohrs
    private List<Vector2> currentPathNodesAnchors = new List<Vector2>();

    public GameObject confirmButton;
    public GameObject doneButton;


    //those are states that check if the process of line creation has begun
    public bool lineCreationStarted = false; //must be reset to false if the line creation was discared or confirmed

    private int addClick = 0; //count how many add station buttons where clicked, if count 2 line is rendered

    Vector2 startPos = new Vector2(0, 0);
    Vector2 endPos = new Vector2(0, 0);

    private bool firstCon = false; //check if first connection created


    private string lineCode;


    private bool shouldGenerateAddButtons = false;
    // Current path lines
    public List<LineData> lines = new List<LineData>();
    public LineData currentLine;

    //all current lines
    public Dictionary<string, LineData> allCurrentLines = new Dictionary<string, LineData>();

    [SerializeField] private TransportType modeType;
    private float baseWeight = 2f;


    //AUDIOSOURCES
    public AudioSource deleteStationSound;
    public AudioSource addStationSound;


    public int busMax, tramMax, trainMax;

    private int currentLineID;

    public Color lineColour;


    // List of Colours available to be assigned to a ligne
    private List<Color> availableColors = new List<Color>{
        new Color(0.937f, 0.482f, 0.062f, 1.0f), // Orange R: 239 G: 123 B: 16 Hex: #EF7B10
        new Color(0.000f, 0.588f, 0.659f, 1.0f), // Blue R: 0 G: 150 B: 168 Hex: #0096A8
        new Color(0.698f, 0.388f, 0.141f, 1.0f), // Brown R: 178 G: 99 B: 36 Hex: #B26300
        new Color(0.529f, 0.808f, 0.922f, 1.0f), // Light Blue R: 135 G: 206 B: 235 Hex: #87CEEB
        new Color(0.863f, 0.627f, 0.627f, 1.0f), // Pink R: 220 G: 160 B: 160 Hex: #DCA0A0
        new Color(0.957f, 0.643f, 0.376f, 1.0f), // Beige R: 244 G: 164 B: 96 Hex: #F4A460
        new Color(0.000f, 0.502f, 0.502f, 1.0f), // Teal R: 0 G: 128 B: 128 Hex: #008080
        new Color(0.502f, 0.000f, 0.502f, 1.0f), // Purple R: 128 G: 0 B: 128 Hex: #800080
        new Color(0.000f, 0.392f, 0.000f, 1.0f), // Dark Green R: 0 G: 100 B: 0 Hex: #006400
        new Color(0.545f, 0.271f, 0.075f, 1.0f), // Dark Brown R: 139 G: 69 B: 19 Hex: #8B4513
        new Color(0.251f, 0.878f, 0.816f, 1.0f), // Turquoise R: 64 G: 224 B: 208 Hex: #40E0D0
        new Color(0.678f, 0.847f, 0.902f, 1.0f), // Light Steel Blue R: 173 G: 216 B: 230 Hex: #ADD8E6
        new Color(0.416f, 0.353f, 0.804f, 1.0f), // Slate Blue R: 106 G: 90 B: 205 Hex: #6A5ACD
        new Color(0.282f, 0.239f, 0.545f, 1.0f), // Dark Slate Blue R: 72 G: 61 B: 139 Hex: #483D8B
        new Color(0.824f, 0.706f, 0.549f, 1.0f), // Tan R: 210 G: 180 B: 140 Hex: #D2B48C
        new Color(0.545f, 0.000f, 0.000f, 1.0f), // Dark Red R: 139 G: 0 B: 0 Hex: #8B0000
        new Color(1.000f, 0.843f, 0.000f, 1.0f), // Gold R: 255 G: 215 B: 0 Hex: #FFD700
        new Color(0.000f, 0.000f, 0.545f, 1.0f), // Navy R: 0 G: 0 B: 139 Hex: #00008B
        new Color(0.251f, 0.878f, 0.816f, 1.0f), // Turquoise R: 64 G: 224 B: 208 Hex: #40E0D0
        new Color(0.275f, 0.510f, 0.706f, 1.0f), // Steel Blue R: 70 G: 130 B: 180 Hex
};

    //list of the already used colours
    private HashSet<Color> usedColors = new HashSet<Color>();

    //Vehicle prefab
    [SerializeField] private GameObject vehiclePrefabTrain, vehiclePrefabTram, vehiclePrefabBus;
    private GameObject vehicleInstance;

    private Vector3 vehicleStartPos;

    // Start is called before the first frame update
    void Start()
    {
        dbManager = GetComponent<DatabaseManager>();
        gridManager = grid.GetComponent<GridManager>();
        contentGeneration = GetComponent<ContentGeneration>();
        transportLineRender = GetComponent<TransportLineRender>();
        gameManager = GetComponent<GameManager>();

        editTranpsortationLines = editLines.GetComponent<EditTranpsortationLines>();


        busMax = 4;
        tramMax = 3;
        trainMax = 1;
        ///ADD LIMITS

    }

    // Update is called once per frame
    void Update()
    {
        if (shouldGenerateAddButtons)
        {
            GenerateAddButtons(endPos);
            shouldGenerateAddButtons = false;
        }
    }


    public void InsertBus(int lineId, TransportType transportType)
    {

        bool lineCreated = false;

        currentLineID = lineId;
        lineColour = GetUniqueColorForAnchor();

        if (lineCreated == false && BusButtonsCreated == false)
        {
            GameObject stationPrefab = null;
            string buttonName = null;
            modeType = transportType;

            switch (transportType)
            {
                case TransportType.Bus:
                    stationPrefab = _busStationPrefab;
                    buttonName = "BusButton";
                    vehicleInstance = vehiclePrefabBus;
                    lineCode = "B"+lineId;
                    busMax -= 1;
                    break;
                case TransportType.Tram:
                    stationPrefab = _tramStationPrefab;
                    buttonName = "TramButton";
                    vehicleInstance = vehiclePrefabTram;
                    lineCode = "T" + lineId;
                    tramMax -= 1;
                    break;
                case TransportType.Train:
                    stationPrefab = _trainStationPrefab;
                    vehicleInstance = vehiclePrefabTrain;
                    buttonName = "TrainButton";
                    lineCode = "TR" + lineId;
                    trainMax -= 1;
                    break;
            }
            if (stationPrefab != null)
            {
                GenerateStationButtons(stationPrefab, buttonName);
            }
            BusButtonsCreated = true;
        }
        string type = transportType.ToString().ToLower(); // converts transportType into string
        int satisfaction_weight = 1;
        int mode_id = lineId;
        string line_name = "Line" + lineCode;
        // array route = ABCD
        string routeArray = "ABCD";

        currentLine = new LineData(transportType, "Line"+lineCode);
        lines.Add(currentLine);

        dbManager.InsertInTransport(type, line_name, mode_id, routeArray, satisfaction_weight);
    }

    //generates buttons to add station
    void GenerateStationButtons(GameObject stationPrefab, string buttonName)
    {
        for (int x = 0; x < gridManager.districtNumberX; x++)
        {
            for (int y = 0; y < gridManager.districtNumberY; y++)
            {
                //spawn/instantiate  district
                var spawnedButtons = Instantiate(stationPrefab, new Vector3((x * gridManager._width / gridManager.districtNumberX),
                                                                                (y * gridManager._height / gridManager.districtNumberY)),
                                                                                Quaternion.identity);
             
                RectTransform rt = spawnedButtons.GetComponent<RectTransform>();
                rt.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
                rt.position = new Vector3(x * gridManager._width / gridManager.districtNumberX, y * gridManager._height / gridManager.districtNumberY, 0);

                //Set name
                spawnedButtons.name = $"{buttonName} {x} {y}";

                //https://discussions.unity.com/t/instantiate-prefab-from-script-inside-a-canvas/170628/2
                spawnedButtons.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
                Vector2 nodePos = new Vector2(spawnedButtons.transform.position.x, spawnedButtons.transform.position.y); // X and Y positions
                                                                                                                         //spawnedButtons.Add(nodePos);
                //event Listener
                spawnedButtons.GetComponent<Button>().onClick.AddListener(() => StationButtonClicked(spawnedButtons));

            }
        }    
    }


    void StationButtonClicked(GameObject buttonClicked)
    {
        // Handles click events
        if (buttonClicked.tag == "BusStationButton")
        {
            startPos = buttonClicked.transform.position; //this is the first station therefore startpos needs to be stored

            

            addClick += 1; //increase add click count to make sure we don't override start pos with addclick

            //https://docs.unity3d.com/ScriptReference/Object.Destroy.html
            //https://discussions.unity.com/t/destroy-multiple-gameobjects-with-tag-c/159371
            Destroy(buttonClicked); //destroy the buttons

            GameObject[] buttons = GameObject.FindGameObjectsWithTag("BusStationButton");
            foreach (GameObject button in buttons)
            GameObject.Destroy(button);
            Vector3 excludedPosition = buttonClicked.transform.position;
            GenerateAddButtons(excludedPosition);

            // Add the node position to the path list
            currentLine.PathNodes.Add(startPos);

            currentPathNodes.Add(startPos);

        }

        if (buttonClicked.tag == "AddStation") // check if add
        {
            confirmButton.SetActive(true);
            //doneButton.SetActive(true);

            if (firstCon && addClick == 0) //if no click was made yet save Button pos as start pos
            {
                AddStationButtonClicked(buttonClicked);
            }

            else if (addClick == 1) //if already one click was made yet save Button pos as start pos
            {
                AddStationButtonClicked(buttonClicked);
            }
            addStationSound.Play();
            GenerateDeleteButton(endPos);
        }

        if (buttonClicked.tag == "DeleteStation")
        {
            // Start the coroutine
            StartCoroutine(ProcessNodeDeletionAndGenerateButtons(buttonClicked.transform.position));
            transportLineRender.RemoveLastPointFromLine(lineCode);
            Destroy(buttonClicked); // Destroy the delete button
            deleteStationSound.Play();
        }
        if (buttonClicked.tag == "Done") //CONFIRM BUTTON
        {
            DoneButtonClicked(); 
        }
        if (buttonClicked.tag == "Discard") //NOT USED CURRENTLY
        {
            DiscardButtonClick();
        }
        Debug.Log(buttonClicked.tag);
    }

    void DiscardButtonClick() // CURRENTLY NOT USED
    {
        // Destroy station buttons
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("AddStation");
        foreach (GameObject button in buttons)
            Destroy(button);

        // Destroy delete station buttons
        GameObject[] delButtons = GameObject.FindGameObjectsWithTag("DeleteStation");
        foreach (GameObject delButton in delButtons)
            Destroy(delButton);

        // Ensures to correctly remove the line from allCurrentLines
        if (allCurrentLines.ContainsKey(currentLine.LineName))
        {
            allCurrentLines.Remove(currentLine.LineName);
        }

        // delete line which should include destroying the GameObject
        transportLineRender.DeleteCurrentLine(lineCode);

     
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        //Destroy existing vehicle if it exists
        if (currentLine.Vehicle != null)
        {
            Destroy(currentLine.Vehicle);
            currentLine.Vehicle = null; // clear  reference to the destroyed vehicle
        }


        GameObject existingVehicle = GameObject.Find($"Vehicle_Line_{currentLine.TransportType}_{currentLine.LineName}");
        Destroy(existingVehicle);

        EditTranpsortationLines editLinesScript = editLines.GetComponent<EditTranpsortationLines>();
        if (editLinesScript != null)
        {
            editLinesScript.RemoveButtonForLine(currentLine.LineName);  
        }

        dbManager.DeleteTransportationLine(currentLine.LineName);

        //UI and state cleanup
       // doneButton.GetComponent<Button>().onClick.RemoveAllListeners();
        confirmButton.GetComponent<Button>().onClick.RemoveAllListeners();
        //doneButton.SetActive(false);
        confirmButton.SetActive(false);
        BusButtonsCreated = false;
        lineCreationStarted = false;
        addClick = 0;
        firstCon = false;
        currentLineID -= 1;
        lineIsbeingEdited = false;


        switch (currentLine.TransportType)
        {
            case TransportType.Bus:
                busMax += 1;
                gameManager.busModeID -= 1;
                Debug.Log("Av: " + gameManager.busLinesAvailable);
                gameManager.AvailableLines("bus"); //Updates available lines
                //gameManager.busAvailable.text = $"{gameManager.busLinesAvailable}";
                break;
            case TransportType.Tram:
                tramMax += 1;
                gameManager.tramModeID -= 1;
                gameManager.AvailableLines("tram"); //Updates available lines
                //gameManager.tramAvailable.text = $"{gameManager.tramLinesAvailable}";
                break;
            case TransportType.Train:
                trainMax += 1;
                gameManager.trainModeID -= 1;
                gameManager.AvailableLines("train"); //Updates available lines
                //gameManager.trainAvailable.text = $"{gameManager.trainLinesAvailable}";
                break;
        }
     
    }

    void AddStationButtonClicked(GameObject buttonClicked)
    {
        if (firstCon && addClick == 0) //if no click was made yet save Button pos as start pos
        {
            startPos = endPos;
            endPos = buttonClicked.transform.position;

            string startPosName = gridManager.GetDistrictNameFromPosition(startPos);
            string endPosName = gridManager.GetDistrictNameFromPosition(endPos);
            //Debug.Log("District Name: " + endPosName);

            //gridManager.CreateLineBetweenStations(startPos, endPos, Color.blue);

            gridManager.AddEdgeBetweenDistrictsTransport(startPosName, endPosName, modeType, baseWeight);

            string startStation = gridManager.GetStationNameFromPosition(startPos);
            string endStation = gridManager.GetStationNameFromPosition(endPos);
            //Debug.Log(endStation);

            GameObject startAnchorObj = GameObject.Find(startStation);
            GameObject endAnchorObj = GameObject.Find(endStation);
            //Debug.Log(endAnchorObj.name);

            // Find and assign anchors for start and end stations
            var (startAnchor, startPosAnchor) = FindAndAssignAnchor(startAnchorObj, modeType, currentLineID);
            var (endAnchor, endPosAnchor) = FindAndAssignAnchor(endAnchorObj, modeType, currentLineID);


            //Debug.Log(startAnchorObj.transform.position);


            //gridManager.CreateLineBetweenStations(startPosAnchor, endPosAnchor, lineColour, modeType);
            transportLineRender.AddPointToLine(endPosAnchor, lineCode);
            Debug.Log("Condition first Statement");

            Destroy(buttonClicked);

            GameObject[] buttons = GameObject.FindGameObjectsWithTag("AddStation");
            foreach (GameObject button in buttons)
                GameObject.Destroy(button);

            Vector2 excludedPosition = buttonClicked.transform.position;
            GenerateAddButtons(excludedPosition);

            // Add the node position to the path list
            currentPathNodes.Add(endPos);
            currentPathNodesAnchors.Add(endPosAnchor);
            currentLine.PathNodesAnchors.Add(endPosAnchor);
            currentLine.PathNodes.Add(endPos);
            //ADD TO STRING FUNCTION


            //ADD A FUNCTION TO ADD THE DELETE BUTTON INSTEAD
            // instantiate start pos
            // then i need a fiunctionality if delete button was clicked that removes line
            //and creates delete button at previous pos of last node

            BusButtonsCreated = false;

            lineCreationStarted = false;
        }

        else if (addClick == 1) //if already one click was made yet save Button pos as start pos
        {
            endPos = buttonClicked.transform.position;

            //Debug.Log(addClick);
            addClick = 0;

            confirmButton.GetComponent<Button>().onClick.AddListener(() => StationButtonClicked(confirmButton));
          //doneButton.GetComponent<Button>().onClick.AddListener(() => StationButtonClicked(doneButton));

            string startStation = gridManager.GetStationNameFromPosition(startPos);
            string endStation = gridManager.GetStationNameFromPosition(endPos);
            //Debug.Log(endStation);

            GameObject startAnchorObj = GameObject.Find(startStation);
            GameObject endAnchorObj = GameObject.Find(endStation);
            //Debug.Log(endAnchorObj.name);

            // Find and assign anchors for start and end stations
            var (startAnchor, startPosAnchor) = FindAndAssignAnchor(startAnchorObj, modeType, currentLineID);
            var (endAnchor, endPosAnchor) = FindAndAssignAnchor(endAnchorObj, modeType, currentLineID);


            //Debug.Log(startAnchorObj.transform.position);

            //gridManager.CreateLineBetweenStations(startPosAnchor, endPosAnchor, lineColour, modeType);
            transportLineRender.newLineInstance(startPosAnchor, endPosAnchor, lineColour, modeType, lineCode);

            vehicleStartPos = startPosAnchor;//initial startpos for the vehicle
            currentPathNodesAnchors.Add(startPosAnchor);
            currentLine.PathNodesAnchors.Add(startPosAnchor);

            string startPosName = gridManager.GetDistrictNameFromPosition(startPos);
            string endPosName = gridManager.GetDistrictNameFromPosition(endPos);
            //Debug.Log("District Name: " + endPosName);

            gridManager.AddEdgeBetweenDistrictsTransport(startPosName, endPosName, modeType, baseWeight);


            firstCon = true;
            //Debug.Log("Cond 3");
            Destroy(buttonClicked);

            GameObject[] buttons = GameObject.FindGameObjectsWithTag("AddStation");
            foreach (GameObject button in buttons)
                GameObject.Destroy(button);

            Vector2 excludedPosition = buttonClicked.transform.position;
            GenerateAddButtons(excludedPosition);

            // Add the node position to the path list
            currentPathNodes.Add(endPos);
            currentPathNodesAnchors.Add(endPosAnchor);
            currentLine.PathNodesAnchors.Add(endPosAnchor);

            currentLine.PathNodes.Add(endPos);

            Debug.Log("Condition second Statement");

        }
    }


    void DoneButtonClicked()
    {
        //https://stackoverflow.com/questions/31412526/deleting-the-last-instantiated-gameobject-from-a-list-and-scene
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("AddStation");
        foreach (GameObject button in buttons)
            GameObject.Destroy(button);

        GameObject[] delButtons = GameObject.FindGameObjectsWithTag("DeleteStation");
        foreach (GameObject delButton in delButtons)
            GameObject.Destroy(delButton);

        Debug.Log(currentLine.LineName);

        if (allCurrentLines.ContainsKey(currentLine.LineName))
        {
            allCurrentLines[currentLine.LineName] = currentLine;
        }
        else
        {
            if (!allCurrentLines.ContainsKey(currentLine.LineName))
            {
                editTranpsortationLines.AssignButtonToLine(lineCode, lineColour);
            }

            allCurrentLines.Add(currentLine.LineName, currentLine);

        }

        UpdateRoute();


        foreach (var entry in allCurrentLines)
        {
            string lineCode = entry.Key;
            LineData lineData = entry.Value;
            Debug.Log($"Line Code: {lineCode}, Details: {lineData.ToString()}");
        }

        confirmButton.GetComponent<Button>().onClick.RemoveAllListeners();

        confirmButton.SetActive(false);
        doneButton.SetActive(false);
        BusButtonsCreated = false;
        lineCreationStarted = false;
        lineIsbeingEdited = false;

    }


    public void ResetAndClearAllLines()
    {
        // Destroy all rendered line GameObjects
        foreach (var lineRenderer in transportLineRender.renderedLines)
        {
            Destroy(transportLineRender.gameObject);
        }
        gridManager.renderedLines.Clear(); // Clear list of rendered lines

        // Clear lines dictionary if used
        lines.Clear();

        // Reset the current line data
        currentPathNodes.Clear();
        currentPathNodesAnchors.Clear();
        currentLine = null; 

        // Resets UI elements and state variables
        confirmButton.SetActive(false);
        BusButtonsCreated = false;
        addClick = 0;
        firstCon = false;

        Debug.Log("All line data reset and cleared.");
    }

    void RestartVehicleMovement(LineData line)
    {
        // Stop the current movement coroutine if  running
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        // Start movement coroutine with the updated line data
        movementCoroutine = StartCoroutine(MoveAlongRoute(line));
    }

    void InstantiateModeVehicle(LineData line)
    {
        GameObject existingVehicle = GameObject.Find($"Vehicle_Line_{line.TransportType}_{line.LineName}"); //this checks if a game object with the same name already exists in the data structure
        //only if it does not exist the vehicle is instanticated
        if (existingVehicle == null)
        {
            var vehicle = Instantiate(vehicleInstance, line.PathNodesAnchors[0], Quaternion.identity);
            SpriteRenderer sprite = vehicle.GetComponent<SpriteRenderer>();
            sprite.color = lineColour; 
            line.Vehicle = vehicle;
            vehicle.name = $"Vehicle_Line_{line.TransportType}_{line.LineName}";

            StartCoroutine(MoveAlongRoute(line));
        }
    }

    IEnumerator MoveAlongRoute(LineData line) //this code handles the movement of the vehicles
    {
        //they have to move a long the line
        //and face the direction of the upcoming node
        GameObject vehicle = line.Vehicle;
        List<Vector2> route = line.PathNodesAnchors;
        int targetIndex = 0;
        bool forward = true;
       
            while (true) //when editing stop moving
            {

                    if (vehicle == null) // Check if the vehicle has been destroyed
                    {
                        yield break; // Exit the coroutine if the vehicle no longer exists
                    }


                    Vector3 targetPoint = route[targetIndex];
                    vehicle.transform.position = Vector2.MoveTowards(vehicle.transform.position, targetPoint, 1f * Time.deltaTime);

                    ////Rotation
                    ////rotate towards new waypoint
                    ////https://docs.unity3d.com/ScriptReference/Vector3.RotateTowards.html

                    //// Determines which direction to rotate towards
                    Vector2 direction = (new Vector2(targetPoint.x, targetPoint.y) - new Vector2(vehicle.transform.position.x, vehicle.transform.position.y)).normalized;

                    ////https://forum.unity.com/threads/how-mathf-atan2-x-y-works.1292667/
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
                    Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                    vehicle.transform.rotation = Quaternion.RotateTowards(vehicle.transform.rotation, q, 360 * Time.deltaTime);

                    if (vehicle.transform.position == targetPoint)
                    {
                        if (forward)
                        {
                            if (++targetIndex >= route.Count)
                            {
                                targetIndex = route.Count - 2; // Set to second-last,  avoid overshooting the bounds
                                forward = false; // Reverse direction
                            }
                        }
                        else
                        {
                            if (--targetIndex < 0)
                            {
                                targetIndex = 1; // Set to second, to  reverse direction at start
                                forward = true; // Reset direction to forward
                            }
                        }
                    }
                    yield return null;

                
            }
        
        
    }


    //Generate Buttons to add stations to a line
    void GenerateAddButtons(Vector3 excludedPosition)
    {
        //Debug.Log("Called");
        for (int x = 0; x < gridManager.districtNumberX; x++)
        {
            for (int y = 0; y < gridManager.districtNumberY; y++)
            {
                //spawn/instantiate  district
                Vector3 buttonPosition = new Vector3((x * gridManager._width / gridManager.districtNumberX),
                                                 (y * gridManager._height / gridManager.districtNumberY), 0);

                Vector2 currentnodePos = new Vector2(buttonPosition.x, buttonPosition.y);

                // Check if the node position is in the current path nodes
                if (currentLine != null && currentLine.PathNodes.Contains(currentnodePos))
                {
                    continue; // This node is already in the path
                }
                // Check if the current position is the excluded position or not a bordering node
                if (buttonPosition == excludedPosition || !IsBorderingNode(buttonPosition, excludedPosition))
                {
                    continue; // Skip this iteration as we don't want a button here
                }

                var spawnedButtons = Instantiate(_addStationPrefab, buttonPosition, Quaternion.identity);

                RectTransform rt = spawnedButtons.GetComponent<RectTransform>();
                rt.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
                rt.position = new Vector3(x * gridManager._width / gridManager.districtNumberX, y * gridManager._height / gridManager.districtNumberY, 0);

                //Set name
                spawnedButtons.name = $"AddButton {x} {y}";

                //https://discussions.unity.com/t/instantiate-prefab-from-script-inside-a-canvas/170628/2
                spawnedButtons.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
                Vector2 nodePos = new Vector2(spawnedButtons.transform.position.x, spawnedButtons.transform.position.y); // X and Y positions
                                                                                                                         //spawnedButtons.Add(nodePos);
                                                                                                                         //event Listener
                spawnedButtons.GetComponent<Button>().onClick.AddListener(() => StationButtonClicked(spawnedButtons));
            }
        }
    }

    //Generate Delete Buttons
    void GenerateDeleteButton(Vector2 endpos)
    {
       Destroy(GameObject.FindGameObjectWithTag("DeleteStation"));
        var spawnedButtons = Instantiate(_deleteStationPrefab, new Vector3(endPos.x,endpos.y),Quaternion.identity);

                RectTransform rt = spawnedButtons.GetComponent<RectTransform>();
                rt.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
                rt.position = new Vector3(endPos.x, endpos.y, 0);

                //Set name
                spawnedButtons.name = $"DeleteButton";

                //https://discussions.unity.com/t/instantiate-prefab-from-script-inside-a-canvas/170628/2
                spawnedButtons.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
                Vector2 nodePos = new Vector2(spawnedButtons.transform.position.x, spawnedButtons.transform.position.y); // X and Y positions
                                                                                                                         //spawnedButtons.Add(nodePos);
                                                                                                                         //event Listener
                spawnedButtons.GetComponent<Button>().onClick.AddListener(() => StationButtonClicked(spawnedButtons));
    }

    bool IsBorderingNode(Vector3 currentPosition, Vector2 excludedPosition)
    {
        // Check if the current node is adjacent to the excluded node
        return Mathf.Abs(currentPosition.x - excludedPosition.x) <= (gridManager._width / gridManager.districtNumberX) &&
               Mathf.Abs(currentPosition.y - excludedPosition.y) <= (gridManager._height / gridManager.districtNumberY);
    }

    void AddStationIcon(Vector2 pos, float margin, GameObject stationIcon)
    {
        //spawn/instantiate  district
        var spawnedButtons = Instantiate(stationIcon, new Vector3(pos.x - margin,pos.y),Quaternion.identity);
    }

    IEnumerator ProcessNodeDeletionAndGenerateButtons(Vector3 buttonPosition)
    {
            currentPathNodes.RemoveAt(currentPathNodes.Count - 1);
            currentPathNodesAnchors.RemoveAt(currentPathNodesAnchors.Count - 1);
            currentLine.PathNodesAnchors.RemoveAt(currentLine.PathNodesAnchors.Count - 1);

        if (currentPathNodes.Count > 1)
            {
                startPos = currentPathNodes[currentPathNodes.Count - 2];
                endPos = currentPathNodes[currentPathNodes.Count - 1];
            }

        // wait for end of frame to ensure all UI elements are updated
        yield return new WaitForEndOfFrame();

        // Set flag instead of directly calling GenerateAddButtons
        shouldGenerateAddButtons = true;

        if (currentPathNodes.Count > 1)
        {
            GenerateDeleteButton(endPos);
        }
        if (currentPathNodes.Count == 1)
        {
            endPos = currentPathNodes[0];
            GenerateDeleteButton(endPos);
        }

        yield break; // Ends the coroutine
    }

    void UpdateRoute()
    {
        List<string> districtNames = currentPathNodes.Select(pos => gridManager.GetDistrictNameFromPosition(pos)).ToList();
        string route = String.Join("-", districtNames);

        int modeId = 1; 
        string type = modeType.ToString().ToLower(); // convert transportType into string 
        int satisfactionWeight = 1; //  satisfaction weight

        // method in DatabaseManager to update the route
        dbManager.UpdateTransportRoute(type, modeId, route, satisfactionWeight);

        contentGeneration.PushDesiredRatio(districtNames, modeType);
        //instatiate moving vehcile
        InstantiateModeVehicle(currentLine);
    }

    private (Transform, Vector3) FindAndAssignAnchor(GameObject station, TransportType transportType, int modeId)
    {
        string anchorPrefix = transportType == TransportType.Train ? $"Anchor TR {modeId}" : $"Anchor {transportType.ToString().Substring(0, 1)} {modeId}";
        Transform anchor = station.transform.Find(anchorPrefix);

        if (anchor != null)
        {
            
            Debug.Log($"Found anchor at {anchor.transform.position}");
            return (anchor, anchor.transform.position);
        }
        else
        {
            // No anchor found with the specified name, handle accordingly
            Debug.LogError($"No anchor found with the name {anchorPrefix} in {station.name}");
            return (null, Vector3.zero); // Returning null to indicate no anchor found
        }
    }


    private Color GetUniqueColorForAnchor()
    {
        foreach (var color in availableColors)
        {
            if (!usedColors.Contains(color))
            {
                usedColors.Add(color);
                return color; // Exit the method once a unique color is assigned
            }
        }

        Debug.LogWarning("All colors have been used. Consider resetting the color list or handling this case differently.");

        // Return a default color 
        return Color.white; //  returning white or any default color
    }


    public void PrepareUIForLineEditing(LineData lineToEdit, string selectedLineCode)
    {
      
        //UI elements for editing
        confirmButton.SetActive(true); // Show confirm button
       // doneButton.SetActive(true); // Show done button
        confirmButton.GetComponent<Button>().onClick.AddListener(() => StationButtonClicked(confirmButton));

        modeType = currentLine.TransportType;
        lineCode = selectedLineCode;
        var last_char = selectedLineCode.Substring(selectedLineCode.Length - 1); ;
        int last_char_as_int = Int32.Parse(last_char);
        currentLineID = last_char_as_int;

        Vector2 lastPathNode = currentLine.PathNodes.Last();
        Vector3 lastExcludedPosition = new Vector3(lastPathNode.x, lastPathNode.y, 0);
        //RestartVehicleMovement(lineToEdit);
        GenerateAddButtons(lastExcludedPosition);
        GenerateDeleteButton(lastExcludedPosition);
        lineIsbeingEdited = true;

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        // Destroy existing vehicle if it exists
        if (lineToEdit.Vehicle != null)
        {
            Destroy(lineToEdit.Vehicle);
            lineToEdit.Vehicle = null; // Clear the reference to the destroyed vehicle
        }


        GameObject existingVehicle = GameObject.Find($"Vehicle_Line_{lineToEdit.TransportType}_{lineToEdit.LineName}");
        Destroy(existingVehicle);


    }

    private void GenerateAddButtonsForLineEditing(LineData lineToEdit, string lineCode)
    {
        List<Vector2> existingNodes = lineToEdit.PathNodes;
        
        for (int x = 0; x < gridManager.districtNumberX; x++)
        {
            for (int y = 0; y < gridManager.districtNumberY; y++)
            {
                Vector3 buttonPosition = new Vector3((x * gridManager._width / gridManager.districtNumberX),
                                                     (y * gridManager._height / gridManager.districtNumberY), 0);

                Vector2 currentnodePos = new Vector2(buttonPosition.x, buttonPosition.y);

                // Skip buttons for nodes already part of the line
                if (existingNodes.Contains(currentnodePos))
                {
                    continue;
                }

                // Instantiate and set up the button for adding new nodes to the line
                var spawnedButton = Instantiate(_addStationPrefab, buttonPosition, Quaternion.identity);

                RectTransform rt = spawnedButton.GetComponent<RectTransform>();
                rt.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
                rt.position = new Vector3(x * gridManager._width / gridManager.districtNumberX, y * gridManager._height / gridManager.districtNumberY, 0);

                spawnedButton.name = $"AddButton {x} {y}";
                spawnedButton.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);

                // Assign a new listener specific for editing this line
                spawnedButton.GetComponent<Button>().onClick.AddListener(() => AddStationToLineEditing(spawnedButton, lineToEdit, lineCode));
            }
        }
    }

    private void AddStationToLineEditing(GameObject buttonClicked, LineData lineToEdit, string lineCode)
    {
        Vector2 newPos = buttonClicked.transform.position;

        Vector2 startPos = lineToEdit.PathNodes.Count > 0 ? lineToEdit.PathNodes.Last() : Vector2.zero;

        // Convert positions to station names
        string startStationName = startPos != Vector2.zero ? gridManager.GetStationNameFromPosition(startPos) : null;
        string endStationName = gridManager.GetStationNameFromPosition(newPos);

        GameObject startAnchorObj = startStationName != null ? GameObject.Find(startStationName) : null;
        GameObject endAnchorObj = GameObject.Find(endStationName);

        Vector2 startPosAnchor = Vector2.zero, endPosAnchor = Vector2.zero;

        // If there's a start station, find its anchor
        if (startAnchorObj != null)
        {
            var (startAnchor, sPosAnchor) = FindAndAssignAnchor(startAnchorObj, lineToEdit.TransportType, currentLineID);
            startPosAnchor = sPosAnchor;
        }

        // Find end station's anchor
        var (endAnchor, ePosAnchor) = FindAndAssignAnchor(endAnchorObj, lineToEdit.TransportType, currentLineID);
        endPosAnchor = ePosAnchor;

        // Add the new position and its anchor to the line's data
        if (startPos != Vector2.zero)
        {
            lineToEdit.PathNodes.Add(startPos); // If startPos is zero, it means this is the first station
            lineToEdit.PathNodesAnchors.Add(startPosAnchor);
        }

        lineToEdit.PathNodes.Add(newPos);
        lineToEdit.PathNodesAnchors.Add(endPosAnchor);

        
        transportLineRender.AddPointToLine(endPosAnchor, lineCode);

        //
        GenerateAddButtons(newPos);
        confirmButton.GetComponent<Button>().onClick.AddListener(() => StationButtonClicked(confirmButton));
    }

    private void GenerateDeleteButtonForLineEditing(LineData lineToEdit)
    {
        
    }

}
