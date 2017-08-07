using UnityEngine;
using System.Collections;

public class FittingObject : MonoBehaviour
{
    public enum MovingObjectType
    {
        Cube,
        Pentagon,
        Cross
    }

    public MovingObjectType myType = new MovingObjectType();
    public GameObject realObj;
    public GameObject ghostObj;
    Vector3 gridPosition = new Vector3();
    Vector2 gridNormalizedPosition = new Vector2();
    int[] gridId = new int[2] {0,0};
    bool isSelected = false;

    public Vector3 GridPosition
    {
        get { return gridPosition; }
        set { gridPosition = value; }
    }

    public Vector2 GridNormalizedPosition
    {
        get { return gridNormalizedPosition; }
        set { gridNormalizedPosition = value; }
    }

    public int[] GridId
    {
        get { return gridId; }
        set { gridId = value; }
    }

    // Use this for initialization
    void Start()
    {
        HideGhost();
        gridPosition = realObj.transform.position;
        float nomalizedX = Mathf.InverseLerp(-6f, 6f, gridPosition.x);
        float nomalizedY = Mathf.InverseLerp(-3.5f, 5f, gridPosition.y);
        gridNormalizedPosition = new Vector2(nomalizedX, nomalizedY);
    }

    public GameObject ShowGhost()
    {
        isSelected = true;
        ghostObj.transform.position = gridPosition;
        ghostObj.SetActive(isSelected);
        return ghostObj;
    }

    public void HideGhost()
    {
        isSelected = false;
        ghostObj.SetActive(isSelected);
    }

	public void UpdatePositions()
    {
        realObj.transform.position = gridPosition;
        if (isSelected)
        {
            Vector3 ghostPos = ghostObj.transform.position;
            ghostPos.y = gridPosition.y;
            ghostObj.transform.position = ghostPos;
        }
    }

    public void TryToMove(Vector3 targetPos)
    {
        if (gridPosition.x != targetPos.x)
        {
            int[] targetId = new int[2] { gridId[0], gridId[1] };
            if (targetPos.x > gridPosition.x)
            {
                targetId[1]++;
                Debug.Log("MAIOR++ gridId: " + gridId[1] + "targetId: " + targetId[1]);
            }
            else
            {
                targetId[1]--;
                Debug.Log("MENOR-- gridId: " + gridId[1] + "/ntargetId: " + targetId[1]);
            }


            FittingObjectsController.Instance.DynamicReplacement(gridId, targetId);
        }
    }
}
