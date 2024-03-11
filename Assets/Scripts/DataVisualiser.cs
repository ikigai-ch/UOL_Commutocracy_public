using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class DataVisualizer : MonoBehaviour
{
    public GameObject dataPrefab; // Assign in inspector
    public TextMeshProUGUI dataText;
    public Transform contentPanel; // Assign in inspector

    private DatabaseManager dbManager;

    //piechart  prefab
    public GameObject _pieChartParent;
    private GameObject industrialSegment;
    private GameObject commercialSegment;
    private GameObject residentialSegment;

    private Image industrialFill;
    private Image commercialFill;
    private Image residentialFill;

    public GameObject visualisationScreen;

    void Start()
    {
        dbManager = FindObjectOfType<DatabaseManager>();


        StartCoroutine(DelayedVisualizationUpdate("District 0 0", 0.5f));
    }

    private void Update()
    {
        //DisplayData();
    }

    IEnumerator DelayedVisualizationUpdate(string districtName, float delayInSeconds)
    {
        yield return new WaitForSeconds(delayInSeconds);

        UpdateVisualization(districtName);
    }


    public void UpdateVisualization(string districtName)
    {
       

            // Update District Name
            UpdateDistrictName(visualisationScreen, districtName);

            // Update Population Count
            //UpdatePopulationCount(visualisationScreen, districtName);

            UpdateList(visualisationScreen, districtName);
            UpdateDistrictSatisfaction(visualisationScreen, districtName);

            DisplayData(districtName);
       
    }


    private void UpdateDistrictName(GameObject visualisationInstance, string districtName)
    {
        // Change name of the district to clicked district
        TextMeshProUGUI districtNameText = visualisationInstance.transform.Find("DistrictName").GetComponent<TextMeshProUGUI>();

        if (districtNameText != null)
        {
            districtNameText.text = districtName; // Sets to the name of the clicked district
        }
        else
        {
            Debug.LogError("DistrictName TextMeshProUGUI component not found in visualisation screen.");
        }
    }

    private void UpdateDistrictSatisfaction(GameObject visualisationInstance, string districtName)
    {
        // Change name of the district to clicked district
        TextMeshProUGUI satScoreText = visualisationInstance.transform.Find("SatScore").GetComponent<TextMeshProUGUI>();
        float score = dbManager.GetDistrictSatisfaction(districtName);
        satScoreText.text = $"{score:F0}% ";
    }



    //List of Transportation needs
    private void UpdateList(GameObject visualisationInstance, string districtName)
    {
        List<(string, int, float)> destinationDistricts = dbManager.GetDestinationDistrictsFromSpecifiedDeparture(districtName);

        // Initialize an empty list to hold formatted strings for each group of up to 5 items
        List<string> formattedGroups = new List<string>();

        // Process  list in chunks of 5
        for (int i = 0; i < destinationDistricts.Count; i += 5)
        {
            var chunk = destinationDistricts.Skip(i).Take(5)
                          .Select(tuple => $"<b>{tuple.Item1}</b>: {tuple.Item3 :F0}% "); 
                                                                                                
            string joinedChunk = string.Join("   ", chunk);
            formattedGroups.Add(joinedChunk);
        }

        // Joingroups with a newline
        string displayText = string.Join("\n", formattedGroups);

        Transform listItem = visualisationInstance.transform.Find("DistrictListItems");
        TextMeshProUGUI districtNameText = listItem.transform.Find("ListItemName").GetComponent<TextMeshProUGUI>();

        districtNameText.text = displayText;
    }


    private void UpdatePopulationCount(GameObject visualisationInstance, string districtName)
    {
        // Update Population Count
        TextMeshProUGUI populationCountText = visualisationInstance.transform.Find("PopulationCount").GetComponent<TextMeshProUGUI>();
        if (populationCountText != null)
        {
            int populationCount = dbManager.GetDistrictPopulation(districtName);
            populationCountText.text = populationCount.ToString();
        }
        else
        {
            Debug.LogError("PopulationCount TextMeshProUGUI component not found in visualisation screen.");
        }

    }



    private void UpdatePieChart(GameObject visualisationInstance, string districtName)
    {
        // Code to update pie chart...
    }

   

    void DisplayData(string districtName)
    {

        // Update references to the instantiated object
        industrialFill = _pieChartParent.transform.GetChild(0).GetComponent<Image>();
        commercialFill = _pieChartParent.transform.GetChild(1).GetComponent<Image>();
        residentialFill = _pieChartParent.transform.GetChild(2).GetComponent<Image>();

        // Now render the pie chart with updated references
        PieChartRenderer(districtName);
    }

    void PieChartRenderer(string districtName)
    {
        var districtData = dbManager.FetchDistrictRatiosByName(districtName);

        float industrialAmount = MapValueToNormalVector(districtData.IndustryAmount);
        float commercialAmount = MapValueToNormalVector(districtData.CommercialAmount);
        float residentialAmount = MapValueToNormalVector(districtData.ResidentialAmount);
    
        //industiral fill does not need to be adjusted since it's lowest in the hirarchy
        industrialFill.fillAmount = 1f;
        commercialFill.fillAmount = 1f - industrialAmount;
        residentialFill.fillAmount = 1f - (industrialAmount + commercialAmount);
    }

    float MapValueToNormalVector(float value)
    {
        return value / 100.0f;
    }

}
