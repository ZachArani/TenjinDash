using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using System.Linq;

public class FinishedHandler : MonoBehaviour, IFinishMessages
{
    // Start is called before the first frame update
    TextMeshProUGUI text;
    public bool isFinished;
    public bool isInSplitFinish;
    public bool isAtFinishLine;
    float finishTime = 0f;
    GameObject restartButton;
    public GameObject player1;
    public GameObject player2;
    public GameObject menuCamera;
    public GameObject player1Cam;
    public GameObject player2Cam;
    MusicHandler musicHandler;
        

    Stopwatch stopwatch;

    void Start()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
        isFinished = false;
        isInSplitFinish = false;
        isAtFinishLine = false;
        musicHandler = GameObject.Find("Music").transform.Find("Race").GetComponent<MusicHandler>();

    }

    // Update is called once per frame
    void Update()
    {
        if(isFinished && finishTime == 0f)
        {
            finishTime = Time.time;
        }  
        else if(finishTime != 0 && Time.time - finishTime > 4f)
        {
            Restart();
        }
        
    }
    public void finishedRace()
    {
        text.text = "FINISH!";
        isFinished = true;
        isInSplitFinish = true;
        bool playerOneWin = player1.transform.position.x > player2.transform.position.x ? true : false; //Get who won
        float winDistance = playerOneWin ? //Get distance between the winners
            player1.transform.position.x - player2.transform.position.x :
            player2.transform.position.x - player1.transform.position.x;
        //player1.transform.position = new Vector3(-1007.75f, 256.880005f, -442.790009f);
        //player2.transform.position = new Vector3((float)(-1006.5 + winDistance), 256.880005f, -439.566f);
        GameObject.Find("Timelines").transform.Find("FinishedTimeline").GetComponent<PlayableDirector>().Play();


    }

    public void nearFinishedRace()
    {
        isInSplitFinish = true;
        UnityEngine.Time.timeScale = 0.03f;
        GameObject.Find("Timelines").transform.Find("NearFinishedTimeline").GetComponent<PlayableDirector>().Play();
    }

    public void atFinishLine()
    {
        isAtFinishLine = true;
        UnityEngine.Time.timeScale = 0f;
        List<PlayableBinding> bindings = GameObject.Find("Timelines").transform.Find("AtFinishLineTimeline").GetComponent<PlayableDirector>().playableAsset.outputs.ToList();
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        if(player1.transform.position.x > player2.transform.position.x) //P1 wins
        {
            GameObject.Find("Timelines").transform.Find("AtFinishLineTimeline").GetComponent<PlayableDirector>().Play();
        }
        else //P2 wins
        {
            GameObject.Find("Timelines").transform.Find("AtFinishLineTimelineP2").GetComponent<PlayableDirector>().Play();
        }
    }

    public void Restart()
    {
        //restartButton.SetActive(true);
        //SceneManager.LoadScene("Course");

    }

    public void exitSplitFinish()
    {
        isInSplitFinish = false;
    }

    public void ResumeTime()
    {
        UnityEngine.Time.timeScale = 1f;
    }

    public void ShowRestart()
    {

    }

    public void SwitchToLoop()
    {

    }

    public void BackToMenu()
    {
        GameObject.Find("Timelines").transform.Find("AtFinishLineTimeline").gameObject.GetComponent<PlayableDirector>().Stop();
        GameObject.Find("Timelines").transform.Find("AtFinishLineTimelineP2").gameObject.GetComponent<PlayableDirector>().Stop();
        //Do whatever we need to reset the game state back to default
        GameObject.Find("UI").transform.Find("Restart").gameObject.SetActive(false);
        GameObject.Find("UI").transform.Find("Menu").gameObject.SetActive(false);
        CleanState();
        GameObject.Find("Timelines").transform.Find("MenuTimeline").gameObject.GetComponent<PlayableDirector>().Play();
    }

    public void RestartGame()
    {
        GameObject.Find("Timelines").transform.Find("AtFinishLineTimeline").gameObject.GetComponent<PlayableDirector>().Stop();
        GameObject.Find("Timelines").transform.Find("AtFinishLineTimelineP2").gameObject.GetComponent<PlayableDirector>().Stop();
        GameObject.Find("UI").transform.Find("Restart").gameObject.SetActive(false);
        GameObject.Find("UI").transform.Find("Menu").gameObject.SetActive(false);
        CleanState();
        //Do whatever we need to reset the game state back to default
        GameObject.Find("Timelines").transform.Find("PreRollTimeline").gameObject.GetComponent<PlayableDirector>().Play();
    }

    public void ToFinalScreen()
    {
        GameObject.Find("PreRollCameras").transform.Find("FinishPlayer1").GetComponent<Animator>().enabled = true;
        GameObject.Find("PreRollCameras").transform.Find("FinishPlayer2").GetComponent<Animator>().enabled = true;
        GameObject.Find("Timelines").transform.Find("AtFinishLineTimeline").gameObject.GetComponent<PlayableDirector>().Stop();
        GameObject.Find("Timelines").transform.Find("FinishedTimeline").gameObject.GetComponent<PlayableDirector>().Play();
    }

    public void CleanState()
    {
        isFinished = isAtFinishLine = isInSplitFinish = false;
        GameObject.Find("UI").transform.Find("Countdown").gameObject.GetComponent<CountdownHandler>().isCounting = false;
        GameObject.Find("UI").transform.Find("Countdown").gameObject.GetComponent<CountdownHandler>().ResetCount();

        menuCamera.SetActive(false);
        player1Cam.SetActive(true);
        player2Cam.SetActive(true);
        musicHandler.ResetState();

        player1.SetActive(true);
        player2.SetActive(true);
        player1.GetComponent<MovementHandler>().startedRun = false;
        player2.GetComponent<MovementHandler>().startedRun = false;
        player1.GetComponent<Animator>().enabled = false;
        player2.GetComponent<Animator>().enabled = false;
        player1.GetComponent<Animator>().enabled = true;
        player2.GetComponent<Animator>().enabled = true;

        GameObject.Find("UI").transform.Find("Menu").transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.white;
        GameObject.Find("UI").transform.Find("Restart").transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.white;
        GameObject.Find("UI").transform.Find("StartButton").transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.white;
        player1.transform.position = new Vector3(-1426.05981f, 257.023987f, -450.068176f);
        player2.transform.position = new Vector3(-1425.58569f, 256.99649f, -453.650909f);
        player1.transform.rotation = new Quaternion(0, 0.62237215f, 0, 0.782721519f);
        player2.transform.rotation = new Quaternion(0, 0.655845106f, 0, 0.754895508f);

        player1.GetComponent<Animator>().Play("Countdown");
        player2.GetComponent<Animator>().Play("Countdown");
    }
}
