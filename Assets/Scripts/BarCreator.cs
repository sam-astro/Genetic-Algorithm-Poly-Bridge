using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using static UnityEditor.PlayerSettings;

public class BarCreator : MonoBehaviour
{
    public GameObject roadBar;
    public GameObject woodBar;
    bool barCreationStarted = false;
    [HideInInspector] public Bar currentBar;
    [HideInInspector] public GameObject barToInstantiate;
    public Transform barParent;
    [HideInInspector] public Point currentStartPoint;
    public GameObject pointToInstantiate;
    public Transform pointParent;
    [HideInInspector] public Point currentEndPoint;

    public Entity entity;

    Vector3 startDecimal;
    Vector3 endDecimal;

    public void CreateBar(Vector3 start, Vector3 end, Vector3 startDecimal, Vector3 endDecimal)
    {
        //// If bar already exists, don't create it
        //if (entity.allBars.ContainsKey(SortV4(start,end)))
        //    return;

        //Otherwise, it ok

        this.startDecimal = startDecimal;
        this.endDecimal = endDecimal;
        StartBarCreation(start);
        //currentBar.UpdateCreatingBar(end + transform.position);
        FinishBarCreation(end);

        //entity.allBars.Add(SortV4(start, end), currentBar);

        if (entity.allBars.ContainsKey(new Vector4(
            currentBar.startPos.x,
            currentBar.startPos.y,
            currentBar.endPos.x,
            currentBar.endPos.y)))
            DeleteCurrentBar();
        else
            //entity.allBars.Add(new Vector4(start.x, start.y, end.x, end.y), currentBar);
            //Add to dictionary when done
            entity.allBars.Add(
            new Vector4(
                currentBar.startPos.x,
                currentBar.startPos.y,
                currentBar.endPos.x,
                currentBar.endPos.y),
            currentBar
            );
    }

    Vector4 SortV4(Vector2 a, Vector2 b)
    {
        Vector4 o = Vector4.zero;

        if (a.magnitude < b.magnitude)
            o = new Vector4(a.x, a.y, b.x, b.y);
        else
            o = new Vector4(a.x, a.y, b.x, b.y);

        return o;
    }

    public void CreateBar(Vector3 start, Vector3 end)
    {
        this.startDecimal = start;
        this.endDecimal = end;
        StartBarCreation(start);
        //currentBar.UpdateCreatingBar(end + transform.position);
        FinishBarCreation(end);
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    if (barCreationStarted == false)
    //    {
    //        barCreationStarted = true;
    //        StartBarCreation((Vector3)(Vector2)Vector2Int.RoundToInt(Camera.main.ScreenToWorldPoint(eventData.position)));
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
        currentBar.startPosition = startPosition + transform.position;

        if (entity.allPoints.ContainsKey((Vector2)startPosition))
        {
            currentStartPoint = entity.allPoints[(Vector2)startPosition];
        }
        else
        {
            currentStartPoint = Instantiate(pointToInstantiate, currentBar.startPosition, Quaternion.identity, pointParent).GetComponent<Point>();
            entity.allPoints.Add((Vector2)startPosition, currentStartPoint);
            entity.originalUnroundedPoints.Add((Vector2)startPosition, startDecimal);
            currentStartPoint.entity = entity;
        }
        currentStartPoint.pointID = startPosition;

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
            entity.originalUnroundedPoints.Add(currentEndPoint.transform.position, endDecimal);
        }

        currentStartPoint.connectedBars.Add(currentBar);
        currentEndPoint.connectedBars.Add(currentBar);

        currentBar.startJoint.connectedBody = currentStartPoint.rb;
        currentBar.startJoint.anchor = currentBar.transform.InverseTransformPoint(currentBar.startPosition);
        currentBar.endJoint.connectedBody = currentEndPoint.rb;
        currentBar.endJoint.anchor = currentBar.transform.InverseTransformPoint(currentEndPoint.transform.position);

        StartBarCreation(currentEndPoint.transform.position);
    }

    void FinishBarCreation(Vector3 pos)
    {
        currentEndPoint.gameObject.transform.position = pos + transform.position;
        currentEndPoint.pointID = pos;
        currentBar.UpdateCreatingBar(currentEndPoint.transform.position);
        if (entity.allPoints.ContainsKey(currentEndPoint.transform.localPosition))
        {
            Destroy(currentEndPoint.gameObject);
            currentEndPoint = entity.allPoints[currentEndPoint.transform.localPosition];
        }
        else
        {
            entity.allPoints.Add(currentEndPoint.transform.localPosition, currentEndPoint);
            entity.originalUnroundedPoints.Add(currentEndPoint.transform.localPosition, endDecimal);
        }

        //// If bar already exists, don't create it
        //if (entity.allBars.ContainsKey(
        //    new Vector4(
        //        currentBar.startPosition.x,
        //        currentBar.startPosition.y,
        //        currentEndPoint.transform.position.x,
        //        currentEndPoint.transform.position.y
        //    )))
        //    DeleteCurrentBar();

        currentStartPoint.connectedBars.Add(currentBar);
        currentEndPoint.connectedBars.Add(currentBar);

        currentBar.startJoint.connectedBody = currentStartPoint.rb;
        currentBar.startJoint.anchor = currentBar.transform.InverseTransformPoint(currentBar.startPosition);
        currentBar.endJoint.connectedBody = currentEndPoint.rb;
        currentBar.endJoint.anchor = currentBar.transform.InverseTransformPoint(currentEndPoint.transform.position);

        //entity.allBars.Add(new Vector4(
        //    Mathf.Round(startDecimal.x),
        //    Mathf.Round(startDecimal.y),
        //    pos.x,
        //    pos.y
        //), currentBar);
    }

    void DeleteCurrentBar()
    {
        Destroy(currentBar.gameObject);
        if (currentStartPoint.connectedBars.Count == 0 && currentStartPoint.runtime == true) Destroy(currentStartPoint.gameObject);
        if (currentEndPoint.connectedBars.Count == 0 && currentEndPoint.runtime == true) Destroy(currentEndPoint.gameObject);
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
