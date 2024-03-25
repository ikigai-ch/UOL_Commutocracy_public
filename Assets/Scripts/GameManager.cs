using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;


public struct PopulationUpdate
{
    public string type;
    public string departure;
    public string destination;
    public float bestTime;
    public float actualTime;
    public bool transportationNeed;
    public float satisfaction;
}

public struct DistrictAmountUpdate
{
    public string districtName;
    public float industrialChange;
    public float commercialChange;
    public float residentialChange;
}

public class GameManager : MonoBehaviour
{

    DatabaseManager dbManager;
    BusManager busManager;
    GridManager gridManager;
    ContentGeneration contentGeneration;
    public Timer timer;

    [SerializeField] public GameObject grid;

    private int populationAmount;

    public TextMeshProUGUI inhabitantsAmount;

    //indicates how many lines per type are available
    [SerializeField]
    public int busLinesAvailable, tramLinesAvailable, trainLinesAvailable;

    // Indicates how many lines of each type are created
    public int busModeID, tramModeID, trainModeID;

    // LineCreation Buttons
    public Button busButton;
    public Button tramButton;
    public Button trainButton;


    //SucessScreen
    public GameObject sucessScreen;
    public GameObject failScreen;

    public TextMeshProUGUI busAvailable, tramAvailable,trainAvailable;

    //public AudioSource backgroundMusic;
    public AudioSource populationPop;
    public AudioSource clickInteractionSound;
    public AudioSource trapBeat;
    public AudioSource trapBeatFast;


    public int numberOfPopulationPerDistrict = 5;


    public int numberOfFirstPopEntries;


    public TextMeshProUGUI satisfactionDisplay;

    //Number of districts
    private int districtCount;

    public Animator inhabitantsAnimator;


    // Start is called before the first frame update
    void Start()
    {
        dbManager = GetComponent<DatabaseManager>();
        busManager = GetComponent<BusManager>();
        gridManager = grid.GetComponent<GridManager>();
        contentGeneration = GetComponent<ContentGeneration>();

        trapBeat.Play();

        districtCount = gridManager.districtNames.Count();

        numberOfFirstPopEntries = numberOfPopulationPerDistrict * districtCount;

        busLinesAvailable = 4;
        tramLinesAvailable = 3;
        trainLinesAvailable = 1;

        busAvailable.text = $"{busLinesAvailable}";
        tramAvailable.text = $"{tramLinesAvailable}";
        trainAvailable.text = $"{trainLinesAvailable}";

        busModeID = 0;
        tramModeID = 0;
        trainModeID = 0;

        populationAmount = 0;

        //InitializeRandomPopulationEntries(20);
        Invoke("InitializeRandomPopulationEntries", 0.1f);


        //PERIODICALLY repeating functions

        //// Calls a function that should be repeated every 10 seconds, starting 2 seconds after game starts
        InvokeRepeating("PopulationUpdate", 4.0f, 20.0f);
        InvokeRepeating("UpdateSatisfactionAvgOverall", 10f, 1f); // calls method to update staisfaction
        InvokeRepeating("UpdateForEveryPopulation", 11f, 1f);


        inhabitantsAmount.text = "Population: " + 120 * 100;
    }

    // Update is called once per frame
    void Update()
    {
        //checks if line cration was finished.
        //Status retrieved from busmanager
        //if(!busManager.lineCreationStarted & (busButton.interactable == false
        //                                        || tramButton.interactable == false
        //                                        || trainButton.interactable == false))
        //{
        //    changeButtonInteractionState();
        //}
    }

