using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Entity : MonoBehaviour
{
    public Dictionary<Vector2, Point> allPoints = new Dictionary<Vector2, Point>();
    public Dictionary<Vector2, Vector2> originalUnroundedPoints = new Dictionary<Vector2, Vector2>();
    public Dictionary<Vector4, Bar> allBars = new Dictionary<Vector4, Bar>();
    [ShowOnly] public int dictSize = 0;
    [ShowOnly] public int dict2Size = 0;
    [ShowOnly] public int barSize = 0;

    public NeuralNetwork net;

    public BarCreator barCreator;
    [ShowOnly] public int netID;
    [ShowOnly] public double totalFitness;
    [ShowOnly] public bool networkRunning = false;

    double[] locations;
    double[] types;

    public List<double> fitnessSources = new List<double>();

    int simulationWaitIterations = 0;

    [ShowOnly] public int timeElapsed = 0;
    int totalIterations;

    float bestCarDistance = 100000f;
    float initialCarDistance = 1f;

    int numPointsConnected = 0;

    public Transform cubePiece;
    public Transform car;
    IsColliding carColliding;
    public Transform ghostCar;
    public Transform endFlag;
    public Point startPoint;

    Rigidbody2D carRb;

    [Header("Fitness Config")]
    public bool useCube = false;
    public bool useCar = false;
    public bool rewardCarDistance = false;
    public bool useBestDistance = false;
    public bool rewardSturdiness = false;
    public bool stressAmountPenalty = false;
    public bool brokenPiecePenalty = false;
    public bool disconnectedPointPenalty = false;
    public bool rewardTimeAlive = false;
    public bool dontAllowBuildingInCarArea = false;
    public bool lowerCostBonus = false;
    public bool lowYPointsPenalty = false;
    public bool highYPointsPenalty = false;
    [Header("Other Config")]
    public bool useRelativeCoordinates = true;

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
        barCreator.CreateBar(new Vector2(-6, 0), new Vector2(-4, 0));
        barCreator.CreateBar(new Vector2(-4, 0), new Vector2(-2, 0));
        barCreator.CreateBar(new Vector2(-2, 0), new Vector2(0, 0));
        barCreator.CreateBar(new Vector2(0, 0), new Vector2(2, 0));
        barCreator.CreateBar(new Vector2(2, 0), new Vector2(4, 0));
        barCreator.CreateBar(new Vector2(4, 0), new Vector2(6, 0));
        //barCreator.barToInstantiate = barCreator.woodBar;
        //barCreator.CreateBar(new Vector2(-4, 0), new Vector2(-2, 0)); // Recreate the first road as wood, to test if it creates (it should not)
        //barCreator.CreateBar(new Vector2(-4, 0), new Vector2(-3, 1));
        //barCreator.CreateBar(new Vector2(-3, 1), new Vector2(-2, 0));
        //barCreator.CreateBar(new Vector2(-2, 0), new Vector2(-1, 1));
        //barCreator.CreateBar(new Vector2(-1, 1), new Vector2(0, 0));
        //barCreator.CreateBar(new Vector2(0, 0), new Vector2(1, 1));
        //barCreator.CreateBar(new Vector2(1, 1), new Vector2(2, 0));
        //barCreator.CreateBar(new Vector2(2, 0), new Vector2(3, 1));
        //barCreator.CreateBar(new Vector2(3, 1), new Vector2(4, 0));
        //barCreator.CreateBar(new Vector2(-3, 1), new Vector2(-1, 1));
        //barCreator.CreateBar(new Vector2(-1, 1), new Vector2(1, 1));
        //barCreator.CreateBar(new Vector2(1, 1), new Vector2(3, 1));
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

            dictSize = allPoints.Count;
            dict2Size = originalUnroundedPoints.Count;
            barSize = allBars.Count;

            if (useCar)
            {
                if (carColliding.isColliding)
                    carRb.velocity = new Vector2(Mathf.Lerp(carRb.velocity.x, 3f * (float)simulationWaitIterations, Time.deltaTime), carRb.velocity.y);
                ghostCar.position = car.position;
            }

            if (rewardCarDistance)
            {
                float dist = Mathf.Clamp(Vector2.Distance(car.transform.position, endFlag.transform.position), 0f, initialCarDistance) / initialCarDistance;

                if (useBestDistance)
                {
                    if (dist < bestCarDistance)
                        bestCarDistance = dist;
                }
                else
                    bestCarDistance = dist;
            }

            //net.pendingFitness += -cubePiece.position.y/3f;

            foreach (Bar b in allBars.Values)
            {
                if (b.isBroken)
                    networkRunning = false;
            }
            //foreach (Point p in allPoints.Values)
            //{
            //    if (p.transform.localPosition.y < -2.5f && p.pointID != (Vector2)Vector2Int.RoundToInt(p.transform.localPosition))
            //        networkRunning = false;
            //}

            // If failing, or this is the last possible iteration, end.
            if (car.position.y <= -2f || timeElapsed >= totalIterations - simulationWaitIterations - 1 || networkRunning == false || bestCarDistance <= 0.05f)
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
        if (car.position.y <= -2)
            net.pendingFitness += 0.5f;

        if (rewardCarDistance)
        {
            //net.pendingFitness += 1f - (Mathf.Clamp(car.transform.position.x, -5f, 5f) + 5f) / 10f;
            //fitnessSources.Add(1f - (Mathf.Clamp(car.transform.position.x, -5f, 5f) + 5f) / 10f);
            net.pendingFitness += bestCarDistance*3f;

            if (bestCarDistance <= 0.05f)
            {
                net.pendingFitness -= 1;
                timeElapsed = totalIterations;
                fitnessSources.Add(-1);
            }
            else
                fitnessSources.Add(bestCarDistance);
        }

        if (rewardTimeAlive)
        {
            net.pendingFitness += (1f - (float)timeElapsed / (float)totalIterations) * 3f;
            fitnessSources.Add((1f - (float)timeElapsed / (float)totalIterations) * 3f);
        }

        // Compare the beginning and ending positions of all
        // of the points. We want this number to be as low as possible.
        float totalDist = 0f;
        foreach (Vector2 v in allPoints.Keys)
        {
            // Compare distance
            float dist = Vector2.Distance(v, allPoints[v].transform.localPosition);
            totalDist += dist;
        }
        //totalDist /= (float)(allPoints.Count - 3); // Average
        if (rewardSturdiness)
        {
            //net.pendingFitness += totalDist / 20f;
            //fitnessSources.Add(totalDist / 20f);
            net.pendingFitness += totalDist / 10f * (1f - (float)timeElapsed / (float)totalIterations);
            fitnessSources.Add(totalDist / 10f * (1f - (float)timeElapsed / (float)totalIterations));
        }


        // Also give a penalty for any high stresses (not including broken)
        float totalStress = 0f;
        foreach (Bar b in allBars.Values)
        {
            if (b.isBroken == false)
                totalStress += b.currentLoad;
        }
        totalStress /= (float)(allBars.Count); // Average
        if (stressAmountPenalty)
        {
            net.pendingFitness += totalStress;
            fitnessSources.Add(totalStress);
        }

        // Also give a penalty for any broken bridge pieces
        float totalBroken = 0f;
        foreach (Bar b in allBars.Values)
        {
            if (b.isBroken)
                totalBroken += 1;
        }
        //totalBroken /= (float)(allBars.Count); // Average
        if (brokenPiecePenalty)
        {
            net.pendingFitness += totalBroken / 10f;
            fitnessSources.Add(totalBroken / 10f);
        }

        networkRunning = false;
    }

    public void Init(NeuralNetwork neti, int generation, int numberOfInputs, int totalIterations, int simulationWaitIterations, bool visible, float maxSegLength, float outputMultiplier)
    {
        //transform.localPosition = Vector3.zero;
        this.net = neti;
        this.totalIterations = totalIterations;
        networkRunning = true;
        this.net.fitness += this.net.pendingFitness;
        this.net.pendingFitness = 0;
        this.bestCarDistance = 10000f;
        fitnessSources = new List<double>();
        //this.genome = net.genome;
        this.netID = net.netID;
        this.simulationWaitIterations = simulationWaitIterations;
        carColliding = car.GetComponent<IsColliding>();
        //this.netUI = netUI;
        //net.error = 0;
        timeElapsed = 0;
        //bestDistance = 10000;

        initialCarDistance = Vector2.Distance(car.transform.position, endFlag.transform.position);

        if (useRelativeCoordinates)
            this.net.useRelativeCoordinates = true;


        // Create bridge pieces
        locations = net.weights[0][0]; // Converting it to vector2, so 0,1 is first location, 2,3 is second, etc.
        types = net.weights[0][1]; // If type is between -1 and 0 it is road, 0 and 1 is wood

        Vector2 lastLocation = startPoint.pointID; // Start point
        Vector2 lastLastLocation = startPoint.pointID; // Start point
        bool isNegativeX = false;
        bool isNegativeY = false;
        bool useNegativeAsDirection = false;

        // If creating relative bridge pieces
        if (useRelativeCoordinates)
        {
            for (int i = 0; i < locations.Length - 2; i += 2)
            {
                if (types[i / 2] < 0) // If type is between -1 and 0 it is road
                    barCreator.barToInstantiate = barCreator.roadBar;
                else if (types[i/2] > 0) // If type is between 0 and 1 it is wood
                    barCreator.barToInstantiate = barCreator.woodBar;

                float xVal = useNegativeAsDirection ? Mathf.Abs((float)locations[i]) : (float)locations[i];
                float yVal = useNegativeAsDirection ? Mathf.Abs((float)locations[i + 1]) : (float)locations[i + 1];
                Vector2 thisLocation = new Vector2((xVal * outputMultiplier) * (isNegativeX && useNegativeAsDirection ? -1f : 1f), (yVal * outputMultiplier) * (isNegativeY && useNegativeAsDirection ? -1f : 1f)); // Start point

                if (dontAllowBuildingInCarArea)
                    if (lastLocation.x + thisLocation.x < startPoint.pointID.x) // Make sure no pieces are placed in the car zone
                        thisLocation = new Vector2(thisLocation.x + (startPoint.pointID.x - (lastLocation.x + thisLocation.x)), thisLocation.y);

                // Make sure distance is not greater than allowed value
                if (thisLocation.magnitude > maxSegLength)
                {
                    thisLocation = (Vector2)Vector2Int.FloorToInt(thisLocation.normalized * maxSegLength); // scale to the maxlength
                }

                if (Vector2Int.RoundToInt(thisLocation).magnitude < 1)
                { // if length is 0 this means the net wants to skip it.
                    lastLocation = lastLastLocation;
                    continue;
                }
                if (locations[i] < 0)
                    isNegativeX = !isNegativeX;
                if (locations[i + 1] < 0)
                    isNegativeY = !isNegativeY;
                barCreator.CreateBar((Vector2)(Vector2Int.RoundToInt(lastLocation)), (Vector2)(Vector2Int.RoundToInt(lastLocation + thisLocation)), lastLocation, thisLocation);
                lastLastLocation = lastLocation;
                lastLocation = lastLocation + thisLocation;
            }
        }
        // Else, use coordinates literally, as world coords
        else
        {
            for (int i = 0; i < locations.Length - 2; i += 2)
            {
                if (types[i/2] < 0) // If type is between -1 and 0 it is road
                    barCreator.barToInstantiate = barCreator.roadBar;
                else if (types[i / 2] > 0) // If type is between 0 and 1 it is wood
                    barCreator.barToInstantiate = barCreator.woodBar;

                float xVal = useNegativeAsDirection ? Mathf.Abs((float)locations[i]) : (float)locations[i];
                float yVal = useNegativeAsDirection ? Mathf.Abs((float)locations[i + 1]) : (float)locations[i + 1];
                Vector2 thisLocation = new Vector2((xVal * outputMultiplier) * (isNegativeX && useNegativeAsDirection ? -1f : 1f), (yVal * outputMultiplier) * (isNegativeY && useNegativeAsDirection ? -1f : 1f)); // Start point

                if (dontAllowBuildingInCarArea)
                    if (thisLocation.x < startPoint.pointID.x) // Make sure no pieces are placed in the car zone
                        thisLocation = new Vector2(startPoint.pointID.x, thisLocation.y);

                // Make sure distance is not greater than allowed value
                if (Vector2.Distance(thisLocation, lastLocation) >= maxSegLength)
                {
                    float mag = Vector2.Distance(thisLocation, lastLocation);
                    thisLocation -= lastLocation;
                    thisLocation /= mag; // normalize to a length of 1
                    thisLocation *= maxSegLength; // scale to the maxlength
                    thisLocation = (Vector2)Vector2Int.FloorToInt(thisLocation);
                    thisLocation += lastLocation;
                }


                if (Vector2.Distance(thisLocation, lastLocation) < 0.5f)
                { // if length is 0 this means the net wants to skip it.
                    lastLocation = lastLastLocation;
                    continue;
                }
                if (locations[i] < 0)
                    isNegativeX = !isNegativeX;
                if (locations[i + 1] < 0)
                    isNegativeY = !isNegativeY;
                barCreator.CreateBar((Vector2)(Vector2Int.RoundToInt(lastLocation)), (Vector2)(Vector2Int.RoundToInt(thisLocation)), lastLocation, thisLocation);
                lastLastLocation = lastLocation;
                lastLocation = thisLocation;
            }
        }

        // If there were no bars created, add penalty
        if (allBars.Count == 0)
            net.fitness += 10f;


        // Count the number of points that only have one or fewer bridge pieces connected
        List<Vector2> disconnectedPoints = new List<Vector2>();
        foreach (Point pt in allPoints.Values)
        {
            if (pt.connectedBars.Count == 0)
                disconnectedPoints.Add(pt.pointID);
            else
                numPointsConnected += 1;
        }

        // Find the distance from the disconnected points to their nearest point, and punish less the closer it is.
        float totalDists = 0f;
        foreach (Vector2 dsPt in disconnectedPoints)
        {
            float bestDist = 9999;
            foreach (Vector2 pt in originalUnroundedPoints.Values)
            {
                float dist = Vector2.Distance(dsPt, pt);
                if (dist < bestDist)
                    bestDist = dist * dist;
            }
            if (bestDist != 9999)
                totalDists += bestDist;
        }

        // Add (punish) net for disconnected points depending on how close the nearest one is.
        if (disconnectedPointPenalty && disconnectedPoints.Count > 0)
        {
            net.pendingFitness += totalDists / 3f;
            fitnessSources.Add(totalDists / 3f);
        }
        else if (disconnectedPointPenalty)
        {
            net.pendingFitness += -0.1f;
            fitnessSources.Add(-0.1f);
        }

        // Add extra for the cost of the bridge pieces
        if (lowerCostBonus)
        {
            float totalCost = 0f;
            foreach (Bar b in allBars.Values)
            {
                totalCost += b.length * b.costPerUnit;
            }
            totalCost /= (float)(allBars.Count) * 400f; // Average

            net.pendingFitness += totalCost/2f;
            fitnessSources.Add(totalCost/2f);
        }

        // Add extra for low point Y positions
        if (lowYPointsPenalty)
        {
            float totalDist = 0f;
            foreach (Vector2 b in allPoints.Keys)
            {
                if (b.y < 1)
                    totalDist -= b.y;
            }
            totalDist /= (float)(allPoints.Count); // Average

            net.pendingFitness += totalDist;
            fitnessSources.Add(totalDist);
        }
        // Add extra for high point Y positions
        if (highYPointsPenalty)
        {
            float totalDist = 0f;
            foreach (Vector2 b in allPoints.Keys)
            {
                if (b.y >= 2)
                    totalDist += b.y - 2f;
            }
            totalDist /= (float)(allPoints.Count); // Average

            net.pendingFitness += totalDist;
            fitnessSources.Add(totalDist);
        }


        if (useCube)
            cubePiece.gameObject.SetActive(true);

        if (useCar)
        {
            car.gameObject.SetActive(true);
            ghostCar.gameObject.SetActive(true);
            carRb = car.GetComponent<Rigidbody2D>();
        }
    }

    private void FixedUpdate()
    {
    }
}
