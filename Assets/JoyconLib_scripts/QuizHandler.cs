using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Math;
using Stopwatch = System.Diagnostics.Stopwatch;


/*QuizHandler does a few things
    1: Keeps track of when its time to spawn a question (May be replaced with hard coded locations in the map to spawn questions)
    2: Generates a random math question with a few rules
        1: Maximum of three terms per equation (tenative)
        2: Addition/subtraction may have up to two digits (1-99)
        3: Multiplication may have only one digit (1-9)
        4. Division may have a two digit numerator but only a one digit denominator
            4.1: Division terms must divide evenly
        5: Maximum of one multiplication OR division operation per question (which makes 3.1 trival to perform)
*/
public class QuizHandler : MonoBehaviour
{
    // Start is called before the first frame update
    Stopwatch timer = new Stopwatch();
    System.Random rand = new System.Random();
    int[] PRIMES = { 1, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //Generate a question along with the answer and the incorrect answers
    //Yes I know I could have used char but strings make handling multiple digit operands much easier--we're not here for efficiency after all
    string[] generateQuestion()
    {
        string[] operations = { "+", "-", "*", "/" }; //quick reference for random generation
        string[] question = new string[9]; //3 possible operands, two max operations, an answer, and three incorrect answers: 3+2+1+3=9

        //First, pick operations involved
        string firstOperation = operations[rand.Next(0, 4)];
        string secondOperation;
        //If operation is mult/division, our second operation needs to be add/sub
        if (firstOperation == "*" || firstOperation == "/")
        {
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
                question[4] = rand.Next(1, 100).ToString();
            }
            else //If we're handling division, things are more tricky
            {
                int numer = getNumer();
                int denom = GetDenom(numer);
                //Now we have our numerator and denominator so we can finish up
                question[0] = numer.ToString();
                question[1] = "/";
                question[2] = denom.ToString();
                question[3] = secondOperation;
                question[4] = rand.Next(1, 100).ToString();
            }
        }
        else //We're fine to have a mult/div in as our secondary operator
        {
            secondOperation = operations[rand.Next(0, 4)];
            //We know our first operator is a +/-. so we can use a two digit number for the FIRST operand--but the second depends on if mulitplication or division
            question[0] = rand.Next(1, 100).ToString();
            question[1] = firstOperation;
            if (secondOperation == "*")
            {
                question[2] = rand.Next(1, 10).ToString(); //Since we're doing multiplication we need to grab a simple 1 digit number
                question[3] = "*";
                question[4] = rand.Next(1, 10).ToString();
            }
            if (secondOperation == "/")
            {
                //First number can be two digit as long as it's not prime
                int numer = getNumer();
                int denom = GetDenom(numer);
                question[2] = numer.ToString();
                question[3] = "/";
                question[4] = denom.ToString();
            }
            else //If addition or subtraction, we can do whatever
            {
                question[2] = rand.Next(1, 100).ToString();
                question[3] = secondOperation;
                question[4] = rand.Next(1, 100).ToString();
            }
        }

        //Now that we've got the problem, we need to solve for the answer.
        int total = calcAnswer(question);
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
            wrongQuestions[0][0] = (int.Parse(wrongQuestions[0][0]) + generateFactor()).ToString(); //Modifies 1st operand of 1st question
            //We need to make sure that the 2nd wrong answer isn't the same as the previous one
            do
            {
                wrongQuestions[1][2] = (int.Parse(wrongQuestions[1][2]) + generateFactor()).ToString(); //Modifies 2nd operand of 2nd question
            } while (calcAnswer(wrongQuestions[0]) == calcAnswer(wrongQuestions[1]));
            do //Make sure the 3rd wrong answer is also distinct from the previous two.
            {
                wrongQuestions[2][4] = (int.Parse(wrongQuestions[2][4]) + generateFactor()).ToString(); //Modifies 3rd operand of 3rd question
            } while (calcAnswer(wrongQuestions[0]) == calcAnswer(wrongQuestions[2]) || calcAnswer(wrongQuestions[1]) == calcAnswer(wrongQuestions[2]));

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
            int numer;
            numer = int.Parse(wrongQuestions[0][numerPos]) + generateFactor();
            //It's a prime and we need to re-generate
            while (!(numer % 2 == 0 || numer % 3 == 0 || numer % 5 == 0 || numer % 7 == 0))
            {
                numer = int.Parse(wrongQuestions[0][numerPos]) + generateFactor();
            }

            //Now that we have a new numerator that isn't prime we can get a denominator
            int denom = GetDenom(numer);

            //Now that we've got both, we can finish up the first of the wrong answers

            wrongQuestions[0][numerPos] = numer.ToString();
            wrongQuestions[0][numerPos + 2] = denom.ToString();

            //For the second and third wrong answer, we'll just mess with the free (non division term) twice to save a bit on computation and to have answers "close" to the real one
            //Make sure these new answers aren't identical to the previous ones
            do
            {
                wrongQuestions[1][freePos] = (int.Parse(wrongQuestions[1][4]) + generateFactor()).ToString();
            } while (calcAnswer(wrongQuestions[0]) == calcAnswer(wrongQuestions[1])); //Keep generating until we get a new answer
            do
            {
                wrongQuestions[2][freePos] = (int.Parse(wrongQuestions[2][4]) + generateFactor()).ToString();
            } while (calcAnswer(wrongQuestions[0]) == calcAnswer(wrongQuestions[2]) || calcAnswer(wrongQuestions[1]) == calcAnswer(wrongQuestions[2])); //Keep generating until we don't repeat the last two
        }

        //Now that we've got our wrong questions, we can figure out our wrong answers and package them with our final resuls
        question[6] = calcAnswer(wrongQuestions[0]).ToString();
        question[7] = calcAnswer(wrongQuestions[1]).ToString();
        question[8] = calcAnswer(wrongQuestions[2]).ToString();

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
            if (numer % denom == 0)
                isDiv = true;
        } while (!isDiv);
        return denom;
    }

    //get numerator that is not prime and between 1-99
    int getNumer()
    {
        int numer = rand.Next(4, 100);
        //If prime, discard and generate a new numerator
        //I'm pretty sure everything under 100 can factorize into these primes
        while (!(numer % 2 == 0 || numer % 3 == 0 || numer % 5 == 0 || numer % 7 == 0))
        {
            numer = rand.Next(4, 100);
        }
        return numer;
    }

    int calcAnswer(string[] question)
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
    int generateFactor()
    {
        int factor;
        do
        {
            factor = rand.Next(-6, 7);
        } while (factor == 0);
        return factor;
    }
}
