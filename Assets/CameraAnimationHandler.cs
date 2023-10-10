using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimationHandler : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject preRollParent;
    GameObject preRoll1;
    GameObject preRoll2;
    GameObject preRoll3;
    GameObject menuRoll1;
    GameObject menuRoll2;
    GameObject menuRoll3;
    GameObject player1;
    GameObject player2;
    public static bool isRunning;
    void Start()
    {
        preRollParent = GameObject.Find("PreRollCameras");
        preRoll1 = preRollParent.transform.GetChild(0).gameObject;
        preRoll2 = preRollParent.transform.GetChild(1).gameObject;
        preRoll3 = preRollParent.transform.GetChild(2).gameObject;
        menuRoll1 = preRollParent.transform.GetChild(4).gameObject;
        menuRoll2 = preRollParent.transform.GetChild(5).gameObject;
        menuRoll3 = preRollParent.transform.GetChild(6).gameObject;
        isRunning = false;

        player1 = GameObject.Find("FakePlayer1");
        player2 = GameObject.Find("FakePlayer2");
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void cameraCallback()
    {
        preRoll3.GetComponent<Camera>().enabled = false;
        GameObject.Find("PreRollCameras").transform.Find("Dolly").GetChild(0).gameObject.GetComponent<Camera>().enabled = true;
        GameObject UI = GameObject.Find("UI");
        UI.transform.Find("P1Pos").gameObject.SetActive(true);
        UI.transform.Find("P2Pos").gameObject.SetActive(true);
        isRunning = false;
        GameObject.Find("UI").transform.Find("Countdown").gameObject.GetComponent<CountdownHandler>().StartCounting();
        //GameObject.Find("PreRollCameras").transform.Find("MainCamera").transform.SetParent(GameObject.Find("player1").transform, true);
    }

    public void ToPreRoll1()
    {
        isRunning = true;
        Debug.Log("Switching to PreRoll1");

        preRoll1.GetComponent<Camera>().enabled = true;
        preRoll1.GetComponent<Animator>().Play("PreRoll1");
    }    

    public void ToPreRoll2()
    {
        isRunning = true;
        Debug.Log("Switching to PreRoll2");
        preRoll1.GetComponent<Camera>().enabled = false;
        preRoll2.GetComponent<Camera>().enabled = true;
        preRoll2.GetComponent<Animator>().Play("PreRoll2");
    }

    public void ToPreRoll3()
    {
        isRunning = true;
        Debug.Log("Switching to PreRoll3");
        preRoll2.GetComponent<Camera>().enabled = false;
        preRoll3.GetComponent<Camera>().enabled = true;
        preRoll3.GetComponent<Animator>().Play("PreRoll3");
    }

    public void ToMenuRoll2()
    {
        Debug.Log("Switching to MenuRoll2");

        player1.GetComponent<Animator>().Play("MenuRun2");
        player2.GetComponent<Animator>().Play("MenuRun2");

        menuRoll1.GetComponent<Camera>().enabled = false;
        menuRoll2.GetComponent<Camera>().enabled = true;
        menuRoll2.GetComponent<Animator>().Play("MenuRoll2", -1, 0f);
        //menuRoll1.GetComponent<Animator>().StopPlayback();
    }

    public void ToMenuRoll3()
    {
        Debug.Log("Switching to MenuRoll3");

        player1.GetComponent<Animator>().Play("MenuRun3");
        player2.GetComponent<Animator>().Play("MenuRun3");

        menuRoll2.GetComponent<Camera>().enabled = false;
        menuRoll3.GetComponent<Camera>().enabled = true;
        menuRoll3.GetComponent<Animator>().Play("MenuRoll3", -1, 0f);
        //menuRoll2.GetComponent<Animator>().StopPlayback();
    }

    public void ToMenuRoll1()
    {
        Debug.Log("Switching back to MenuRoll1");

        player1.GetComponent<Animator>().Play("MenuRun1");
        player2.GetComponent<Animator>().Play("MenuRun1");


        menuRoll3.GetComponent<Camera>().enabled = false;
        menuRoll1.GetComponent<Camera>().enabled = true;
        menuRoll1.GetComponent<Animator>().Play("MenuRoll1", -1, 0f);
        //menuRoll3.GetComponent<Animator>().StopPlayback();
    }



}
