using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class IBLTask : Experiment
{
    // MANAGERS
    private VisualStimulusManager vsmanager;
    private AudioManager audmanager;

    private LickBehavior lickBehavior;
    private WheelRotationBehavior wheelRotationBehavior;
    private MouseAIBehavior mouseAI;
    private GameObject uiPanel;

    // TASK PARAMETERS
    private float stimAz = 25;
    //private float stimAlt = 0;
    //private float stimTime = 0;
    //private float stimTimeOut = 60;
    //private float onsetToneTime = 0;
    //private float onsetToneDur = 0.1f;
    private float quiMin = 0.2f;
    private float quiMax = 0.5f;
    private float ITIcorr = 0.2f;
    private float ITIerror = 2.0f;
    private float[] percCorrLevels = { 0.5f, 0.75f, 0.9f }; // 
    private float[] rtLevels = { 5f, 0.5f, 0.25f }; // maximum reaction time
    private float[] stimPositionTriggers = { 0, 45 };

    // LOCAL TASK PARAMETER
    // 37 52 62 99 (250 total)
    private int stimOnIdx = 37 - 1;
    private int firstWheelIdx = (37 + 52) - 1;
    private int feedbackIdx = (37 + 52 + 62) - 1;
    private int endIdx = (37 + 52 + 62 + 99) - 1;

    // INTERNAL TRACKING
    // 0=qui 1=stim on 2=wheel moving 3=reward 4=ITI
    private int state;
    private bool trialInit;
    private bool paused;
    private int level = 1;
    private int trialTimeIndex;
    private float sessionStartTime;
    private float sessionCurrentTime;

    // VARIABLE TRACKING
    private float _initWheelAngle;
    private float _prevWheelAngle;
    private float _wheelDelta;
    private float _wheelVelocity;

    private float _stimOnWindow = 1f; // 150 ms effect
    private float _stimOnTime;
    private float _stimOn;

    private float _feedbackWindow = 1f;
    private float _feedbackTime;
    private float _feedback;

    // TRIAL TRACKING
    private float t_trialQui;
    private float t_stateTime;
    private float t_wheelDuration;

    private GameObject t_stimulus;
    private float t_reactionTime;

    private float t_stimPositionTriggers;

    private float t_iti;

    private int t_stimSide;
    private bool t_corr;
    private int t_respSide;
    private float t_trialStartTime;

    public IBLTask(VisualStimulusManager vsmanager, 
        AudioManager audmanager, LickBehavior lickBehavior, WheelRotationBehavior wheelRotationBehavior, GameObject uiPanel)
         : base("IBL Task")
    {
        this.vsmanager = vsmanager;
        this.audmanager = audmanager;
        this.lickBehavior = lickBehavior;
        this.wheelRotationBehavior = wheelRotationBehavior;
        this.uiPanel = uiPanel;
    }

    public override void LoadTask()
    {
        SetTaskLoaded(true);
    }

    public override void TaskUpdate()
    {
        if (TaskRunning())
        {
            sessionCurrentTime += Time.deltaTime;

            if (!trialInit)
            {
                NewTrial();
            }
            else
            {
                t_stateTime += Time.deltaTime;

                switch (state)
                {
                    // Each state returns true when it has completed, triggering some next state functionality
                    case 0:

                        // lerp and round to get the index
                        trialTimeIndex = Mathf.RoundToInt(Mathf.Lerp(0, stimOnIdx, t_stateTime / t_trialQui));

                        // Quiescent
                        if (StateQuiescent())
                        {
                            // When true, evaluate whatever you need to do to move to the next state

                            GameObject[] vStims = GameObject.FindGameObjectsWithTag("Stimulus");
                            foreach (GameObject stim in vStims)
                            {
                                UnityEngine.Object.Destroy(stim);
                            }

                            // Add stimulus to screen
                            t_stimulus = vsmanager.AddNewStimulus("gabor");
                            t_stimulus.GetComponent<VisualStimulus>().SetScale(5);
                            // Set the position properly
                            _stimOn = t_stimSide;
                            _stimOnTime = Time.realtimeSinceStartup;

                            vsmanager.SetStimPositionDegrees(t_stimulus, new Vector2(t_stimSide * stimAz, 0));
                            audmanager.PlayGoTone();

                            NextState();
                        }
                        break;
                    case 1:
                        // Stimulus

                        // lerp and round to get the index
                        trialTimeIndex = Mathf.RoundToInt(Mathf.Lerp(stimOnIdx, firstWheelIdx, t_stateTime / t_reactionTime));

                        // Clear stimOnThisFrame
                        if (_stimOn != 0 && Time.realtimeSinceStartup > (_stimOnTime + _stimOnWindow))
                            _stimOn = 0f;

                        if (StateStimulus())
                        {

                            // Move the wheel toward or away from the target, once the wheel is rotated +/- 35 degrees, end
                            _initWheelAngle = wheelRotationBehavior.CurrentWheelAngle();
                            t_wheelDuration = wheelRotationBehavior.RotateWheelSteps(-1 * t_respSide * 10, t_stimulus);

                            NextState();
                        }
                        break;
                    case 2:
                        // Wheel moving

                        // while stimulus is on screen, calculate the wheelDelta
                        float wheelAngle = wheelRotationBehavior.CurrentWheelAngle();
                        _wheelDelta = Mathf.DeltaAngle(wheelAngle,_prevWheelAngle);
                        _prevWheelAngle = wheelRotationBehavior.CurrentWheelAngle();

                        // lerp and round to get the index
                        // note that we add the perecentage of wheel turning 
                        trialTimeIndex = Mathf.RoundToInt(Mathf.Lerp(firstWheelIdx, feedbackIdx, Mathf.Abs(Mathf.DeltaAngle(_initWheelAngle, wheelAngle)) / 80));

                        if (StateWheel())
                        {
                            // make sure wheelDelta is cleared
                            _wheelDelta = 0f;
                            // make sure stimOn is cleared
                            _stimOn = 0f;

                            wheelRotationBehavior.StopAllCoroutines();
                            wheelRotationBehavior.ResetPaws();

                            if (t_corr)
                            {
                                // play success tone?
                                lickBehavior.Lick();
                                t_iti = ITIcorr;
                                _feedback = 1f;
                            }
                            else
                            {
                                t_iti = ITIerror;
                                audmanager.PlayWhiteNoise();
                                _feedback = -1f;
                            }
                            _feedbackTime = Time.realtimeSinceStartup;

                            NextState();
                        }
                        break;
                    case 3:
                        // Reward + ITI
                        // lerp and round to get the index
                        trialTimeIndex = Mathf.RoundToInt(Mathf.Lerp(feedbackIdx, endIdx, t_stateTime / t_iti));

                        if (_feedback != 0 && Time.realtimeSinceStartup > (_feedbackTime + _feedbackWindow))
                            _feedback = 0f;

                        if (StateITI())
                        {
                            // clear feedback
                            _feedback = 0f;

                            // Remove stimulus 
                            vsmanager.DestroyVisualStimulus(t_stimulus);
                            // Trial is over
                            trialInit = false;
                        }
                        break;
                }
                Debug.Log(trialTimeIndex);

            }
        }
    }

    public int GetTimeIndex()
    {
        return trialTimeIndex;
    }

    // ** GET FUNCTIONS **
    // These functions return the current state of the experiment
    // see NeuronEntitySystem for how these are used
    public float GetWheelVelocity()
    {
        return _wheelDelta * Mathf.PI / 180 / Time.deltaTime;
    }

    public float GetStimOnFrame()
    {
        return _stimOn;
    }

    public float GetFeedback()
    {
        return _feedback;
    }

    // ** HELPER FUNCTIONS **

    public override void SetLevel(int level)
    {
        this.level = level;
    }

    private void NextState()
    {
        state++;
        t_stateTime = 0;
    }

    private bool StateQuiescent()
    {
        // TODO: Occasionally the mice should jitter a little for realism, resetting the quiescent state
        return t_stateTime > t_trialQui;
    }

    private bool StateStimulus()
    {
        // The mouse has a reaction time -- once elapsed, move the stimulus to the center
        return t_stateTime > t_reactionTime;
    }

    private bool StateWheel()
    {
        Vector2 stimPosition = t_stimulus.GetComponent<VisualStimulus>().StimPosition();

        bool stimOutsideRange = t_stimSide > 0 ?
            (stimPosition.x <= stimPositionTriggers[0]) || (stimPosition.x >= stimPositionTriggers[1]) :
            (stimPosition.x >= stimPositionTriggers[0]) || (stimPosition.x <= -stimPositionTriggers[1]);

        // Check state to see if the stimulus has been moved to the center, or off the screen
        return stimOutsideRange;
    }
    private bool StateITI()
    {
        return t_stateTime > t_iti;
    }

    /**
     * Return whether this trial is a left stimulus or right stimulus trial
     */
    public int GetSide()
    {
        return t_stimSide;
    }

    /**
     * Return whether this trial is a correct trial or incorrect
     */
    public bool GetCorrect()
    {
        return t_corr;
    }

    private void NewTrial()
    {
        t_trialQui = Random.value * (quiMax - quiMin) + quiMin;
        t_stimSide = (Random.value < 0.5) ? 1 : -1;
        t_corr = Random.value < percCorrLevels[level];
        t_respSide = t_corr ? t_stimSide : t_stimSide * -1;
        t_reactionTime = Random.value * rtLevels[level];

        t_trialStartTime = Time.realtimeSinceStartup;
        t_stateTime = 0;

        state = 0;
        trialTimeIndex = 0;

        trialInit = true;
    }

    public override void RunTask()
    {
        if (paused)
        {
            paused = false;
            SetTaskRunning(true);
            return;
        }

        if (uiPanel)
            uiPanel.SetActive(true);

        sessionStartTime = Time.realtimeSinceStartup;
        sessionCurrentTime = 0f;

        trialInit = false;

        SetTaskRunning(true);
    }

    public override void PauseTask()
    {
        paused = true;
        SetTaskRunning(false);
    }

    public override void StopTask()
    {
        if (uiPanel)
            uiPanel.SetActive(false);
        SetTaskRunning(false);
    }

    public override float TaskTime()
    {
        return sessionCurrentTime;
    }
}