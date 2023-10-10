using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Math;
using TMPro;
using System.Linq;
using Image = UnityEngine.UI.Image;
using UnityEngine.EventSystems;
using Time = UnityEngine.Time;
using Math = System.Math;

/*QuizHandler does a few things
    1: Keeps track of when its time to spawn a question (May be replaced with hard coded locations in the map to spawn questions)
    2: Generates a random math question with a few rules
        1: Maximum of three terms per equation (tenative)
        2: Addition/subtraction may have up to two digits (1-99)
        3: Multiplication may have only one digit (1-9)
        4. Division may have a two digit numerator but only a one digit denominator
            4.1: Division terms must divide evenly
        5: Maximum of one multiplication OR division operation per question (which makes 3.1 trival to perform)
        6: ONLY ONE of each operation (no repeat adds or subs)
*/
public class QuizHandler : MonoBehaviour
{
    // Start is called before the first frame update
    
    System.Random rand = new System.Random();
    
    string[] question;
    
    GameObject qPanel;
    
    public int correctAnswer;
    
    const int MAX_ADD_TERM = 40;

    FinishedHandler finishedHandler;
    
    bool isVisible = false;
    float startedQuestionTime = 0f;
    float lastFinishedQuestionTime = 0; //Timestamp of when the last question was finished
    public float questionTime; //Time currently spent in a question
    float nonQuestionTime; //Time currently spent outside of a question
    float bothAnsweredTime = 0f;
    public float Q_MIN_WAIT = 2f; //Min time to wait between questions (in ms)
    public float Q_MAX_WAIT = 5f; //Max time to wait between questions (in ms)
    public float Q_NOFLASH = 8f;  //How long the question remains on screen before flashing
    public float Q_FLASH = 2f; //How long the question will flash before disappearing
    public bool inQuiz = false; //Tells the outside world if we're currently in a quiz.
    bool startedQuestion = false;
    GameObject[] currentAnswerBlock;
    GameObject p1;
    GameObject p2;

    bool p1AnsweredQuestion = false;
    bool p2AnsweredQuestion = false;

    //Keep track of both player's answers and if they got the question right
    int p1Answer = -1;
    bool p1AnswerRight = false;

    int p2Answer = -1;
    bool p2AnswerRight = false;

    void Start()
    {
        currentAnswerBlock = new GameObject[2];
        ShowPanels(false); //TODO: Actually figure out how to handle timings for quizes + answer recognizition 
        p1 = GameObject.Find("P1Status");

        p1.transform.Find("Arrow").GetComponent<CanvasRenderer>().SetAlpha(0f);
        p1.transform.Find("PlayerAnswer").GetComponent<CanvasRenderer>().SetAlpha(0f);


        p2 = GameObject.Find("P2Status");

        p2.transform.Find("Arrow").GetComponent<CanvasRenderer>().SetAlpha(0f);
        p2.transform.Find("PlayerAnswer").GetComponent<CanvasRenderer>().SetAlpha(0f);

        finishedHandler = GameObject.Find("Finish").GetComponent<FinishedHandler>();

    }

