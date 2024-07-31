using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using UnityEngine.Playables;
using PathCreation;
using System.Diagnostics;

public class ButtonHandler : MonoBehaviour
{
    // Start is called before the first frame update

    Button button;
    GameObject loading;
    GameObject preRoll;
    public static bool inMenu = true;
    Options options;

    void Start()
    {
        button = gameObject.GetComponent<Button>();
        loading = GameObject.Find("Loading");
        preRoll = GameObject.Find("PreRollCameras").transform.GetChild(0).gameObject;
        options = GameObject.Find("Options").GetComponent<Options>();
        if (loading != null)
            loading.GetComponent<CanvasRenderer>().SetAlpha(0f);
        inMenu = true;
        if (options.SkipMenu)
        {
            GameObject.Find("Timelines").transform.Find("MenuTimeline").GetComponent<PlayableDirector>().Stop();
            if (options.skipPreroll)
            {
                GoToGame();
            }
            else
            {
                GoToAnimations();
            }
        }
        else if(options.skipPreroll)
        {
            GoToGame();
        }
    }

    public void Auto()
    {
        GameObject.Find("Player1").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.MOCK_DATA;
        GameObject.Find("Player2").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.MOCK_DATA;
        GoToAnimations(); 
    }

    public void GoToAnimations()
    {
        if(!new StackFrame(1).GetMethod().Name.Equals("Auto")) //Figure out if we're getting called from the auto method or not
        {
            GameObject.Find("Player1").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.TWO_PLAYER; //If not, then set us to Two Player mode
            GameObject.Find("Player2").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.TWO_PLAYER;
        }
        else if(options.isAuto)
        {
            GameObject.Find("Player1").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.MOCK_DATA;
            GameObject.Find("Player2").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.MOCK_DATA;
        }
        else if(!options.isAuto && !new StackFrame(1).GetMethod().Name.Equals("Auto"))
        {
            GameObject.Find("Player1").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.TWO_PLAYER;
            GameObject.Find("Player2").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.TWO_PLAYER;
        }
        UnityEngine.Debug.Log("Switching to intro animation");
        GameObject.Find("MenuLogo").SetActive(false);
        GameObject.Find("StartButton").SetActive(false);
        GameObject.Find("Auto").SetActive(false);
        GameObject.Find("Credits").SetActive(false);
        GameObject.Find("JoyCons").SetActive(false);
        GameObject.Find("Timelines").transform.Find("MenuTimeline").GetComponent<PlayableDirector>().Stop();
        GameObject.Find("Timelines").transform.Find("PreRollTimeline").GetComponent<PlayableDirector>().Play();
        inMenu = false;


    }
    public void GoToGame()
    {
        GameObject.Find("MenuLogo").SetActive(false);
        GameObject.Find("StartButton").SetActive(false);
        GameObject.Find("Auto").SetActive(false);
        GameObject.Find("Credits").SetActive(false);
        GameObject.Find("JoyCons").SetActive(false);
        GameObject UI = GameObject.Find("UI");
        //GameObject.Find("Music").transform.Find("Menu").GetComponent<MenuMusicHandler>().StopMusic();
        UI.transform.Find("P1Pos").gameObject.SetActive(true);
        UI.transform.Find("P2Pos").gameObject.SetActive(true);

        //GameObject.Find("MenuCamera").gameObject.SetActive(false);

        GameObject.Find("PreRollCameras").transform.Find("FinishPlayer1").gameObject.SetActive(false);
        GameObject.Find("PreRollCameras").transform.Find("FinishPlayer2").gameObject.SetActive(false);

        GameObject.Find("FakePlayer1").gameObject.SetActive(false);
        GameObject.Find("FakePlayer2").gameObject.SetActive(false);

        if(options.isAuto)
        {
            GameObject.Find("Player1").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.MOCK_DATA;
            GameObject.Find("Player2").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.MOCK_DATA;
        }
        else
        {
            GameObject.Find("Player1").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.TWO_PLAYER;
            GameObject.Find("Player2").GetComponent<MovementHandler>().GAME_MODE = GAME_MODES.TWO_PLAYER;
        }

        GameObject.Find("UI").transform.Find("Restart").gameObject.SetActive(false);
        GameObject.Find("UI").transform.Find("Menu").gameObject.SetActive(false);

        GameObject.Find("UI").transform.Find("Countdown").gameObject.GetComponent<CountdownHandler>().StartCounting();

        inMenu = false;
    }

    public void Pressed() { }



}
