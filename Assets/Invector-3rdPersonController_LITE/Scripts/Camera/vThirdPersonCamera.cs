using Invector;
using UnityEngine;

public class vThirdPersonCamera : MonoBehaviour
{
    #region inspector properties    

    public Transform target;
    [Tooltip("Lerp speed between Camera States")]
    public float smoothCameraRotation = 12f;
    [Tooltip("What layer will be culled")]
    public LayerMask cullingLayer = 1 << 0;
    [Tooltip("Debug purposes, lock the camera behind the character for better align the states")]
    public bool lockCamera;

    [Header("Camera Mode Settings")]
    [Tooltip("Current camera mode")]
    public CameraMode currentMode = CameraMode.ThirdPerson;
    [Tooltip("Speed of transition between first and third person")]
    public float modeTransitionSpeed = 5f;
    [Tooltip("Key to toggle between camera modes")]
    public KeyCode toggleModeKey = KeyCode.R;
    [Tooltip("Auto-switch to first person when aiming")]
    public bool autoFirstPersonWhenAiming = true;

    [Header("Third Person Settings")]
    public float rightOffset = 0f;
    public float defaultDistance = 2.5f;
    public float height = 1.4f;
    public float smoothFollow = 10f;
    public float xMouseSensitivity = 3f;
    public float yMouseSensitivity = 3f;
    public float yMinLimit = -40f;
    public float yMaxLimit = 80f;

    [Header("First Person Settings")]
    public float firstPersonHeight = 1.7f;
    public float firstPersonSensitivity = 2f;
    public float firstPersonYMinLimit = -60f;
    public float firstPersonYMaxLimit = 60f;
    public Vector3 firstPersonOffset = new Vector3(0, 0, 0.1f);

    public enum CameraMode
    {
        FirstPerson,
        ThirdPerson
    }

    #endregion

    #region hide properties    

    [HideInInspector]
    public int indexList, indexLookPoint;
    [HideInInspector]
    public float offSetPlayerPivot;
    [HideInInspector]
    public string currentStateName;
    [HideInInspector]
    public Transform currentTarget;
    [HideInInspector]
    public Vector2 movementSpeed;

    private Transform targetLookAt;
    private Vector3 currentTargetPos;
    private Vector3 lookPoint;
    private Vector3 current_cPos;
    private Vector3 desired_cPos;
    private Camera _camera;
    private float distance = 5f;
    private float targetDistance = 5f;
    private float mouseY = 0f;
    private float mouseX = 0f;
    private float currentHeight;
    private float targetHeight;
    private float cullingDistance;
    private float checkHeightRadius = 0.4f;
    private float clipPlaneMargin = 0f;
    private float forward = -1f;
    private float xMinLimit = -360f;
    private float xMaxLimit = 360f;
    private float cullingHeight = 0.2f;
    private float cullingMinDist = 0.1f;

    // Mode transition variables
    private CameraMode previousMode;
    private bool isTransitioning = false;
    private float transitionProgress = 0f;
    private Vector3 firstPersonTargetPos;
    private Vector3 thirdPersonTargetPos;
    private Quaternion firstPersonTargetRot;
    private Quaternion thirdPersonTargetRot;

    #endregion

    void Start()
    {
        Init();
    }

    void Update()
    {
        // Handle mode switching input
        if (Input.GetKeyDown(toggleModeKey))
        {
            ToggleCameraMode();
        }

        // Handle smooth transitions
        if (isTransitioning)
        {
            UpdateTransition();
        }
    }

    public void Init()
    {
        if (target == null)
            return;

        _camera = GetComponent<Camera>();
        currentTarget = target;
        currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);

        targetLookAt = new GameObject("targetLookAt").transform;
        targetLookAt.position = currentTarget.position;
        targetLookAt.hideFlags = HideFlags.HideInHierarchy;
        targetLookAt.rotation = currentTarget.rotation;

        mouseY = currentTarget.eulerAngles.x;
        mouseX = currentTarget.eulerAngles.y;

        // Initialize based on current mode
        if (currentMode == CameraMode.FirstPerson)
        {
            distance = 0f;
            targetDistance = 0f;
            currentHeight = firstPersonHeight;
            targetHeight = firstPersonHeight;
        }
        else
        {
            distance = defaultDistance;
            targetDistance = defaultDistance;
            currentHeight = height;
            targetHeight = height;
        }