    // Update is called once per frame
    void Update()
    {
        //Logic for showing/flashing/hiding the question
        if(nonQuestionTime > Q_MIN_WAIT && !finishedHandler.isFinished) //If we've gone X seconds without a question while the race is going on
        {
            questionTime = Time.time - startedQuestionTime; //Now we're actually in a question, so start keeping track of questionTime
            if (!startedQuestion) //If we're just starting out, generate a new question and show it on screen
            {
                isVisible = true;
                startedQuestion = true;
                startedQuestionTime = Time.time;
                UpdateQuestion(GenerateQuestion());
                ShowPanels(true);
                ExecuteEvents.Execute<IQuizMessages>(GameObject.Find("Player 1"), null, (x, y) => x.QuizStarted());
                ExecuteEvents.Execute<IQuizMessages>(GameObject.Find("Player 2"), null, (x, y) => x.QuizStarted());
            }
            else if (questionTime > Q_NOFLASH && questionTime < Q_NOFLASH + Q_FLASH) //if we're nearing the end of our no-flashing portion of question time
            {
                if (UnityEngine.Time.frameCount % 2 == 0) //Start flashing the question
                {
                    isVisible = !isVisible;
                    ShowPanels(isVisible);
                }
            }
            else if (questionTime > Q_NOFLASH + Q_FLASH) //If we've run out of time (Total length of current question time > Total length of possible question time)
            {
                startedQuestion = false;
                questionTime = 0f;
                nonQuestionTime = 0f;
                isVisible = false;
                ShowPanels(false);
                lastFinishedQuestionTime = Time.time;
                p1Answer = 0;
                p1AnswerRight = false;
                p2Answer = 0;
                p2AnswerRight = false;
                p1AnsweredQuestion = false;
                p2AnsweredQuestion = false;

                if (currentAnswerBlock[0] != null)
                {
                    //p1.transform.Find("Arrow").GetComponent<CanvasRenderer>().SetAlpha(0f);
                   // p1.transform.Find("PlayerAnswer").GetComponent<CanvasRenderer>().SetAlpha(0f);
                }
                if (currentAnswerBlock[1] != null)
                {
                  //  p2.transform.Find("Arrow").GetComponent<CanvasRenderer>().SetAlpha(0f);
                   // p2.transform.Find("PlayerAnswer").GetComponent<CanvasRenderer>().SetAlpha(0f);
                }
                currentAnswerBlock[0] = null;
                currentAnswerBlock[1] = null;

                ExecuteEvents.Execute<IQuizMessages>(GameObject.Find("Player 1"), null, (x, y) => x.QuizEnded());
                ExecuteEvents.Execute<IQuizMessages>(GameObject.Find("Player 2"), null, (x, y) => x.QuizEnded());

            }
            if (p1AnsweredQuestion && p2AnsweredQuestion)
            {
                if (bothAnsweredTime == 0f)
                {
                    bothAnsweredTime = Time.time;
                }
                if(Time.time - bothAnsweredTime > 2.0f)
                {
                    startedQuestion = false;
                    questionTime = 0f;
                    nonQuestionTime = 0f;
                    isVisible = false;
                    ShowPanels(false);
                    lastFinishedQuestionTime = Time.time;
                    p1Answer = 0;
                    p1AnswerRight = false;
                    p2Answer = 0;
                    p2AnswerRight = false;
                    p1AnsweredQuestion = false;
                    p2AnsweredQuestion = false;
                    bothAnsweredTime = 0f;

                    if (currentAnswerBlock[0] != null)
                    {
                        //p1.transform.Find("Arrow").GetComponent<CanvasRenderer>().SetAlpha(0f);
                       // p1.transform.Find("PlayerAnswer").GetComponent<CanvasRenderer>().SetAlpha(0f);
                    }
                    if (currentAnswerBlock[1] != null)
                    {
                      //  p2.transform.Find("Arrow").GetComponent<CanvasRenderer>().SetAlpha(0f);
                       // p2.transform.Find("PlayerAnswer").GetComponent<CanvasRenderer>().SetAlpha(0f);
                    }
                    currentAnswerBlock[0] = null;
                    currentAnswerBlock[1] = null;

                    ExecuteEvents.Execute<IQuizMessages>(GameObject.Find("Player 1"), null, (x, y) => x.QuizEnded());
                    ExecuteEvents.Execute<IQuizMessages>(GameObject.Find("Player 2"), null, (x, y) => x.QuizEnded());
                }
            }


        }
        else if(!startedQuestion) //If we're not currently in a question, keep track of the current dead-air time amount.
            nonQuestionTime = Time.time - lastFinishedQuestionTime;
    }

