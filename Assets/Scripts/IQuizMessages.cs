using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IQuizMessages : IEventSystemHandler
{
    // functions that can be called via the messaging system
    void QuizStarted();

    void QuizEnded();

}