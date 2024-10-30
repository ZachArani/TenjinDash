using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class InputRecorder : MonoBehaviour
{

    StreamWriter file;
    Joycon j;
    StringBuilder data;
    string filename;

    // Start is called before the first frame update
    void Start()
    {
        data = new StringBuilder();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void writeLine(Vector3 line)
    {
        data.AppendLine($"{StateManager.instance.stateStopwatch.ElapsedTicks},{line.magnitude},");
    }

    private void OnEnable()
    {
        GetComponent<JoyconReceiver>().j.OnNewGyroData += writeLine;
    }

    private void OnDisable()
    {
        GetComponent<JoyconReceiver>().j.OnNewGyroData -= writeLine;
        WriteFile();
    }

    private void WriteFile()
    {
        filename = $"{Application.persistentDataPath}/{gameObject.name}_{System.DateTime.Now:yyyyMMddTHHmmss}.csv";
        file = File.CreateText(filename);
        Debug.Log($"writing {filename}");
        using (file)
        {
            file.WriteLine(data);
        }
    }



}