    public void OnButtonClick()
    {

        clickInteractionSound.Play();
        //Debug.Log("Button Clicked");
        //dbManager.InsertInTable();

        //get name of the button that was clicked
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        //Debug.Log("Button Name: " + clickedButton);
        if(!busManager.lineCreationStarted)
        {
            switch (clickedButton.name)
            {
                case "Bus":
                    removeOtherEventListeners("Bus");
                    busManager.lineCreationStarted = true;
                    busLinesAvailable -= 1;
                    if (busLinesAvailable == 0)
                    {
                        busButton.interactable = false;
                        //https://stackoverflow.com/questions/66155526/how-to-change-a-ui-button-disabled-color-on-runtime
                        var colors = busButton.colors;
                        Color buttonColour = new Color(0.933f, 0.627f, 0.173f, 1.0f);
                        colors.disabledColor = buttonColour;

                    }
                    busAvailable.text = $"{busLinesAvailable}";
                    busModeID += 1;
                    busManager.InsertBus(busModeID, TransportType.Bus);
                    break;

                case "Tram":
                    removeOtherEventListeners("Tram");
                    busManager.lineCreationStarted = true;
                    tramLinesAvailable -= 1;
                    if (tramLinesAvailable == 0)
                    {
                        tramButton.interactable = false;
                    }
                    tramAvailable.text = $"{tramLinesAvailable}";
                    tramModeID += 1;
                    busManager.InsertBus(tramModeID, TransportType.Tram);
                    break;

                case "Train":
                    removeOtherEventListeners("Train");
                    busManager.lineCreationStarted = true;
                    trainLinesAvailable -= 1;
                    if (trainLinesAvailable == 0)
                    {
                        trainButton.interactable = false;
                    }
                    trainAvailable.text = $"{trainLinesAvailable}";
                    trainModeID += 1;
                    busManager.InsertBus(trainModeID, TransportType.Train);
                    break;
            }
        }

        if (clickedButton.name == "NodeButton")
        {
            //Debug.Log("BusButton Was CLICKED");
        }
    }

    public void changeButtonInteractionState()
    {
        if (busLinesAvailable != 0)
        {
            busButton.interactable = true;
        }

        if(tramLinesAvailable != 0)
        {
            tramButton.interactable = true;
        }

        if (trainLinesAvailable != 0)
        {
            trainButton.interactable = true;
        }
    }

    //removes event listeners
    //https://docs.unity3d.com/530/Documentation/ScriptReference/UI.Selectable-interactable.html
    private void removeOtherEventListeners(string mode)
    {

        tramButton.interactable = false;
        trainButton.interactable = false;
        busButton.interactable = false;

            switch (mode)
        {
            case "Bus":
                tramButton.interactable = false;
                trainButton.interactable = false;
                break;
            case "Tram":
                busButton.interactable = false;
                trainButton.interactable = false;
                break;
            case "Train":
                busButton.interactable = false;
                tramButton.interactable = false;
                break;
        }
    }

    public void AvailableLines(string type)
    {
        switch (type)
        {
            case "bus":
                busLinesAvailable += 1;
                busAvailable.text = $"{busLinesAvailable}";
                break;
            case "tram":
                tramLinesAvailable += 1;
                break;
            case "train":
                tramLinesAvailable += 1;
                break;
        }
       
    }


    void InitializeRandomPopulationEntries()
    {
        bool transportationNeed = false;

        for (var i = 0; i < 10; i++)
        {
            StartCoroutine(InitialPopulation(transportationNeed));
        }

    }




    IEnumerator InitialPopulation(bool need = false)
    {
        // loop ensures that each population generation and its effects are processed one at a time
        foreach (var districtName in dbManager.districtNames)
        {
            var (nDep, nDest, ntype) = contentGeneration.GetNextDistricts();

            string type = ntype;
            bool transportationNeed = need;

            string departure = nDep;
            string destination = nDest;

            //Set to zero as not relevant for satisfaction calculation
            float best_time = 0f;
            float actual_time = 0f; 
            float satisfaction = 0f;
           
            dbManager.InsertPopulation(type, departure, destination, best_time, actual_time, transportationNeed, satisfaction);
            dbManager.UpdateDistrictPopulation(departure, 100);

            yield return StartCoroutine(UpdateDistrictAmountsCoroutine(departure, destination, type));

            yield return null; // Wait for the next frame
        }

    }

    public void UpdateDistrictAmounts(string departureDistrict, string destinationDistrict, string type)
    {
        //// Check if  type is industrial, update the industrial amount in  destination district
        if (type == "industry")
        {
            dbManager.UpdateDistrictAmounts(destinationDistrict, 5.0f, 0, 0);
        }//// Check if the type is commercial 
        else if (type == "commercial")
        {
            dbManager.UpdateDistrictAmounts(destinationDistrict, 0, 5.0f, 0);
        }

        // Increase the residential amount in the departure district
        //dbManager.UpdateDistrictAmounts(departureDistrict, 0, 0, 5.0f);
    }