        previousMode = currentMode;
    }

    void FixedUpdate()
    {
        if (target == null || targetLookAt == null) return;

        CameraMovement();
    }

    /// <summary>
    /// Toggle between first and third person camera modes
    /// </summary>
    public void ToggleCameraMode()
    {
        previousMode = currentMode;
        currentMode = currentMode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
        StartTransition();
    }

    /// <summary>
    /// Set camera mode directly
    /// </summary>
    /// <param name="mode"></param>
    public void SetCameraMode(CameraMode mode)
    {
        if (currentMode != mode)
        {
            previousMode = currentMode;
            currentMode = mode;
            StartTransition();
        }
    }

    /// <summary>
    /// Start smooth transition between camera modes
    /// </summary>
    private void StartTransition()
    {
        isTransitioning = true;
        transitionProgress = 0f;

        // Set target values based on new mode
        if (currentMode == CameraMode.FirstPerson)
        {
            targetDistance = 0f;
            targetHeight = firstPersonHeight;
        }
        else
        {
            targetDistance = defaultDistance;
            targetHeight = height;
        }
    }

    /// <summary>
    /// Update smooth transition between camera modes
    /// </summary>
    private void UpdateTransition()
    {
        transitionProgress += modeTransitionSpeed * Time.deltaTime;
        
        if (transitionProgress >= 1f)
        {
            transitionProgress = 1f;
            isTransitioning = false;
        }

        // Smooth interpolation of camera parameters
        float smoothProgress = Mathf.SmoothStep(0f, 1f, transitionProgress);
        
        // Interpolate distance and height
        distance = Mathf.Lerp(distance, targetDistance, smoothProgress);
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, smoothProgress);
    }

    /// <summary>
    /// Set the target for the camera
    /// </summary>
    /// <param name="New cursorObject"></param>
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget ? newTarget : target;
    }

    public void SetMainTarget(Transform newTarget)
    {
        target = newTarget;
        currentTarget = newTarget;
        mouseY = currentTarget.rotation.eulerAngles.x;
        mouseX = currentTarget.rotation.eulerAngles.y;
        Init();
    }

    /// <summary>    
    /// Convert a point in the screen in a Ray for the world
    /// </summary>
    /// <param name="Point"></param>
    /// <returns></returns>
    public Ray ScreenPointToRay(Vector3 Point)
    {
        return this.GetComponent<Camera>().ScreenPointToRay(Point);
    }

    /// <summary>
    /// Camera Rotation behaviour
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void RotateCamera(float x, float y)
    {
        // Adjust sensitivity based on camera mode
        float currentXSensitivity = currentMode == CameraMode.FirstPerson ? firstPersonSensitivity : xMouseSensitivity;
        float currentYSensitivity = currentMode == CameraMode.FirstPerson ? firstPersonSensitivity : yMouseSensitivity;
        
        // Free rotation 
        mouseX += x * currentXSensitivity;
        mouseY -= y * currentYSensitivity;

        movementSpeed.x = x;
        movementSpeed.y = -y;
        
        if (!lockCamera)
        {
            // Use different limits for first person vs third person
            float currentYMinLimit = currentMode == CameraMode.FirstPerson ? firstPersonYMinLimit : yMinLimit;
            float currentYMaxLimit = currentMode == CameraMode.FirstPerson ? firstPersonYMaxLimit : yMaxLimit;
            
            mouseY = vExtensions.ClampAngle(mouseY, currentYMinLimit, currentYMaxLimit);
            mouseX = vExtensions.ClampAngle(mouseX, xMinLimit, xMaxLimit);
        }
        else
        {
            mouseY = currentTarget.root.localEulerAngles.x;
            mouseX = currentTarget.root.localEulerAngles.y;
        }
    }

    /// <summary>
    /// Camera behaviour
    /// </summary>    
    void CameraMovement()
    {
        if (currentTarget == null)
            return;

        // Smooth distance interpolation
        distance = Mathf.Lerp(distance, targetDistance, smoothFollow * Time.deltaTime);
        cullingDistance = Mathf.Lerp(cullingDistance, distance, Time.deltaTime);

        var camDir = (forward * targetLookAt.forward) + (rightOffset * targetLookAt.right);
        camDir = camDir.normalized;

        var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot, currentTarget.position.z);
        currentTargetPos = targetPos;

        // Different positioning logic for first person vs third person
        if (currentMode == CameraMode.FirstPerson)
        {
            FirstPersonCameraMovement(targetPos, camDir);
        }
        else
        {
            ThirdPersonCameraMovement(targetPos, camDir);
        }

        movementSpeed = Vector2.zero;
    }

    /// <summary>
    /// First person camera movement logic
    /// </summary>
    #region FirstPersonCamera  
    void FirstPersonCameraMovement(Vector3 targetPos, Vector3 camDir)
    {
        // Position camera at character's head level
        desired_cPos = targetPos + new Vector3(0, currentHeight, 0);
        current_cPos = desired_cPos;

        // Add slight offset to prevent clipping with character model
        Vector3 firstPersonPos = current_cPos + (currentTarget.forward * firstPersonOffset.z) + 
                                (currentTarget.right * firstPersonOffset.x) + 
                                (currentTarget.up * firstPersonOffset.y);

        targetLookAt.position = current_cPos;

        // Rotation for first person
        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
        targetLookAt.rotation = Quaternion.Slerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.deltaTime);

        // Set camera position and rotation
        transform.position = firstPersonPos;
        transform.rotation = targetLookAt.rotation;
    }
    #endregion
    /// <summary>
    /// Third person camera movement logic (original logic)
    /// </summary>
    #region ThirdPersonCamera  
    void ThirdPersonCameraMovement(Vector3 targetPos, Vector3 camDir)
    {
        desired_cPos = targetPos + new Vector3(0, height, 0);
        current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
        RaycastHit hitInfo;

        ClipPlanePoints planePoints = _camera.NearClipPlanePoints(current_cPos + (camDir * (distance)), clipPlaneMargin);
        ClipPlanePoints oldPoints = _camera.NearClipPlanePoints(desired_cPos + (camDir * distance), clipPlaneMargin);

        //Check if Height is not blocked 
        if (Physics.SphereCast(targetPos, checkHeightRadius, Vector3.up, out hitInfo, cullingHeight + 0.2f, cullingLayer))
        {
            var t = hitInfo.distance - 0.2f;
            t -= height;
            t /= (cullingHeight - height);
            cullingHeight = Mathf.Lerp(height, cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
        }

        //Check if desired target position is not blocked       
        if (CullingRayCast(desired_cPos, oldPoints, out hitInfo, distance + 0.2f, cullingLayer, Color.blue))
        {
            distance = hitInfo.distance - 0.2f;
            if (distance < defaultDistance)
            {
                var t = hitInfo.distance;
                t -= cullingMinDist;
                t /= cullingMinDist;
                currentHeight = Mathf.Lerp(cullingHeight, height, Mathf.Clamp(t, 0.0f, 1.0f));
                current_cPos = currentTargetPos + new Vector3(0, currentHeight, 0);
            }
        }
        else
        {
            currentHeight = height;
        }

        //Check if target position with culling height applied is not blocked
        if (CullingRayCast(current_cPos, planePoints, out hitInfo, distance, cullingLayer, Color.cyan)) 
            distance = Mathf.Clamp(cullingDistance, 0.0f, defaultDistance);

        var lookPoint = current_cPos + targetLookAt.forward * 2f;
        lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));
        targetLookAt.position = current_cPos;

        Quaternion newRot = Quaternion.Euler(mouseY, mouseX, 0);
        targetLookAt.rotation = Quaternion.Slerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.deltaTime);
        transform.position = current_cPos + (camDir * (distance));
        var rotation = Quaternion.LookRotation((lookPoint) - transform.position);

        transform.rotation = rotation;
    }
    #endregion

    /// <summary>
    /// Custom Raycast using NearClipPlanesPoints
    /// </summary>
    /// <param name="_to"></param>
    /// <param name="from"></param>
    /// <param name="hitInfo"></param>
    /// <param name="distance"></param>
    /// <param name="cullingLayer"></param>
    /// <returns></returns>
    bool CullingRayCast(Vector3 from, ClipPlanePoints _to, out RaycastHit hitInfo, float distance, LayerMask cullingLayer, Color color)
    {
        bool value = false;

        if (Physics.Raycast(from, _to.LowerLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            cullingDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.LowerRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperLeft - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        if (Physics.Raycast(from, _to.UpperRight - from, out hitInfo, distance, cullingLayer))
        {
            value = true;
            if (cullingDistance > hitInfo.distance) cullingDistance = hitInfo.distance;
        }

        return hitInfo.collider && value;
    }

    /// <summary>
    /// Get current camera mode
    /// </summary>
    /// <returns></returns>
    public CameraMode GetCurrentMode()
    {
        return currentMode;
    }

    /// <summary>
    /// Check if camera is in first person mode
    /// </summary>
    /// <returns></returns>
    public bool IsFirstPerson()
    {
        return currentMode == CameraMode.FirstPerson;
    }

    /// <summary>
    /// Check if camera is currently transitioning between modes
    /// </summary>
    /// <returns></returns>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}
