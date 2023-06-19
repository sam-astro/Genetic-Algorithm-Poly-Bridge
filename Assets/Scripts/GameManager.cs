using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Dictionary<Vector2, Point> allPoints = new Dictionary<Vector2, Point>();

    public BarCreator barCreator;

    private void Awake()
    {
        allPoints.Clear();
        Time.timeScale = 0;
    }

    [ContextMenu("Test Game Play")]
    public void Play()
    {
        Time.timeScale = 1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            barCreator.barToInstantiate = barCreator.roadBar;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            barCreator.barToInstantiate = barCreator.woodBar;
    }
}
