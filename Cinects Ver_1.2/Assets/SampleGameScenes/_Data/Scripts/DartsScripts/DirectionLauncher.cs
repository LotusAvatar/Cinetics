using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DirectionLauncher : MonoBehaviour
{
    enum LauncherState
    {
        WaitingPos1 = 0,
        WaitingPos2,
        OnDelay
    }

    public GameObject[] prefabsArray;
    public Transform laucherProjectileBase;
    public Transform laucherProjectileTarget;
    public Transform referenceTransform;
    public Transform mainCamera;
    public GUIText scoreText;
    public GUIText stateText;
    LauncherInteractionManager manager;
    Vector3 screenMaxLimitPos = new Vector3(10f, 5f, 10f);
    Vector3 screenMinLimitPos = new Vector3(-10f, -5f, 0f);
    Vector3 baseMaxLimitPos = new Vector3(1f, 0.5f, 1f);
    Vector3 baseMinLimitPos = new Vector3(-1f, -0.5f, 0f);
    Vector3 currentBasePos = new Vector3();
    Vector3 targetMaxLimitPos = new Vector3(25f, 24f, 100f);
    Vector3 targetMinLimitPos = new Vector3(-25f, -24f, 100f);
    Vector3 currentTargetPos = new Vector3();
    Vector3 cameraInitalPosition;
    Vector3 cameraHitPosition;
    Vector3 lastProjectileDirection;
    Vector3 shakeCameraDirection;
    GameObject currentPrefab;
    Quaternion initialRotation;
    LauncherState currentState;
    float lastProjectileSpeed;
    float delayTimer = 0f;
    float delayLimit = 0.5f;
    float shakingDelayTimer = 0f;
    float shakingDelayLimit = 2f;
    float shakingAmount = 0.75f;
    int score = 0;
    int launcherCounter;
    bool cameraOnMove;
    bool cameraShake;
    bool onShake;
    List<MovingObject> listOfProjectiles = new List<MovingObject>();
    #region Singleton

    static DirectionLauncher _instance;

    public static DirectionLauncher Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DirectionLauncher>() as DirectionLauncher;
            }
            return _instance;
        }
    }

    #endregion

    void Start()
    {
        cameraInitalPosition = mainCamera.position;
        initialRotation = referenceTransform.rotation;
        UpdateScore();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCurrentState();
        if (cameraOnMove)
        {
            mainCamera.position += lastProjectileDirection * lastProjectileSpeed;
            return;
        }
        else if (cameraShake)
        {
            ShakeCamera();
            return;
        }
        LaucherStateBehavior();
    }

    void ShakeCamera()
    {
        shakingDelayTimer += Time.deltaTime;
        if (shakingDelayTimer >= shakingDelayLimit)
        {
            shakingDelayTimer = 0f;
            cameraShake = false;
            shakingAmount = 0.75f;
            return;
        }

        if (onShake)
        {
            shakeCameraDirection *= shakingAmount;
            mainCamera.position = cameraHitPosition + shakeCameraDirection;
            shakeCameraDirection *= -1f;
        }
        else
        {
            mainCamera.position = cameraHitPosition;
        }
        onShake = !onShake;
        shakingAmount -= Time.deltaTime * 0.5f;
    }

    void LaucherStateBehavior()
    {
        if (manager == null)
        {
            manager = LauncherInteractionManager.Instance;
        }

        if (manager != null && manager.IsInteractionInited())
        {
            if (currentState == LauncherState.OnDelay)
            {
                delayTimer += Time.deltaTime;
                if (delayTimer >= delayLimit)
                {
                    referenceTransform.gameObject.SetActive(true);
                    currentState = 0;
                    delayTimer = 0f;
                    UpdateScore();
                }
                return;
            }

            if (cameraOnMove)
            {
                cameraOnMove = false;
                mainCamera.transform.position = cameraInitalPosition;
            }
            else if (mainCamera.transform.position != cameraInitalPosition)
            {
                mainCamera.transform.position = cameraInitalPosition;
            }

            if (launcherCounter >= 5)
            {
                if (listOfProjectiles.Count > 0)
                {
                    foreach (MovingObject obj in listOfProjectiles)
                        Destroy(obj.gameObject);

                    listOfProjectiles.Clear();
                }

                launcherCounter = 0;
                score = 0;
                UpdateScore();
            }

            if (manager.IsLeftHandPrimary())
            {
                SetDartBehaviorByState(manager.GetLeftHandScreenPos());
                //referenceTransform.localPosition = CalculatePosition(manager.GetLeftHandScreenPos(), true);

                // if the left hand is primary, check for left hand grip
                if (manager.GetLastLeftHandEvent() == LauncherInteractionManager.HandEventType.Grip)
                {
                    if (currentState == LauncherState.WaitingPos1)
                    {
                        Debug.Log("Grip on WaitingPos1!");
                        currentBasePos = CalculatePosition(manager.GetLeftHandScreenPos(), true);
                        laucherProjectileBase.localPosition = currentBasePos;
                        currentState++;
                    }
                }
                else if (manager.GetLastLeftHandEvent() == LauncherInteractionManager.HandEventType.Release)
                {
                    if (currentState == LauncherState.WaitingPos2)
                    {
                        Debug.Log("Release on WaitingPos2!");
                        currentBasePos = CalculatePosition(manager.GetLeftHandScreenPos());
                        laucherProjectileTarget.localPosition = currentBasePos;
                        Launch(prefabsArray[0]);
                        launcherCounter++;
                        currentState++;
                    }
                }
            }
            else if (manager.IsRightHandPrimary())
            {
                SetDartBehaviorByState(manager.GetRightHandScreenPos());
                //referenceTransform.localPosition = CalculatePosition(manager.GetRightHandScreenPos(), true);

                // if the right hand is primary, check for right hand grip
                if (manager.GetLastRightHandEvent() == LauncherInteractionManager.HandEventType.Grip)
                {
                    if (currentState == LauncherState.WaitingPos1)
                    {
                        Debug.Log("Grip on WaitingPos1!");
                        currentBasePos = CalculatePosition(manager.GetRightHandScreenPos(), true);
                        laucherProjectileBase.localPosition = currentBasePos;
                        currentState++;
                    }
                }
                else if (manager.GetLastRightHandEvent() == LauncherInteractionManager.HandEventType.Release)
                {
                    if (currentState == LauncherState.WaitingPos2)
                    {
                        Debug.Log("Release on WaitingPos2!");
                        currentBasePos = CalculatePosition(manager.GetRightHandScreenPos());
                        laucherProjectileTarget.localPosition = currentBasePos;
                        Launch(prefabsArray[0]);
                        launcherCounter++;
                        currentState++;
                    }
                }
            }
        }
    }

    void SetDartBehaviorByState(Vector3 screenNormalPos)
    {
        if (currentState == LauncherState.WaitingPos1)
        {
            if (referenceTransform.rotation != initialRotation)
                referenceTransform.rotation = initialRotation;
            
			referenceTransform.localPosition = CalculatePosition(screenNormalPos, true);
        }
        else if (currentState == LauncherState.WaitingPos2)
        {
            currentTargetPos = CalculatePosition(screenNormalPos);
            Quaternion newRot = Quaternion.LookRotation(currentTargetPos);

            if (newRot != referenceTransform.rotation)
            {
                referenceTransform.rotation = Quaternion.Lerp(referenceTransform.rotation, newRot, 0.1f);
            }
        }
    }

    Vector3 CalculatePosition(Vector3 screenNormalPos, bool isSphere = false)
    {
        Vector3 newPos = Vector3.zero;

        if (isSphere)
        {
            currentBasePos.x = Mathf.Lerp(screenMinLimitPos.x, screenMaxLimitPos.x, screenNormalPos.x);
            currentBasePos.y = Mathf.Lerp(screenMinLimitPos.y, screenMaxLimitPos.y, screenNormalPos.y);
            currentBasePos.z = Mathf.Lerp(screenMinLimitPos.z, screenMaxLimitPos.z, screenNormalPos.z);
            newPos = currentBasePos;
        }
        else if (currentState == LauncherState.WaitingPos1)
        {
            currentBasePos.x = Mathf.Lerp(baseMinLimitPos.x, baseMaxLimitPos.x, screenNormalPos.x);
            currentBasePos.y = Mathf.Lerp(baseMinLimitPos.y, baseMaxLimitPos.y, screenNormalPos.y);
            currentBasePos.z = Mathf.Lerp(baseMinLimitPos.z, baseMaxLimitPos.z, screenNormalPos.z);
            newPos = currentBasePos;
        }
        else if (currentState == LauncherState.WaitingPos2)
        {
            currentTargetPos.x = Mathf.Lerp(targetMinLimitPos.x, targetMaxLimitPos.x, screenNormalPos.x);
            currentTargetPos.y = Mathf.Lerp(targetMinLimitPos.y, targetMaxLimitPos.y, screenNormalPos.y);
            currentTargetPos.z = Mathf.Lerp(targetMinLimitPos.z, targetMaxLimitPos.z, screenNormalPos.z);
            newPos = currentTargetPos;
        }
        return newPos;
    }

    void Launch(GameObject prefabObj)
    {
        GameObject tiro = Instantiate(prefabObj, referenceTransform.position, referenceTransform.rotation) as GameObject;
        referenceTransform.gameObject.SetActive(false);
        Vector3 dir = laucherProjectileTarget.position - laucherProjectileBase.position;
        dir = Vector3.Normalize(dir);
        lastProjectileDirection = dir;
        tiro.GetComponent<MovingObject>().Initialize(dir);
        lastProjectileSpeed = tiro.GetComponent<MovingObject>().speed;
        cameraOnMove = true;
    }

    void StopCameraMovement()
    {
        mainCamera.transform.position = cameraInitalPosition;
    }

    public void AddProjectileInList(MovingObject projectile)
    {
        cameraOnMove = false;
        cameraHitPosition = mainCamera.position;
        shakeCameraDirection = Vector3.zero;
        do
        {
            Vector3 shakePos = Random.onUnitSphere;
            shakePos.z = 0f;
            shakeCameraDirection = Vector3.Normalize(shakePos);
        }
        while (shakeCameraDirection == Vector3.zero);
        cameraShake = true;
        listOfProjectiles.Add(projectile);
    }

    void UpdateCurrentState()
    {
        if (stateText)
        {
            string sGuiText = string.Empty;
            sGuiText += "State: " + currentState.ToString();
            stateText.text = sGuiText;
        }
    }

    public void ForceToReset()
    {
        delayTimer = delayLimit;
        cameraOnMove = false;
    }

    public void UpdateScore(int hexScore = 0)
    {
        score += hexScore;
        // display debug information
        if (scoreText)
        {
            string sGuiText = string.Empty;
            sGuiText += "Score: " + score.ToString();
            sGuiText += "\nDarts: " + (5 -launcherCounter).ToString();

            scoreText.text = sGuiText;
        }
    }
}
