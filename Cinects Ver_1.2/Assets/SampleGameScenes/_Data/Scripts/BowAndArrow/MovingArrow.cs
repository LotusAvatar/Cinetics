using UnityEngine;
using System.Collections;

public class MovingArrow : MonoBehaviour
{
    public string tagName = "Target";
    public float speed = 1f;
    public Transform arrow;
    Vector3 direction;
    bool canMove;

    public void Initialize(Vector3 _direction)
    {
        direction = _direction;
        TurnTowardAimigPosition();
        canMove = true;
        Invoke("AutoDestruct", 4f);
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove == true)
        {
            transform.position += direction * speed;

            arrow.Rotate(Vector3.left, 4f);
        }
    }

    void TurnTowardAimigPosition()
    {
        Quaternion newRot = Quaternion.LookRotation(direction);

        if (newRot != transform.rotation)
        {
            transform.rotation = newRot;
        }
    }

    public void AutoDestruct()
    {
        Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == tagName)
        {
            CancelInvoke();
            canMove = false;
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
            hit.transform.parent.GetComponent<HexagonController>().UpdateScore();
            BowAndArrowController.Instance.AddProjectileInList(this);
        }
    }
}
