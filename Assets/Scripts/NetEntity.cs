using System.Collections;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.Build;

[System.Serializable]
public class Sense
{
    public string name;
    [Header("What to sense for:")]
    public bool distanceToObject;
    public bool horizontalDifference;
    public bool verticalDifference;
    public bool intersectingTrueFalse;
    public bool intersectingDistance;
    public bool timeElapsedAsSine;
    public bool rotationX;
    public bool rotationZ;
    public bool checkIfColliding;
    public bool xVelocity;
    [Header("Optional Variables")]
    public string objectToSenseForTag;
    public Transform objectToSenseFor;
    public LayerMask intersectionMask;
    public float initialDistance = 10.0f;
    public float sinMultiplier = 10.0f;

    [ShowOnly] public float lastOutput;

    public void Initialize(GameObject obj)
    {
        if (checkIfColliding)
            return;

        if (objectToSenseFor == null && objectToSenseForTag != "")
            objectToSenseFor = GameObject.FindGameObjectWithTag(objectToSenseForTag).transform;
        if (initialDistance == 0)
            initialDistance = Vector2.Distance(obj.transform.position, objectToSenseFor.position);

        lastOutput = (float)GetSensorValue(obj);
    }

    // Gets sensor value, normalized between 0 and 1
    public double GetSensorValue(int type, GameObject obj)
    {
        double val = 0;
        if (type == 0 && distanceToObject)
            val = Vector2.Distance(obj.transform.position, objectToSenseFor.position) / initialDistance;
        else if (type == 1 && horizontalDifference)
            val = (Mathf.Abs(obj.transform.position.x) + Mathf.Abs(objectToSenseFor.position.x)) / initialDistance;
        else if (type == 2 && verticalDifference)
            val = (Mathf.Abs(obj.transform.position.y) + Mathf.Abs(objectToSenseFor.position.y)) / initialDistance;
        else if (type == 3 && intersectingTrueFalse)
        {
            RaycastHit2D r = Physics2D.Linecast(obj.transform.position, objectToSenseFor.position, intersectionMask);
            if (r)
                val = 1;
            else
                val = 0;
        }
        else if (type == 4 && intersectingDistance)
        {
            RaycastHit2D r = Physics2D.Linecast(obj.transform.position, objectToSenseFor.position, intersectionMask);
            if (r)
            {
                val = Vector2.Distance(r.point, obj.transform.position) / initialDistance;
            }
            else
                val = 1;
        }
        else if (timeElapsedAsSine)
        {
            val = Mathf.Sin(type * sinMultiplier);
        }
        else if (xVelocity)
            val = objectToSenseFor.GetComponent<Rigidbody>().velocity.x / (float)type;

        lastOutput = (float)val;
        return val;
    }

    // Gets sensor value, normalized between 0 and 1
    public double GetSensorValue(GameObject obj)
    {
        double val = 0;
        if (distanceToObject)
            val = Vector2.Distance(obj.transform.position, objectToSenseFor.position) / initialDistance;
        else if (horizontalDifference)
            val = (Mathf.Abs(obj.transform.position.x) + Mathf.Abs(objectToSenseFor.position.x)) / initialDistance;
        else if (verticalDifference)
            val = (Mathf.Abs(obj.transform.position.y) + Mathf.Abs(objectToSenseFor.position.y)) / initialDistance;
        else if (intersectingTrueFalse)
        {
            RaycastHit2D r = Physics2D.Linecast(obj.transform.position, objectToSenseFor.position, intersectionMask);
            if (r)
                val = 1;
            else
                val = 0;
        }
        else if (intersectingDistance)
        {
            RaycastHit2D r = Physics2D.Linecast(obj.transform.position, objectToSenseFor.position, intersectionMask);
            if (r)
                val = Vector2.Distance(r.point, obj.transform.position) / initialDistance;
            else
                val = 1;
        }
        else if (rotationX)
            // Rotation normalized between -1 and 1
            val = ((objectToSenseFor.transform.localRotation.eulerAngles.x > 180 ? 180 - (objectToSenseFor.transform.localRotation.eulerAngles.x - 180) : -objectToSenseFor.transform.localRotation.eulerAngles.x)) / 180.0f;

        else if (rotationZ)
            // Rotation normalized between -1 and 1
            val = ((objectToSenseFor.transform.localRotation.eulerAngles.z > 180 ? 180 - (objectToSenseFor.transform.localRotation.eulerAngles.z - 180) : -objectToSenseFor.transform.localRotation.eulerAngles.z)) / 180.0f;
        //Debug.Log(((obj.transform.eulerAngles.z>180? 180-(obj.transform.eulerAngles.z-180): obj.transform.eulerAngles.z)) / 180.0f);
        //else if (checkIfColliding)
        //    try
        //    {
        //        val = objectToSenseFor.GetComponent<IsColliding3D>().isColliding ? 1 : 0;
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError(e.ToString());
        //        throw;
        //    }
        else if (xVelocity)
            val = objectToSenseFor.GetComponent<Rigidbody>().velocity.x / (float)4;

        lastOutput = (float)val;
        return val;
    }
}

