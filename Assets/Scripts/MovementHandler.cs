using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Pipes;
using UnityEngine.AI;
using Time = UnityEngine.Time;
using System.Linq;
using System.Runtime;
using static GAME_MODES;
using UnityEngine.Playables;
using PathCreation;
using UnityEditor;
using Cinemachine;

public enum GAME_MODES //Types of game modes. 
{
	NO_INPUT, //No input at all
	MOCK_DATA, //Dummy input from files
	ONE_PLAYER, //One player input
	TWO_PLAYER //Two player input (regular mode)

}

public class MovementHandler : MonoBehaviour
{


	//Joycon stuff
	private List<Joycon> joycons;

	public GAME_MODES GAME_MODE = MOCK_DATA;
	public float[] stick;
	public Vector3 accel;
	public Quaternion orientation;
	Joycon jL; //Joycon for player strapped to leg
	public int jc_ind = 0;

	public bool isLogging = false;

	//Speed calculation stuff
	[Range(0, 0.1f)]
	public float UPPER_THRESH = 0.1f;
	[Range(1f, 10f)]
	public float TANK_MAX = 10f;
	[Range(3f, 35f)]
	public float SPEED_MAX = 20f;
	[Range(0f, 15f)]
	public float SPEED_MIN = 10f;
	[Range(0.5f, 3f)]
	public float ACCEL_MAX = 1f;
	[Range(-0.5f, -3f)]
	public float ACCEL_MIN = -1f;
	[Range(0.1f, 1f)]
	public float CHECKING_TIME = 0.6f;
	[Range(0.01f, 1f)]
	public float PERCENT_DIFF = 0.1f;
	[Range(1, 10)]
	public float NEGATIVE_DIFF = 1;
	[Range(0.01f, 0.06f)]
	public float speedModifer = 0.03f;
	private bool aboveThresh = false;
	public float speedMultiplier;
	[Range(0f, 1f)]
	public float timeScale = 1f;
	[Range(0.5f, 2f)]
	public float boostFactor = 1.2f;
	[Range(0f, 8f)]
	public float goodScore = 3f;

	public float FINISH_LINE = -1005.5f;
	public float END_LINE = -1007f;

	public float percentFilled;

	GameObject otherPlayer;

	bool isFirst;
	float distanceTraveled;

	//New Speed calculation stuff 
	List<float> dataPoints = new List<float>();

	CountdownHandler countdown;
	FinishedHandler finished;

	float playerEnergy = 0f; //The fuel for our player's speed. If they have some, they speed up, if they don't, they slow down.
	public float mockSpeed = 5f; //For testing speed calculations



	//Keeps track of if the current player is currently being rewarded for answering a question correctly. 1==reward, -1==punishment, 0==neutral
	Animator animator;

	string path;


	//Mock data reader
	string[] mockDataStrings;
	List<float> mockData = new List<float>();
	int currentMockPoint = 0;

	[HideInInspector] public bool startedRun = false;

	PlayableDirector menuDirector;
	PlayableDirector preRollDirector;

	public PathCreator pathCreator;

	public float currentSpeed;

	public float diff;

    [Range(0.01f, 1f)]
	public float TICK;

	public float currentTick;
	public float tickBonus;
	public float accelValue;
	public float finalSpeed;
	bool finishedOpening;

	string mockSpeedData;

	float lastTime;

	public float timeInPlace;
	public bool isFirstPlace;
	float raceTime;
	bool useFakeSpeed;
	float fakeSpeed;

