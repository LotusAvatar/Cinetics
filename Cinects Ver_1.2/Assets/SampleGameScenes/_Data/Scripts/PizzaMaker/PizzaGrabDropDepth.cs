using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PizzaGrabDropDepth : MonoBehaviour
{
    [Tooltip("GUI-Text used to display information messages.")]
    public GUIText infoGuiText;

    // interaction manager reference
	private PizzaInteractionManager manager;

    // currently dragged object and its parameters
    private GameObject draggedObject;
	private PizzaObjectCollider draggedObjectReference;
    private float draggedObjectDepth;
    private Vector3 draggedObjectOffset;

    private bool hasReference;

	public static PizzaGrabDropDepth _instance;

	public static PizzaGrabDropDepth Instance
    {
        get
        {
            if (_instance == null)
            {
				_instance = (PizzaGrabDropDepth)GameObject.FindObjectOfType(typeof(PizzaGrabDropDepth));
            }
            return _instance;
        }
    }

    void Update()
    {
        // get the interaction manager instance
        if (manager == null)
			manager = PizzaInteractionManager.Instance;

        if (manager != null && manager.IsInteractionInited)
        {
			if (manager.IsHandClickDetected())
			{
				Vector3 screenNormalPos = Vector3.zero;
				Vector3 screenPixelPos = Vector3.zero;
				screenNormalPos = manager.GetHandScreenPos();
                
                // check if there is an underlying object to be selected
                if (screenNormalPos != Vector3.zero)
                {
                    // convert the normalized screen pos to pixel pos
                    screenPixelPos.x = (int)(screenNormalPos.x * Camera.main.pixelWidth);
                    screenPixelPos.y = (int)(screenNormalPos.y * Camera.main.pixelHeight);
                    Ray ray = Camera.main.ScreenPointToRay(screenPixelPos);

                    // check if there is an underlying objects
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.gameObject.tag == "KniveCollider")
                        {
                            draggedObjectReference = hit.collider.gameObject.GetComponent<PizzaObjectCollider>();

                            switch (PizzaGameController.Instance.CurrentState)
                            {
                                case ItemState.Cursor: draggedObjectReference.GetItem(); break;
                                case ItemState.Dragging: draggedObjectReference.ReleaseItem(); break;
                                default:
                                    break;
                            }
                        }
                        else if ((hit.collider.gameObject.tag == "RawPizzaCollider" &&
                            PizzaGameController.Instance.RawPizzaHasIngredient) || 
                            (hit.collider.gameObject.tag == "CuttingTableCollider" &&
                            PizzaGameController.Instance.CanGrabIngredient)  ||
                            (hit.collider.gameObject.tag == "OvenCollider" &&
                            !PizzaGameController.Instance.OvenIsEmpty) ||
                            hit.collider.gameObject.tag == "IngredientType" ||
                            hit.collider.gameObject.tag == "IngredientChoice")
                        {
                            draggedObjectReference = hit.collider.gameObject.GetComponent<PizzaObjectCollider>();
                            draggedObjectReference.InteractWithCollider();
						}
                    }
                }
            }
			else if (manager.GetLastHandEvent() == HandEventType.Release)
				draggedObjectReference.ReleaseItem();
        }
    }
}