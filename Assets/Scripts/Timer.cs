using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

//https://gamedevbeginner.com/how-to-make-countdown-timer-in-unity-minutes-seconds/
public class Timer : MonoBehaviour
{

    public GameObject gameManagerObj;
    GameManager gameManger;

    public TextMeshProUGUI timerText;
    public float timerTime = 300;

    public GameObject pauseButton;
    public GameObject playButton;


    public bool timeStopped = false;
    private bool changeSoundCalled = false;

    private void Start()
    {
        gameManger = gameManagerObj.GetComponent<GameManager>();
        gameManagerObj = GameObject.Find("GameManager");
      
    }

    void Update()
    {
        if(timeStopped == false) //checks if play or pause button was clicked
        {
            if (timerTime > 0)
            {
                timerTime -= Time.deltaTime;
                if (timerTime <= 30.0f && !changeSoundCalled)
                {
                    gameManger.ChangeSound();
                    changeSoundCalled = true; // Set the flag to true to prevent multiple calls
                }
            }
            else
            {
                timerTime = 0;
               

            }

            DisplayTime(timerTime);
        }

    }


    void DisplayTime(float timerTime)
    {
        if(timerTime < 0)
        {
            timerTime = 0;
            timeStopped = true;
            gameManger.CheckLevelSuccess();
        }
        else if(timerTime > 0)
        {
            timerTime += 1; //to display 1 second if player has 1 second left
        }
        float minutes = Mathf.FloorToInt(timerTime / 60);
        float seconds = Mathf.FloorToInt(timerTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        //Debug.Log(timerText.text);
    }

   public void PauseButtonClick() //checks if play or pause button was clicked
   {

        Debug.Log("Everybody on mute");
        if(!timeStopped)
        {
            timeStopped = true;
        }
        else if(timeStopped)
        {
            timeStopped = false;
        }
        Debug.Log("time is " + timeStopped);


   }

      

    //void Start()
    //{
    //    // Start the timer
    //    StartTimer();
    //}

    //public void StartTimer()
    //{

    //}





}