    IEnumerator UpdateDistrictAmountsCoroutine(string departureDistrict, string destinationDistrict, string type)
    {
        // Prepare the amount changes based on type
        float industrialChange = type == "industry" ? 1.0f : 0f;
        float commercialChange = type == "commercial" ? 1.5f : 0f;
        float residentialChange = 2.5f;

        // update database (if dbManager.UpdateDistrictAmounts is synchronous)
        dbManager.UpdateDistrictAmounts(departureDistrict, industrialChange, commercialChange, residentialChange);
        dbManager.UpdateDistrictAmounts(destinationDistrict, industrialChange, commercialChange, residentialChange);

        // Since the update is synchronous, immediately yield to the next frame to keep things responsive
        yield return null;
    }


    void PopulateRandomEntry(bool need = true)
    {
        float footPathWeight = 3f; // Default weight of a foot path

        if (dbManager.districtNames.Count >= 2)
        {
            // Randomly assign a type to the population
            string[] types = { "industry", "commercial" };
            string type = types[Random.Range(0, types.Length)];

            bool transportationNeed = need;

            // Get two different random districts for departure and destination
            string departure = dbManager.districtNames[Random.Range(0, dbManager.districtNames.Count)];
            string destination;
            do
            {
                destination = dbManager.districtNames[Random.Range(0, dbManager.districtNames.Count)];
            } while (destination == departure);

            // Calculates times and satisfaction
            float best_time = gridManager.FindAndDisplayPath(departure, destination); 
            float worst_time = best_time * footPathWeight;
            float actual_time = gridManager.FindAndDisplayPathTransport(departure, destination); 
            float satisfaction = CalculateSatisfaction(best_time, worst_time, actual_time);

            // Insert the population data into the database
            dbManager.InsertPopulation(type, departure, destination, best_time, actual_time, transportationNeed, satisfaction);
            dbManager.UpdateDistrictPopulation(departure, 100);

            // Get departure
            Debug.Log(departure);
            ActivateStationAnimation(departure);
            populationPop.Play();

        }
        else
        {
            Debug.LogError("Not enough districts to choose from for population initialization.");
        }
    }

    public void UpdateForEveryPopulation()
    {
        if (timer.timeStopped) return;

        //get number of districts
        //districtCount = gridManager.districtNames.Count();

        for (int i = 0; i < gridManager.districtNames.Count(); i++)
        {
            for (int j = 0; j < gridManager.districtNames.Count(); j++)

            {
                float footPathWeight = 3f; // Default weight of a foot path
                float best_time = gridManager.FindAndDisplayPath(gridManager.districtNames[i], gridManager.districtNames[j]); 
                float worst_time = best_time * footPathWeight;
                float actual_time = gridManager.FindAndDisplayPathTransport(gridManager.districtNames[i], gridManager.districtNames[j]); 
                float satisfaction = CalculateSatisfaction(best_time, worst_time, actual_time);
                //Debug.Log(gridManager.districtNames[i] + " " + gridManager.districtNames[j] + " " + actual_time + " " + satisfaction);
                dbManager.UpdatePopulationDatabase(gridManager.districtNames[i], gridManager.districtNames[j], actual_time, satisfaction);
            }
        }

    }

    public void ActivateStationAnimation(string departure)
    {
        if (timer.timeStopped) return;
        // gets the last three characters from departure
        string lastThree = departure.Substring(departure.Length - 3);

        // Get station name
        string stationName = "Station " + lastThree;

        // Find the object with the constructed name
        GameObject stationObject = GameObject.Find(stationName);

        // finds the inhabitants
        Transform inhabitantsTransform = stationObject.transform.Find("Inhabitants");
        GameObject inhabitants = inhabitantsTransform.gameObject;

        inhabitantsAnimator = inhabitants.GetComponent<Animator>();

        inhabitants.SetActive(true);

    }

    public void FinsihStationAnimation(string districtName)
    {
        // gets the last three characters from departure
        string lastThree = districtName.Substring(districtName.Length - 3);

        // Get station name
        string stationName = "Station " + lastThree;

        Debug.Log(stationName);

        // Find the object with the constructed name
        GameObject stationObject = GameObject.Find(stationName);

        // finds the inhabitants
        Transform inhabitantsTransform = stationObject.transform.Find("Inhabitants");
        GameObject inhabitants = inhabitantsTransform.gameObject;

        inhabitantsAnimator = inhabitants.GetComponent<Animator>();

        inhabitantsAnimator.SetBool("ButtonWasClicked", true);
        //inhabitants.SetActive(true);
    }



