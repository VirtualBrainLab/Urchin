using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WheelRotationBehavior : MonoBehaviour
{
    public VisualStimulusManager vsmanager;
    public ElectrodeManager emanager;

    public Transform wheelTransform;
    public Transform wheelTargetTransform;

    public TwoBoneIKConstraint leftPawIK;
    public TwoBoneIKConstraint rightPawIK;

    private Transform leftPawTarget;
    private Transform rightPawTarget;

    public Transform leftPawDefault;
    public Transform rightPawDefault;

    private List<Transform> wheelTargets;
    private float wheelRadius = 32;
    private int centerTargetIdx;
    private Transform currentCenterTarget;
    private Transform cwTarget;
    private Transform ccwTarget;

    //int degreesLeft = 0;

    float moveDurationFast = 0.1f;
    float moveDurationSlow = 0.15f;
    //float moveDurationFast = 0.5f;
    //float moveDurationSlow = 1.0f;

    int degreesPerStep = 20;
    float visualDegreesRatio = 0.25f;

    private void Awake()
    {
        // Create the hand targets
        wheelTargets = new List<Transform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        leftPawTarget = leftPawIK.data.target.transform;
        rightPawTarget = rightPawIK.data.target.transform;

        if (Mod(360, degreesPerStep) != 0)
        {
            Debug.LogError("(WheelRotationBehavior) Degrees per step must scale evenly to 360 degrees or wheel rotation will de-sync over time");
        }

        for (int i = 0; i < 360; i += degreesPerStep)
        {
            GameObject nextRot = new GameObject();
            nextRot.name = "rot" + i;
            nextRot.transform.parent = wheelTargetTransform;
            nextRot.transform.localPosition = new Vector3(3, 0, 0);
            nextRot.transform.localRotation = Quaternion.Euler(i, 0, 0);

            GameObject nextTarget = new GameObject();
            nextTarget.name = "ref_paw_" + i;
            nextTarget.transform.parent = nextRot.transform;
            nextTarget.transform.localPosition = new Vector3(0, wheelRadius, 0);
            wheelTargets.Add(nextTarget.transform);
        }
        // At the start, the centerTarget is just the ref_paw_0
        centerTargetIdx = 0;
        UpdateTargets();

        // Reset paw positions to default at start
        ResetPaws();
    }

    /**
     * Rotate wheel a set number of steps, return the expected time to perform this action
     */
    public float RotateWheelSteps(int steps, GameObject linkedStimulus)
    {
        StartCoroutine(RotateWheel(steps, linkedStimulus));
        return 2 * moveDurationFast + Mathf.Abs(steps) * moveDurationSlow;
    }

    public float CurrentWheelAngle()
    {
        return wheelTransform.rotation.eulerAngles.x;
    }

    public void ResetPaws()
    {
        emanager.SetWheelVelocity(0f);
        // Move the paws back to the seat
        StartCoroutine(MoveToDefault(rightPawTarget, rightPawDefault));
        StartCoroutine(MoveToDefault(leftPawTarget, leftPawDefault));
        // Un-rotate the wheelTargets so that the center idx is zero again
        wheelTargetTransform.localRotation = Quaternion.Inverse(wheelTransform.localRotation);
        //wheelTargetTransform.localRotation = Quaternion.Euler(-wheelTransform.eulerAngles.x, 0, 0);
        centerTargetIdx = 0;
        UpdateTargets();
    }

    IEnumerator RotateWheel(int steps, GameObject linkedStimulus)
    {
        if (steps==0)
        {
            ResetPaws();
            yield break;
        }


        Vector3 leftStartPosition = leftPawTarget.position;
        Vector3 rightStartPosition = rightPawTarget.position;
        Transform leftEndTransform;
        Transform rightEndTransform;

        if (steps > 0)
        {
            // We are rotating *CLOCKWISE* put hands l=ccw, r=center
            leftEndTransform = ccwTarget.transform;
            rightEndTransform = currentCenterTarget.transform;
        }
        else
        {
            // We rotating *CCW* put hands on l = center, r = cw
            leftEndTransform = currentCenterTarget.transform;
            rightEndTransform = cwTarget.transform;
        }

        float timeElapsed = 0;

        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / moveDurationFast;

            leftPawTarget.position = Vector3.Lerp(leftStartPosition, leftEndTransform.position, normalizedTime);
            rightPawTarget.position = Vector3.Lerp(rightStartPosition, rightEndTransform.position, normalizedTime);
            
            yield return null;
        }
        while (timeElapsed < moveDurationFast);


        // Wheel parameters
        Quaternion startRotation = wheelTransform.rotation;
        Quaternion toRotation = startRotation * Quaternion.Euler(degreesPerStep * Mathf.Sign(steps), 0, 0);
        // The hands are easy -- we just have them track the leftEnd/rightEnd transforms
        // For the stimulus we grab it's current position in degrees and then tack on the distance we'll move this time
        Vector2 originalStimPosition = linkedStimulus.GetComponent<VisualStimulus>().StimPosition();
        Vector2 endStimPosition = originalStimPosition + new Vector2(Mathf.Sign(steps) * degreesPerStep * visualDegreesRatio, 0);

        timeElapsed = 0;

        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / moveDurationSlow;
            // Wheel rotation
            wheelTransform.rotation = Quaternion.Slerp(startRotation, toRotation, normalizedTime);
            emanager.SetWheelVelocity(Mathf.Sign(steps) * 0.1f);
            // Hand position
            leftPawTarget.position = leftEndTransform.position;
            rightPawTarget.position = rightEndTransform.position;
            // Stimulus position
            vsmanager.SetStimPositionDegrees(linkedStimulus, Vector2.Lerp(originalStimPosition, endStimPosition, normalizedTime));

            yield return null;
        }
        while (timeElapsed < moveDurationSlow);

        emanager.SetWheelVelocity(0f);

        int inc = (int)-Mathf.Sign(steps);

        // Change the steps and centerTarget index
        steps += inc;
        centerTargetIdx = Mod(centerTargetIdx + inc, wheelTargets.Count);
        UpdateTargets();

        StartCoroutine(RotateWheel(steps, linkedStimulus));
    }

    IEnumerator MoveToDefault(Transform target, Transform defaultPoint)
    {
        Vector3 startPoint = target.position;
        Vector3 endPoint = defaultPoint.position;
        Quaternion startRotation = target.rotation;
        Quaternion endRotation = defaultPoint.rotation;

        float timeElapsed = 0;
        float moveDuration = 0.5f;

        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / moveDuration;

            target.position = Vector3.Lerp(startPoint, endPoint, normalizedTime);
            target.rotation = Quaternion.Slerp(startRotation, endRotation, normalizedTime);

            yield return null;
        }
        while (timeElapsed < moveDuration);
    }

    void UpdateTargets()
    {
        currentCenterTarget = wheelTargets[centerTargetIdx];
        if (centerTargetIdx < (wheelTargets.Count - 1))
        {
            cwTarget = wheelTargets[centerTargetIdx + 1];
        }
        else
        {
            cwTarget = wheelTargets[0];
        }
        if (centerTargetIdx > 0)
        {
            ccwTarget = wheelTargets[centerTargetIdx - 1];
        }
        else
        {
            ccwTarget = wheelTargets[wheelTargets.Count - 1];
        }
    }

    //TODO: Variable rotation based on a number of degrees
    /*public void Rotate(int degrees)
    {
        if (degreesLeft != 0)
        {
            degreesLeft += degrees;
        }
    }*/
    int Mod(int a, int b)
    {
        return (a % b + b) % b;
    }
}
