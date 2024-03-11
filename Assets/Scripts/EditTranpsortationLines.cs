using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class EditTranpsortationLines : MonoBehaviour
{

    private Button[] lineButtons;
    BusManager busManager;
    public GameObject gameManager;
    private Dictionary<string, Button> lineButtonDictionary = new Dictionary<string, Button>();


    // Start is called before the first frame update
    void Start()
    {
        busManager = gameManager.GetComponent<BusManager>();

        GameObject[] buttonGOs = GameObject.FindGameObjectsWithTag("lineButtons");

        //https://stackoverflow.com/questions/69838537/get-every-button-with-a-certain-tag
        lineButtons = buttonGOs.Select(go => go.GetComponent<Button>()).ToArray();

        //https://learn.microsoft.com/en-us/dotnet/api/system.array.reverse?view=net-8.0
        Array.Reverse(lineButtons);

        // Disable all buttons initially
        foreach (var btn in lineButtons)
        {
            btn.gameObject.SetActive(false);
            lineButtonDictionary.Add(btn.name, btn);

        }
    }

    public void AssignButtonToLine(string lineCode, Color lineColor)
    {
        Button lineButton = lineButtons.FirstOrDefault(b => !b.gameObject.activeInHierarchy);
        if (lineButton != null)
        {
            ActivateLineButton(lineButton, lineCode, lineColor);
        }
    }

    private void ActivateLineButton(Button lineButton, string lineCode, Color lineColor)
    {
        lineButton.gameObject.SetActive(true);
        lineButton.onClick.RemoveAllListeners();

        var colors = lineButton.colors;
        colors.normalColor = lineColor;
        colors.highlightedColor = lineColor * 1.2f;
        lineButton.colors = colors;

        lineButton.onClick.AddListener(() => OnLineButtonClicked(lineCode, colors.normalColor));

        // Add this button to the dictionary with the associated line code
        lineButtonDictionary[lineCode] = lineButton;
    }


    void OnLineButtonClicked(string lineCode, Color lineColor)
    {
        Debug.Log($"Edit line: {lineCode}");

        LineData lineToEdit = busManager.allCurrentLines["Line"+lineCode];

        Debug.Log($"Editing line: {lineToEdit.LineName}");

        busManager.currentLine = lineToEdit;
        busManager.lineColour = lineColor;
        busManager.currentLine.TransportType = lineToEdit.TransportType;

        busManager.PrepareUIForLineEditing(lineToEdit, lineCode);
    }

    public void RemoveButtonForLine(string lineCode)
    {
        if (lineButtonDictionary.TryGetValue(lineCode, out Button lineButton))
        {
            lineButton.gameObject.SetActive(false);
            lineButton.onClick.RemoveAllListeners();
 
        }
        else
        {
            return;
        }
    }
}