	void Start()
	{
		lastTime = 0;
		Debug.developerConsoleVisible = true;
		accel = new Vector3(0, 0, 0);
		//If we're playing on one of the 'real' input modes, then grab our joycon manager, 
		joycons = (GAME_MODE > MOCK_DATA) ? JoyconManager.Instance.j : null;
		currentSpeed = SPEED_MIN;
		animator = gameObject.GetComponent<Animator>();
		countdown = GameObject.Find("Countdown").GetComponent<CountdownHandler>();
		speedMultiplier = 1f;
		fakeSpeed = SPEED_MAX / 2;


		menuDirector = GameObject.Find("Timelines").transform.Find("MenuTimeline").GetComponent<PlayableDirector>();
		preRollDirector = GameObject.Find("Timelines").transform.Find("PreRollTimeline").GetComponent<PlayableDirector>();

		finished = GameObject.Find("UI").transform.Find("Finish").GetComponent<FinishedHandler>();

		path = @"C:\Users\cloud\OneDrive\Desktop\AutoData.csv";

		distanceTraveled = 0f;
		isFirst = false;

		otherPlayer = name.Equals("Player1") ? GameObject.Find("Player2") : GameObject.Find("Player1").gameObject;

		mockSpeedData = name.Equals("Player1") ? 
			Resources.Load<TextAsset>("Data/autoData").ToString() :
			Resources.Load<TextAsset>("Data/autoData").ToString();
		using (System.IO.StringReader reader = new System.IO.StringReader(mockSpeedData))
		{
			while(reader.Peek() != -1)
				mockData.Add(float.Parse(reader.ReadLine()));
		}
		Debug.Log(mockData);
	}