public class NetEntity : MonoBehaviour
{
    public NeuralNetwork net;

    public Sense[] senses;

    [ShowOnly] public double[] outputs;
    public bool networkRunning = false;
    public int generation;

    public int numberOfInputs;

    int timeElapsed = 0;
    int iterationsBetweenNetworkIteration = 0;

    float totalRotationalDifference = 0;
    float totalheightDifference = 0;
    float totalDistanceOverTime = 0;
    float totalXVelocity = 0;

    public MeshRenderer[] modelPieces;
    public HingeJoint[] hinges;
    public bool randomizeSpriteColor = true;

    float bestDistance = -10000;
    float bestHeight = -10;

    [Header("Fitness Modifiers")]
    public bool bodyTouchingGroundIsBad = false;
    public bool upperLegsTouchingGroundIsBad = false;
    public bool touchingLaserIsBad = true;
    public bool rotationIsBad = false;
    public bool rewardTimeAlive = false;
    public bool heightIsGood = false;
    public bool slowRotationIsBad = false;
    public bool distanceIsGood = false;
    public bool useAverageDistance = false;
    public bool directionChangeIsGood = false;
    public bool xVelocityIsGood = false;
    public bool outputAffectsSin = false;
    public bool feetOffGroundTimeIsBetter = false;

    [Range(0.1f, 10f)]
    public float aliveTimeWeight = 1f;

    [Range(0.1f, 10f)]
    public float distanceWeight = 0.5f;

    private bool[] directions = { false, false, false, false, false, false, false, false, false, false, false, false }; // Array of directions of each motor
    private float[] directionTimes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // Amount of time each direction has been used
    private float finalErrorOffset = 0;
    private float feetOnGroundTime = 0;

    [ShowOnly] public string genome = "blankgen";
    [ShowOnly] public double totalFitness;
    int totalIterations;
    [ShowOnly] public int trial;
    public float[] trialValues;

    [ShowOnly] public double[] mutVars;
    [ShowOnly] public int netID;
    [ShowOnly] public string weightsHash;

    public GameObject bestCrown;

    //[HideInInspector] public NetUI netUI;

    public Material invisibleMat;

    private float startMeasure;