    //Allows a controller to pass an answer (1=top, 2=right, 3=bottom, 4=left (clockwise). You can also see this hierarchy in the Quiz View node (QPanel=0, UPanel=1, RPanel=2...)
    //If we're currently in a quiz it'll return a 1 if they got the question right (they earn a reward)
    //Or -1 if they got the question wrong (they earn a penalty)
    public int AnswerQuestion(int answer, int player)
    {
        //First get the info
        Debug.Log($"Correct answer is: {correctAnswer}");
        if (player == 1)
        {
            p1Answer = answer;
            p1AnsweredQuestion = true;
        }
        else
        {
            p2Answer = answer;
            p2AnsweredQuestion = true;
        }
        if (answer == correctAnswer)
        {
            if (player == 1)
                p1AnswerRight = true;
            else
                p2AnswerRight = true;

        }
        else //Wrong answer
        {
            if (player == 1)
                p1AnswerRight = false;
            else
                p2AnswerRight = false;
        }
        
        if(player == 1)
        {
            currentAnswerBlock[0] = p1;
            Image arrow = p1.transform.Find("Arrow").gameObject.GetComponent<Image>();
            //Set arrow color and orientation
            arrow.color = p1AnswerRight ? Color.green : Color.red;
            arrow.gameObject.GetComponent<RectTransform>().rotation = p1AnswerRight ? new Quaternion(0, 0, 0, 1) : new Quaternion(0, 0, 180, 1);
            p1.transform.Find("PlayerAnswer").GetComponent<CanvasRenderer>().SetAlpha(1f);
            arrow.GetComponent<CanvasRenderer>().SetAlpha(1f);


        }
        else if(player == 2)
        {
            currentAnswerBlock[1] = p2;
            Image arrow = p2.transform.Find("Arrow").gameObject.GetComponent<Image>();

            //Set arrow color and orientation
            arrow.color = p2AnswerRight ? Color.green : Color.red;
            arrow.gameObject.GetComponent<RectTransform>().rotation = p2AnswerRight ? new Quaternion(0, 0, 0, 1) : new Quaternion(0, 0, 180, 1);
            p2.transform.Find("PlayerAnswer").GetComponent<CanvasRenderer>().SetAlpha(1f);
            arrow.GetComponent<CanvasRenderer>().SetAlpha(1f);
        }
        
        if (player == 1)
            return p1AnswerRight ? 1 : -1;
        else
            return p2AnswerRight ? 1 : -1;
    }

    //Given all the relevant info, update panels to show the new question/answers
    void UpdateQuestion(string[] question)
    {

        //First start with the Question 
        foreach(Transform child in transform)
        {
            Image img = child.gameObject.GetComponent<Image>();
            if(img.color != Color.blue)
            {
                img.color = Color.blue;
            }    
        }
        transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = $"{question[0]} {question[1]} {question[2]} {question[3]} {question[4]} = ?";
        //Now we need to randomly get an answer and then remove it from the list
        //So first lets make an array with just those numbers to make keeping track of things easy
        //string[] answers = { question[5], question[6], question[7], question[8] };
        int[] answers = { 0, 1, 2, 3};
        int currentAnswer;
        for (int i = 4;  i> 0; i--)
        {
            currentAnswer = answers[rand.Next(0, i)];
            //We need to keep track of which quiz box has the correct answer, which will be the first in this array
            if(currentAnswer == 0)
            {
                correctAnswer = 5 - i; //This value indicates which child of "Quiz View" has the right answer. So if i=2 then the correct answer will be in the  3rd answer panel (BLPanel)
            }
            //Get child panel (starting with 1, going to 4), then child text object, then add in answer
            transform.GetChild(5-i).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = $"{question[currentAnswer+5]}";
            //Now we need to remove the chosen value from the list of candidates
            //Some Lambda work here. Make a new array of values that *aren't* equal to the value we just chose
            answers = answers.Where(val => val != currentAnswer).ToArray();
        }       


    }    

