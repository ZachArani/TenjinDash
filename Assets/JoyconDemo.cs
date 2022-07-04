using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Math;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine.UI;
using Stopwatch = System.Diagnostics.Stopwatch;
using Debug = UnityEngine.Debug;

public class JoyconDemo : MonoBehaviour {
	
	private List<Joycon> joycons;

    // Values made available via Unity
    public float[] stick;
    public Vector3 gyro;
    public Vector3 accel;
    public int jc_ind = 0;
	public float lastJump = 0.0f;
	public static NamedPipeClientStream clientX = new NamedPipeClientStream("X");
	public static NamedPipeClientStream clientY = new NamedPipeClientStream("Y");
	public static NamedPipeClientStream clientZ = new NamedPipeClientStream("Z");
	public static NamedPipeClientStream clientSpeed = new NamedPipeClientStream("Speed");
	StreamWriter writerX;
	StreamWriter writerY;
	StreamWriter writerZ;
	StreamReader readerSpeed;
	private const float UPPER_THRESH = 1.3f;
	private const float LOWER_THRESH = 0.8f;
	float newPointX = 0.0f;
	private float[] speeds = new float[5] { 0, 0, 0, 0, 0 };
	private Stopwatch jogTimer = new Stopwatch();
	float rollingSumSpeed = 0;
	private bool aboveThresh = false;
	private bool belowThresh = false;
	public GameObject goal;
	public NavMeshAgent agent;
	public float speed;
	public static bool DEBUG = false;

	public string data = "time, acceleration"; //CSV format?

    public Quaternion orientation;

    void Start()
    {

        gyro = new Vector3(0, 0, 0);
        accel = new Vector3(0, 0, 0);
        // get the public Joycon array attached to the JoyconManager in scene
        joycons = JoyconManager.Instance.j;
        if (joycons.Count < jc_ind + 1)
        {
            Destroy(gameObject);
        }
        goal = GameObject.Find("Goal");

		if(DEBUG)
		{
			clientX.Connect();
			clientY.Connect();
			clientZ.Connect();
			clientSpeed.Connect();
			writerX = new StreamWriter(clientX);
			writerY = new StreamWriter(clientY);
			writerZ = new StreamWriter(clientZ);
			readerSpeed = new StreamReader(clientSpeed);
		}

		agent = gameObject.GetComponent<NavMeshAgent>();

        agent.destination = goal.transform.position;
        agent.speed = 0;

	}

