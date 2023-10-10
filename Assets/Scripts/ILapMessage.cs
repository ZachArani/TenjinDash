using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ILapMessage : IEventSystemHandler
{
    // functions that can be called via the messaging system
    void Lapped();

}