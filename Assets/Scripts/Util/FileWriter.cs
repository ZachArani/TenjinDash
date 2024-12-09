using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Util class for writing files based on player data.
/// </summary>
public class FileWriter : MonoBehaviour
{

    StreamWriter file;
    StringBuilder data;
    public string filename;
    public NewMovement player;

    // Start is called before the first frame update
    void OnEnable()
    {
        data = new StringBuilder();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        data.AppendLine($"{player.t}, {player.targetSpeed}, {player.currentSpeed}");
    }



    private void OnDisable()
    {
        WriteFile();
    }

    private void WriteFile()
    {
        filename = $"{Application.persistentDataPath}/{filename}.csv";
        file = File.CreateText(filename);
        Debug.Log($"writing {filename}");
        using (file)
        {
            file.WriteLine(data);
        }
    }



}
