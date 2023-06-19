using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public class BarCreator : MonoBehaviour, IPointerDownHandler
{
    bool barCreationStarted = false;
    public Bar currentBar;
    public GameObject barToInstantiate;
    public Transform barParent;
    public Point currentStartPoint;
    public GameObject pointToInstantiate;
    public Transform pointParent;
    public Point currentEndPoint;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (barCreationStarted == false)
        {
            barCreationStarted = true;
            StartBarCreation(Vector2Int.RoundToInt(Camera.main.ScreenToWorldPoint(eventData.position)));
        }
        else
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                FinishBarCreation();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                barCreationStarted = false;
                DeleteCurrentBar();
            }
        }
    }

    void StartBarCreation(Vector2 startPosition)
    {
        currentBar = Instantiate(barToInstantiate, barParent).GetComponent<Bar>();
        currentBar.startPosition = startPosition;

        if (GameManager.allPoints.ContainsKey(startPosition))
        {
            currentStartPoint = GameManager.allPoints[startPosition];
        }
        else
        {
            currentStartPoint = Instantiate(pointToInstantiate, startPosition, Quaternion.identity, pointParent).GetComponent<Point>();
            GameManager.allPoints.Add(startPosition, currentStartPoint);
        }

        currentEndPoint = Instantiate(pointToInstantiate, startPosition, Quaternion.identity, pointParent).GetComponent<Point>();
    }

    void FinishBarCreation()
    {
        if (GameManager.allPoints.ContainsKey(currentEndPoint.transform.position))
        {
            Destroy(currentEndPoint.gameObject);
            currentEndPoint = GameManager.allPoints[currentEndPoint.transform.position];
        }
        else
        {
            GameManager.allPoints.Add(currentEndPoint.transform.position, currentEndPoint);
        }

        currentStartPoint.connectBars.Add(currentBar);
        currentEndPoint.connectBars.Add(currentBar);
        StartBarCreation(currentEndPoint.transform.position);
    }

    void DeleteCurrentBar()
    {
        Destroy(currentBar.gameObject);
        if (currentStartPoint.connectBars.Count == 0 && currentStartPoint.runtime == true) Destroy(currentStartPoint.gameObject);
        if (currentEndPoint.connectBars.Count == 0 && currentEndPoint.runtime == true) Destroy(currentEndPoint.gameObject);
    }

    private void Update()
    {
        if (barCreationStarted)
        {
            Vector2 endPosition = (Vector2)Vector2Int.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Vector2 dir = endPosition - currentBar.startPosition;
            Vector2 clampPos = currentBar.startPosition + Vector2.ClampMagnitude(dir, currentBar.maxLength);

            currentEndPoint.transform.position = (Vector2)Vector2Int.FloorToInt(clampPos);
            currentEndPoint.pointID = currentEndPoint.transform.position;
            currentBar.UpdateCreatingBar(currentEndPoint.transform.position);
        }
    }
}
