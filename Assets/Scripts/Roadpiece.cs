using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Roadpiece : MonoBehaviour
{
    public Vector3 startPoint;
    public Vector3 endPoint;

    public Transform modelEffects;
    public Transform modelFinal;

    public HingeJoint2D otherPiece;

    public float distance = 0;

    private Quaternion lookRotation;
    public Vector3 direction;

    //private void Awake()
    //{

    //}

    private void Awake()
    {
        transform.position = startPoint;

        distance = Vector3.Distance(startPoint, endPoint);

        modelEffects.localScale = new Vector3(modelEffects.localScale.x, distance, modelEffects.localScale.z);

        direction = (endPoint - startPoint).normalized;
        lookRotation = Quaternion.LookRotation(direction);
        lookRotation = Quaternion.Euler(lookRotation.eulerAngles.x + 90f, lookRotation.eulerAngles.y, 0f);
        modelEffects.rotation = lookRotation;

        // Apply transform to piece
        modelEffects.GetChild(0).parent = modelFinal;
        Destroy(modelEffects.gameObject);

        if (otherPiece != null)
        {
            //modelFinal.GetComponent<HingeJoint2D>().connectedBody = otherPiece.GetComponent<Rigidbody2D>();
            ////modelFinal.GetComponent<HingeJoint2D>().connectedAnchor = otherPiece.GetComponent<Roadpiece>().direction * otherPiece.GetComponent<Roadpiece>().distance;

            modelFinal.GetComponent<HingeJoint2D>().connectedBody = otherPiece.GetComponent<Rigidbody2D>();
            modelFinal.GetComponent<HingeJoint2D>().connectedAnchor = otherPiece.GetComponentInParent<Roadpiece>().direction * otherPiece.GetComponentInParent<Roadpiece>().distance;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startPoint, 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(endPoint, 0.2f);
    }
}
