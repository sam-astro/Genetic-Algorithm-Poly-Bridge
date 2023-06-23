using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Entity : MonoBehaviour
{
    public Dictionary<Vector2, Point> allPoints = new Dictionary<Vector2, Point>();
    public Dictionary<Vector4, Bar> allBars = new Dictionary<Vector4, Bar>();
    [ShowOnly] public int dictSize = 0;
    [ShowOnly] public int barSize = 0;

    public NeuralNetwork net;

    public BarCreator barCreator;
    [ShowOnly] public int netID;
    [ShowOnly] public double totalFitness;
    [ShowOnly] public bool networkRunning = false;

    [ShowOnly] public double[] locations;

    int simulationWaitIterations = 0;

    int timeElapsed = 0;
    int totalIterations;

    int numPointsConnected = 0;

    public Transform trackerPiece;

    [Header("Fitness Config")]
    public bool useCube = false;
    public bool rewardSturdiness = false;
    public bool brokenPiecePenalty = false;
    public bool disconnectedPointPenalty = false;
    public bool rewardTimeAlive = false;

    private void Awake()
    {
        //allPoints.Clear();
        //Time.timeScale = 0;
    }

    [ContextMenu("Test Game Play")]
    public void Play()
    {
        //Time.timeScale = 1;
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
        barCreator.CreateBar(new Vector2(-4, 0), new Vector2(-2, 0)); // Recreate the first road as wood, to test if it creates (it should not)
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

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //        barCreator.barToInstantiate = barCreator.roadBar;
    //    else if (Input.GetKeyDown(KeyCode.Alpha2))
    //        barCreator.barToInstantiate = barCreator.woodBar;
    //}

    public List<List<Vector2>> barGenes = new List<List<Vector2>>();
    //[ContextMenu("Elapse Entity")]
    public bool Elapse()
    {
        if (networkRunning)
        {
            //net.pendingFitness += -trackerPiece.position.y/3f;

            foreach (Bar b in allBars.Values)
            {
                if (b.isBroken)
                    networkRunning = false;
            }

            // If failing, or this is the last possible iteration, end.
            if (/*trackerPiece.position.y <= -1f || */timeElapsed >= totalIterations - simulationWaitIterations - 1 || networkRunning == false)
            {
                End();
                return false;
            }
            ////GenerateBridge();
            //locations = net.mutatableVariables; // Converting it to vector2, so 0,1 is first location, 2,3 is second, etc.
            //Vector2 lastLocation = new Vector2(-4, 0); // Start point
            //for (int i = 0; i < locations.Length - 1; i++)
            //{
            //    Vector2 thisLocation = new Vector2((float)locations[i] * 5, (float)locations[i + 1] * 5); // Start point
            //    barCreator.CreateBar(lastLocation, lastLocation + thisLocation);
            //    lastLocation = thisLocation;
            //}
            ////foreach (List<Vector2> points in barGenes)
            ////{
            ////    barCreator.CreateBar(points[0], points[1]);
            ////}
            timeElapsed += simulationWaitIterations;
            return true;
        }
        else
            return false;
    }

    void End()
    {
        //if (trackerPiece.position.y <= -3)
        //    net.pendingFitness += -trackerPiece.position.y /3f;

        if (rewardTimeAlive)
            net.pendingFitness -= (float)timeElapsed / (float)totalIterations;

        // Compare the beginning and ending positions of all
        // of the points. We want this number to be as low as possible.
        float totalDist = 0f;
        foreach (Vector2 v in allPoints.Keys)
        {
            // Compare distance
            float dist = Vector2.Distance(v, allPoints[v].transform.position);
            totalDist += Mathf.Sqrt(dist);
        }
        totalDist /= (allPoints.Count - 3); // Average
        if (rewardSturdiness)
            net.pendingFitness += totalDist / 3f;


        // Also give a penalty for any broken bridge pieces / high stresses
        float totalStress = 0f;
        foreach (Bar b in allBars.Values)
        {
            totalStress += b.currentLoad;
        }
        totalStress /= allBars.Count; // Average
        if (brokenPiecePenalty)
            net.pendingFitness += totalStress;
    }

    public void Init(NeuralNetwork neti, int generation, int numberOfInputs, int totalIterations, int simulationWaitIterations, bool visible)
    {
        //transform.localPosition = Vector3.zero;
        this.net = neti;
        this.totalIterations = totalIterations;
        networkRunning = true;
        this.net.fitness += this.net.pendingFitness;
        this.net.pendingFitness = 0;
        //this.genome = net.genome;
        this.netID = net.netID;
        this.simulationWaitIterations = simulationWaitIterations;
        //this.netUI = netUI;
        //net.error = 0;
        timeElapsed = 0;
        //bestDistance = 10000;


        // Create bridge pieces
        locations = net.weights[0][0]; // Converting it to vector2, so 0,1 is first location, 2,3 is second, etc.

        // Roads
        Vector2 lastLocation = new Vector2(-4, 0); // Start point
        barCreator.barToInstantiate = barCreator.roadBar;
        for (int i = 0; i < (locations.Length - 2) / 2; i++)
        {
            Vector2 thisLocation = new Vector2Int((int)(locations[i] * 5d), (int)(locations[i + 1] * 5d)); // Start point
            if (thisLocation == Vector2.zero) // if location is 0,0 this means the net wants to skip it.
                continue;
            barCreator.CreateBar(lastLocation, lastLocation + thisLocation);
            lastLocation = lastLocation + thisLocation;
        }

        // Wood Supports
        //lastLocation = new Vector2(-4, 0); // Start point
        barCreator.barToInstantiate = barCreator.woodBar;
        for (int i = (locations.Length - 2) / 2; i < locations.Length - 2; i++)
        {
            Vector2 thisLocation = new Vector2Int((int)(locations[i] * 5d), (int)(locations[i + 1] * 5d)); // Start point
            if (thisLocation == Vector2.zero) // if location is 0,0 this means the net wants to skip it.
                continue;
            barCreator.CreateBar(lastLocation, lastLocation + thisLocation);
            lastLocation = lastLocation + thisLocation;
        }


        // Count the number of points that only have one or fewer bridge pieces connected
        List<Vector2> disconnectedPoints = new List<Vector2>();
        foreach (Point pt in allPoints.Values)
        {
            if (pt.connectedBars.Count != 0)
                numPointsConnected += 1;
            else
                disconnectedPoints.Add(pt.pointID);
        }

        // Find the distance from the disconnected points to their nearest point, and punish less the closer it is.
        float totalDists = 0f;
        foreach (Vector2 dsPt in disconnectedPoints)
        {
            float bestDist = float.MaxValue;
            foreach (Vector2 pt in allPoints.Keys)
            {
                if (pt != dsPt)
                { // Don't compare if it is the same one
                    float dist = Vector2.Distance(dsPt, pt);
                    if (dist < bestDist)
                        bestDist = dist;
                }
            }
            if (bestDist < float.MaxValue)
                totalDists += bestDist;
        }

        //// Add (punish) net for disconnected points
        //net.pendingFitness += (allPoints.Count - numPointsConnected) * 10;
        // Add (punish) net for disconnected points depending on how close the nearest one is.
        if (disconnectedPointPenalty)
            net.pendingFitness += totalDists / (float)(disconnectedPoints.Count);


        if (useCube)
            trackerPiece.gameObject.SetActive(true);
    }

    private void Update()
    {
        dictSize = allPoints.Count;
        barSize = allBars.Count;
    }
}
