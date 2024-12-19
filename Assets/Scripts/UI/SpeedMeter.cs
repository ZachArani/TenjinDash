using Assets.Scripts.FSM.States;
using System;
using UnityEngine;

/// <summary>
/// Handles the rotationSpeed meter each player has in the UI.
/// </summary>
public class SpeedMeter : MonoBehaviour
{

    /// <summary>
    /// WIP Asset that visually shows the current rotationSpeed. 
    /// Basically just a rectangle that raises and lowers to fill the rotationSpeed meter. 
    /// </summary>
    public RectTransform meterUI;

    /// <summary>
    /// Factor that affects how smooth the meter transitions from one rotationSpeed to another. 
    /// Higher values: more smooth/more delay.
    /// Lower values: less smooth/more reactive.
    /// </summary>
    [Range(0f, 1f)]
    public float smoothFactor = 0.1f;

    public NewMovement player;

    RaceState raceManager;

    /// <summary>
    /// Adjusts what % of the meter is filled when players are at 0 rotationSpeed. 
    /// Makes players "feel" faster even if they're jogging slowly.
    /// For example, a base of 0.25 means a player nearing 0 effort will still register 25% rotationSpeed on the UI
    /// Once a player enters a "stop" mode, the bar will still move to 0%.l
    /// </summary>
    [Range(0f, 1f)]
    public float baseSpeed;


    // Start is called before the first frame update
    void Start()
    {
        raceManager = StateManager.instance.stateDictionary[GAME_STATE.RACE].GetComponent<RaceState>();
    }

    // Update is called once per frame
    void Update()
    {
        float ratio = (player.currentSpeed / (0.3f * raceManager.maxSpeed)) - 2.33f; //Mathematically derived value. Basically a shortcutted percent diff. 
        ratio = ratio > 0 ? ratio : 0;

        if (player.isOpening)
        {
            ratio = player.CalcTargetSpeed() / raceManager.maxSpeed; //Standard ratio since we haven't established a 'min' speed during the opening
        }
        else if (!player.isStopping)
        {
            ratio = ((1 - baseSpeed) * ratio) + baseSpeed; //Adjust ratio to fit baseSpeed value (to make players "feel" faster)
        }

        ratio = Mathf.Clamp(ratio, 0f, 1f); //Clamp value

        //Lerp to new rotationSpeed based on smoothing factor
        float newMeterValue = Mathf.Lerp(meterUI.offsetMax.y, -300 + (300f * ratio), smoothFactor);
        meterUI.offsetMax = new Vector2(meterUI.offsetMax.x, newMeterValue); //Update UI
    }

    public void CleanUp()
    {
        meterUI.offsetMax = new Vector2(meterUI.offsetMax.x, -300);
        enabled = false;
    }
}
