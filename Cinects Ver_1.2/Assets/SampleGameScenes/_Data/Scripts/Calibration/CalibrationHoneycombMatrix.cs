using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CalibrationHoneycombMatrix : MonoBehaviour
{
    public GameObject hexagon;
    public GameObject hexagon2;
    public int dimentionX;
    public int dimentionY;
    public int dimentionHoneycomb;
    public Vector2 vectorNormal =  new Vector2();
    public Vector2 vectorPointingUp =  new Vector2();
    public float depth = 0.5f;
    public float currentAngle = 0f;
    public HoneycombMatrixType matrixType;
    Vector3 position = new Vector3();
    Vector3 fingerTipPosition = new Vector3();
    bool canTrackPosition;
    int totalHexagons = 0;
    LauncherInteractionManager manager;
    CalibrationHexagonController[] hexagonArray;
    CalibrationHexagonController[,] hexagonMatrix;
    public float springConstant = 200.0f;
    public float damping = 0.014f;
    bool neighborsReady = false;

    #region Constants
    const float localPosZ_Rise = -0.275f;
    #endregion

    private static CalibrationHoneycombMatrix instance;

    public static CalibrationHoneycombMatrix Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CalibrationHoneycombMatrix>();
            }
            return instance;
        }
    }

    public CalibrationHexagonController GetHexagon(int x, int y)
    {
        if (x < 0 || y < 0 || x >= dimentionX || y >= dimentionY)
            return null;
        
        return hexagonMatrix[y, x];
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
                        hex.GetComponent<CalibrationHexagonController>().Initialize(-1, -1, matrixType, normalizedDistance);
                    }
                    else
                    {
                        movingPoint.y -= vectorPointingUp.y * i;
                        GameObject hex = Instantiate(hexagon2, movingPoint, hexagon.transform.rotation) as GameObject;
                        hex.transform.SetParent(this.transform);
                        float hexDistance = Vector3.Distance(movingPoint, transform.position);
                        normalizedDistance = Mathf.InverseLerp(0f, maxDistance, hexDistance);
                        hex.GetComponent<CalibrationHexagonController>().Initialize(-1, -1, matrixType, normalizedDistance);
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
                    hex.GetComponent<CalibrationHexagonController>().Initialize(-1, -1, matrixType, normalizedDistance);
                    movingPoint.x -= vectorPointingUp.x;
                }
                currentWidth--;
            }
        }
    }

    void CreateSquareMatrix()
    {
        totalHexagons = dimentionX * dimentionY;
        hexagonArray = new CalibrationHexagonController[totalHexagons];
        hexagonMatrix = new CalibrationHexagonController[dimentionY, dimentionX];
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
            //StartCoroutine(CreateMatrixCoroutine());
        }
        canTrackPosition = true;
    }

    void Update()
    {
        if (neighborsReady)
        {
            for (int i = 1; i < hexagonMatrix.GetLength(1)-1; i++)
            {
                for (int j = 1; j < hexagonMatrix.GetLength(0)-1; j++)
                {
                    // Discrete wave equation with damping.
                    float neighbor_sum = 0;

                    for (int l = 0; l < hexagonMatrix[j, i].Neghbors().Count; l++)
                        neighbor_sum += hexagonMatrix[j, i].Neghbors()[l].localPosZ;
                    

                    float delta_from_rest = (1f / (float)hexagonMatrix[j, i].Neghbors().Count) * neighbor_sum - hexagonMatrix[j, i].localPosZ;

                    hexagonMatrix[j, i].AddForce(springConstant, delta_from_rest, damping);
                }
            }
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
	
    IEnumerator CreateMatrixCoroutine()
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
                hexagonArray[count] = hex.GetComponent<CalibrationHexagonController>();
                hexagonArray[count].Initialize(j, i);
                hexagonMatrix[j, i] = hexagonArray[count];
                count++;
                position.y += vectorNormal.y;
                yield return new WaitForEndOfFrame();
            }
            position.y = 0f;
            position.x += vectorNormal.x;
        }
        SetHexagonNeighbors();
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
                hexagonArray[count] = hex.GetComponent<CalibrationHexagonController>();

                if (i == 0 || j == 0 || i == dimentionX-1 || j == dimentionY-1)
                    hexagonArray[count].Initialize(j, i, HoneycombMatrixType.SquareMatrixVertical, -1, true);
                else
                    hexagonArray[count].Initialize(j, i);

                hexagonMatrix[j, i] = hexagonArray[count];
                count++;
                position.y += vectorNormal.y;               
               
            }
            position.y = 0f;
            position.x += vectorNormal.x;
        }
        SetHexagonNeighbors();
    }

    void SetHexagonNeighbors()
    {
        for (int i = 0; i < hexagonMatrix.GetLength(1); i++)
        {
            for (int j = 0; j < hexagonMatrix.GetLength(0); j++)
            {
                if (i - 1 >= 0)
                    hexagonMatrix[j, i].AddNeighbor(hexagonMatrix[j, i - 1]);

                if (i + 1 < hexagonMatrix.GetLength(1))
                    hexagonMatrix[j, i].AddNeighbor(hexagonMatrix[j, i + 1]);

                if (j - 1 >= 0)
                    hexagonMatrix[j, i].AddNeighbor(hexagonMatrix[j - 1, i]);

                if (j + 1 < hexagonMatrix.GetLength(0))
                    hexagonMatrix[j, i].AddNeighbor(hexagonMatrix[j + 1, i]);

                if (i % 2 == 0)
                {
                    if (j + 1 < hexagonMatrix.GetLength(0))
                    {
                        if (i + 1 < hexagonMatrix.GetLength(1))
                            hexagonMatrix[j, i].AddNeighbor(hexagonMatrix[j + 1, i + 1]);

                        if (i - 1 >= 0)
                            hexagonMatrix[j, i].AddNeighbor(hexagonMatrix[j + 1, i - 1]);
                    }
                }
                else
                {
                    if (j - 1 >= 0)
                    {
                        if (i + 1 < hexagonMatrix.GetLength(1))
                            hexagonMatrix[j, i].AddNeighbor(hexagonMatrix[j - 1, i + 1]);

                        if (i - 1 >= 0)
                            hexagonMatrix[j, i].AddNeighbor(hexagonMatrix[j - 1, i - 1]);
                    }
                }
            }
        }

        neighborsReady = true;
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
                hexagonArray[count] = hex.GetComponent<CalibrationHexagonController>();
                hexagonMatrix[j,i] = hex.GetComponent<CalibrationHexagonController>();
                count++;
                position.x += vectorPointingUp.x;

            }
            position.x = 0f;
            position.y += vectorPointingUp.y;
        }
    }

    void RiseBorders()
    {
        JointOverlayerCalibration.Instance.ContinueHandAnimation(CalibrationState.HandToMid);
        Vector3 localPosRise = new Vector3(0f, 0f, localPosZ_Rise);
        for (int i = 0; i < hexagonMatrix.GetLength(1); i++)
        {
            for (int j = 0; j < hexagonMatrix.GetLength(0); j++)
            {
                if (i == 0 || j == 0 || i == hexagonMatrix.GetLength(1) -1 || j == hexagonMatrix.GetLength(0) -1)
                    hexagonMatrix[j, i].hexagon.transform.localPosition = localPosRise;

                hexagonMatrix[j, i].localPosZ = localPosZ_Rise;
            }
        }
    }

    public void OnHandChosen(BendingDirection direction)
    {
        Vector3 waterDrop = new Vector3(0f, 0f, 10f);
        if (direction == BendingDirection.Right)
        {
            hexagonMatrix[7, 32].hexagon.transform.localPosition = waterDrop;
            for (int i = 0; i < hexagonMatrix[7, 32].Neghbors().Count; i++)
                hexagonMatrix[7, 32].Neghbors()[i].hexagon.transform.localPosition = waterDrop;
        }
        else
        {
            hexagonMatrix[7, 8].hexagon.transform.localPosition = waterDrop;
            for (int i = 0; i < hexagonMatrix[7, 8].Neghbors().Count; i++)
                hexagonMatrix[7, 8].Neghbors()[i].hexagon.transform.localPosition = waterDrop;
        }
        Invoke("RiseBorders", 2f);
    }
}