    //Hides/shows all of the Quiz stuff
    public void ShowPanels(bool val)
    {
        foreach (Transform child in transform)
        {
            //child.gameObject.SetActive(val);
            if(!child.name.Contains("Status"))
                child.gameObject.GetComponent<CanvasRenderer>().SetAlpha(val? 1f : 0f);
        }
        foreach(GameObject text in GameObject.FindGameObjectsWithTag("Text"))
        {
            text.GetComponent<CanvasRenderer>().SetAlpha(val ? 1f : 0f);
        }
    }

    //Generate a question along with the answer and the incorrect answers
    //Yes I know I could have used char but strings make handling multiple digit operands much easier--we're not here for efficiency after all
    string[] GenerateQuestion()
    {
        string[] operations = { "+", "-", "*", "/" }; //quick reference for random generation
        string[] question = new string[9]; //3 possible operands, two max operations, an answer, and three incorrect answers: 3+2+1+3=9


        //First, pick operations involved
        string firstOperation = operations[rand.Next(0, 4)];
        string secondOperation;
        //If operation is mult/division, our second operation needs to be add/sub
        if (firstOperation == "*" || firstOperation == "/")
        {
            operations = operations.Where(val => val != "*" || val != "/").ToArray(); //Drop the mult/div options
            secondOperation = operations[rand.Next(0, 2)]; //Get either sub or add operation
            //Since we know our first operator, we can decide our operands since they're bound by the rules above (only one digit allowed for multiplication and only one mult/div per question)
            //Multiplication is simple, since communitive property applies
            if (firstOperation == "*")
            {
                //So we can generate the entire question, for example: 5 * 2 + 57 or 7 * 6 - 5 or 8 * 4 - 13
                question[0] = rand.Next(2, 10).ToString();
                question[1] = "*";
                question[2] = rand.Next(2, 10).ToString();
                question[3] = secondOperation;
                question[4] = rand.Next(1, MAX_ADD_TERM).ToString();
            }
            else //If we're handling division, things are more tricky
            {
                int numer = GetNumer();
                int denom = GetDenom(numer);
                //Now we have our numerator and denominator so we can finish up
                question[0] = numer.ToString();
                question[1] = "/";
                question[2] = denom.ToString();
                question[3] = secondOperation;
                question[4] = rand.Next(1, 10).ToString(); 
            }
        }
        else //We're fine to have a mult/div in as our secondary operator
        {
            operations = operations.Where(val => val != firstOperation).ToArray();
            secondOperation = operations[rand.Next(0, 3)];
            //We know our first operator is a +/-. so we can use a two digit number for the FIRST operand--but the second depends on if mulitplication or division
            question[0] = rand.Next(1, MAX_ADD_TERM).ToString();
            question[1] = firstOperation;
            if (secondOperation == "*")
            {
                question[2] = rand.Next(1, 10).ToString(); //Since we're doing multiplication we need to grab a simple 1 digit number
                question[3] = "*";
                question[4] = rand.Next(1, 10).ToString();
            }
            else if (secondOperation == "/")
            {
                //First number can be two digit as long as it's not prime
                int numer = GetNumer();
                int denom = GetDenom(numer);
                question[2] = numer.ToString();
                question[3] = "/";
                question[4] = denom.ToString();
            }
            else //If addition or subtraction, we can do whatever
            {
                question[2] = rand.Next(1, MAX_ADD_TERM).ToString();
                question[3] = secondOperation;
                question[4] = rand.Next(1, 10).ToString(); // TODO: Decide a better algorithm for making one of the terms 'simple' instead of just using the last one.
            }
        }

        //Now that we've got the problem, we need to solve for the answer.
        int total = CalcAnswer(question);
        //Now total should be our correct answer. Lets stash it with the rest of our data to return
        question[5] = total.ToString();

        /*Now that we've gotten that out of the way, we need to calculate some *wrong* answers.
            There are many ways to do this, but to make the game challenging, it would be wise if we computed answers that were rather close to the correct answer to make things tricky
            Using this fact, we can streamline the methodology:
                1. For each wrong answer, modify one of the oeprands slightly
                    1.1 If addition/subtraction/multiplication, modify the digit by + or - 3. 
                    1.2 If division, recalculate the denominator by modifying the numerator by a + or - 3 and ensuring the new numerator isn't a prime.
         */

        //We just need to modify each operand by + or - 3 and re-calculate an answer
        //First lets make copies of the original question. One for each wrong answer. NOTE: I know this is lazy and space inefficient but we can spare a few bytes.
        string[][] wrongQuestions = new string[3][];
        wrongQuestions[0] = new string[9];
        wrongQuestions[1] = new string[9];
        wrongQuestions[2] = new string[9];
        System.Array.Copy(question, wrongQuestions[0], 9);
        System.Array.Copy(question, wrongQuestions[1], 9);
        System.Array.Copy(question, wrongQuestions[2], 9);

        //Now lets iterate through each and make a new wrong question and solve for a wrong answer
        //Lets start with the easy case, no division
        if (wrongQuestions[0][1] != "/" & wrongQuestions[0][3] != "/")
        {
            //Modify the 1st, 2nd, and 3rd operand--one per each question
            //Make sure the first wrong answer doesn't equal the real answer
            do
            {
                wrongQuestions[0][0] = (int.Parse(wrongQuestions[0][0]) + GenerateFactor()).ToString(); //Modifies 1st operand of 1st question

            } while (CalcAnswer(wrongQuestions[0]) == CalcAnswer(question));
            //We need to make sure that the 2nd wrong answer isn't the same as the previous one
            do
            {
                wrongQuestions[1][2] = (int.Parse(wrongQuestions[1][2]) + GenerateFactor()).ToString(); //Modifies 2nd operand of 2nd question
            } while (CalcAnswer(wrongQuestions[0]) == CalcAnswer(wrongQuestions[1]));
            do //Make sure the 3rd wrong answer is also distinct from the previous two.
            {
                wrongQuestions[2][4] = (int.Parse(wrongQuestions[2][4]) + GenerateFactor()).ToString(); //Modifies 3rd operand of 3rd question
            } while (CalcAnswer(wrongQuestions[0]) == CalcAnswer(wrongQuestions[2]) || CalcAnswer(wrongQuestions[1]) == CalcAnswer(wrongQuestions[2]));

        }
        else //We've got a division
        {
            //First we need to figure out which position the division is in, because we aren't going to modify the denominator, but the numerator. Which means we can only mess with two operands
            //Case 1: first operation is division. We can mess with the 1st and 3rd operands
            //Case 2: first operation is not division. We can mess with the 1st and 2nd operands.

            //Lets see where the numerator is located. Either 1st or 2nd term
            int numerPos = wrongQuestions[0][1] == "/" ? 0 : 2;
            //Now lets see where the non-division term is located. Either at the 3rd or 1st term
            int freePos = numerPos == 0 ? 4 : 0;

            //First we need to modify the numerator and check the new one isn't a prime number

            //Make sure that it's not also getting the *right* answer
            do
            {
                int numer;
                numer = int.Parse(wrongQuestions[0][numerPos]) + GenerateFactor();
                //It's a prime and we need to re-generate
                while (IsPrime(numer))
                {
                    numer = int.Parse(wrongQuestions[0][numerPos]) + GenerateFactor();
                }

                //Now that we have a new numerator that isn't prime we can get a denominator
                int denom = GetDenom(numer);

                //Now that we've got both, we can finish up the first of the wrong answers

                wrongQuestions[0][numerPos] = numer.ToString();
                wrongQuestions[0][numerPos + 2] = denom.ToString();
            } while (CalcAnswer(wrongQuestions[0]) == CalcAnswer(question));
            
            //For the second and third wrong answer, we'll just mess with the free (non division term) twice to save a bit on computation and to have answers "close" to the real one
            //Make sure these new answers aren't identical to the previous ones
            do
            {
                wrongQuestions[1][freePos] = (int.Parse(wrongQuestions[1][4]) + GenerateFactor()).ToString();
            } while (CalcAnswer(wrongQuestions[0]) == CalcAnswer(wrongQuestions[1])); //Keep generating until we get a new answer
            do
            {
                wrongQuestions[2][freePos] = (int.Parse(wrongQuestions[2][4]) + GenerateFactor()).ToString();
            } while (CalcAnswer(wrongQuestions[0]) == CalcAnswer(wrongQuestions[2]) || CalcAnswer(wrongQuestions[1]) == CalcAnswer(wrongQuestions[2])); //Keep generating until we don't repeat the last two
        }

        //Now that we've got our wrong questions, we can figure out our wrong answers and package them with our final resuls
        question[6] = CalcAnswer(wrongQuestions[0]).ToString();
        question[7] = CalcAnswer(wrongQuestions[1]).ToString();
        question[8] = CalcAnswer(wrongQuestions[2]).ToString();

        return question;
    }

