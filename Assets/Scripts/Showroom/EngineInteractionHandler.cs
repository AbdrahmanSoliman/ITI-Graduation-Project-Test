using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EngineStates {DontShow, WholeEngine, ExternalParts, InternalParts}
public class EngineInteractionHandler : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private TextMeshProUGUI textUINext;
    [SerializeField] private TextMeshProUGUI textUIPrev;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Settings")]
    [SerializeField] private string description;

    private EngineStates engineState;

    private bool isNextButtonClicked;
    private bool isPrevButtonClicked;
    //private EngineStates prevEngineState; // store old state, in order not to overhead Update() as we're using switch cases inside it

    private void Awake() 
    {
        textUINext.enabled = false;
        textUIPrev.enabled = false;
        nextButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);
    }


    void Start()
    {
        PlayerController.onNextButtonClicked += HandleNextButtonClicked;
        PlayerController.onPrevButtonClicked += HandlePrevButtonClicked;
        engineState = EngineStates.WholeEngine;
    }

    private void OnDestroy() 
    {
        PlayerController.onNextButtonClicked -= HandleNextButtonClicked;
        PlayerController.onPrevButtonClicked -= HandlePrevButtonClicked;    
    }

    
    private void HandleNextButtonClicked()
    {
        isNextButtonClicked = true;
    }

    private void HandlePrevButtonClicked()
    {
        isPrevButtonClicked = true;
    }

    private void Update() 
    {
        //if(prevEngineState == engineState) return;

        switch(engineState)
        {
            case EngineStates.WholeEngine:
                WholeEngine();
                break;

            case EngineStates.ExternalParts:
                ExternalParts();
                break;
            
            case EngineStates.InternalParts:
                InternalParts();
                break;
        }

        //prevEngineState = engineState;
    }

    private void WholeEngine()
    {
        ResetNextPrevBtns();

        textUINext.text = "External Parts";
        textUINext.enabled = true;
        nextButton.gameObject.SetActive(true);

        if(isNextButtonClicked)
        {
            engineState = EngineStates.ExternalParts;
        }
    }

    private void ExternalParts()
    {
        ResetNextPrevBtns();

        textUINext.text = "Unfold Internal Parts";
        textUIPrev.text = "Whole Engine";

        nextButton.gameObject.SetActive(true);
        prevButton.gameObject.SetActive(true);

        if(isNextButtonClicked)
        {
            engineState = EngineStates.InternalParts;
        }
        else if(isPrevButtonClicked)
        {
            engineState = EngineStates.WholeEngine;
        }
    }

    private void InternalParts()
    {
        ResetNextPrevBtns();

        textUIPrev.text = "Fold Internal Parts";

        textUINext.enabled = false;
        nextButton.gameObject.SetActive(false);

        if(isPrevButtonClicked)
        {
            engineState = EngineStates.ExternalParts;
        }
    }

    private void ResetNextPrevBtns()
    {
        isNextButtonClicked = false;
        isPrevButtonClicked = false;
    }
}
