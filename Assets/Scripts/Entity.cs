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

    int timeElapsed = 0;
    int totalIterations;

    float bestCarDistance = 100000f;

    int numPointsConnected = 0;

    public Transform cubePiece;
    public Transform car;
    public Transform ghostCar;
    public Transform endFlag;

    Rigidbody2D carRb;

    [Header("Fitness Config")]
    public bool useCube = false;
    public bool useCar = false;
    public bool rewardCarDistance = false;
    public bool useBestDistance = false;
    public bool rewardSturdiness = false;
    public bool brokenPiecePenalty = false;
    public bool disconnectedPointPenalty = false;
    public bool rewardTimeAlive = false;
    public bool dontAllowBuildingInCarArea = false;
    public bool lowerCostBonus = false;
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
        //barCreator.barToInstantiate = barCreator.roadBar;
        //barCreator.CreateBar(new Vector2(-4, 0), new Vector2(-2, 0));
        //barCreator.CreateBar(new Vector2(-2, 0), new Vector2(0, 0));
        //barCreator.CreateBar(new Vector2(0, 0), new Vector2(2, 0));
        //barCreator.CreateBar(new Vector2(2, 0), new Vector2(4, 0));
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
                carRb.velocity = new Vector2(Mathf.Lerp(carRb.velocity.x, 3f * (float)simulationWaitIterations, Time.deltaTime), carRb.velocity.y);
                ghostCar.position = car.position;
            }

            if (rewardCarDistance)
            {
                float dist = Mathf.Clamp(Vector2.Distance(car.transform.position, endFlag.transform.position), 0f, 15f) / 15f;

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
            net.pendingFitness += bestCarDistance;
            fitnessSources.Add(bestCarDistance);

            if (bestCarDistance <= 0.05f)
            {
                net.pendingFitness -= 1;
                timeElapsed = totalIterations;
            }
        }

        if (rewardTimeAlive)
        {
            net.pendingFitness += 1f - (float)timeElapsed / (float)totalIterations;
            fitnessSources.Add(1f - (float)timeElapsed / (float)totalIterations);
        }

        // Compare the beginning and ending positions of all
        // of the points. We want this number to be as low as possible.
        float totalDist = 0f;
        foreach (Vector2 v in allPoints.Keys)
        {
            // Compare distance
            float dist = Vector2.Distance(v, allPoints[v].transform.localPosition);
            totalDist += dist * dist;
        }
        totalDist /= (float)(allPoints.Count - 3); // Average
        if (rewardSturdiness)
        {
            net.pendingFitness += totalDist / 10f * (1f-(float)timeElapsed / (float)totalIterations);
            fitnessSources.Add(totalDist / 10f * (1f-(float)timeElapsed / (float)totalIterations));
        }


        // Also give a penalty for any broken bridge pieces / high stresses
        float totalStress = 0f;
        foreach (Bar b in allBars.Values)
        {
            totalStress += b.currentLoad;
        }
        totalStress /= (float)(allBars.Count); // Average
        if (brokenPiecePenalty)
        {
            net.pendingFitness += totalStress * 2f;
            fitnessSources.Add(totalStress * 2f);
        }

        networkRunning = false;
    }

    public void Init(NeuralNetwork neti, int generation, int numberOfInputs, int totalIterations, int simulationWaitIterations, bool visible)
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
        //this.netUI = netUI;
        //net.error = 0;
        timeElapsed = 0;
        //bestDistance = 10000;

        if (useRelativeCoordinates)
            this.net.useRelativeCoordinates = true;


        // Create bridge pieces
        locations = net.weights[0][0]; // Converting it to vector2, so 0,1 is first location, 2,3 is second, etc.
        types = net.weights[0][1]; // If type is between -1 and 0 it is road, 0 and 1 is wood

        Vector2 lastLocation = new Vector2(-4, 0); // Start point
        bool isNegativeX = false;
        bool isNegativeY = false;
        bool useNegativeAsDirection = false;
        float maxLength = 3f;
        float outputMultiplier = 1f;

        // If creating relative bridge pieces
        if (useRelativeCoordinates)
        {
            for (int i = 0; i < locations.Length - 2; i++)
            {
                if (types[i] < 0) // If type is between -1 and 0 it is road
                    barCreator.barToInstantiate = barCreator.roadBar;
                else if (types[i] > 0) // If type is between 0 and 1 it is wood
                    barCreator.barToInstantiate = barCreator.woodBar;

                float xVal = useNegativeAsDirection ? Mathf.Abs((float)locations[i]) : (float)locations[i];
                float yVal = useNegativeAsDirection ? Mathf.Abs((float)locations[i + 1]) : (float)locations[i + 1];
                Vector2 thisLocation = new Vector2((xVal * outputMultiplier) * (isNegativeX && useNegativeAsDirection ? -1f : 1f), (yVal * outputMultiplier) * (isNegativeY && useNegativeAsDirection ? -1f : 1f)); // Start point

                // Make sure distance is not greater than allowed value
                if (thisLocation.magnitude > maxLength)
                {
                    thisLocation /= thisLocation.magnitude; // normalize to a length of 1
                    thisLocation *= maxLength; // scale to the maxlength
                }

                if (dontAllowBuildingInCarArea)
                    if (lastLocation.x + thisLocation.x < -4f) // Make sure no pieces are placed in the car zone
                        thisLocation = new Vector2(thisLocation.x + (-4f - (lastLocation.x + thisLocation.x)), thisLocation.y);

                if (Vector2Int.RoundToInt(thisLocation).magnitude < 1) // if length is 0 this means the net wants to skip it.
                    continue;
                if (locations[i] < 0)
                    isNegativeX = !isNegativeX;
                if (locations[i + 1] < 0)
                    isNegativeY = !isNegativeY;
                barCreator.CreateBar((Vector2)(Vector2Int.RoundToInt(lastLocation)), (Vector2)(Vector2Int.RoundToInt(lastLocation + thisLocation)), lastLocation, thisLocation);
                lastLocation = lastLocation + thisLocation;
            }
        }
        // Else, use coordinates literally, as world coords
        else
        {
            for (int i = 0; i < locations.Length - 2; i++)
            {
                if (types[i] < 0) // If type is between -1 and 0 it is road
                    barCreator.barToInstantiate = barCreator.roadBar;
                else if (types[i] > 0) // If type is between 0 and 1 it is wood
                    barCreator.barToInstantiate = barCreator.woodBar;

                float xVal = useNegativeAsDirection ? Mathf.Abs((float)locations[i]) : (float)locations[i];
                float yVal = useNegativeAsDirection ? Mathf.Abs((float)locations[i + 1]) : (float)locations[i + 1];
                Vector2 thisLocation = new Vector2((xVal * outputMultiplier) * (isNegativeX && useNegativeAsDirection ? -1f : 1f), (yVal * outputMultiplier) * (isNegativeY && useNegativeAsDirection ? -1f : 1f)); // Start point

                if (dontAllowBuildingInCarArea)
                    if (thisLocation.x < -4f) // Make sure no pieces are placed in the car zone
                        thisLocation = new Vector2(-4f, thisLocation.y);

                // Make sure distance is not greater than allowed value
                if (Vector2.Distance(thisLocation, lastLocation) >= maxLength)
                {
                    //Vector2 offset = lastLocation;
                    thisLocation -= lastLocation;
                    thisLocation /= Vector2.Distance(thisLocation, lastLocation); // normalize to a length of 1
                    thisLocation *= maxLength; // scale to the maxlength
                    thisLocation += lastLocation;
                }

                if (Vector2.Distance(thisLocation, lastLocation) < 1) // if length is 0 this means the net wants to skip it.
                    continue;
                if (locations[i] < 0)
                    isNegativeX = !isNegativeX;
                if (locations[i + 1] < 0)
                    isNegativeY = !isNegativeY;
                barCreator.CreateBar((Vector2)(Vector2Int.RoundToInt(lastLocation)), (Vector2)(Vector2Int.RoundToInt(thisLocation)), lastLocation, thisLocation);
                lastLocation = thisLocation;
            }
        }


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
            net.pendingFitness += -0.2f;
            fitnessSources.Add(-0.2f);
        }

        // Add extra for the cost of the bridge pieces
        if (lowerCostBonus)
        {
            float totalCost = 0f;
            foreach (Bar b in allBars.Values)
            {
                totalCost += b.length * b.costPerUnit;
            }
            totalCost /= (float)(allBars.Count); // Average

            net.pendingFitness += totalCost / 2f;
            fitnessSources.Add(totalCost / 2f);
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
