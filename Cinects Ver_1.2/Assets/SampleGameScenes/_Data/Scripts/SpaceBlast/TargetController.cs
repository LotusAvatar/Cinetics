using UnityEngine;
using System.Collections;

public enum TargetState
{
    Nope,
    Moving
}

public class TargetController : MonoBehaviour
{
    public Bezier bezier;
    public Transform mainTargetAxis_X;
    public Transform mainTargetAxis_Y;
    public Transform target_2;
    public Transform cameraRef;
    public Material target_1_mat;
    public Material target_2_mat;
    public float handUpperLimit = 2f;
    float handLowerLimit = 0f;
    float lockedTargetDelay;
    float lockedTargetCounter;
    float lockedTargetSwithCounter;
    Vector3 mainTargetPos;
    Vector3 target_2_tPos;
    Vector3 bezierPos;
    bool onTargetLocked;
    bool lockedTargetSwith;

    public Vector3 AimPos
    {
        get { return mainTargetAxis_Y.position; }
    }

    public void UpdateTargetPos(Vector3 cursorPosition, bool onlySecondTarget = false)
    {
        if (!onlySecondTarget)
        {
            bezierPos = bezier.GetBezierPosition(cursorPosition.x);
            mainTargetAxis_X.position = bezierPos;
            float upperAmount = Mathf.Lerp(0f, handUpperLimit, cursorPosition.y);
            mainTargetPos = mainTargetAxis_X.localPosition;
            mainTargetPos.y = upperAmount;
            mainTargetAxis_X.localPosition = mainTargetPos;
        }
        target_2_tPos = Vector3.Lerp(target_2.position, mainTargetAxis_X.position, 0.1f);
        target_2.position = target_2_tPos;
    }

    public void OnTargetFound()
    {
        target_1_mat.color = Color.green;
    }

    public void OnTargetMiss()
    {
        target_1_mat.color = Color.white;
    }

    public void OnTargetLocked()
    {
        onTargetLocked = true;
        target_2_mat.color = Color.green;
        lockedTargetSwith = true;
    }

    public void OnTargetRelese()
    {
        onTargetLocked = false;
        target_2_mat.color = Color.red;
    }

    void Start()
    {
        OnTargetMiss();
        lockedTargetDelay = SpaceBlastGameController.Instance.AimLockedDelay / 60f;
    }


    void Update()
    {
        if (onTargetLocked)
            OnTargetBlink();
    }

    void OnTargetBlink()
    {
        if (lockedTargetSwith)
            lockedTargetCounter += Time.deltaTime;
        else
            lockedTargetCounter -= Time.deltaTime;

        if (lockedTargetCounter >= lockedTargetDelay)
        {
            lockedTargetCounter = lockedTargetDelay;
            lockedTargetSwith = !lockedTargetSwith;
        }
        else if (lockedTargetCounter <= 0f)
        {
            lockedTargetCounter = 0f;
            lockedTargetSwith = !lockedTargetSwith;
        }

        float progress = lockedTargetCounter / lockedTargetDelay;
        Color nextColor = Color.Lerp(Color.green, Color.white, progress);
        target_2_mat.color = nextColor;
    }
}
