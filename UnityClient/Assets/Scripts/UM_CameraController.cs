using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UM_CameraController : MonoBehaviour
{
    [SerializeField] private Camera brainCamera;
    [SerializeField] private GameObject brainCameraRotator;
    [SerializeField] private GameObject brain;

    private Vector3 initialCameraRotatorPosition;

    public float minFoV = 15.0f;
    public float maxFoV = 90.0f;
    public float fovDelta = 15.0f;
    public float orthoDelta = 5.0f;
    public float moveSpeed = 50.0f;
    public float rotSpeed = 1000.0f;
    public float minXRotation = -90;
    public float maxXRotation = 90;
    public float minZRotation = -90;
    public float maxZRotation = 90;

    private bool mouseDownOverBrain;
    private int mouseButtonDown;
    private bool brainTransformChanged;
    private float doubleClickTime = 0.15f;
    private float lastLeftClick;
    private float lastRightClick;

    private float totalYaw;
    private float totalPitch;
    private float totalSpin;

    // auto-rotation
    private bool autoRotate;
    private float autoRotateSpeed = 10.0f;

    // Targeting
    private Vector3 cameraTarget;

    // Start is called before the first frame update
    void Start()
    {
        initialCameraRotatorPosition = brainCameraRotator.transform.position;
        lastLeftClick = Time.realtimeSinceStartup;
        lastRightClick = Time.realtimeSinceStartup;

        cameraTarget = brain.transform.position;

        autoRotate = false;
    }

    private void Update()
    {
        bool anyEvent = false;
        // Check the scroll wheel and deal with the field of view
        float fov = brainCamera.orthographic ? brainCamera.orthographicSize : brainCamera.fieldOfView;

        float scroll = -Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            fov += (brainCamera.orthographic ? orthoDelta : fovDelta) * scroll;
            fov = Mathf.Clamp(fov, minFoV, maxFoV);

            if (brainCamera.orthographic)
                brainCamera.orthographicSize = fov;
            else
                brainCamera.fieldOfView = fov;

            anyEvent = true;
        }

        // Now check if the mouse wheel is being held down
        if (Input.GetMouseButton(1))
        {
            mouseDownOverBrain = true;
            mouseButtonDown = 1;

            anyEvent = true;
        }

        // Now deal with dragging
        if (Input.GetMouseButtonDown(0))
        {
            //BrainCameraDetectTargets();
            mouseDownOverBrain = true;
            mouseButtonDown = 0;

            anyEvent = true;
        }

        if (anyEvent)
            SetCameraContinuousRotation(false);

        if (autoRotate)
        {
            totalSpin += autoRotateSpeed * Time.deltaTime;
            ApplyBrainCameraRotatorRotation();
        }
        else
            BrainCameraControl_noTarget();
    }

    public void BlockDragging()
    {
        // Call this function anywhere you need to prevent the brain from moving from another script
        mouseDownOverBrain = false;
    }

    void BrainCameraControl_noTarget()
    {

        // Deal with releasing the mouse (anywhere)
        if (mouseDownOverBrain && mouseButtonDown == 0 && Input.GetMouseButtonUp(0))
        {
            if (!brainTransformChanged)
            {
                // All we did was click through the brain window 
                //if (brainCameraClickthroughTarget)
                //{
                //    BrainCameraClickthrough();
                //}
                if ((Time.realtimeSinceStartup - lastLeftClick) < doubleClickTime)
                {
                    totalYaw = 0f;
                    totalPitch = 0f;
                    ApplyBrainCameraRotatorRotation();
                }
            }

            lastLeftClick = Time.realtimeSinceStartup;
            ClearMouseDown(); return;
        }
        if (mouseDownOverBrain && mouseButtonDown == 1 && Input.GetMouseButtonUp(1))
        {
            if (!brainTransformChanged)
            {
                // Check for double click
                if ((Time.realtimeSinceStartup - lastRightClick) < doubleClickTime)
                {
                    // Reset the brainCamera transform position
                    brainCamera.transform.localPosition = Vector3.zero;
                }
            }

            lastRightClick = Time.realtimeSinceStartup;
            ClearMouseDown(); return;
        }

        if (mouseDownOverBrain && mouseButtonDown == 1)
        {
            // While right-click is held down 
            float xMove = -Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            float yMove = -Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

            if (xMove != 0 || yMove != 0)
            {
                brainTransformChanged = true;
                brainCamera.transform.Translate(xMove, yMove, 0, Space.Self);
            }
        }

        // If the mouse is down, even if we are far way now we should drag the brain
        if (mouseDownOverBrain && mouseButtonDown == 0)
        {
            float xRot = -Input.GetAxis("Mouse X") * rotSpeed * Time.deltaTime;
            float yRot = Input.GetAxis("Mouse Y") * rotSpeed * Time.deltaTime;

            if (xRot != 0 || yRot != 0)
            {
                brainTransformChanged = true;

                // Pitch Locally, Yaw Globally. See: https://gamedev.stackexchange.com/questions/136174/im-rotating-an-object-on-two-axes-so-why-does-it-keep-twisting-around-the-thir
                
                // if shift is down, we can apply spin instead of yaw
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    totalSpin = Mathf.Clamp(totalSpin + xRot, minXRotation, maxXRotation);
                }
                else
                {
                    totalYaw = Mathf.Clamp(totalYaw + yRot, minXRotation, maxXRotation);
                    totalPitch = Mathf.Clamp(totalPitch + xRot, minZRotation, maxZRotation);
                }

                ApplyBrainCameraRotatorRotation();
            }
        }
    }
    void ApplyBrainCameraRotatorRotation()
    {
        Quaternion curRotation = Quaternion.Euler(totalYaw, totalSpin, totalPitch);
        // Move the camera back to zero, perform rotation, then offset back
        brainCameraRotator.transform.position = initialCameraRotatorPosition;
        brainCameraRotator.transform.LookAt(cameraTarget, Vector3.back);
        brainCameraRotator.transform.position = curRotation * (brainCameraRotator.transform.position - cameraTarget) + cameraTarget;
        brainCameraRotator.transform.rotation = curRotation * brainCameraRotator.transform.rotation;
    }

    public void SetCameraY(float newSpin)
    {
        // Set the camera position around the Y axis
        totalSpin = newSpin;
        ApplyBrainCameraRotatorRotation();
    }

    public void SetCameraTarget(Vector3 newTarget)
    {
        Debug.Log("Setting camera target to: " + newTarget);

        // Reset any panning 
        brainCamera.transform.localPosition = Vector3.zero;

        cameraTarget = new Vector3(5.7f - newTarget.z, 4f - newTarget.y, -6.6f + newTarget.x);

        ApplyBrainCameraRotatorRotation();
    }

    private void SetCameraContinuousRotation(bool state)
    {
        autoRotate = state;
    }

    public void SetCameraRotationSpeed(float speed)
    {
        autoRotateSpeed = speed;
    }
    public void CameraContinuousRotationButton()
    {
        SetCameraContinuousRotation(true);
    }

    void ClearMouseDown()
    {
        mouseDownOverBrain = false;
        //brainCameraClickthroughTarget = null;
        brainTransformChanged = false;
    }
}
