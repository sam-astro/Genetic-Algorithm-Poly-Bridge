using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Dictionary<Vector2, Point> allPoints = new Dictionary<Vector2, Point>();

    private void Awake()
    {
        allPoints.Clear();
    }
}
