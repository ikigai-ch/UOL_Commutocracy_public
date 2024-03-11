using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ContentGeneration : MonoBehaviour
{

    Dictionary<string, DistrictDataDistribution> currentRatioDict = new Dictionary<string, DistrictDataDistribution>();
    Dictionary<string, DesiredDistrictDataDistribution> desiredRatioDict = new Dictionary<string, DesiredDistrictDataDistribution>();
    Dictionary<string, DesiredDistrictDataDistribution> newDesiredRatioDict = new Dictionary<string, DesiredDistrictDataDistribution>();

    DatabaseManager databaseManager;

    // Start is called before the first frame update
    void Start()
    {
        databaseManager = GetComponent<DatabaseManager>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetCurrentRatio()
    {
        currentRatioDict = databaseManager.FetchCurrentDistrictRatios();

    }

    public void GetDesiredRatio()
    {
        desiredRatioDict = databaseManager.FetchDesiredDistrictRatios();
    
    }

    public (string, string, string)GetNextDistricts()
    {
        //calculate distance
        //highest distance stets destination type for inhabitant
        (string nextDepartureDistrict, float distResidential) = databaseManager.GetDistrictWithHighestResidentialDifference();
        Debug.Log("Next is: " + nextDepartureDistrict);

        // If the district name is empty, select a random one
        if (string.IsNullOrEmpty(nextDepartureDistrict))
        {
            Debug.Log("No district with residential difference found, selecting a random departure district.");
            nextDepartureDistrict = databaseManager.districtNames[Random.Range(0, databaseManager.districtNames.Count)];
        }

        (string nextDestinationDistrict, string populationType) = CompareDifferencesAndGetHighest();
        return (nextDepartureDistrict, nextDestinationDistrict, populationType);
    }

    public (string, string) CompareDifferencesAndGetHighest()
    {
        var (commercialDistrict, commercialProportionalDiff) = databaseManager.GetDistrictWithHighestCommercialDifference();
        var (industrialDistrict, industrialProportionalDiff) = databaseManager.GetDistrictWithHighestIndustrialDifference();

        if (commercialProportionalDiff > industrialProportionalDiff)
        {
            return (commercialDistrict, "commercial");
        }
        else if (industrialProportionalDiff > commercialProportionalDiff)
        {
            return (industrialDistrict, "industry");
        }
        else
        {

            // Randomly assign a type to the population
            string[] types = { "industry", "commercial" };
            string type = types[Random.Range(0, types.Length)];
            string destination = databaseManager.districtNames[Random.Range(0, databaseManager.districtNames.Count)];
            return (destination, type); // No clear winner or differences are very close
        }
    }

    public void DistanceRatioPerDistrict(string inhabitantType)
    {
        //add inhabitant type to database
        //get highet distance for residential
        // set this to the departure

        //get distanace of current distance for ratio of inhabtant type machting to distribution
        //set this to

    }

    //Updated the desired ratio
    public void PushDesiredRatio(List<string> districtNames, TransportType transportType) 
    {
        float maxIndustry = 5f;
        float maxCommerical = 10f;
        float maxResidential = 15f;

        int mediumIncrease = 2;
        int weakIncrase = 3;

        foreach(var districtName in districtNames)
        {
            if(desiredRatioDict.ContainsKey(districtName))
            {
                var data = desiredRatioDict[districtName];

                if (transportType == TransportType.Bus)
                {
                    data.DesiredResidentialAmount += maxResidential;
                }
                else if (transportType == TransportType.Tram)
                {
                    // residential little
                    data.DesiredCommercialAmount += maxCommerical;
                    //commercial a lot
                    data.DesiredResidentialAmount += maxResidential/ mediumIncrease;

                    data.DesiredIndustryAmount += maxIndustry/ weakIncrase;

                }
                else if (transportType == TransportType.Train)
                {
                    //increase industrial desire a lot
                    data.DesiredIndustryAmount += maxIndustry;
                    // and residential a little
                    data.DesiredResidentialAmount += maxResidential / weakIncrase;
                }
                newDesiredRatioDict[districtName] = NormalizeDistribution(data);
            }
        }
        PushUpdatesToDatabase();
    }

    private DesiredDistrictDataDistribution NormalizeDistribution(DesiredDistrictDataDistribution data)
    {
        float total = data.DesiredResidentialAmount + data.DesiredCommercialAmount + data.DesiredIndustryAmount;
        if (total != 100)
        {
            float scale = 100 / total;
            data.DesiredResidentialAmount *= scale;
            data.DesiredCommercialAmount *= scale;
            data.DesiredIndustryAmount *= scale;
        }
        return data;
    }

    public void PushUpdatesToDatabase()
    {
        foreach (var entry in newDesiredRatioDict)
        {
            databaseManager.UpdateDistrictDesiredRatios(entry.Key, entry.Value);
        }

        databaseManager.UpdateDistrictSatisfactionScore();
    
        newDesiredRatioDict.Clear();
    }

    public void InitialDesirePush()
    {
 

    }
}
