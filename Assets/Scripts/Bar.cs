using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Bar : MonoBehaviour
{
    public float maxLength = 1;
    [HideInInspector] public Vector3 startPosition;
    SpriteRenderer barSpriteRenderer;
    public BoxCollider2D boxCollider;
    [HideInInspector] public HingeJoint2D startJoint;
    [HideInInspector] public HingeJoint2D endJoint;

    float startJointCurrentload = 0;
    float endJointCurrentload = 0;
    [ShowOnly] public float currentLoad;

    public Color stressColor;
    private Color startColor;

    private Transform model;

    [ShowOnly] public bool isBroken = false;

    [ShowOnly] public float length = 0f;
    public float costPerUnit = 1f;


    private void Awake()
    {
        model = transform.GetChild(0);
        barSpriteRenderer = GetComponent<SpriteRenderer>();
        startColor = barSpriteRenderer.color;
    }

    public void UpdateCreatingBar(Vector3 toPosition)
    {
        transform.position = (toPosition + startPosition) / 2.0f;

        Vector2 dir = toPosition - startPosition;
        float angle = Vector2.SignedAngle(Vector2.right, dir);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        length = dir.magnitude;
        barSpriteRenderer.size = new Vector2(length, barSpriteRenderer.size.y);

        if (boxCollider != null)
            boxCollider.size = barSpriteRenderer.size;

        model.localScale = new Vector3(length, 1, 1);
    }

    public void UpdateMaterial()
    {
        if (startJoint != null)
            startJointCurrentload = startJoint.reactionForce.magnitude / startJoint.breakForce;
        else
            isBroken = true;
        if (endJoint != null)
            endJointCurrentload = endJoint.reactionForce.magnitude / endJoint.breakForce;
        else
            isBroken = true;

        if (!isBroken)
        {
            float maxLoad = Mathf.Max(startJointCurrentload, endJointCurrentload);
            currentLoad = maxLoad;
            barSpriteRenderer.color = Color.Lerp(startColor, stressColor, maxLoad);
        }
        else
        {
            barSpriteRenderer.color = startColor;
            currentLoad = 1.0f;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 1 && !isBroken)
            UpdateMaterial();
    }
}
