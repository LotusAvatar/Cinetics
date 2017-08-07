using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum HoneycombMatrixType
{
    HoneyCombMatrix,
    SquareMatrixVertical,
    SquareMatrixHorizontal
}

public class HoneycombMatrix : MonoBehaviour
{

    public GameObject hexagon;
    public GameObject hexagon2;
    public int dimentionX;
    public int dimentionY;
    public int dimentionHoneycomb;
    public Vector2 vectorNormal =  new Vector2();
    public Vector2 vectorPointingUp =  new Vector2();
    public HoneycombMatrixType matrixType;
    Vector3 position = new Vector3();
    Vector3 fingerTipPosition = new Vector3();
    bool canTrackPosition;
    int totalHexagons = 0;
    LauncherInteractionManager manager;
    HexagonController[] hexagonArray;
    HexagonController[,] hexagonMatrix;

    private static HoneycombMatrix instance;

    public static HoneycombMatrix Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<HoneycombMatrix>();
            }
            return instance;
        }
    }

    public HexagonController GetHexagon(int x, int y)
    {
        if (x < 0 || y < 0 || x >= dimentionX || y >= dimentionY)
            return null;
        
        return hexagonMatrix[x, y];
    }

    void Start ()
    {
        if (manager == null)
            manager = LauncherInteractionManager.Instance;

        switch (matrixType)
        {
            case HoneycombMatrixType.HoneyCombMatrix: CreateHoneycombMatrix(); break;
            case HoneycombMatrixType.SquareMatrixHorizontal: CreateSquareMatrix(); break;
            case HoneycombMatrixType.SquareMatrixVertical: CreateSquareMatrix(); break;
            default: break;
        }


    }

    void CreateHoneycombMatrix()
    {
        CreateHoneycombMatrixHexagon(dimentionHoneycomb);
    }

    void CreateHoneycombMatrixHexagon(int width)
    {
        int amplitude = (width / 2) + 1;
        int currentWidth = width;
        float normalizedDistance = 0f;
        float maxDistance = 0f;

        bool hexagonSwitch = false;
        for (int i = 0; i < amplitude; i++)
        {
            Vector3 movingPoint = transform.localPosition;
            if (currentWidth != width)
            {
                float multiplyX = ((float)currentWidth / 2f) - 0.5f;

                movingPoint.x = vectorPointingUp.x * multiplyX;

                int hexagonAmount = currentWidth * 2;

                for (int j = 0; j < hexagonAmount; j++)
                {
                        movingPoint.y = transform.position.y;
                    if (!hexagonSwitch)
                    {
                        movingPoint.y += vectorPointingUp.y * i;
                        GameObject hex = Instantiate(hexagon2, movingPoint, hexagon.transform.rotation) as GameObject;
                        hex.transform.SetParent(this.transform);
                        float hexDistance = Vector3.Distance(movingPoint, transform.position);
                        normalizedDistance = Mathf.InverseLerp(0f, maxDistance, hexDistance);
                        hex.GetComponent<HexagonController>().Initialize(-1, -1, matrixType, normalizedDistance);
                    }
                    else
                    {
                        movingPoint.y -= vectorPointingUp.y * i;
                        GameObject hex = Instantiate(hexagon2, movingPoint, hexagon.transform.rotation) as GameObject;
                        hex.transform.SetParent(this.transform);
                        float hexDistance = Vector3.Distance(movingPoint, transform.position);
                        normalizedDistance = Mathf.InverseLerp(0f, maxDistance, hexDistance);
                        hex.GetComponent<HexagonController>().Initialize(-1, -1, matrixType, normalizedDistance);
                        movingPoint.x -= vectorPointingUp.x;
                    }

                    hexagonSwitch = !hexagonSwitch;
                }
                hexagonSwitch = false;
                currentWidth--;
            }
            else
            {
                float multiply = (width / 2f) -0.5f;
                movingPoint.x = vectorPointingUp.x * multiply;
                movingPoint.y = transform.position.y;
                maxDistance = movingPoint.x;
                for (int j = 0; j < currentWidth; j++)
                {
                    GameObject hex = Instantiate(hexagon2, movingPoint, hexagon.transform.rotation) as GameObject;
                    hex.transform.SetParent(this.transform);
                    float hexDistance = Vector3.Distance(movingPoint, transform.position);
                    normalizedDistance = Mathf.InverseLerp(0f, maxDistance, hexDistance);
                    hex.GetComponent<HexagonController>().Initialize(-1, -1, matrixType, normalizedDistance);
                    movingPoint.x -= vectorPointingUp.x;
                }
                currentWidth--;
            }
        }
    }

    void CreateSquareMatrix()
    {
        totalHexagons = dimentionX * dimentionY;
        hexagonArray = new HexagonController[totalHexagons];
        hexagonMatrix = new HexagonController[dimentionX, dimentionY];
        float displacementX = ((float)dimentionX * vectorNormal.x / 2);
        float displacementY = ((float)dimentionY * vectorNormal.y / 2);
        Vector3 initialPos = new Vector3(-displacementX, displacementY, 0f);
        transform.position += initialPos;
        position = Vector3.zero;
        if (matrixType == HoneycombMatrixType.SquareMatrixVertical)
        {
            CreateMatrixPointungUp();
        }
        else
        {
            CreateMatrix();
        }
        canTrackPosition = true;
    }

    void Update()
    {
        if (canTrackPosition)
        {
            //UpdatePosition(IronmanDebugHand.Instance.PalmPosition);
        }    
    }

    void UpdatePosition(Vector3 bonePosition)
    {
        Vector2 relativeScreenPos = new Vector2(bonePosition.x, bonePosition.y);
        for (int i = 0; i < totalHexagons; i++)
        {
            Vector2 hexagonPosition = new Vector2(hexagonArray[i].transform.position.x, hexagonArray[i].transform.position.y);
            float distance = Vector2.Distance(relativeScreenPos, hexagonPosition);
            float radius = 3f;
            if (distance < radius && manager.IsInteractionInited())
            {
                float normalize = distance / radius;
                hexagonArray[i].hexagon.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.green, Color.gray, normalize);
            }
            else
            {
                hexagonArray[i].hexagon.GetComponent<MeshRenderer>().material.color = Color.gray;
            }
            //relativeScreenPos
        }
    }
	
    void CreateMatrix()
    {
        int count = 0;
        for (int i = 0; i < dimentionX; i++)
        {
            float displacement = 0f;                
            if ((i % 2) != 0)
            {
                displacement = vectorNormal.y / 2;
            }
            for (int j = 0; j < dimentionY; j++)
            {
                Vector3 currentPos = transform.position + position;
                currentPos.y -= displacement;
                GameObject hex = Instantiate(hexagon, currentPos, hexagon.transform.rotation) as GameObject;
                hex.transform.SetParent(transform);
                hexagonArray[count] = hex.GetComponent<HexagonController>();
                count++;
                position.y += vectorNormal.y;               
               
            }
            position.y = 0f;
            position.x += vectorNormal.x;
        }
            
    }

    void SetHexagonNeighbors()
    {
        for (int i = 0; i < totalHexagons; i++)
        {

        }
    }

    void CreateMatrixPointungUp()
    {
        int count = 0;
        for (int i = 0; i < dimentionY; i++)
        {
            float displacement = 0f;
            if ((i % 2) != 0)
            {
                displacement = vectorPointingUp.x / 2;
            }
            for (int j = 0; j < dimentionX; j++)
            {
                Vector3 currentPos = transform.position + position;
                currentPos.x -= displacement;
                GameObject hex = Instantiate(hexagon2, currentPos, hexagon.transform.rotation) as GameObject;
                hex.transform.SetParent(transform);
                hexagonArray[count] = hex.GetComponent<HexagonController>();
                hexagonMatrix[j,i] = hex.GetComponent<HexagonController>();
                count++;
                position.x += vectorPointingUp.x;

            }
            position.x = 0f;
            position.y += vectorPointingUp.y;
        }

    }
}
