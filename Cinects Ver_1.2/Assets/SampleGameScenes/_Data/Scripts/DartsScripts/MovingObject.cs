using UnityEngine;
using System.Collections;

public class MovingObject : MonoBehaviour
{
    public string tagName = "Target";
    public float speed = 1f;
    public float rotationSpeed = 0.1f;
    Vector3 direction;
    bool canMove;
    bool canRotate;

    public void Initialize(Vector3 _direction)
    {
        Debug.Log("direction: " + _direction);
        direction = _direction;
        canMove = true;
        canRotate = true;
        Invoke("AutoDestruct", 4f);
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove == true)
        {
            transform.position += direction * speed;
            TurnTowardNextPosition();
        }
    }

    void TurnTowardNextPosition()
    {
        if (!canRotate)
            return;

       Quaternion newRot = Quaternion.LookRotation(direction);

        if (newRot != transform.rotation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, rotationSpeed);
        }
        else
            canRotate = false;
    }

    public void AutoDestruct()
    {
        DirectionLauncher.Instance.ForceToReset();
        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == tagName)
        {
            CancelInvoke();
            canMove = false;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            hit.transform.parent.GetComponent<HexagonController>().UpdateScore();
            DirectionLauncher.Instance.AddProjectileInList(this);
        }
    }
}
