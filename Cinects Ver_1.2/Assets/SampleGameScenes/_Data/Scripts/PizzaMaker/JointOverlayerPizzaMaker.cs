using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum OverlayerObjectState
{
	InHand = 0,
	MovingToRest,
	MovingToHand,
    BeginCutting,
	OnCutting,
    PizzaToOven,
    PizzaToPlayer,
	Resting
}

public enum Items
{
    Knive = 0,
    CuttingTable,
    Oven,
    Pizza,
    Ingredient,
    Cursor
}

[System.Serializable]
public struct KniveOverlayerObject
{
	public Items name;
	public int id;
	public Transform overlayerTransform;
	public Transform restPos;
	public Transform handPos;
	public Transform cuttingRightUpperLimit;
	public Transform cuttingRightLowerLimit;
	public Transform cuttingLeftUpperLimit;
	public Transform cuttingLeftLowerLimit;
	public Transform cuttingUpperReference;
	public Transform cuttingLowerReference;
    public GameObject collider;
	public GameObject border;
}

[System.Serializable]

public class JointOverlayerPizzaMaker : MonoBehaviour
{
    [Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
    public GUITexture backgroundImage;

    [Tooltip("Filled circle sprite reference.")]
    public UISprite filledCircleSprite;

    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    public float cuttingSpeed = 10f;

	public KniveOverlayerObject kniveOverlayObject;
	OverlayerObjectState overlayerObjectState = OverlayerObjectState.Resting;

    private PizzaIngredientPerType currentPizzaIngredient;

    public PizzaIngredientPerType CurrentPizzaIngredient
    {
        get { return currentPizzaIngredient; }
    }

    private Quaternion initialRotation = Quaternion.identity;

    public delegate void OnBeginInteraction();
    public static event OnBeginInteraction OnInteraction;

    int cuttingCounter = 0;
    public float lerpAmount = 0.5f;
	float progress = 0f;
	bool handPositionInited;
    bool isIboxValid;
	bool hasOverlayerObject;
    bool circleScreenPosInited;
    bool hasCut;
    bool movingRight;
    Vector3 handPos = Vector3.zero;
    Vector3 spineMidPos = Vector3.zero;
    Vector3 dynamicIboxLeftBotBack = Vector3.zero;
    Vector3 dynamicIboxRightTopFront = Vector3.zero;
    Vector3 handSpineMidDistance = Vector3.zero;
    Vector3 IboxLeftBotBack = Vector3.zero;
    Vector3 handScreenPos = Vector3.zero;
    Vector3 circleScreenPos = Vector3.zero;
    Vector3 handMaxLimitPos = new Vector3(18.45f, 11.4f, -5.25f);
	Vector3 handMinLimitPos = new Vector3(-18.45f, -8.56f, -5.25f);
    Vector3 handMaxLimitPosIngredients = new Vector3(18.45f, 11.4f, -5.25f);
    Vector3 handMinLimitPosIngredients = new Vector3(-18.45f, -8.56f, -5.25f);
    Vector3 currentHandPos = new Vector3();
    Vector3 IboxRightTopFront = Vector3.zero;
    Vector3 referenceInitialPos = new Vector3();

	Transform initialRef;
	Transform finalRef;
    Transform rawPizzaInitRef;
    Transform rawPizzaFinalRef;
    Transform rawPizzaRef;
    Transform bakedPizzaInitRef;
    Transform bakedPizzaFinalRef;
    Transform bakedPizzaPlayerRef;
    Transform bakedPizzaRef;
    ItemState nextState;
    Items draggedItem;

    private Collider_ID currentCollider_ID;
    public Collider_ID CurrentCollider_ID

    {
        get { return currentCollider_ID; }
    }


    public Items DraggedItem
    {
        get { return draggedItem; }
        set { draggedItem = value; }
    }


    public OverlayerObjectState OverlayerObjectState 
	{
		get {
			return this.overlayerObjectState;
		}
		set {
			overlayerObjectState = value;
		}
	}

	private static JointOverlayerPizzaMaker instance;
	/// <summary>
	/// Gets the single JointOverlayerPizzaMaker instance.
	/// </summary>
	/// <value>The JointOverlayerPizzaMaker instance.</value>
	public static JointOverlayerPizzaMaker Instance
	{ 
		get
		{
			if (instance == null)
			{
				instance = (JointOverlayerPizzaMaker)GameObject.FindObjectOfType(typeof(JointOverlayerPizzaMaker));
			}
			return instance;
		}
	}

	void Start()
	{
		hasOverlayerObject = true;
        draggedItem = Items.Cursor;
        kniveOverlayObject.collider.GetComponent<PizzaObjectCollider>().OverlayerObject_ID(kniveOverlayObject.id);
	}

    void LateUpdate()
    {
		if (PizzaGameController.Instance.CurrentState != ItemState.Cursor) 
		{
            if (overlayerObjectState != OverlayerObjectState.Resting)
            {
			    switch (overlayerObjectState) 
			    {
			        case OverlayerObjectState.InHand: InHandMovement(); break;
			        case OverlayerObjectState.MovingToHand: TranslateObject (CalculatePosition (handScreenPos, ref currentHandPos), 2f); break;
			        case OverlayerObjectState.MovingToRest: TranslateObject (Vector3.zero, 2f); break;
			        case OverlayerObjectState.PizzaToOven: MoveBothPizzas(); break;
                    case OverlayerObjectState.PizzaToPlayer: FromOvenToPlayer(); break;
                    default: OnCuttingInteraction(); break;
			    }
            }
		} 
		else if (PizzaGameController.Instance.CurrentState == ItemState.Interacting) 
		{
			//TODO: Implement interaction
		}
    }

    Vector3 CalculatePosition(Vector3 screenNormalPos, ref Vector3 currentPos)
    {
        float height = 0f;

        if (draggedItem == Items.Knive)
        {
            currentPos.x = Mathf.Lerp(handMinLimitPos.x, handMaxLimitPos.x, screenNormalPos.x);
            currentPos.z = Mathf.Lerp(handMinLimitPos.z, handMaxLimitPos.z, screenNormalPos.z);
            height = Mathf.Lerp(handMinLimitPos.y, handMaxLimitPos.y, screenNormalPos.y);
            if (height >= 2.75f) currentPos.y = 2.75f;
		    else if (height <= -2.4f) currentPos.y = -2.4f;
		    else currentPos.y = height;
        }
        else if (draggedItem == Items.Ingredient)
        {
            currentPos.x = Mathf.Lerp(handMinLimitPosIngredients.x, handMaxLimitPosIngredients.x, screenNormalPos.x);
            currentPos.z = Mathf.Lerp(handMinLimitPosIngredients.z, handMaxLimitPosIngredients.z, screenNormalPos.z);
            height = Mathf.Lerp(handMinLimitPosIngredients.y, handMaxLimitPosIngredients.y, screenNormalPos.y);
            if (height >= 2.75f) currentPos.y = 2.75f;
            else if (height <= -1.6f) currentPos.y = -1.6f;
            else currentPos.y = height;
        }

        return currentPos;
    }

    void InHandMovement()
    {
        if (draggedItem == Items.Knive)
            kniveOverlayObject.overlayerTransform.localPosition = CalculatePosition(handScreenPos, ref currentHandPos);
        else if (draggedItem == Items.Ingredient)
            currentPizzaIngredient.prefab.transform.localPosition = CalculatePosition(handScreenPos, ref currentHandPos);

        FilledCircleInObjectMovement();
    }

    void FilledCircleInObjectMovement()
    {  
        if (circleScreenPos.y >= 0.5666f)
            circleScreenPos.y = 0.5666f;
        else if (circleScreenPos.y <= 0.3087f)
            circleScreenPos.y = 0.3087f;
                  
        Vector3 objectFilledSpritePosition = new Vector3(circleScreenPos.x * Screen.width - Screen.width / 2,
        circleScreenPos.y * Screen.height - Screen.height / 2, 0f);

        if (circleScreenPosInited)
            filledCircleSprite.transform.localPosition = objectFilledSpritePosition;
        else
            circleScreenPosInited = true;

    }

    void OnCuttingInteraction()
    {

        if (overlayerObjectState == OverlayerObjectState.BeginCutting)
        {
            if (TranslateObject(Vector3.zero, 1f))
            {
                InitializeUpperAndLowerReferences();

                initialRef = TransformLerp(kniveOverlayObject.cuttingUpperReference, kniveOverlayObject.cuttingRightUpperLimit, lerpAmount);
                finalRef = TransformLerp(kniveOverlayObject.cuttingLowerReference, kniveOverlayObject.cuttingRightLowerLimit, lerpAmount);
            }
        }
        else if (overlayerObjectState == OverlayerObjectState.OnCutting)
        {
            if (TranslateObject(Vector3.zero, cuttingSpeed))
            {
                if (lerpAmount <= 0f)
                {
                    lerpAmount = 0f;
                    movingRight = false;
                }
                else if (lerpAmount >= 1f)
                {
                    lerpAmount = 1f;
                    movingRight = true;
                }


                hasCut = !hasCut;

                InitializeUpperAndLowerReferences();

                if (hasCut)
                {
                    if (movingRight)
                        lerpAmount -= 0.1f;
                    else
                        lerpAmount += 0.1f;

                    if (lerpAmount < 0f)
                        lerpAmount = 0f;
                    else if (lerpAmount > 1f)
                        lerpAmount = 1f;

                    initialRef = TransformLerp(kniveOverlayObject.cuttingLowerReference, kniveOverlayObject.cuttingRightLowerLimit, lerpAmount);
                    finalRef = TransformLerp(kniveOverlayObject.cuttingUpperReference, kniveOverlayObject.cuttingRightUpperLimit, lerpAmount);
                }
                else
                {
                    initialRef = TransformLerp(kniveOverlayObject.cuttingUpperReference, kniveOverlayObject.cuttingRightUpperLimit, lerpAmount);
                    finalRef = TransformLerp(kniveOverlayObject.cuttingLowerReference, kniveOverlayObject.cuttingRightLowerLimit, lerpAmount);
                }
                cuttingCounter++;
                if (cuttingCounter >= 50)
                {
                    cuttingCounter = 0;
                    overlayerObjectState = OverlayerObjectState.MovingToHand;
                    initialRef = kniveOverlayObject.overlayerTransform;
                    finalRef = kniveOverlayObject.handPos;
                    finalRef.localPosition = Vector3.zero;
                    nextState = ItemState.Dragging;
                    PizzaGameController.Instance.CurrentState = ItemState.Changing;
                    kniveOverlayObject.overlayerTransform.GetComponent<BoxCollider>().enabled = true;
                }
            }
        }
    }

    void FromOvenToPlayer()
    {
        progress += Time.deltaTime;
        if (progress >= 1f)
        {
            progress = 0;
            bakedPizzaInitRef = TransformLerp(bakedPizzaRef, bakedPizzaPlayerRef, 1f);
            overlayerObjectState = OverlayerObjectState.Resting;
            Invoke("EnoughtStaringAtPizza", 3f);
        }
        else
            bakedPizzaInitRef = TransformLerp(bakedPizzaRef, bakedPizzaPlayerRef, progress);
    }

    void EnoughtStaringAtPizza()
    {
        PizzaGameController.Instance.ClearOven();
        PizzaGameController.Instance.CurrentState = nextState;
    }

    void MoveBothPizzas()
    {
        progress += Time.deltaTime;
        if (progress >= 1f)
        {
            progress = 0;
            rawPizzaInitRef = TransformLerp(rawPizzaRef, rawPizzaFinalRef, 1f);
            bakedPizzaInitRef = TransformLerp(bakedPizzaRef, bakedPizzaFinalRef, 1f);
            overlayerObjectState = OverlayerObjectState.Resting;
            PizzaGameController.Instance.CurrentState = nextState;
        }
        else
        {
            rawPizzaInitRef = TransformLerp(rawPizzaRef, rawPizzaFinalRef, progress);
            bakedPizzaInitRef = TransformLerp(bakedPizzaRef, bakedPizzaFinalRef, progress);
        }
    }

    void InitializeUpperAndLowerReferences()
    {
        kniveOverlayObject.cuttingLowerReference.localPosition = kniveOverlayObject.cuttingLeftLowerLimit.localPosition;
        kniveOverlayObject.cuttingLowerReference.localRotation = kniveOverlayObject.cuttingLeftLowerLimit.localRotation;

        kniveOverlayObject.cuttingUpperReference.localPosition = kniveOverlayObject.cuttingLeftUpperLimit.localPosition;
        kniveOverlayObject.cuttingUpperReference.localRotation = kniveOverlayObject.cuttingLeftUpperLimit.localRotation;
    }

    bool TranslateObject(Vector3 _currentHandPos, float progressMultiplier)
	{
		Quaternion rotate = new Quaternion();
		Vector3 translate = new Vector3();

		progress += Time.deltaTime * progressMultiplier;

		if (overlayerObjectState == OverlayerObjectState.MovingToHand) 
			finalRef.localPosition = _currentHandPos;

		if (progress >= 1f) 
		{
			rotate = Quaternion.Lerp(initialRef.localRotation, finalRef.localRotation, 1f);
			translate = Vector3.Lerp(initialRef.localPosition, finalRef.localPosition, 1f);
            if (draggedItem == Items.Knive)
            {
                kniveOverlayObject.overlayerTransform.localRotation = rotate;
                kniveOverlayObject.overlayerTransform.localPosition = translate;
            }
            else if (draggedItem == Items.Ingredient)
            {
                currentPizzaIngredient.prefab.transform.localRotation = rotate;
                currentPizzaIngredient.prefab.transform.localPosition = translate;
            }
            progress = 0f;
			PizzaGameController.Instance.CurrentState = nextState;

            switch (overlayerObjectState)
            {
                case OverlayerObjectState.MovingToRest:
                    overlayerObjectState = OverlayerObjectState.Resting;
                    draggedItem = Items.Cursor;
                    break;
                case OverlayerObjectState.MovingToHand:
                    overlayerObjectState = OverlayerObjectState.InHand; break;
                case OverlayerObjectState.BeginCutting:
                    overlayerObjectState = OverlayerObjectState.OnCutting; break;
                default: break;
            }

            return true;
		}

        rotate = Quaternion.Lerp(initialRef.localRotation, finalRef.localRotation, progress);
		translate = Vector3.Lerp(initialRef.localPosition, finalRef.localPosition, progress);

        if (draggedItem == Items.Knive)
        {
		    kniveOverlayObject.overlayerTransform.localRotation = rotate;
		    kniveOverlayObject.overlayerTransform.localPosition = translate;
        }
        else if (draggedItem == Items.Ingredient)
        {
            currentPizzaIngredient.prefab.transform.localRotation = rotate;
            currentPizzaIngredient.prefab.transform.localPosition = translate;
        }

        return false;
    }

    public void UpdateScreenPos(Vector3 _handScreenPos, Vector3 _circleScreenPos)
    {
        handScreenPos = _handScreenPos;
        circleScreenPos = _circleScreenPos;
    }

    public void SetCurrentOverlayerBorderCondition(bool codition)
    {
        kniveOverlayObject.border.SetActive(codition);
    }

    Transform TransformLerp(Transform initialReference, Transform finalReference, float _progress)
    {
        Transform result = initialReference;
        Vector3 initialPosReference = initialReference.localPosition;
        Vector3 finalPosReference = finalReference.localPosition;
        Quaternion initialRotationReference = initialReference.localRotation;
        Quaternion finalRotationReference = finalReference.localRotation;
        Vector3 stepLocalPosition = Vector3.Lerp(initialPosReference, finalPosReference, _progress);
        Quaternion stepLocalRotation = Quaternion.Lerp(initialRotationReference, finalRotationReference, _progress);
        result.localPosition = stepLocalPosition;
        result.localRotation = stepLocalRotation;

        return result;
    }

    public void InitializePizzasTransform(Transform _bakedPizzaRef, Transform _bakedPizzaInitRef, Transform _bakedPizzaFinalRef,
        Transform _bakedPizzaPlayerRef, Transform _rawPizzaRef, Transform _rawPizzaInitRef, Transform _rawPizzaFinalRef)
    {
        bakedPizzaRef = _bakedPizzaRef;
        bakedPizzaInitRef = _bakedPizzaInitRef;
        bakedPizzaFinalRef = _bakedPizzaFinalRef;
        bakedPizzaPlayerRef = _bakedPizzaPlayerRef;
        rawPizzaRef = _rawPizzaRef;
        rawPizzaInitRef = _rawPizzaInitRef;
        rawPizzaFinalRef = _rawPizzaFinalRef; 
    }

	public void OverlayerNextState(ItemState _nextState, Collider_ID collider_ID = Collider_ID.Knive)
	{
        currentCollider_ID = collider_ID;
        if (_nextState == ItemState.Dragging)
        {
            if (collider_ID == Collider_ID.Knive)
            {
                draggedItem = Items.Knive;
                initialRef = kniveOverlayObject.restPos;
                finalRef = kniveOverlayObject.handPos;
                overlayerObjectState = OverlayerObjectState.MovingToHand;
                finalRef.localPosition = Vector3.zero;
                nextState = _nextState;
                PizzaGameController.Instance.CurrentState = ItemState.Changing;
            }
        }
        else if (_nextState == ItemState.Interacting)
        {
            if (collider_ID == Collider_ID.CuttingTable)
            {
                if (overlayerObjectState == OverlayerObjectState.InHand)
                {
                    if (draggedItem == Items.Knive)
                    {
                        if (OnInteraction != null)
                            OnInteraction();
                        kniveOverlayObject.overlayerTransform.GetComponent<BoxCollider>().enabled = false;
                        lerpAmount = 0.5f;
                        overlayerObjectState = OverlayerObjectState.BeginCutting;
                        initialRef = kniveOverlayObject.overlayerTransform;

                        InitializeUpperAndLowerReferences();

                        finalRef = TransformLerp(kniveOverlayObject.cuttingUpperReference, kniveOverlayObject.cuttingRightUpperLimit, lerpAmount);
                        nextState = ItemState.Dragging;
                        PizzaGameController.Instance.CurrentState = _nextState;
                    }
                    else if (draggedItem == Items.Ingredient)
                    {
                        overlayerObjectState = OverlayerObjectState.MovingToRest;
                        initialRef = currentPizzaIngredient.prefab.transform;
                        currentPizzaIngredient.cuttingTableReference.localPosition = referenceInitialPos;
                        finalRef = currentPizzaIngredient.cuttingTableReference;
                        nextState = ItemState.Cursor;
                        PizzaGameController.Instance.CurrentState = ItemState.Changing;
                    }
                }
                else if (overlayerObjectState == OverlayerObjectState.Resting)
                {
                    if (OnInteraction != null)
                        OnInteraction();

                    draggedItem = Items.Ingredient;
                    currentPizzaIngredient = IngredientsController.Instance.OnTheTable;
                    initialRef = currentPizzaIngredient.prefab.transform;
                    handMaxLimitPosIngredients.z = initialRef.transform.localPosition.z;
                    handMinLimitPosIngredients.z = initialRef.transform.localPosition.z;
                    overlayerObjectState = OverlayerObjectState.MovingToHand;
                    finalRef = currentPizzaIngredient.cuttingTableReference;
                    referenceInitialPos = finalRef.localPosition;
                    nextState = ItemState.Dragging;
                    PizzaGameController.Instance.CurrentState = ItemState.Changing;
                }
            }
            else if (collider_ID == Collider_ID.RawPizza)
            {
                overlayerObjectState = OverlayerObjectState.PizzaToOven;
                nextState = ItemState.Cursor;
                PizzaGameController.Instance.CurrentState = ItemState.Changing;
            }
            else if (collider_ID == Collider_ID.Oven)
            {
                overlayerObjectState = OverlayerObjectState.PizzaToPlayer;
                nextState = ItemState.Cursor;
                PizzaGameController.Instance.CurrentState = ItemState.Changing;
            }
        }
        else if (_nextState == ItemState.Cursor)
        {
            if (collider_ID == Collider_ID.Knive)
            {
                overlayerObjectState = OverlayerObjectState.MovingToRest;
                initialRef = kniveOverlayObject.overlayerTransform;
                finalRef = kniveOverlayObject.restPos;
                nextState = _nextState;
                PizzaGameController.Instance.CurrentState = ItemState.Changing;
            }
            else if (collider_ID == Collider_ID.RawPizza)
            {
                draggedItem = Items.Cursor;
                overlayerObjectState = OverlayerObjectState.Resting;
                IngredientsController.Instance.ClearCuttingTable();
                currentPizzaIngredient.cuttingTableReference.localPosition = referenceInitialPos;
                currentPizzaIngredient.prefab.transform.localPosition = currentPizzaIngredient.cuttingTableReference.localPosition;
                IngredientsController.Instance.EnableIngredientInPizza(currentPizzaIngredient.ingredient);
                PizzaGameController.Instance.CurrentState = ItemState.Cursor;
                currentPizzaIngredient.cuttingTableReference.localPosition = referenceInitialPos;
                PizzaGameController.Instance.AddIngredientInPizza(currentPizzaIngredient);
            }
        }
	}
}