using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Bar : MonoBehaviour
{
    public float maxLength = 1;
    public Vector3 startPosition;
    public SpriteRenderer barSpriteRenderer;
    public BoxCollider2D boxCollider;
    public HingeJoint2D startJoint;
    public HingeJoint2D endJoint;

    public float startJointCurrentload = 0;
    public float endJointCurrentload = 0;

    public Color stressColor;
    private Color startColor;

    private SpriteRenderer sr;

    private Transform model;

    public bool isBroken = false;

    private void Awake()
    {
        model = transform.GetChild(0);
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;
    }

    public void UpdateCreatingBar(Vector3 toPosition)
    {
        transform.position = (toPosition + startPosition) / 2.0f;

        Vector2 dir = toPosition - startPosition;
        float angle = Vector2.SignedAngle(Vector2.right, dir);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        float length = dir.magnitude;
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
            sr.color = Color.Lerp(startColor, stressColor, maxLoad);
        }
        else
            sr.color = startColor;
    }

    private void Update()
    {
        if (Time.timeScale == 1 && !isBroken)
            UpdateMaterial();
    }
}