    public bool Elapse()
    {
        if (networkRunning == true)
        {
            // Every 10 times this function is called, measure the distance between this and the last position.
            if ((timeElapsed / iterationsBetweenNetworkIteration) % 10 == 0 && timeElapsed/iterationsBetweenNetworkIteration >= 50)
            {
                //// If this is not the first time measuring, then compare with the last position, and fail if it hasn't moved enough
                //if (timeElapsed != 0)
                //    if (Mathf.Abs(startMeasure - modelPieces[0].transform.localPosition.x) < 0.3f)
                //        return Fail(0.05f);


                startMeasure = modelPieces[0].transform.localPosition.x;
            }

            if (modelPieces[0].transform.localPosition.x <= -2f)
                return Fail(0.05f);

            if (modelPieces[0].transform.localPosition.z >= 0.2f|| modelPieces[0].transform.localPosition.z <= -0.2f)
                return Fail(0.025f);

            //string weightLengths = "";
            //for (int i = 0; i < net.weights.Length; i++)
            //{
            //    weightLengths += net.weights[i].Length * net.weights[i][0].Length + ", ";
            //}
            //Debug.Log(weightLengths);

            //if (timeElapsed % 2 == 0)
            //{
            double[] inputs = new double[numberOfInputs];

            for (int p = 0; p < inputs.Length; p++)
            {
                if (p == 1 || p == 2)
                    continue;
                inputs[p] = senses[p].GetSensorValue(modelPieces[0].gameObject);
            }
            inputs[0] = senses[0].GetSensorValue(timeElapsed / iterationsBetweenNetworkIteration, modelPieces[0].gameObject);

            //float rotX = ((modelPieces[0].transform.eulerAngles.x > 180 ? 180 - (modelPieces[0].transform.eulerAngles.x - 180) : -modelPieces[0].transform.eulerAngles.x)) / 180.0f;
            //float rotZ = ((modelPieces[0].transform.eulerAngles.z > 180 ? 180 - (modelPieces[0].transform.eulerAngles.z - 180) : -modelPieces[0].transform.eulerAngles.z)) / 180.0f;
            //senses[1].lastOutput = rotX;
            //inputs[1] = rotX;
            //senses[2].lastOutput = rotZ;
            //inputs[2] = rotZ;

            //inputs[1] = 1;
            //inputs[2] = 1;

            senses[1].lastOutput = modelPieces[0].transform.position.y / 10f;
            inputs[1] = modelPieces[0].transform.position.y / 10f;
            //senses[2].lastOutput = ((modelPieces[0].transform.localRotation.eulerAngles.x > 360.0f ? 360.0f - (modelPieces[0].transform.localRotation.eulerAngles.x - 360.0f) : -modelPieces[0].transform.localRotation.eulerAngles.x)) / 360.0f;
            //inputs[2] = ((modelPieces[0].transform.localRotation.eulerAngles.x > 360.0f ? 360.0f - (modelPieces[0].transform.localRotation.eulerAngles.x - 360.0f) : -modelPieces[0].transform.localRotation.eulerAngles.x)) / 360.0f;

            senses[2].lastOutput = ((modelPieces[0].transform.localRotation.eulerAngles.x + 90f > 180f ? 180f - (modelPieces[0].transform.localRotation.eulerAngles.x + 90f - 180f) : -modelPieces[0].transform.localRotation.eulerAngles.x + 90f)) / 180f;
            inputs[2] = ((modelPieces[0].transform.localRotation.eulerAngles.x + 90f > 180f ? 180f - (modelPieces[0].transform.localRotation.eulerAngles.x + 90f - 180f) : -modelPieces[0].transform.localRotation.eulerAngles.x + 90f)) / 180f;

            outputs = net.FeedForward(inputs);
            //if (net.isBest)
            //{
            //    netUI.UpdateInputs(inputs);
            //    netUI.UpdateOutputs(outputs);
            //}
            //}

            // Iterate through all of the servos, and change the speed accordingly to the outputs
            for (int i = 0; i < hinges.Length; i++)
            {
                //if (timeElapsed % 2 == 0)
                //{
                JointMotor changemotor = hinges[i].motor;
                changemotor.targetVelocity = ((float)outputs[i] - 0.5f) * 180.0f * 4;
                //changemotor.targetVelocity = 0.3f * 180.0f * 2;
                if ((i % 2) != 0)
                    changemotor.targetVelocity *= -1;
                hinges[i].motor = changemotor;
                //}

                // Get direction and see if it changed for joints
                if (directionChangeIsGood)
                    if (i >= 4)
                    {
                        bool direction = hinges[i].motor.targetVelocity > 0 ? true : false;
                        if (directions[i] == direction)
                        {  // It is still going in the same direction
                           //directionTimes[i] += 1+Mathf.Abs(hinges[i].motor.motorSpeed)/90.0f;
                            directionTimes[i] += 1;
                        }
                        else // It changed direction
                        {
                            finalErrorOffset += Mathf.Pow(directionTimes[i], 2) / (float)(totalIterations * totalIterations);
                            directionTimes[i] = 0;
                            directions[i] = !directions[i];
                        }
                        if (slowRotationIsBad)
                            // If slow motor speed, also add penalty
                            if (Mathf.Abs(hinges[i].motor.targetVelocity) < 20)
                                directionTimes[i] += (20f - Mathf.Abs(hinges[i].motor.targetVelocity)) / 20f;
                    }
            }

            CalculatePendingFitness();



            if (bodyTouchingGroundIsBad)
                // If body touched ground, end and turn invisible
                if (senses[23].GetSensorValue(modelPieces[0].gameObject) == 1)
                {
                    networkRunning = false;
                    return Fail(0.3f);
                }
            if (upperLegsTouchingGroundIsBad)
                // If upper leg parts touched ground, end and turn invisible
                if (senses[19].GetSensorValue(modelPieces[0].gameObject) == 1 ||
                    senses[20].GetSensorValue(modelPieces[0].gameObject) == 1 ||
                    senses[21].GetSensorValue(modelPieces[0].gameObject) == 1 ||
                    senses[22].GetSensorValue(modelPieces[0].gameObject) == 1)
                {
                    networkRunning = false;
                    return Fail(0.3f);
                }
            //if (touchingLaserIsBad)
            //    // If any body part touches the laser, end and turn invisible
            //    if (senses[12].objectToSenseFor.GetComponent<IsColliding3D>().failed ||  // Body
            //        senses[9].objectToSenseFor.GetComponent<IsColliding3D>().failed ||   // Leg A
            //        senses[10].objectToSenseFor.GetComponent<IsColliding3D>().failed     // Leg B
            //        )
            //    {
            //        networkRunning = false;
            //        for (int i = 0; i < modelPieces.Length; i++)
            //            Destroy(modelPieces[i].gameObject);
            //        //net.pendingFitness += 0.15f;
            //        //return false;
            //    }

            timeElapsed += iterationsBetweenNetworkIteration;


            totalFitness = net.fitness + net.pendingFitness;

            return true;
        }
        return false;
    }

