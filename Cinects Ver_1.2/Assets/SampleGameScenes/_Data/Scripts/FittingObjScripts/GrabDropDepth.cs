using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrabDropDepth : MonoBehaviour
{
    [Tooltip("Material used to outline the currently selected object.")]
    public Material selectedObjectMaterial;

    [Tooltip("Speed of dragging of the selected object.")]
    public float dragSpeed = 3.0f;

    [Tooltip("Choose to move de objects depth")]
    public bool useDepth = false;

    [Tooltip("GUI-Text used to display information messages.")]
    public GUIText infoGuiText;

    // interaction manager reference
    private InteractionManager manager;
    private bool isLeftHandDrag;

    // currently dragged object and its parameters
    private GameObject draggedObject;
    private FittingObjectCollider draggedObjectReference;
    private float draggedObjectDepth;
    private Vector3 draggedObjectOffset;

    private bool hasReference;

    public static GrabDropDepth _instance;

    public static GrabDropDepth Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (GrabDropDepth)GameObject.FindObjectOfType(typeof(GrabDropDepth));
            }
            return _instance;
        }
    }

    void Update()
    {
        // get the interaction manager instance
        if (manager == null)
        {
            manager = InteractionManager.Instance;
        }

        if (manager != null && manager.IsInteractionInited())
        {
            Vector3 screenNormalPos = Vector3.zero;
            Vector3 screenPixelPos = Vector3.zero;

            if (draggedObject == null)
            {
                // if there is a hand grip, select the underlying object and start dragging it.
                if (manager.IsLeftHandPrimary())
                {
                    // if the left hand is primary, check for left hand grip
                    if (manager.GetLastLeftHandEvent() == InteractionManager.HandEventType.Grip)
                    {
                        isLeftHandDrag = true;
                        screenNormalPos = manager.GetLeftHandScreenPos();
                    }
                }
                else if (manager.IsRightHandPrimary())
                {
                    // if the right hand is primary, check for right hand grip
                    if (manager.GetLastRightHandEvent() == InteractionManager.HandEventType.Grip)
                    {
                        isLeftHandDrag = false;
                        screenNormalPos = manager.GetRightHandScreenPos();
                    }
                }

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
                        if (hit.collider.gameObject.tag == "Draggable")
                        {
                            // an object was hit by the ray. select it and start drgging
                            draggedObjectReference = hit.collider.gameObject.GetComponent<FittingObjectCollider>();
                            draggedObject = draggedObjectReference.HasBeenSelectet();
                            hasReference = true;
                            draggedObjectDepth = draggedObject.transform.position.z - Camera.main.transform.position.z;
                            draggedObjectOffset = hit.point - draggedObject.transform.position;
                        }
                    }
                }
            }
            else
            {
                // continue dragging the object
                screenNormalPos = isLeftHandDrag ? manager.GetLeftHandScreenPos() : manager.GetRightHandScreenPos();

                // convert the normalized screen pos to 3D-world pos
                screenPixelPos.x = (int)(screenNormalPos.x * Camera.main.pixelWidth);
                screenPixelPos.y = (int)(draggedObjectReference.MyHeight() * Camera.main.pixelHeight);
                screenPixelPos.z = screenNormalPos.z + draggedObjectDepth;

                Vector3 newObjectPos = Camera.main.ScreenToWorldPoint(screenPixelPos) - draggedObjectOffset;
                newObjectPos.y = draggedObject.transform.position.y;

                if (!useDepth)
                    newObjectPos.z = 0f;

                draggedObject.transform.position = Vector3.Lerp(draggedObject.transform.position, newObjectPos, dragSpeed * Time.deltaTime);

                // check if the object (hand grip) was released
                bool isReleased = isLeftHandDrag ? (manager.GetLastLeftHandEvent() == InteractionManager.HandEventType.Release) :
                    (manager.GetLastRightHandEvent() == InteractionManager.HandEventType.Release);

                if (isReleased)
                {
                    if (hasReference)
                    {
                        hasReference = false;
                        draggedObjectReference.HasBeenReleased();
                    }
                    
                    draggedObject = null;
                }
            }
        }
    }

    void OnGUI()
    {
        if (infoGuiText != null && manager != null && manager.IsInteractionInited())
        {
            string sInfo = string.Empty;

            long userID = manager.GetUserID();
            if (userID != 0)
            {
                if (draggedObject != null)
                    sInfo = "Dragging the " + draggedObject.name + " around.";
                else
                    sInfo = "Please grab and drag an object around.";
            }
            else
            {
                KinectManager kinectManager = KinectManager.Instance;

                if (kinectManager && kinectManager.IsInitialized())
                {
                    sInfo = "Waiting for Users...";
                }
                else
                {
                    sInfo = "Kinect is not initialized. Check the log for details.";
                }
            }

            infoGuiText.GetComponent<GUIText>().text = sInfo;
        }
    }
}
