using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QuizAnswerHandler : MonoBehaviour, IQuizMessages
{
    //Joycon stuff
    private List<Joycon> joycons;
    Joycon jH; //Joycon in hand
    public int jc_ind = 0;

	//Quiz Stuff
	private float quizTimer = 0f;
    QuizHandler quizzer;
	public int isCorrect = 0;
	bool inQuiz = false;
	bool hasAnswered = false;
	int player;
	NavMeshAgent agent;
	public bool OnePlayer = false;
	public string buttonPressed;
	Joycon test;
	bool justAnswered = false;

	void Start()
    {
        joycons = JoyconManager.Instance.j;
        quizzer = GameObject.Find("Quiz View").GetComponent<QuizHandler>();
		player = gameObject.name.Contains("1") ? 1 : 2;
		agent = gameObject.GetComponent<NavMeshAgent>();
	}

    void Update()
    {
		jH = OnePlayer ? gameObject.name.Contains("1") ? joycons[0] : joycons[1] : gameObject.name.Contains("1") ? joycons[1] : joycons[3];
		if (jH.GetButton(Joycon.Button.DPAD_UP))
		{
			buttonPressed = "UP";
			if (quizzer.questionTime > 0 && !hasAnswered)
			{
				isCorrect = quizzer.AnswerQuestion(1, player);
				quizTimer = Time.time;
				hasAnswered = true;
				justAnswered = true;
			}
		}
		else if (jH.GetButton(Joycon.Button.DPAD_RIGHT))
		{
			buttonPressed = "RIGHT";
			if (quizzer.questionTime > 0 && !hasAnswered)
			{
				isCorrect = quizzer.AnswerQuestion(2, player);
				quizTimer = Time.time;
				hasAnswered = true;
				justAnswered = true;

			}

		}
		else if (jH.GetButton(Joycon.Button.DPAD_DOWN))
		{
			buttonPressed = "DOWN";
			if (quizzer.questionTime > 0 && !hasAnswered)
			{
				isCorrect = quizzer.AnswerQuestion(3, player);
				quizTimer = Time.time;
				hasAnswered = true;
				justAnswered = true;
			}
		}
		else if (jH.GetButton(Joycon.Button.DPAD_LEFT))
		{
			buttonPressed = "LEFT";
			if (quizzer.questionTime > 0 && !hasAnswered)
			{
				isCorrect = quizzer.AnswerQuestion(4, player);
				quizTimer = Time.time;
				hasAnswered = true;
				justAnswered = true;
			}
		}


		if (isCorrect == 1 && justAnswered)
		{
			justAnswered = false;
		}
		else if (isCorrect == -1 && justAnswered)
        {
			justAnswered = false;
		}

        if (Time.time - quizTimer > 5)
		{
			isCorrect = 0;
			hasAnswered = false;

		}

	}

    public void QuizStarted()
    {
		inQuiz = true;
    }

    public void QuizEnded()
    {
		hasAnswered = false;
		inQuiz = false;
    }
}
