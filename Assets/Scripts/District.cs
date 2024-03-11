using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;



// Data structure to represent a district 
public class DistrictData
{
    public string DistrictName;
    public float IndustrialAmount;
    public float ResidentialAmount;
    public float CommercialAmount;
    public float DesiredResidentialAmount;
    public float DesiredCommercialAmount;
    public float DesiredIndustrialAmount;
    public string Characteristic;
    public int Population;
    public float Satisfaction;


    public DistrictData(string districtName, float industrialAmount, float residentialAmount, float commercialAmount, float desiredIndustrialAmount, float desiredResidentialAmount, float desiredCommercialAmount, string characteristic,  int population, float satisfaction)
    {
        DistrictName = districtName;
    
        ResidentialAmount = residentialAmount;
        IndustrialAmount = industrialAmount;
        CommercialAmount = commercialAmount;
        DesiredIndustrialAmount = desiredIndustrialAmount;
        DesiredResidentialAmount = desiredResidentialAmount;
        DesiredCommercialAmount = desiredCommercialAmount;
        Characteristic = characteristic;
        Population = population;
        Satisfaction = satisfaction;
    }
}

//https://www.youtube.com/watch?v=kkAjpQAM-jE
public class District : MonoBehaviour
{

    public string districtName;
    public TextMeshProUGUI districtText;
    public GameObject districtButton;
    public GameObject mainStationIcon;
    public GameObject highwayIcon;
    //DatabaseManager dbManager;

    public GameObject visualisationScreen; 

    public float totalDistribution = 100f;
    public float devidableBy = 5f;
    public float industiralAmount;
    public float residentialAmount;
    public float commercialAmount;
    public float desiredResidentialAmount;
    public float desiredCommercialAmount;
    public float desiredIndustiralAmount;
    public string characteristic;
    public int population = 0;
    public float satisfaction = 100f;

    public GameObject gManager;
    GridManager gridManager;

    public GameObject gameManagerObj;
    GameManager gameManger;


    // Start is called before the first frame update
    void Start()
    {

        gManager = GameObject.Find("GridManager");
        gameManagerObj = GameObject.Find("GameManager");
        gridManager = gManager.GetComponent<GridManager>();
        gameManger = gameManagerObj.GetComponent<GameManager>();
        //assign visualisation screen.
        //visualisationScreen = GameObject.FindWithTag("VisualisationScreen");
        ////deactivate immediately
        //visualisationScreen.SetActive(false);

        var posX = gameObject.transform.position.x;
        var posY = gameObject.transform.position.y;
        districtName = gameObject.name;
        districtText.text = districtName;

        Debug.Log(districtName);
        Debug.Log($"District {gridManager.firstIdNr} {gridManager.secondIdNr}");
        Debug.Log($"District {gridManager.motorDistrictIdX} {gridManager.motorDistrictIdY}");

        //Assigns a characteristic such as motorway exit or main station
        //https://discussions.unity.com/t/how-to-tell-if-a-string-contains-some-specified-text/18677
        //if (districtName.Contains($"District {gridManager.firstIdNr} {gridManager.secondIdNr}")) //If the name contains the ids defined in grid manager
        //{
        //    characteristic = "MainStation";
        //    mainStationIcon.SetActive(true);
        //}
        //else
        //{
        //    characteristic = "None";
        //}

        //if (districtName.Contains($"District {gridManager.motorDistrictIdX} {gridManager.motorDistrictIdY}"))
        //{
        //    characteristic = "MotorWay";
        //    highwayIcon.SetActive(true);
        //}
        


        districtButton.GetComponent<Button>().onClick.AddListener(() => DistrictButtonClicked(districtButton));


        StartCoroutine(InitialSetupCoroutine());

    }

    private IEnumerator InitialSetupCoroutine()
    {

        // Starts the initial distribution of inhabitant types
        yield return StartCoroutine(InitialDistributionCoroutine());

        // Once InitialDistribution is done, get district data
        DistrictData data = GetDistrictData();

        // Then, insert the data into the database
        StartCoroutine(DbDistrictEntryCoroutine(data));
    }

    private IEnumerator InitialDistributionCoroutine()
    {
        // Randomly distribute the amounts
        residentialAmount = Random.Range(20f, 90f);
        commercialAmount = Random.Range(10f, 90f);
        industiralAmount = Random.Range(1f, 20f);

        NormalizeDistribution();

        yield return null;
    }

    //Return district data as an object 
    public DistrictData GetDistrictData()
    {
        return new DistrictData(districtName, industiralAmount, residentialAmount, commercialAmount, desiredIndustiralAmount, desiredResidentialAmount, desiredCommercialAmount, characteristic, population, satisfaction);
    }

    // Coroutine for database entry
    private IEnumerator DbDistrictEntryCoroutine(DistrictData data)
    {
        DatabaseManager dbManager = FindObjectOfType<DatabaseManager>();
        if (dbManager != null)
        {
            yield return StartCoroutine(dbManager.InsertDistrictData(data));
        }
        else
        {
            Debug.LogError("DatabaseManager not found in the scene.");
        }
    }


    private void NormalizeDistribution()
    {
        float total = residentialAmount + commercialAmount + industiralAmount;

        if (total != totalDistribution)
        {
            // Calculates factor to scale each amount
            float scale = totalDistribution / total;

            // Scale amount proportionally and round to nearest multiple of 5
            residentialAmount = Mathf.Round((residentialAmount * scale) / devidableBy) * devidableBy;
            commercialAmount = Mathf.Round((commercialAmount * scale) / devidableBy) * devidableBy;
            industiralAmount = Mathf.Round((industiralAmount * scale) / devidableBy) * devidableBy;

            // Check new total, adjust to ensure sum == totalDistribution
            float newTotal = residentialAmount + commercialAmount + industiralAmount;
            float discrepancy = totalDistribution - newTotal;

            // Correct discrepancies 
            if (discrepancy != 0)
            {
                if (residentialAmount >= commercialAmount && residentialAmount >= industiralAmount)
                {
                    residentialAmount += discrepancy;
                }
                else if (commercialAmount >= residentialAmount && commercialAmount >= industiralAmount)
                {
                    commercialAmount += discrepancy;
                }
                else
                {
                    industiralAmount += discrepancy;
                }
            }

            desiredIndustiralAmount = industiralAmount;
            desiredResidentialAmount = residentialAmount;
            desiredCommercialAmount = commercialAmount;

            //RESET IT TO 0 for next calculation cycle
            industiralAmount = 0;
            residentialAmount = 0;
            commercialAmount = 0;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void DistrictButtonClicked(GameObject districtButton)
    {
        //Debug.Log(districtName);
        DataVisualizer dataVisualizer = FindObjectOfType<DataVisualizer>();
        if (dataVisualizer != null)
        {
            dataVisualizer.UpdateVisualization(districtName);
        }
        else
        {
            Debug.LogError("DataVisualizer not found in the scene.");
        }
        
    }


    public void ContinueStationAnimation() //if the district button is clicked the station animation is continued
    {
        gameManger.FinsihStationAnimation(districtName);
    }



}
