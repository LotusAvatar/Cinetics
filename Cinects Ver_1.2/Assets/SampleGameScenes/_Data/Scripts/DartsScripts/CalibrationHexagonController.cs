using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CalibrationHexagonController : MonoBehaviour
{
    public GameObject hexagon;

    int lineIndex;
    int columIndex;
    float currentDepth;
    float centerDistance;
    float scoreMultiplier;
    float currentAngle;
    bool trigger;
    bool isCorner;
    float depth = 0.5f;
    public float localPosZ;
    public float relativeHeight;
    public float heightSpeed;
    public int angle;
    Vector3 localPos;
    System.Random rand = new System.Random();
    HoneycombMatrixType matrixType;
    List<CalibrationHexagonController> neighbors = new List<CalibrationHexagonController>();

    void Start()
    {
        UpdatePosZ();
    }

    void Update()
    {
        if (!isCorner)
            SetHexagonColor();
    } 

    void UpdatePosZ()
    {
        localPos = hexagon.transform.localPosition;
        localPosZ = localPos.z;
    }

    void SelfMovement()
    {
        localPos = hexagon.transform.localPosition;
        localPos.z = currentDepth;
        localPosZ = localPos.z;
        currentAngle += Time.deltaTime * heightSpeed;
        relativeHeight = Mathf.Sin(currentAngle);
        currentDepth = depth * relativeHeight;
        hexagon.transform.localPosition = localPos;
        SetHexagonColor();
    }

    public void Initialize(int _lineIndex = -1, int _columIndex = -1, HoneycombMatrixType _matrixType = HoneycombMatrixType.SquareMatrixVertical, float _distance = -1f, bool _isCorner = false)
    {
        if (_matrixType == HoneycombMatrixType.HoneyCombMatrix)
        {
            matrixType = _matrixType;
            centerDistance = _distance;
            SetHexagonColor();
        }
        else
        {
            isCorner = _isCorner;
            if (isCorner)
            {
                hexagon.GetComponent<MeshRenderer>().material.color = Color.black;
                hexagon.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }
                        
            //currentAngle = ((float) rand.Next(0, 6000)) / 1000f;
            //Debug.Log("currentAngle: " + currentAngle);
            lineIndex = _lineIndex;
            columIndex = _columIndex;
        }
    }

    public void UpdateScore()
    {
        if (HoneycombMatrix.Instance.transform.GetComponentInParent(typeof(BowAndArrowController)))
        {
            BowAndArrowController.Instance.UpdateScore((int)Mathf.Lerp(0f, 100f, scoreMultiplier));
        }
        else 
        {
            DirectionLauncher.Instance.UpdateScore((int)Mathf.Lerp(0f, 100f, scoreMultiplier));
        }
    }

    public void AddNeighbor(CalibrationHexagonController newNeighbor)
    {
        neighbors.Add(newNeighbor);
    }

    public List<CalibrationHexagonController> Neghbors()
    {
        UpdatePosZ();
        return neighbors;
    }

    public void AddForce(float springConstant, float delta_from_rest, float damping)
    {
        hexagon.GetComponent<Rigidbody>().AddForce(Vector3.forward * springConstant * delta_from_rest);
        hexagon.GetComponent<Rigidbody>().velocity *= (1 - damping);
    }


    void SetHexagonColor()
    {
        if (localPosZ >= 0.66f * CalibrationHoneycombMatrix.Instance.depth)
        {
            float interpolate = localPosZ - 0.66f * CalibrationHoneycombMatrix.Instance.depth;
            interpolate /= 0.33f * CalibrationHoneycombMatrix.Instance.depth;
            hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.blue, Color.black, interpolate);
        }
        else if (localPosZ >= 0.33f * CalibrationHoneycombMatrix.Instance.depth)
        {
            float interpolate = localPosZ - 0.33f * CalibrationHoneycombMatrix.Instance.depth;
            interpolate /= 0.33f * CalibrationHoneycombMatrix.Instance.depth;
            hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.cyan, Color.blue, interpolate);
        }
        else if(localPosZ >= 0f)
        {
            float interpolate = localPosZ * CalibrationHoneycombMatrix.Instance.depth;
            interpolate /= 0.33f * CalibrationHoneycombMatrix.Instance.depth;
            hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.green, Color.cyan, interpolate);
        }
        else if(localPosZ >= -0.33f * CalibrationHoneycombMatrix.Instance.depth)
        {
            float interpolate = localPosZ + 0.33f * CalibrationHoneycombMatrix.Instance.depth;
            interpolate /= 0.33f * CalibrationHoneycombMatrix.Instance.depth;
            hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.yellow, Color.green, interpolate);
        }
        else if(localPosZ >= -0.66f * CalibrationHoneycombMatrix.Instance.depth)
        {
            float interpolate = localPosZ + 0.66f * CalibrationHoneycombMatrix.Instance.depth;
            interpolate /= 0.33f * CalibrationHoneycombMatrix.Instance.depth;
            hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red, Color.yellow, interpolate);
        }
    }
}
