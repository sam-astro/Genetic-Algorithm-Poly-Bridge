using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Point : MonoBehaviour
{
    public bool runtime;
    public Rigidbody2D rb;
    public List<Bar> connectedBars;
    public Vector2 pointID;

    public Entity entity;

    private void Awake()
    {
        if(runtime == false)
        {
            rb.bodyType = RigidbodyType2D.Static;
            pointID = (Vector2)Vector2Int.RoundToInt(transform.localPosition);
            if(entity.allPoints.ContainsKey(pointID) == false)
            {
                entity.allPoints.Add(pointID, this);
            }
            runtime = true;
        }
    }

    public void RefreshID()
    {
        pointID = (Vector2)transform.localPosition;
    }

    //private void Update()
    //{
    //    if (runtime == false)
    //    {
    //        if (transform.hasChanged == true)
    //        {
    //            transform.hasChanged = false;
    //            transform.position = Vector3Int.RoundToInt(transform.position);
    //            pointID = transform.position;
    //        }
    //    }
    //}
}
