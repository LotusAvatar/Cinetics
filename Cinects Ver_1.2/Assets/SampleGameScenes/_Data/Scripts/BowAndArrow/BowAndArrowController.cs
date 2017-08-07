using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BowAndArrowController : MonoBehaviour
{
    enum BowAndArrowState
    {
        Nope = 0,
        BowRelease,
        HoldingBow,
        Aiming,
        ArrowShot
    }

    public GameObject[] prefabsArray;
    public GameObject bowObject;
    public GameObject arrowObject;
    public Transform bowTransform;
    public Transform arrowTransform;
    public Transform aimTransform;
    public Transform bowAndArrowTransform;
    public Animator bowAndArrowAnimator;
    public Transform mainCamera;
    public GUIText scoreText;
    public GUIText stateText;
    BowAndArrowInteractionManager manager;
    Quaternion initialRotation = new Quaternion();
    Vector3 aimingMaxLimitPos = new Vector3(10f, 6f, 100f);
    Vector3 aimingMinLimitPos = new Vector3(-10f, -4f, 100f);
    Vector3 currentAimingPos = new Vector3();
    Vector3 bowMaxLimitPos = new Vector3(10f, 6f, 2.25f);
    Vector3 bowMinLimitPos = new Vector3(-10f, -4f, -2.25f);
    Vector3 currentBowPos = new Vector3();
    Vector3 arrowMaxLimitPos = new Vector3(10f, 6f, 2.25f);
    Vector3 arrowMinLimitPos = new Vector3(-10f, -4f, -2.25f);
    Vector3 currentArrowPos = new Vector3();
    Vector3 currentBowAndArrowPos = new Vector3();
    Vector3 cameraInitalPosition;
    Vector3 cameraHitPosition;
    Vector3 lastArrowDirection;
    Vector3 shakeCameraDirection;
    GameObject currentPrefab;
    BowAndArrowState currentState = BowAndArrowState.Nope;
    float lastArrowSpeed;
    float delayTimer = 0f;
    float delayLimit = 3f;
    float shakingAmount = 0.75f;
    float animationLength = 0f;
    public float stretchSpeed;
    public float squashSpeed;
    int score = 0;
    int shootCounter;
    bool cameraOnMove;
    bool cameraShake;
    bool onShake;
    bool bowEnabled = true;
    bool arrowEnabled = true;
    bool hasShot;
    List<MovingArrow> listOfArrows = new List<MovingArrow>();
    #region Singleton

    static BowAndArrowController _instance;

    public static BowAndArrowController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BowAndArrowController>() as BowAndArrowController;
            }
            return _instance;
        }
    }

    #endregion

    void Start()
    {
        OnNopeBehavior();
        initialRotation = bowAndArrowTransform.rotation;
        cameraInitalPosition = mainCamera.position;
        UpdateScore();
        aimingMaxLimitPos.x = HoneycombMatrix.Instance.vectorPointingUp.x * (((float)HoneycombMatrix.Instance.dimentionHoneycomb) / 2f);
        aimingMinLimitPos.x = HoneycombMatrix.Instance.vectorPointingUp.x * (((float)HoneycombMatrix.Instance.dimentionHoneycomb) / -2f);
        aimingMaxLimitPos.y = HoneycombMatrix.Instance.vectorPointingUp.y * (((float)HoneycombMatrix.Instance.dimentionHoneycomb) / 2f);
        aimingMinLimitPos.y = HoneycombMatrix.Instance.vectorPointingUp.y * (((float)HoneycombMatrix.Instance.dimentionHoneycomb) / -2f);
        Vector2 saveDepth = new Vector2(aimingMaxLimitPos.z, aimingMinLimitPos.z);
        aimingMaxLimitPos *= 2f;
        aimingMinLimitPos *= 2f;
        aimingMaxLimitPos.z = saveDepth.x;
        aimingMinLimitPos.z = saveDepth.y;
 
   }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevelAsync(2);
        }

        BowAndArrowStateChecker();
        if (cameraOnMove)
        {
            mainCamera.position += lastArrowDirection * lastArrowSpeed;
        }
        else if (cameraShake)
        {
            ShakeCamera();
        }
        BowAndArrowStateBehavior();
        UpdateState();
        TurnTowardBowAndArrowRotation();
    }

    void ShakeCamera()
    {
        if (delayTimer >= delayLimit)
        {
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
        shakingAmount -= Time.deltaTime * 0.1f;
    }

    void OnAimingBehavior()
    {
        OnMoveBowAndArrow(CalculateMiddlePosition(bowTransform.localPosition, arrowTransform.localPosition));
        if (animationLength < 1f)
        {
            BowAndArrowAnimationLength();
            animationLength += Time.deltaTime * stretchSpeed;
        }
        else
        {
            animationLength = 1f;
            BowAndArrowAnimationLength();
        }

        if (!bowEnabled)
        {
            bowEnabled = true;
            bowObject.SetActive(bowEnabled);
        }
        if (!arrowEnabled)
        {
            arrowEnabled = true;
            arrowObject.SetActive(arrowEnabled);
        }
    }
    void OnBowReleaseBehavior()
    {
        OnNopeBehavior();
    }

    void OnHoldingBowBehavior()
    {
        if (animationLength != 0f)
        {
            animationLength = 0f;
            BowAndArrowAnimationLength();
        }
        OnMoveBowAndArrow(bowTransform.localPosition);
        if (!bowEnabled)
        {
            bowEnabled = true;
            bowObject.SetActive(bowEnabled);
        }
        if (arrowEnabled)
        {
            arrowEnabled = false;
            arrowObject.SetActive(arrowEnabled);
        }
    }
    void OnArrowShotBehavior()
    {
        if (animationLength > 0f)
        {
            BowAndArrowAnimationLength();
            animationLength -= Time.deltaTime * squashSpeed;
        }
        else 
        {
            if (!hasShot)
            {
                hasShot = true;

                animationLength = 0f;
                BowAndArrowAnimationLength();
                if (arrowEnabled)
                {
                    arrowEnabled = false;
                    arrowObject.SetActive(arrowEnabled);
                }

                Shoot(prefabsArray[0]);
            }
        }
    }

    void OnNopeBehavior()
    {
        if (animationLength != 0f)
        {
            animationLength = 0f;
            BowAndArrowAnimationLength();
        }
        hasShot = false;
        if (bowEnabled)
        {
            bowEnabled = false;
            bowObject.SetActive(bowEnabled);
        }
        if (arrowEnabled)
        {
            arrowEnabled = false;
            arrowObject.SetActive(arrowEnabled);
        }
    }


    void BowAndArrowStateBehavior()
    {
        switch (currentState)
        {
            case BowAndArrowState.Nope: OnNopeBehavior();
                break;
            case BowAndArrowState.BowRelease: OnBowReleaseBehavior();
                break;
            case BowAndArrowState.HoldingBow: OnHoldingBowBehavior();
                break;
            case BowAndArrowState.Aiming: OnAimingBehavior();
                break;
            case BowAndArrowState.ArrowShot: OnArrowShotBehavior();
                break;
            default:
                break;
        }
    }

    void BowAndArrowStateChecker()
    {
        if (manager == null)
        {
            manager = BowAndArrowInteractionManager.Instance;
        }

        if (manager != null && manager.IsInteractionInited())
        {
            
            if (currentState == BowAndArrowState.ArrowShot)
            {
                delayTimer += Time.deltaTime;
                if (delayTimer >= delayLimit && !cameraShake)
                {
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

            if (shootCounter >= 5)
            {
                if (listOfArrows.Count > 0)
                {
                    foreach (MovingArrow arrow in listOfArrows)
                        Destroy(arrow.gameObject);

                    listOfArrows.Clear();
                }

                shootCounter = 0;
                score = 0;
                UpdateScore();
            }

            if (manager.IsLeftHandInteracting() && manager.IsRightHandInteracting())
            {
                float leftHandDepth = manager.GetLeftHandScreenPos().z;
                float rightHandDepth = manager.GetRightHandScreenPos().z;

                if (leftHandDepth >= rightHandDepth)
                {
                    bowTransform.localPosition = CalculatePosition(manager.GetLeftHandScreenPos(), bowMaxLimitPos, bowMinLimitPos, ref currentBowPos);
                    arrowTransform.localPosition = CalculatePosition(manager.GetRightHandScreenPos(), arrowMaxLimitPos, arrowMinLimitPos, ref currentArrowPos);
                    aimTransform.localPosition = CalculatePosition(manager.GetLeftHandScreenPos(), aimingMaxLimitPos, aimingMinLimitPos, ref currentAimingPos);
                    
                    // if the left hand is primary, check for left hand grip
                    if (manager.GetLeftHandEvent() == BowAndArrowInteractionManager.HandEventType.Grip && manager.GetRightHandEvent() == BowAndArrowInteractionManager.HandEventType.Grip)
                    {
                        currentState = BowAndArrowState.Aiming;
                    }
                    else if (manager.GetLeftHandEvent() == BowAndArrowInteractionManager.HandEventType.Grip && manager.GetRightHandEvent() == BowAndArrowInteractionManager.HandEventType.Release)
                    {
                        if (currentState == BowAndArrowState.Aiming)
                        {
                            currentState = BowAndArrowState.ArrowShot;
                            Vector3 dir = aimTransform.position - arrowObject.transform.position;
                            dir = Vector3.Normalize(dir);
                            lastArrowDirection = dir;
                            cameraOnMove = true;
                        }
                        else
                        {
                            currentState = BowAndArrowState.HoldingBow;
                        }
                    }
                    else if (manager.GetLastLeftHandEvent() == BowAndArrowInteractionManager.HandEventType.Release)
                    {
                        currentState = BowAndArrowState.BowRelease;
                    }
                }
                else
                {
                    bowTransform.localPosition = CalculatePosition(manager.GetRightHandScreenPos(), bowMaxLimitPos, bowMinLimitPos, ref currentBowPos);
                    arrowTransform.localPosition = CalculatePosition(manager.GetLeftHandScreenPos(), arrowMaxLimitPos, arrowMinLimitPos, ref currentArrowPos);
                    aimTransform.localPosition = CalculatePosition(manager.GetRightHandScreenPos(), aimingMaxLimitPos, aimingMinLimitPos, ref currentAimingPos);

                    // if the left hand is primary, check for left hand grip
                    if (manager.GetRightHandEvent() == BowAndArrowInteractionManager.HandEventType.Grip && manager.GetLeftHandEvent() == BowAndArrowInteractionManager.HandEventType.Grip)
                    {
                        currentState = BowAndArrowState.Aiming;
                    }
                    else if (manager.GetRightHandEvent() == BowAndArrowInteractionManager.HandEventType.Grip && manager.GetLeftHandEvent() == BowAndArrowInteractionManager.HandEventType.Release)
                    {
                        if (currentState == BowAndArrowState.Aiming)
                        {
                            currentState = BowAndArrowState.ArrowShot;
                            Vector3 dir = aimTransform.position - arrowObject.transform.position;
                            dir = Vector3.Normalize(dir);
                            lastArrowDirection = dir;
                            cameraOnMove = true;
                        }
                        else
                        {
                            currentState = BowAndArrowState.HoldingBow;
                        }
                    }
                    else if (manager.GetRightHandEvent() == BowAndArrowInteractionManager.HandEventType.Release)
                    {
                        currentState = BowAndArrowState.BowRelease;
                    }
                }
            }
            else if (manager.IsLeftHandInteracting() && !manager.IsRightHandInteracting())
            {
                bowTransform.localPosition = CalculatePosition(manager.GetLeftHandScreenPos(), bowMaxLimitPos, bowMinLimitPos, ref currentBowPos);
                aimTransform.localPosition = CalculatePosition(manager.GetLeftHandScreenPos(), aimingMaxLimitPos, aimingMinLimitPos, ref currentAimingPos);

                if (manager.GetLeftHandEvent() == BowAndArrowInteractionManager.HandEventType.Grip)
                {
                    currentState = BowAndArrowState.HoldingBow;
                }
                else
                {
                    currentState = BowAndArrowState.BowRelease;
                }
            }
            else if (manager.IsRightHandInteracting() && !manager.IsLeftHandInteracting())
            {
                bowTransform.localPosition = CalculatePosition(manager.GetRightHandScreenPos(), bowMaxLimitPos, bowMinLimitPos, ref currentBowPos);
                aimTransform.localPosition = CalculatePosition(manager.GetRightHandScreenPos(), aimingMaxLimitPos, aimingMinLimitPos, ref currentAimingPos);
                if (manager.GetRightHandEvent() == BowAndArrowInteractionManager.HandEventType.Grip)
                {
                    currentState = BowAndArrowState.HoldingBow;
                }
                else
                {
                    currentState = BowAndArrowState.BowRelease;
                }
            }
            else
            {
                currentState = BowAndArrowState.BowRelease;
            }
            BowAndArrowStateBehavior();
        }
    }

    void BowAndArrowAnimationLength()
    {
        bowAndArrowAnimator.SetFloat("AnimationLength", animationLength);
    }

    void TurnTowardBowAndArrowRotation()
    {
        if (currentState == BowAndArrowState.Aiming)
	    {
            Vector3 direction = currentAimingPos - currentArrowPos;
            Quaternion newRot = Quaternion.LookRotation(direction);

            if (newRot != bowAndArrowTransform.rotation)
            {
                bowAndArrowTransform.rotation = Quaternion.Lerp(bowAndArrowTransform.rotation, newRot, 0.1f);
            }
	    }
        else if (bowAndArrowTransform.rotation != initialRotation)
        {
            bowAndArrowTransform.rotation = Quaternion.Lerp(bowAndArrowTransform.rotation, initialRotation, 0.1f);
        }
    }

    void OnMoveBowAndArrow(Vector3 referenceLocalPosition)
    {
        float distance = 0;
        distance = Vector3.Distance(bowAndArrowTransform.localPosition, referenceLocalPosition);
        
        if (distance > 0.1f)
            bowAndArrowTransform.localPosition = Vector3.Lerp(bowAndArrowTransform.localPosition, referenceLocalPosition, 0.1f);
        else
            bowAndArrowTransform.localPosition = referenceLocalPosition;

    }

    Vector3 CalculateMiddlePosition(Vector3 bowPos, Vector3 arrowPos)
    {
        Vector3 newPos = Vector3.zero;

        currentBowAndArrowPos.x = Mathf.Lerp(bowPos.x, arrowPos.x, 0.5f);
        currentBowAndArrowPos.y = Mathf.Lerp(bowPos.y, arrowPos.y, 0.5f);
        currentBowAndArrowPos.z = Mathf.Lerp(bowPos.z, arrowPos.z, 0.5f);
        newPos = currentBowAndArrowPos;

        return newPos;
    }

    Vector3 CalculatePosition(Vector3 screenNormalPos, Vector3 maxLimitPos, Vector3 minLimitPos, ref Vector3 currentPos)
    {
        currentPos.x = Mathf.Lerp(minLimitPos.x, maxLimitPos.x, screenNormalPos.x);
        currentPos.y = Mathf.Lerp(minLimitPos.y, maxLimitPos.y, screenNormalPos.y);
        currentPos.z = Mathf.Lerp(minLimitPos.z, maxLimitPos.z, screenNormalPos.z);

        return currentPos;
    }

    void Shoot(GameObject prefabObj)
    {
        GameObject arrow = Instantiate(prefabObj, arrowObject.transform.position, arrowObject.transform.rotation) as GameObject;
        arrowObject.SetActive(false);
        /*
        Vector3 dir = aimTransform.position - arrowObject.transform.position;
        dir = Vector3.Normalize(dir);
        lastArrowDirection = dir;
        */
        arrow.GetComponent<MovingArrow>().Initialize(lastArrowDirection);
        lastArrowSpeed = arrow.GetComponent<MovingArrow>().speed;
        cameraOnMove = true;
        shootCounter++;
        UpdateScore();
    }

    void StopCameraMovement()
    {
        mainCamera.transform.position = cameraInitalPosition;
    }

    public void AddProjectileInList(MovingArrow projectile)
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
        listOfArrows.Add(projectile);
    }

    void UpdateState()
    {
        if (stateText)
        {
            string sGuiText = string.Empty;
            sGuiText += "State: " + currentState.ToString();
            stateText.text = sGuiText;
        }
    }

    public void UpdateScore(int hexScore = 0)
    {
        score += hexScore;
        // display debug information
        if (scoreText)
        {
            string sGuiText = string.Empty;
            sGuiText += "Score: " + score.ToString();
            sGuiText += "\nArrows: " + (5 - shootCounter).ToString();
            scoreText.text = sGuiText;
        }
    }
}