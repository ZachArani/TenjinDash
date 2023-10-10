using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RestartButtonHandler : MonoBehaviour
{
    // Start is called before the first frame update

    Button button;

    void Start()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(Task);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void Task()
    {
        Debug.Log("loading scene");
        SceneManager.LoadScene("Menu");
    }
}