    void PopulationUpdate()
    {
        if (timer.timeStopped)
        {
            Debug.Log("Timer is stopped; PopulationUpdate is returning early.");
            return; // prevents updates to the game environment when the time is stopped
        }

        Debug.Log("PopulationUpdate started");

        try
        {
            float footPathWeight = 3f; // the default weight of a foot path

            (string nextDepartureDistrict, string nextDestinationDistrict, string nextType) = contentGeneration.GetNextDistricts();
            Debug.Log("Next departure: " + nextDepartureDistrict + ", Next destination: " + nextDestinationDistrict + ", Next type: " + nextType);

            if (nextType == "none") // if there were no player interactions or desired distribution matches actual distribution
            {
                Debug.Log("Populating a random entry because nextType is 'none'.");
                PopulateRandomEntry();
            }
            else
            {
                Debug.Log("Processing population update with type: " + nextType);
                if (dbManager.districtNames.Count >= 2)
                {
                    bool transportationNeed = true;
                    string type = nextType;
                    string departure = nextDepartureDistrict;
                    string destination = nextDestinationDistrict;

                    if (destination == departure)
                    {
                        do
                        {
                            destination = dbManager.districtNames[Random.Range(0, dbManager.districtNames.Count)];
                        } while (destination == departure);
                    }

                    float best_time = gridManager.FindAndDisplayPath(departure, destination);
                    float worst_time = best_time * footPathWeight;
                    float actual_time = gridManager.FindAndDisplayPathTransport(departure, destination);
                    float satisfaction = CalculateSatisfaction(best_time, worst_time, actual_time);

                    dbManager.InsertPopulation(type, departure, destination, best_time, actual_time, transportationNeed, satisfaction);
                    dbManager.UpdateDistrictPopulation(departure, 100);
                    populationAmount += 100;
                    inhabitantsAmount.text = "Population: " + ((120 * 100) + populationAmount);
                    ActivateStationAnimation(departure);
                }
                else
                {
                    Debug.LogError("Not enough districts to choose from.");
                }

                contentGeneration.GetCurrentRatio();
                contentGeneration.GetDesiredRatio();

                Debug.Log(contentGeneration.GetNextDistricts());
                UpdateSatisfactionAvgOverall(); // calls method to update satisfaction
              
                populationPop.Play();
                StartCoroutine(UpdateDistrictAmountsCoroutine(nextDepartureDistrict, nextDestinationDistrict, nextType));
            }
            Debug.Log("Finished updating population.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error in PopulationUpdate: " + ex.ToString());
        }
    }


    public void UpdateSatisfactionAvgOverall()
    {
        float avg = dbManager.GetOverallAverageSat();
        //Debug.Log(avg);
        satisfactionDisplay.text = $" {avg:F0}%";

        dbManager.UpdateDistrictSatisfactionScore();
    }

    public float CalculateSatisfaction(float bestTime, float worstTime, float actualTime)
    {
        // ensuring the actual time is within the range [bestTime, worstTime]
        actualTime = Mathf.Clamp(actualTime, bestTime, worstTime);

        if(actualTime == worstTime)
        {
            return 0f;
        }

        // normalising the actual time within the range [0, 100]
        float satisfaction = ((actualTime - worstTime) / (bestTime - worstTime)) * 100;

        //https://forum.unity.com/threads/converting-float-to-integer.27511/
        satisfaction = Mathf.CeilToInt(satisfaction);
        return satisfaction;
    }



    public void LevelCheck() // This code checks which level was selected
    {
        
    }

    public void SetLevelGoal() //This method sets the goal end scenario
    {

    }

    public void CheckLevelSuccess() // This method checks if the scenario was succesfully finished
    {
        float avgSatisfaction = dbManager.GetOverallAverageSat();

        if (avgSatisfaction > 65.0f)
        {
            sucessScreen.SetActive(true);
        }
        else
        {
            failScreen.SetActive(true);
        }
    }

    public void ChangeSound()
    {
        trapBeat.Stop();
        trapBeatFast.Play();

    }


    //UPDATE SATISFACTION CALCULATION
}


//available line - 1 bevor inserting data into table