using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public class BarCreator : MonoBehaviour
{
    public GameObject roadBar;
    public GameObject woodBar;
    bool barCreationStarted = false;
    public Bar currentBar;
    [HideInInspector]
    public GameObject barToInstantiate;
    public Transform barParent;
    public Point currentStartPoint;
    public GameObject pointToInstantiate;
    public Transform pointParent;
    public Point currentEndPoint;

    public Entity entity;

    public void CreateBar(Vector3 start, Vector3 end)
    {
        StartBarCreation(Vector3Int.RoundToInt(start));
        //currentBar.UpdateCreatingBar(end + transform.position);
        FinishBarCreation(end);
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    if (barCreationStarted == false)
    //    {
    //        barCreationStarted = true;
    //        StartBarCreation(Vector2Int.RoundToInt(Camera.main.ScreenToWorldPoint(eventData.position)));
    //    }
    //    else
    //    {
    //        if (eventData.button == PointerEventData.InputButton.Left)
    //        {
    //            FinishBarCreation();
    //        }
    //        else if (eventData.button == PointerEventData.InputButton.Right)
    //        {
    //            barCreationStarted = false;
    //            DeleteCurrentBar();
    //        }
    //    }
    //}

    void StartBarCreation(Vector3 startPosition)
    {
        currentBar = Instantiate(barToInstantiate, barParent).GetComponent<Bar>();
        currentBar.startPosition = startPosition+transform.position;

        if (entity.allPoints.ContainsKey((Vector2)startPosition))
        {
            currentStartPoint = entity.allPoints[(Vector2)startPosition];
        }
        else
        {
            currentStartPoint = Instantiate(pointToInstantiate, currentBar.startPosition, Quaternion.identity, pointParent).GetComponent<Point>();
            entity.allPoints.Add((Vector2)startPosition, currentStartPoint);
            currentStartPoint.entity = entity;
        }
        currentStartPoint.pointID = Vector2Int.RoundToInt(currentStartPoint.transform.position);

        currentEndPoint = Instantiate(pointToInstantiate, currentBar.startPosition, Quaternion.identity, pointParent).GetComponent<Point>();
        currentEndPoint.entity = entity;
    }

    void FinishBarCreation()
    {
        if (entity.allPoints.ContainsKey(currentEndPoint.transform.position))
        {
            Destroy(currentEndPoint.gameObject);
            currentEndPoint = entity.allPoints[currentEndPoint.transform.position];
        }
        else
        {
            entity.allPoints.Add(currentEndPoint.transform.position, currentEndPoint);
        }

        currentStartPoint.connectBars.Add(currentBar);
        currentEndPoint.connectBars.Add(currentBar);

        currentBar.startJoint.connectedBody = currentStartPoint.rb;
        currentBar.startJoint.anchor = currentBar.transform.InverseTransformPoint(currentBar.startPosition);
        currentBar.endJoint.connectedBody = currentEndPoint.rb;
        currentBar.endJoint.anchor = currentBar.transform.InverseTransformPoint(currentEndPoint.transform.position);

        StartBarCreation(currentEndPoint.transform.position);
    }

    void FinishBarCreation(Vector3 pos)
    {
        currentEndPoint.gameObject.transform.position = pos+transform.position;
        currentEndPoint.pointID = Vector2Int.RoundToInt(currentEndPoint.transform.position);
        currentBar.UpdateCreatingBar(currentEndPoint.transform.position);
        if (entity.allPoints.ContainsKey(currentEndPoint.transform.localPosition))
        {
            Destroy(currentEndPoint.gameObject);
            currentEndPoint = entity.allPoints[currentEndPoint.transform.localPosition];
        }
        else
        {
            entity.allPoints.Add(currentEndPoint.transform.localPosition, currentEndPoint);
        }

        currentStartPoint.connectBars.Add(currentBar);
        currentEndPoint.connectBars.Add(currentBar);

        currentBar.startJoint.connectedBody = currentStartPoint.rb;
        currentBar.startJoint.anchor = currentBar.transform.InverseTransformPoint(currentBar.startPosition);
        currentBar.endJoint.connectedBody = currentEndPoint.rb;
        currentBar.endJoint.anchor = currentBar.transform.InverseTransformPoint(currentEndPoint.transform.position);

    }

    void DeleteCurrentBar()
    {
        Destroy(currentBar.gameObject);
        if (currentStartPoint.connectBars.Count == 0 && currentStartPoint.runtime == true) Destroy(currentStartPoint.gameObject);
        if (currentEndPoint.connectBars.Count == 0 && currentEndPoint.runtime == true) Destroy(currentEndPoint.gameObject);
    }

    //private void Update()
    //{
    //    if (barCreationStarted)
    //    {
    //        Vector2 endPosition = (Vector2)Vector2Int.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    //        Vector2 dir = endPosition - currentBar.startPosition;
    //        Vector2 clampPos = currentBar.startPosition + Vector2.ClampMagnitude(dir, currentBar.maxLength);

    //        currentEndPoint.transform.position = (Vector2)Vector2Int.FloorToInt(clampPos);
    //        currentEndPoint.pointID = Vector2Int.RoundToInt(currentEndPoint.transform.position);
    //        currentBar.UpdateCreatingBar(currentEndPoint.transform.position);
    //    }
    //}
}