    private bool Fail(float penalty)
    {
        CalculatePendingFitness();
        net.pendingFitness += penalty;
        networkRunning = false;
        for (int i = 0; i < modelPieces.Length; i++)
            Destroy(modelPieces[i].gameObject);
        return false;
    }

    private void CalculatePendingFitness()
    {
        if (rotationIsBad)
            totalRotationalDifference += Mathf.Abs(senses[1].lastOutput);

        // if (senses[2].GetSensorValue(gameObject) <= 0.25d) // If touching ground
        // {
        //     if (Mathf.Abs((float)outputs[0]) > 0.25f)
        //         transform.position += transform.right / ((1.0f - (float)outputs[0]) * 100.0f);
        // }
        // else
        //     transform.position -= new Vector3(0, 0.01f);


        ////transform.position += new Vector3((float)outputs[0]*2.0f-1.0f, (float)outputs[1] * 2.0f - 1.0f) / 100.0f;
        //Vector3 directionVector = new Vector2((float)Math.Cos((float)outputs[0] * 6.28319f), (float)Math.Sin((float)outputs[0] * 6.28319f));
        //transform.position += directionVector / 100.0f;

        //Vector3 dir = (transform.position - senses[0].objectToSenseFor.position).normalized;
        //net.AddFitness(Vector3.Distance(dir, directionVector));
        //net.error += (senses[0].GetSensorValue(0, gameObject));

        //if (timeElapsed % 50 == 0)
        //{
        //    double[] correct = { 1.0f };
        //    //net.BackProp(correct);
        //}
        float height = modelPieces[0].transform.position.y;
        totalheightDifference += -height;
        if (height > bestHeight)
            bestHeight = height;

        //float d = (float)senses[11].GetSensorValue(modelPieces[0].gameObject);
        //float distance = (200f - (modelPieces[0].transform.position.x + 7.3f)) / 200f;
        //float distance = (200f - 
        //    Mathf.Sqrt(Mathf.Pow(modelPieces[0].transform.localPosition.x, 2)+ 
        //    Mathf.Pow(modelPieces[0].transform.localPosition.z, 2)))
        //    / 200f;
        //float distance = (200f -
        //    (Mathf.Pow(modelPieces[0].transform.localPosition.x, 2)))
        //    / 200f;
        float distance = modelPieces[0].transform.localPosition.x;
        //totalDistanceOverTime += Mathf.Clamp(distance, -100000f, 0);
        totalDistanceOverTime += distance;
        if (distance > bestDistance)
            bestDistance = distance;

        // If any of the feet are on the ground, add 1 for each foot.
        if (senses[15].GetSensorValue(modelPieces[0].gameObject) == 1)
            feetOnGroundTime += 1;
        if (senses[16].GetSensorValue(modelPieces[0].gameObject) == 1)
            feetOnGroundTime += 1;
        if (senses[17].GetSensorValue(modelPieces[0].gameObject) == 1)
            feetOnGroundTime += 1;
        if (senses[18].GetSensorValue(modelPieces[0].gameObject) == 1)
            feetOnGroundTime += 1;

        float xVelocity = modelPieces[0].GetComponent<Rigidbody>().velocity.x;
        totalXVelocity += xVelocity;

        //if (senseVal < bestDistance)
        //{
        net.pendingFitness = 0;
        if (distanceIsGood)
            if (useAverageDistance)
                //net.pendingFitness += (totalDistanceOverTime / (float)timeElapsed) + (distance / 2.0f);
                net.pendingFitness += -totalDistanceOverTime / (float)timeElapsed* distanceWeight;
            else
                //net.pendingFitness += (bestDistance < 0 ? -1 : 1) * Mathf.Pow(Mathf.Abs(bestDistance) + 1f, 2f) * distanceWeight;
                //net.pendingFitness += -bestDistance * distanceWeight;
                net.pendingFitness += -modelPieces[0].transform.localPosition.x*0.1f * distanceWeight;
        if (directionChangeIsGood)
        {
            net.pendingFitness += finalErrorOffset;
            for (int i = 0; i < directionTimes.Length; i++)
                net.pendingFitness += Mathf.Pow(directionTimes[i], 2) / (float)(totalIterations * totalIterations);
        }
        if (rotationIsBad)
            net.pendingFitness += totalRotationalDifference / (float)timeElapsed;
        if (rewardTimeAlive)
            net.pendingFitness += ((totalIterations - timeElapsed) / (float)totalIterations) * aliveTimeWeight;
        if (heightIsGood)
            //net.pendingFitness += totalheightDifference / (float)timeElapsed;
            net.pendingFitness += -bestHeight / 2f;
        if (xVelocityIsGood)
            net.pendingFitness += 1.0f-(totalXVelocity / (float)timeElapsed);
        if (feetOffGroundTimeIsBetter)  // Calculate average percent of the time feet are on ground
            net.pendingFitness += (feetOnGroundTime / 4f) / (float)timeElapsed * 2;
    }