	// Update is called once per frame
	void Update () {
		// make sure the Joycon only gets checked if attached
		if (joycons.Count > 0)
		{				
			Joycon j = joycons[jc_ind];
			// GetButtonDown checks if a button has been pressed (not held)
			if (j.GetButtonDown(Joycon.Button.SHOULDER_2))
			{
				UnityEngine.Debug.Log("Shoulder button 2 pressed");
				// GetStick returns a 2-element vector with x/y joystick components
				UnityEngine.Debug.Log(string.Format("Stick x: {0:N} Stick y: {1:N}", j.GetStick()[0], j.GetStick()[1]));

				// Joycon has no magnetometer, so it cannot accurately determine its yaw value. Joycon.Recenter allows the user to reset the yaw value.
				j.Recenter();
			}
			// GetButtonDown checks if a button has been released
			if (j.GetButtonUp(Joycon.Button.SHOULDER_2))
			{
				UnityEngine.Debug.Log("Shoulder button 2 released");
			}
			// GetButtonDown checks if a button is currently down (pressed or held)
			if (j.GetButton(Joycon.Button.SHOULDER_2))
			{
				UnityEngine.Debug.Log("Shoulder button 2 held");
			}

			if (j.GetButtonDown(Joycon.Button.DPAD_DOWN)) {
				UnityEngine.Debug.Log("Rumble");

				// Rumble for 200 milliseconds, with low frequency rumble at 160 Hz and high frequency rumble at 320 Hz. For more information check:
				// https://github.com/dekuNukem/Nintendo_Switch_Reverse_Engineering/blob/master/rumble_data_table.md

				j.SetRumble(160, 480, 0.6f, 200);

				// The last argument (time) in SetRumble is optional. Call it with three arguments to turn it on without telling it when to turn off.
				// (Useful for dynamically changing rumble values.)
				// Then call SetRumble(0,0,0) when you want to turn it off.
			}

			stick = j.GetStick();

			// Gyro values: x, y, z axis values (in radians per second)
			gyro = j.GetGyro();

            // Accel values:  x, y, z axis values (in Gs)
            accel = j.GetAccel();


			orientation = j.GetVector();
			if (j.GetButton(Joycon.Button.DPAD_UP)) {
				gameObject.GetComponent<Renderer>().material.color = Color.red;
			} else {
				//gameObject.GetComponent<Renderer>().material.color = Color.blue;
			}
			Vector3 position = new Vector3(accel.x - 0.166f, accel.y - 0.07f, accel.z + 1.02f);

			float currentTime = Time.time;
		if(DEBUG)
            {
				Debug.Log(DEBUG);
				if(System.DateTime.UtcNow.Millisecond % 10 != 0)
				{
					writerX.WriteLine($"{accel.x}");
					writerX.Flush();
					writerY.WriteLine($"{accel.y}");
					writerY.Flush();
					writerZ.WriteLine($"{accel.z}");
					writerZ.Flush();
					var line = readerSpeed.ReadLine();
					if(line != null)
					{
						speed = float.Parse(line);
						agent.speed = speed;
					}
				

				}
            }
        else
        {
			newPointX = accel.x;
			if (!aboveThresh && newPointX > UPPER_THRESH) //If we high above thresh for first time
			{
				belowThresh = false;
				aboveThresh = true; //Mark the flag
				jogTimer.Restart(); //Resarts the time (it was running in the period between lower/upper thresh).
			}
			else if (aboveThresh && newPointX < LOWER_THRESH) //If we've previously hit above thresh but haven't hit below
			{
				jogTimer.Stop(); //Stop the timer
				aboveThresh = false; //Flip flags
				belowThresh = true;
				float newSpeed = 1000 / jogTimer.ElapsedMilliseconds;
				jogTimer.Reset(); //Reset the clock for later   
				newSpeed /= 25.0f;

				//Do speed calculations
				rollingSumSpeed = rollingSumSpeed - speeds[4] + newSpeed;

				speeds[4] = speeds[3];
				speeds[3] = speeds[2];
				speeds[2] = speeds[1];
				speeds[1] = speeds[0];
				speeds[0] = newSpeed;

			}
			else if (belowThresh && newPointX > LOWER_THRESH) //If we're just leaving belowThresh
			{
				belowThresh = false; //Ammend that info
			}
			else if ((!aboveThresh && !belowThresh) || (aboveThresh && jogTimer.ElapsedMilliseconds > 500)) //If we're just hanging out in the middle
			{
				if (!jogTimer.IsRunning)
				{
					jogTimer.Start();
				}
				else if (jogTimer.ElapsedMilliseconds > 300) //IF we haven't hit a the upper threshold for awhile
				{
					//Knock off the oldest speed
					rollingSumSpeed = rollingSumSpeed - speeds[4];
					//Replace with 0
					speeds[4] = speeds[3];
					speeds[3] = speeds[2];
					speeds[2] = speeds[1];
					speeds[1] = speeds[0];
					speeds[0] = 0;
					jogTimer.Restart(); //Stop timer so that we can restart it and try again.
				}
			}
			agent.speed = rollingSumSpeed;
        }

			if (gameObject.transform.position.y > 2.2f && currentTime - lastJump > 1.0f) //Jump logic and tolerance
			{
				UnityEngine.Debug.Log("current y " + gameObject.transform.position.y);
				gameObject.GetComponent<Renderer>().material.color = Color.red;
				UnityEngine.Debug.Log("JUMPED");
				lastJump = currentTime;
			}
			else if (currentTime - lastJump > 1.0f)
			{
				gameObject.GetComponent<Renderer>().material.color = Color.blue;
			}
			
			//UnityEngine.Debug.Log(data);
			//File.WriteAllText("Fullpower.csv", data);
        }
    }
	
	public static float Sigmoid(double value) {
    float k = (float)System.Math.Exp(value);
    return (float)Round(k / (1.0f + k));
	}
}