	// Update is called once per frame
	void Update()
	{
		//Speed fixes if needed
		if (SPEED_MIN > SPEED_MAX)
		{
			SPEED_MAX = SPEED_MIN;
		}

		//Assign joycons 
		if (GAME_MODE == ONE_PLAYER)
			jL = joycons[0];
		else if (GAME_MODE == TWO_PLAYER)
			jL = gameObject.name.Contains("1") ? joycons[0] : joycons[1];
		else
			jL = null;

		float data = GetAcceleration(); //Grab our current raw acceleration value 
		if (isLogging) //If logging, send to file
		{
			string dataPoint = $"{UnityEngine.Time.frameCount},{data},\r";
			File.AppendAllText(path, dataPoint);
			Debug.Log(dataPoint);
		}

		//If we're done with the race

		if (finished.isFinished)
		{
			startedRun = false;
		}
		else if (GAME_MODE > NO_INPUT && menuDirector.state != PlayState.Playing && preRollDirector.state != PlayState.Playing && !countdown.isCounting && !finished.isFinished) //Otherwise if we're not in some menu or startup, let's get running
		{
			if (!startedRun)
			{
				Debug.Log("Starting Run!");
				distanceTraveled = 0;
				startedRun = true;
				//currentSpeed = SPEED_MIN + (SPEED_MAX - SPEED_MIN) /2f; //Start at half speed and let
				currentSpeed = 5;
				animator.SetTrigger("start");
			}
			if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("No Z"))
			{
				//distanceTraveled += 2 * Time.deltaTime;
				//transform.position = pathCreator.path.GetPointAtDistance(distanceTraveled);
			}

			if (transform.position.x >= END_LINE && !finished.isInSplitFinish)
			{
				finalSpeed = currentSpeed;
				otherPlayer.GetComponent<MovementHandler>().finalSpeed = otherPlayer.GetComponent<MovementHandler>().currentSpeed;
				finished.nearFinishedRace();
			}
			else if (transform.position.x >= FINISH_LINE && !finished.isAtFinishLine)
			{
				finished.atFinishLine();
			}

			isFirst = gameObject.transform.position.x > otherPlayer.transform.position.x ? true : false;

			data -= 1.01f; //Adjust data point. This will get close to 0==no movement

			//If we're already above the threshold 
			if (aboveThresh && data > UPPER_THRESH)
			{
				dataPoints.Add(data); //Add point to our list of points
			}
			else if (!aboveThresh && data > UPPER_THRESH)  //If we're moving above the positve threshold for the first time
			{
				aboveThresh = true; //Make a note of it
				dataPoints.Add(data); //Add point to list
			}
			else if (aboveThresh && data < UPPER_THRESH) //If we move below the threshold 
			{
				aboveThresh = false; //Make a note of it 
				dataPoints = dataPoints.OrderBy(x => x).ToList(); //Sort our data points
				float peakPoint = dataPoints.Last(); //Grab the highest point (our peak)
				playerEnergy += peakPoint; //Add peak point to playerEnergy TODO: Actually make this calculation logical
				dataPoints.Clear(); //Clear the list for later use
			}

			//SECOND: handle movement speed based on player energy

			//dispense the fuel tank
			if (Time.time - lastTime > CHECKING_TIME)
			{
				lastTime = Time.time;
				//FIRST: Calculate speed changes:
				playerEnergy = playerEnergy > TANK_MAX ? TANK_MAX : playerEnergy; //Round player energy down to the max value
				percentFilled = -((TANK_MAX / 2) - playerEnergy) / (TANK_MAX / 2); //If playerEnergy == MAX_TANK, then percentFilled=1%. If playerEnergy==0, percentFilled=-1f
				currentTick = TICK * percentFilled;
				currentTick = playerEnergy < 0.3f ? currentTick * 2.1f : currentTick;
				if (currentSpeed < 10 && playerEnergy > 1)
				{
					currentTick = currentTick * 3;
				}

				playerEnergy = 0; //Empty the tank
				if (animator.GetBool("speedingUp") && currentSpeed < 4 && currentSpeed > 0)
				{

				}
				else
				{
					animator.SetBool("speedingUp", percentFilled > 0);
				}


				UpdateAnimationSpeed();

				if (gameObject.transform.position.x < otherPlayer.transform.position.x && otherPlayer.transform.position.x - transform.position.x > 3)
				{
					//Debug.Log(gameObject.name + " is BEHIND!");
					//currentSpeed = currentSpeed * 1.1f; //Give a small boost to 2nd place
				}

				if (GAME_MODE == MOCK_DATA) //If demoing, then slightly vary speed within range
					speedMultiplier = Random.value * (1.1f - 0.98f) + 0.98f;
			}
			if (animator.IsInTransition(0) &&
				animator.GetNextAnimatorClipInfo(0).Length != 0 &&
				animator.GetNextAnimatorClipInfo(0)[0].clip.name.Equals("SprintMove") &&
				animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("No Z"))
			{
				currentSpeed = SPEED_MIN + (SPEED_MAX - SPEED_MIN) / 2;
				finishedOpening = true;
			}
			/*else if (raceTime < 2f)
			{
				useFakeSpeed = true;
			}
			else if (raceTime > 2f)
            {
				useFakeSpeed = false;
            }*/
			currentSpeed = currentSpeed > SPEED_MAX ? SPEED_MAX : currentSpeed;
			currentSpeed = currentSpeed + currentTick > 0 ? currentSpeed + currentTick : 0;

			//TODO: 
			//Consider speed qualifications (above X speed)
			//Change method of speed (Perhaps make it equal to the other player's speed x1.5
			if(!isFirstPlace && timeInPlace > 3f && Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(otherPlayer.transform.position.x)) > 2f)
            {
				//speedMultiplier = 2f;
				speedMultiplier = 1.17f;
				Debug.Log(gameObject.name + " behind. " + "P1: " + currentSpeed * speedMultiplier + ", P2: " + otherPlayer.GetComponent<MovementHandler>().currentSpeed);
            }
			if(!isFirstPlace && transform.position.x > -1050 && Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(otherPlayer.transform.position.x)) > 2f)
            {
				speedMultiplier = 1.19f;
            }
			else if(isFirstPlace)
            {
				speedMultiplier = 1f;
            }
			else if(Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(otherPlayer.transform.position.x)) < 2f)
            {
				Debug.Log(gameObject.name + " Is neck and neck!");
				//Debug.Log(transform.position.x + " otherPlayer: " + otherPlayer.transform.position.x + ", ABS: ");
				speedMultiplier = Random.Range(0.98f, 1.02f);
            }
			

			if (finished.isInSplitFinish || finished.isAtFinishLine)
            {
				currentSpeed = finalSpeed;
            }
			if(useFakeSpeed)
            {
				distanceTraveled += fakeSpeed * Time.deltaTime * speedMultiplier;
			}
			else
            {
				distanceTraveled += currentSpeed * Time.deltaTime * speedMultiplier;
			}
			transform.position = pathCreator.path.GetPointAtDistance(distanceTraveled);
			if(finishedOpening)
				transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTraveled);
			if (useFakeSpeed)
				animator.SetFloat("speed", fakeSpeed);
			else
				animator.SetFloat("speed", currentSpeed);
			if (name.Equals("Player2"))
			{
				//Debug.Log(transform.position.x - otherPlayer.transform.position.x);
				//Debug.Log(isFirstPlace);
				//Debug.Log(timeInPlace);
			}
		}
		else if (GAME_MODE == NO_INPUT)
		{
		}


	}

    private void FixedUpdate()
    {
		if(finishedOpening)
        {
			timeInPlace += 0.02f;
			raceTime += 0.02f;
		}
	}

    public float GetAcceleration()
	{
		if (GAME_MODE > MOCK_DATA) //PLAYER_ONE or PLAYER_TWO
			return jL.GetAccel().x;
		else if (GAME_MODE == MOCK_DATA)
		{
			float temp = mockData[currentMockPoint];    //Grab our current mock data value
			currentMockPoint = currentMockPoint < mockData.Count - 1 ? //Increment mock data point
				   currentMockPoint + 1 : 0; //or return to first point if at end of data
			return temp;
		}
		else //NO_INPUT
			return 0f;
	}

	//Return one of four different animation speeds based on some basic buckets (SLOWEST, SLOW, FAST, FASTEST)
	public void UpdateAnimationSpeed()
	{
		float randFactor = Random.value * (1.02f - 0.98f) + 0.98f; //slightly alter speed to stop mirrored motion between characters.
		float speedDiff = SPEED_MAX - SPEED_MIN;
		float usedSpeed;
		if(useFakeSpeed)
        {
			usedSpeed = fakeSpeed;
        }
		else
        {
			usedSpeed = currentSpeed;
        }
		if (usedSpeed < SPEED_MIN + (speedDiff * 0.15)) //SLOWEST
		{
			animator.SetFloat("animationSpeed", 0.75f * randFactor);
        }
		if (usedSpeed < SPEED_MIN + (speedDiff * 0.25)) //SLOW
		{
			animator.SetFloat("animationSpeed", 0.85f * randFactor);
		}
		else if (usedSpeed < SPEED_MIN + (speedDiff * 0.5)) //MID
		{
			animator.SetFloat("animationSpeed", 1.0f * randFactor);
		}
		else if (usedSpeed < SPEED_MIN + (speedDiff * 0.75)) //FAST
		{
			animator.SetFloat("animationSpeed", 1.12f * randFactor);
		}
		else //FASTEST
		{
			animator.SetFloat("animationSpeed", 1.25f * randFactor);
		}
	}

	public float GetTickBonus()
    {
		int divisions = (int)SPEED_MAX / 6;
		if(currentSpeed < SPEED_MAX - 5 * divisions)
        {
			return 2.5f;
        }
		else if(currentSpeed < SPEED_MAX - 4 * divisions)
        {
			return 2f;
        }
		else if(currentSpeed < SPEED_MAX - 3 * divisions)
        {
			return 1.5f;
        }
		else if (currentSpeed < SPEED_MAX - 2 * divisions)
        {
			return 1.25f;
        }
		else if (currentSpeed < SPEED_MAX - divisions)
        {
			return 1;
        }
		return 0;
    }

	public void ResetTimeInPlace()
	{
		timeInPlace = 0f;
	}

		private void Awake()
    {
        
    }

}