    public void Init(NeuralNetwork neti, int generation, int numberOfInputs, int totalIterations, int iterationsBetweenNetworkIteration, int trial, bool visible)
    {
        transform.localPosition = Vector3.zero;
        transform.eulerAngles = Quaternion.Euler(0, 0, trialValues[trial]).eulerAngles;
        this.net = neti;
        this.generation = generation;
        this.numberOfInputs = numberOfInputs;
        this.totalIterations = totalIterations;
        this.totalRotationalDifference = 0;
        this.trial = trial;
        networkRunning = true;
        this.net.fitness += this.net.pendingFitness;
        this.net.pendingFitness = 0;
        this.genome = net.genome;
        this.netID = net.netID;
        this.weightsHash = net.weightsHash;
        //this.netUI = netUI;
        this.iterationsBetweenNetworkIteration = iterationsBetweenNetworkIteration;
        //net.error = 0;
        timeElapsed = 0;
        //bestDistance = 10000;

        // Set the sin multiplier based off of mutVar 0
        if (outputAffectsSin)
            senses[0].sinMultiplier = (float)net.mutatableVariables[0];
        //senses[0].sinMultiplier = 2.0f * (float)net.mutatableVariables[0];

        mutVars = net.mutatableVariables;
        if (net.isBest)
        {
            //netUI.RemakeDrawing(net.droppedNeurons);
            //netUI.UpdateWeightLines(net.weights);

            // Count total dropped neurons and print
            int total = 0;
            for (int i = 0; i < net.droppedNeurons.Length; i++)
                for (int j = 0; j < net.droppedNeurons[i].Length; j++)
                    total += net.droppedNeurons[i][j] == true ? 1 : 0;
            //Debug.Log(total);
        }

        // Show the crown if this is the best network
        bestCrown.SetActive(net.isBest);
        // Set the sprite layer to be the very front if this is the best network
        //if (net.isBest)
        //    for (int i = 0; i < mainSprites.Length; i++)
        //        mainSprites[i].sortingOrder = 1000;
        //else
        //    for (int i = 0; i < mainSprites.Length; i++)
        //        mainSprites[i].sortingOrder = netID;

        foreach (var s in senses)
            s.Initialize(modelPieces[0].gameObject);


        if (visible == false)
            for (int i = 0; i < modelPieces.Length; i++)
                modelPieces[i].material = invisibleMat;

        //if (randomizeSpriteColor)
        //{
        //    Color col = new Color32((byte)UnityEngine.Random.Range(0, 256),
        //            (byte)UnityEngine.Random.Range(0, 256),
        //            (byte)UnityEngine.Random.Range(0, 256), 255);
        //    for (int i = 0; i < mainSprites.Length; i++)
        //        mainSprites[i].color = col;
        //}
    }

    //public void End()
    //{
    //    net.fitness = senses[8].GetSensorValue(mainSprites[0].gameObject);

    //    double[] correct = { 0, 0 };
    //    //net.BackPropagation(correct);
    //    networkRunning = false;
    //}
}

