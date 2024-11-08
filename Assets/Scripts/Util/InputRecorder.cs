using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class InputRecorder : MonoBehaviour
{

    StreamWriter file;
    Joycon joycon;
    StringBuilder data;
    string filename;

    // Start is called before the first frame update
    void OnEnable()
    {
        data = new StringBuilder();
        joycon = JoyconManager.Instance.GetJoyconByPlayer(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        var gyro = joycon.GetGyro();
        var accel = joycon.GetAccel();
        //data.AppendLine($"{joycon.GetGyro().magnitude},");
        data.AppendLine($"{Mathf.Abs(accel.y)},");
    }


  
    private void OnDisable()
    {
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