    int GetDenom(int numer)
    {
        //we need to generate a denominator that can cleanly divide the numerator.
        int denom;
        bool isDiv = false;
        do
        {
            //Don't pick 1, that's trivial
            denom = rand.Next(2, 10); //Certainly not the most efficient way to do this. Probably should do a 'drop from possible options' when you discover a number that isn't divisible but whatever.
            if (numer % denom == 0 && numer != denom)
                isDiv = true;
        } while (!isDiv);
        return denom;
    }

    //get numerator that is not prime and between 1-99
    int GetNumer()
    {
        const int MAX_DIV_TERM = 31;
        int numer = rand.Next(4, MAX_DIV_TERM);
        //If prime, discard and generate a new numerator
        //I'm pretty sure everything under 100 can factorize into these primes
        while (IsPrime(numer))
        {
            numer = rand.Next(4, MAX_DIV_TERM);
        }
        return numer;
    }

    int CalcAnswer(string[] question)
    {
        int total = 0;

        //So just take the operands and do the operations
        int num1 = int.Parse(question[0]);
        int num2 = int.Parse(question[2]);
        int num3 = int.Parse(question[4]);
        int temp;

        //First we need to see if the second operation will screw with the first (if the second is mult/div it will due to order of operations). I.E. 2 + 4 * 3 or 33-52 / 2
        if (question[3] == "*" || question[3] == "/")
        {
            if (question[3] == "*")
            {
                temp = num2 * num3;
            }
            else
            {
                temp = num2 / num3;
            }
            //Now we can evaluate the first part of the equation
            if (question[1] == "+") //IF addition
            {
                total = num1 + temp;
            }
            else //If subtraction
            {
                total = num1 - temp;
            }
        } //Same as above in case the FIRST operation is a mult/div
        else if (question[1] == "*" || question[1] == "/")
        {
            if (question[1] == "*")
            {
                temp = num1 * num2;
            }
            else
            {
                temp = num1 / num2;
            }
            if (question[3] == "+")
            {
                total = temp + num3;
            }
            else
            {
                total = temp - num3;
            }
        }
        else //All are +/- operations
        {
            if (question[1] == "+")
            {
                temp = num1 + num2;
            }
            else
            {
                temp = num1 - num2;
            }

            if (question[3] == "+")
            {
                total = temp + num3;
            }
            else
            {
                total = temp - num3;
            }
        }

        return total;
    }

    //Generates a factor to offset the correct values to make incorrect answers. Makes sure not the generate a 0 value.
    int GenerateFactor()
    {
        int factor;
        do
        {
            factor = rand.Next(-6, 7);
        } while (factor == 0);
        return factor;
    }

    public static bool IsPrime(int number)
    {
        if (number < 2) return false;
        if (number % 2 == 0) return (number == 2);
        int root = (int)Math.Sqrt((double)number);
        for (int i = 3; i <= root; i += 2)
        {
            if (number % i == 0) return false;
        }
        return true;
    }
}
