using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[ExecuteInEditMode]
public class Point : MonoBehaviour
{
    public bool runtime = true;
    public List<Bar> connectBars;
    public Vector2 pointID;

    private void Start()
    {
        if(runtime == false)
        {
            pointID = transform.position;
            if(GameManager.allPoints.ContainsKey(pointID) == false)
            {
                GameManager.allPoints.Add(pointID, this);
            }
            runtime = true;
        }
    }

    private void Update()
    {
        if (runtime == false)
        {
            if (transform.hasChanged == true)
            {
                transform.hasChanged = false;
                transform.position = Vector3Int.RoundToInt(transform.position);
            }
        }
    }
}
