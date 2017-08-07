using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexagonController : MonoBehaviour
{
    public GameObject hexagon;

    int lineIndex;
    int columIndex;
    float centerDistance;
    float scoreMultiplier;
    HoneycombMatrixType matrixType;

    List<HexagonController> neighbors =  new List<HexagonController>();

    public void Initialize(int _lineIndex = -1, int _columIndex = -1, HoneycombMatrixType _matrixType = HoneycombMatrixType.SquareMatrixVertical, float _distance = -1f)
    {
        if (_matrixType == HoneycombMatrixType.HoneyCombMatrix)
        {
            matrixType = _matrixType;
            centerDistance = _distance;
            SetHexagonColor();
        }
        else
        {
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

    void SetHexagonColor()
    {
        scoreMultiplier = 1f - centerDistance;
        if (centerDistance >= 0.8f)
        {
            float interpolate = centerDistance - 0.8f;
            interpolate /= 0.2f;
            hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.blue, Color.black, interpolate);
        }
        else if (centerDistance >= 0.6f)
        {
            float interpolate = centerDistance - 0.6f;
            interpolate /= 0.2f;
            hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.cyan, Color.blue, interpolate);
        }
        else if(centerDistance >= 0.4f)
        {
            float interpolate = centerDistance - 0.4f;
            interpolate /= 0.2f;
            hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.green, Color.cyan, interpolate);
        }
        else if(centerDistance >= 0.2f)
        {
            float interpolate = centerDistance - 0.2f;
            interpolate /= 0.2f;
            hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.yellow, Color.green, interpolate);
        }
        else if(centerDistance >= 0f)
        {
            float interpolate = centerDistance /= 0.2f;
            hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red, Color.yellow, interpolate);
        }
    }
}
