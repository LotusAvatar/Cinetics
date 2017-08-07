using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct FittingObjectAddress
{
    public FittingObjectsController.GridAddressState gridState;
    public FittingObject myObject;
}

public class FittingObjectsController : MonoBehaviour
{
    public enum GridAddressState
    {
        Occupied,
        Empty
    }

    [Tooltip("List of the prefabs objects.")]
    public List<GameObject> draggablePrefabs;

    [Tooltip("List of the socket colliders objects.")]
    public List<SocketObject> socketObjects;

    [Tooltip("Dictionary of the objects that may be created in scene.")]
    public Dictionary<FittingObject.MovingObjectType, GameObject> draggablePrefabsDictionary =  new Dictionary<FittingObject.MovingObjectType, GameObject>();

    [Tooltip("Delay between movements.")]
    public float delay = 4f;

    private InteractionManager manager;
    FittingObjectAddress[,] fittingGrid;
    FittingObjectAddress[] fittingVectorFirstState;
    FittingObjectAddress[] fittingVectorLastState;
    List<FittingObject[]> fittingListOfVectorObjects;
    FittingObject currentObj;
    FittingObject nextObj;
    float timer = 10f;
    float verticalSpace;
    int colums = 3;
    int lines = 7;

    public static FittingObjectsController _instance;

    public static FittingObjectsController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (FittingObjectsController)GameObject.FindObjectOfType(typeof(FittingObjectsController));
            }
            return _instance;
        }
    }

    // Use this for initialization
    void Start ()
    {
        verticalSpace = 8.5f / (float)lines;
        FillDictionary();
        fittingGrid = new FittingObjectAddress[lines, colums];
        fittingVectorFirstState = new FittingObjectAddress[colums];
        fittingVectorLastState = new FittingObjectAddress[colums];
        for (int i = 0; i < lines; i++)
        {
            for (int j = 0;j < colums; j++)
            {
                fittingGrid[i, j].gridState = GridAddressState.Empty;
                fittingGrid[i, j].myObject = null;
                if (i == 0)
                {
                    fittingVectorFirstState[j] = fittingGrid[i, j];
                }
                else if (i == lines-1)
                {
                    fittingVectorLastState[j] = fittingGrid[i, j];
                }
            }
        }

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (manager == null)
        {
            manager = InteractionManager.Instance;
        }

        if (manager != null && manager.IsInteractionInited())
        {
            long userID = manager.GetUserID();
            if (userID != 0)
            {
                timer += Time.deltaTime;
                if (timer >= delay)
                {
                    timer = 0f;
                    FillLineAndMoveDown();
                }
            }
        }
    }

    void FillDictionary()
    {
        foreach (GameObject prefab in draggablePrefabs)
        {
            draggablePrefabsDictionary.Add(prefab.GetComponent<FittingObject>().myType, prefab);
        }
    }

    public void DynamicReplacement(int[] movingObjectId, int[] targetObjectId)
    {
        FittingObjectAddress aux = fittingGrid[targetObjectId[0], targetObjectId[1]];
        fittingGrid[targetObjectId[0], targetObjectId[1]] = fittingGrid[movingObjectId[0], movingObjectId[1]];
        fittingGrid[movingObjectId[0], movingObjectId[1]] = aux;

        for (int i = 0; i < lines; i++)
        {
            for (int j = 0; j < colums; j++)
            {
                if (fittingGrid[i, j].gridState == GridAddressState.Occupied)
                {
                    int[] testArray = new int[2] { i, j };
                    if (testArray != fittingGrid[i, j].myObject.GridId)
                    {
                        fittingGrid[i, j].myObject.GridId = testArray;
                        float beginning_X = -4.5f + (4.5f * j);
                        Vector3 pos = fittingGrid[i, j].myObject.GridPosition;
                        pos.x = beginning_X;
                        fittingGrid[i, j].myObject.GridPosition = pos;
                        float nomalizedX = Mathf.InverseLerp(-6f, 6f, fittingGrid[i, j].myObject.GridPosition.x);
                        float nomalizedY = Mathf.InverseLerp(-3.5f, 5f, fittingGrid[i, j].myObject.GridPosition.y);
                        fittingGrid[i, j].myObject.GridNormalizedPosition = new Vector2(nomalizedX, nomalizedY);
                        fittingGrid[i, j].myObject.UpdatePositions();
                    }
                }
            }
        }

    }

    void FillLineAndMoveDown()
    {
        for (int i = 0; i < colums; i++)
        {
            fittingVectorFirstState[i].gridState = GridAddressState.Empty;
            fittingVectorFirstState[i].myObject = null;
        }
        int randPos = Random.Range(0, colums);
        fittingVectorFirstState[randPos].gridState = GridAddressState.Occupied;
        int randType = Random.Range(0, 3);
        GameObject obj =  Instantiate(draggablePrefabsDictionary[(FittingObject.MovingObjectType)randType], Vector3.zero, transform.rotation) as GameObject;
        fittingVectorFirstState[randPos].myObject = obj.GetComponent<FittingObject>();
        fittingVectorFirstState[randPos].myObject.GridId[1] = randPos;
        Debug.Log("GridId: " + fittingVectorFirstState[randPos].myObject.GridId[0] + fittingVectorFirstState[randPos].myObject.GridId[1]);
        FittingObject lastObj = fittingVectorFirstState[randPos].myObject;
        float beginning_X = -4.5f + (4.5f * randPos);
        float beginning_y = 4f + verticalSpace;
        Vector3 pos = new Vector3(beginning_X, beginning_y, 0f);
        lastObj.GridPosition = pos;

        float nomalizedX = Mathf.InverseLerp(-6f, 6f, beginning_X);
        float nomalizedY = Mathf.InverseLerp(-3.5f, 5f, beginning_y);
        lastObj.GridNormalizedPosition = new Vector2(nomalizedX, nomalizedY);
        lastObj.UpdatePositions();

        for (int i = 0; i < colums; i++)
        {
            fittingGrid[0, i] = fittingVectorFirstState[i];
            fittingVectorLastState[i] = fittingGrid[lines - 1, i];
        }

        for (int i = lines - 1; i > 0; i--)
        {
            for (int j = 0; j < colums; j++)
            {
                fittingGrid[i, j] = fittingGrid[i - 1, j];
            }
        }
        
        for (int i = 0; i < colums; i++)
        {
            if (fittingVectorLastState[i].gridState == GridAddressState.Occupied)
            {
                Destroy(fittingVectorLastState[i].myObject.gameObject);
            }
        }

        for (int i = 0; i < lines; i++)
        {
            for (int j = 0; j < colums; j++)
            {
                if (fittingGrid[i, j].gridState == GridAddressState.Occupied)
                {
                    fittingGrid[i, j].myObject.GridId[0] = i;
                    fittingGrid[i, j].myObject.GridId[1] = j;
                    pos = new Vector3(fittingGrid[i, j].myObject.GridPosition.x, beginning_y, 0f);
                    fittingGrid[i, j].myObject.GridPosition = pos;
                    nomalizedX = Mathf.InverseLerp(-6f, 6f, beginning_X);
                    nomalizedY = Mathf.InverseLerp(-3.5f, 5f, beginning_y);
                    fittingGrid[i, j].myObject.GridNormalizedPosition = new Vector2(nomalizedX, nomalizedY);
                    fittingGrid[i, j].myObject.UpdatePositions();
                }
            }
            beginning_y -= verticalSpace;
        }
    }
}
