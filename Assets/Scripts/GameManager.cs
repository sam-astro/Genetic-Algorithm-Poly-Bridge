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

    [ContextMenu("Generate Bridge")]
    public void GenerateBridge()
    {
        barCreator.barToInstantiate = barCreator.roadBar;
        barCreator.CreateBar(new Vector2(-4, 0), new Vector2(-2, 0));
        barCreator.CreateBar(new Vector2(-2, 0), new Vector2(0, 0));
        barCreator.CreateBar(new Vector2(0, 0), new Vector2(2, 0));
        barCreator.CreateBar(new Vector2(2, 0), new Vector2(4, 0));
        barCreator.barToInstantiate = barCreator.woodBar;
        barCreator.CreateBar(new Vector2(-4, 0), new Vector2(-3, 1));
        barCreator.CreateBar(new Vector2(-3, 1), new Vector2(-2, 0));
        barCreator.CreateBar(new Vector2(-2, 0), new Vector2(-1, 1));
        barCreator.CreateBar(new Vector2(-1, 1), new Vector2(0, 0));
        barCreator.CreateBar(new Vector2(0, 0), new Vector2(1, 1));
        barCreator.CreateBar(new Vector2(1, 1), new Vector2(2, 0));
        barCreator.CreateBar(new Vector2(2, 0), new Vector2(3, 1));
        barCreator.CreateBar(new Vector2(3, 1), new Vector2(4, 0));
        barCreator.CreateBar(new Vector2(-3, 1), new Vector2(-1, 1));
        barCreator.CreateBar(new Vector2(-1, 1), new Vector2(1, 1));
        barCreator.CreateBar(new Vector2(1, 1), new Vector2(3, 1));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            barCreator.barToInstantiate = barCreator.roadBar;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            barCreator.barToInstantiate = barCreator.woodBar;
    }
}
