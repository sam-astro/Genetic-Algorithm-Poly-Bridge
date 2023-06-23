using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;

[System.Serializable]
class SaveData
{
    public double[][][] weights;
    public double[] mutVars;
    public bool[][] droppedNeurons;
    public int[] layers;
}

public class NetManager : MonoBehaviour
{
    public int populationSize = 100;

    //private int waitBetweenTestResults = 1;

    public int[] layers = new int[] { 1, 8, 12, 1 }; // No. of inputs and No. of outputs

    //public float learningRate = 0.1f;

    [ShowOnly] public int remainingIterations = -10;
    public int maxIterations = 1000;
    public int simulationWaitIterations = 10;

    int currentTrial;
    public int maxTrialsPerGeneration;

    public int mutVarSize = 1;

    //public int maxTrialsPerGeneration = 1;
    //[ShowOnly] public int currentTrial;

    //public Transform spawnPoint;

    public TMP_Text generationText;
    //public TMP_Text droppedNeuronsText;
    //public TMP_Text genomeList;
    //public Slider dropChanceSlider;
    //public Toggle optimizeAndShrinkToggle;
    //public Toggle onlyShowBestToggle;
    public int dropChance = 0;

    public TimeManager timeManager;

    //public LaserScript laser;

    //public CameraFollow cameraFollow;


    //public NetUI netUI;

    public bool optimizeAndShrinkNet = false;
    public bool onlyShowBest = false;

    //double promptMin = 0;
    //double promptMax = 0;
    //public double[][] prompt =
    //    {
    //        new double[1]{180},
    //    };
    //double answerMin = 0;
    //double answerMax = 0;
    //double[][] answer =
    //    {
    //        new double[1]{0.8011526357},
    //    };


    #region Internal Variables
    [ShowOnly] public int populationLeft;

    NeuralNetwork persistenceNetwork;
    [ShowOnly] public string bestGenome = "";
    [ShowOnly] public string bestHash = "";
    private List<NeuralNetwork> topGenomes;

    [ShowOnly] public int currentGeneration = 1;
    double lastWorst = 100000000;
    [ShowOnly] public double lastBest = 100000000;
    double bestError = 100000000;
    public double bestEverError = 100000000;
    double worstError = 0;

    bool queuedForUpload = false;
    private List<NeuralNetwork> nets;
    public List<GameObject> entityList = null;
    //bool startup = true;

    public GameObject netEntityPrefab;

    int bestDroppedNeuronsAmnt = 0;
    int totalNeurons = 0;

    //[ShowOnly] public double[] bestMutVarsBefore;
    //[ShowOnly] public double[] bestMutVars;
    #endregion

    private void Start()
    {
        InitEntityNeuralNetworks();
        CreateEntityBodies();

        remainingIterations = maxIterations;
        //currentTrial = maxTrialsPerGeneration;

        generationText.text = currentGeneration.ToString() + " : " + currentTrial.ToString();

        totalNeurons = persistenceNetwork.CountTotalNeurons();


        //dropChanceSlider.value = dropChance;

        // If the dat folder does not exist, create it
        if (!Directory.Exists("./Assets/dat/"))
            Directory.CreateDirectory("./Assets/dat/");

        // If the hist.csv file does not exist, create it and add data labels
        if (!File.Exists("./Assets/dat/hist.csv"))
            using (StreamWriter sw = File.AppendText("./Assets/dat/hist.csv"))
                sw.WriteLine("generation, Top Error, Gen Error, Dropped %");


        //// Only iterate the bridge entities a single time; this is when they
        //// create their bridges, then we just wait for physics.
        //IterateNetEntities();
    }

    public void FixedUpdate()
    {
        //// Initial iteration
        //if(remainingIterations == -10)
        //{
        //    // Only iterate the bridge entities a single time; this is when they
        //    // create their bridges, then we just wait for physics.
        //    Debug.Log("Iterating Net entities for first time");
        //    IterateNetEntities();
        //    remainingIterations = maxIterations;
        //    return;
        //}

        if (remainingIterations <= 0) // If this currentTrial is over, do another one
        {
            if (currentTrial >= maxTrialsPerGeneration - 1) // If the final currentTrial is over, finalize and go to next generation
            {
                //generationText.text = "processing...";

                //// Make sure final pendingFitness is added
                //for (int i = 0; i < populationSize; i++)
                //    nets[i].fitness+=nets[i].pendingFitness;

                //nets.Sort();

                //bestError = nets[nets.Count - 1].fitness;
                //worstError = nets[0].fitness;



                //if (bestError < bestEverError || queuedForUpload == true || currentGeneration == 0)
                //{
                //    //persistenceNetwork = nets[nets.Count - 1];
                //    //persistenceNetwork = new NeuralNetwork(nets[nets.Count - 1]);
                //    //persistenceNetwork.CopyWeights(nets[nets.Count - 1].weights);
                //    persistenceNetwork.weights = nets[nets.Count - 1].weights;
                //    //Array.Copy(nets[nets.Count - 1].mutatableVariables, persistenceNetwork.mutatableVariables, mutVarSize);
                //    //persistenceNetwork.mutatableVariables = nets[nets.Count - 1].mutatableVariables;
                //    persistenceNetwork.genome = nets[nets.Count - 1].genome;
                //    bestEverError = bestError;
                //    bestGenome = persistenceNetwork.genome.Substring(0, 8) + "a";

                //    using (StreamWriter sw = File.AppendText("./Assets/dat/hist.csv"))
                //        sw.WriteLine((currentGeneration).ToString() + ", " + bestEverError);

                //    // Save best weights
                //    BinaryFormatter bf = new BinaryFormatter();
                //    using (FileStream fs = new FileStream("./Assets/dat/WeightSave.bin", FileMode.Create))
                //        bf.Serialize(fs, persistenceNetwork.weights);

                //    // Get hash of best weights
                //    bestHash = ByteArrayToString(new MD5CryptoServiceProvider().ComputeHash(System.IO.File.ReadAllBytes("./Assets/dat/WeightSave.bin")));

                ////    // Save best mutatable variables
                ////    BinaryFormatter bf2 = new BinaryFormatter();
                ////    using (FileStream fs2 = new FileStream("./Assets/dat/MutVars.bin", FileMode.Create))
                ////        bf2.Serialize(fs2, persistenceNetwork.mutatableVariables);

                //}
                //else if (currentGeneration % timeBetweenGenerationProgress == 0)
                //    using (StreamWriter sw = File.AppendText("./Assets/dat/hist.csv"))
                //        sw.WriteLine((currentGeneration).ToString() + ", " + bestEverError);

                //if (currentGeneration % timeBetweenSave == 0 && timeBetweenSave != -1)
                //{
                //    // Save metadata
                //    StreamWriter ps = new StreamWriter("./Assets/dat/WeightSaveMeta.mta");
                //    ps.WriteLine((currentGeneration).ToString() + "#" +
                //        (bestEverError).ToString() + "#" +
                //        ((int)Time.realtimeSinceStartup + timeManager.offsetTime).ToString() + "#" +
                //        bestGenome);
                //    ps.Close();
                //}

                //// Find the top 3 *individual* genomes and add them to a `topGenomes` list
                //Debug.Log("lGenome: " + bestGenome);
                //string lastGenome = bestGenome.Substring(0, 8);
                //topGenomes = new List<NeuralNetwork>();
                //topGenomes.Add(persistenceNetwork);
                //for (int i = nets.Count - 1; i >= 0; i--)
                //{
                //    if (topGenomes.Count >= 3)
                //        break;
                //    if (nets[i].genome.Substring(0, 8) != lastGenome)
                //    {
                //        topGenomes.Add(new NeuralNetwork(nets[i]));
                //        lastGenome = nets[i].genome.Substring(0, 8);
                //    }
                //}

                //ListBestGenomes();

                //Finalizer();

                //lastBest = bestError;
                //lastWorst = worstError;
                //currentGeneration++;
                //currentTrial = 0;
                //remainingIterations = maxIterations;
                //laser.ResetPosition();

                //CreateEntityBodies();

                //generationText.text = currentGeneration.ToString() + " : " + currentTrial.ToString();



                //
                // OLD SYSTEM: 
                //

                Time.timeScale = 0;
                generationText.text = "processing...";

                //dropChance = (int)dropChanceSlider.value;

                // Make sure final pendingFitness is added
                for (int i = 0; i < populationSize; i++)
                {
                    nets[i].AddFitness(nets[i].pendingFitness);
                    nets[i].fitness /= maxTrialsPerGeneration;
                    nets[i].dropChance = dropChance; // (Also apply drop chance here before the finalizer)
                }

                nets.Sort();

                bestError = nets[nets.Count - 1].fitness;
                worstError = nets[0].fitness;

                //optimizeAndShrinkNet = optimizeAndShrinkToggle.isOn;
                //onlyShowBest = onlyShowBestToggle.isOn;


                bestGenome = persistenceNetwork.genome.Substring(0, 8) + "a";

                StreamWriter persistence = new StreamWriter("./Assets/dat/WeightSaveMeta.mta");
                persistence.WriteLine((currentGeneration).ToString() + "#" +
                    (bestEverError).ToString() + "#" +
                    ((int)Time.realtimeSinceStartup + timeManager.offsetTime).ToString() + "#" +
                    bestGenome);

                #region OLD SAVE SYSTEM
                //// Save best weights
                //BinaryFormatter bf = new BinaryFormatter();
                //using (FileStream fs = new FileStream("./Assets/dat/WeightSave.bin", FileMode.Create))
                //    bf.Serialize(fs, persistenceNetwork.weights);

                //// Get hash of best weights
                //bestHash = ByteArrayToString(new MD5CryptoServiceProvider().ComputeHash(System.IO.File.ReadAllBytes("./Assets/dat/WeightSave.bin")));

                //// Save best mutatable variables
                //BinaryFormatter bf2 = new BinaryFormatter();
                //using (FileStream fs2 = new FileStream("./Assets/dat/MutVars.bin", FileMode.Create))
                //    bf2.Serialize(fs2, persistenceNetwork.mutatableVariables);

                //// Save best dropped neurons
                //BinaryFormatter bf3 = new BinaryFormatter();
                //using (FileStream fs3 = new FileStream("./Assets/dat/DroppedNeurons.bin", FileMode.Create))
                //    bf3.Serialize(fs3, persistenceNetwork.droppedNeurons);
                #endregion


                // NEW SAVE SYSTEM

                // Save all data into file
                SaveData sd = new SaveData();
                sd.droppedNeurons = persistenceNetwork.droppedNeurons;
                sd.weights = persistenceNetwork.weights;
                sd.layers = persistenceNetwork.layers;
                sd.mutVars = persistenceNetwork.mutatableVariables;
                BinaryFormatter bf3 = new BinaryFormatter();
                using (FileStream fs3 = new FileStream("./Assets/dat/NetworkSaveData.bin", FileMode.Create))
                    bf3.Serialize(fs3, sd);

                persistence.Close();

                // Change the best ever network data and score to beat if it matches any criteria below
                if ((bestError < bestEverError && !optimizeAndShrinkNet) || // If the error is better than the best ever
                    currentGeneration == 0 ||  // If this is the first generation

                    // If the optimizeAndShrinkNet option is true, and the used
                    // neuron amount is less than the previous best, but the error
                    // is still within 3% or lower of the best ever to not have too much of a deviation
                    (optimizeAndShrinkNet && (nets[nets.Count - 1].CountDroppedNeurons() >= bestDroppedNeuronsAmnt) && (bestError <= bestEverError * 1.005f)))
                {
                    //if (optimizeAndShrinkNet && (nets[nets.Count - 1].CountDroppedNeurons() < bestDroppedNeuronsAmnt))
                    //    goto skip0;
                    persistenceNetwork.weights = nets[nets.Count - 1].weights;
                    persistenceNetwork.mutatableVariables = nets[nets.Count - 1].mutatableVariables;
                    persistenceNetwork.droppedNeurons = nets[nets.Count - 1].droppedNeurons;
                    persistenceNetwork.genome = nets[nets.Count - 1].genome;
                    if (bestError < bestEverError)
                        bestEverError = bestError;
                    bestDroppedNeuronsAmnt = nets[nets.Count - 1].CountDroppedNeurons();

                    //skip0:

                    using (StreamWriter sw = File.AppendText("./Assets/dat/hist.csv"))
                        sw.WriteLine((currentGeneration).ToString() + ", " + bestEverError + ", " + bestError + ", " + ((float)bestDroppedNeuronsAmnt / (float)totalNeurons).ToString());

                }
                else
                    using (StreamWriter sw = File.AppendText("./Assets/dat/hist.csv"))
                        sw.WriteLine((currentGeneration).ToString() + ", " + bestEverError + ", " + bestError + ", " + ((float)bestDroppedNeuronsAmnt / (float)totalNeurons).ToString());

                // Find the top 3 *individual* genomes and add them to a `topGenomes` list
                //Debug.Log("lGenome: " + bestGenome);
                string lastGenome = bestGenome.Substring(0, 8);
                topGenomes = new List<NeuralNetwork>();
                topGenomes.Add(persistenceNetwork);
                for (int i = nets.Count - 1; i >= 0; i--)
                {
                    if (topGenomes.Count >= 3)
                        break;
                    if (nets[i].genome.Substring(0, 8) != lastGenome)
                    {
                        topGenomes.Add(new NeuralNetwork(nets[i]));
                        lastGenome = nets[i].genome.Substring(0, 8);
                    }
                }
                ListBestGenomes();

                Finalizer();

                lastBest = bestError;
                lastWorst = worstError;
                currentGeneration++;
                remainingIterations = maxIterations;
                currentTrial = 0;

                CreateEntityBodies();
                //laser.ResetPosition();

                generationText.text = currentGeneration.ToString() + " : " + currentTrial.ToString();
                //droppedNeuronsText.text = "Dropped Neurons: " + bestDroppedNeuronsAmnt.ToString() + ", " + Math.Round((float)bestDroppedNeuronsAmnt / (float)totalNeurons * 100f, 1).ToString() + "%";


                // Only iterate the bridge entities a single time; this is when they
                // create their bridges, then we just wait for physics.
                IterateNetEntities();
                Time.timeScale = 1;
            }
            else // Otherwise, create next currentTrial and reset entities
            {
                remainingIterations = maxIterations;
                currentTrial += 1;
                CreateEntityBodies();

                generationText.text = currentGeneration.ToString() + " : " + currentTrial.ToString();
            }
        }
        else
        {
            remainingIterations -= 1;

            if (remainingIterations % simulationWaitIterations == 0)
                if (IterateNetEntities() == false)
                    remainingIterations = 0;
        }
    }

    private void ListBestGenomes()
    {
        string outS = "";
        int count = 10;
        for (int i = nets.Count - 1; i >= 0; i--)
        {
            if (count <= 0)
                break;
            if (!outS.Contains(nets[i].genome))
            {
                count--;
                outS += (nets.Count - 1 - i).ToString() + ". " + nets[i].genome + " " + Math.Round(nets[i].fitness, 3).ToString() + "\n";
            }
        }
        //genomeList.text = outS;
    }

    private void CreateEntityBodies()
    {
        if (entityList != null)
        {
            for (int i = 0; i < entityList.Count; i++)
            {
                Destroy(entityList[i]);
            }
        }
        //if (entityList == null)
        //{
        entityList = new List<GameObject>();

        for (int i = 0; i < populationSize; i++)
        {
            GameObject tempEntity = Instantiate(netEntityPrefab, Vector3.right * i * 15, Quaternion.identity);
            entityList.Add(tempEntity);
            //if (i == 0)
            //{
            //    cameraFollow.target = entityList[i].GetComponent<Entity>().modelPieces[0].transform;
            //}
            entityList[i].GetComponent<Entity>().Init(nets[i], currentGeneration, layers[0], maxIterations, simulationWaitIterations, onlyShowBest ? (i == 0 ? true : false) : true);
        }
    }

    private bool IterateNetEntities()
    {
        int amnt = entityList.Count;
        for (int i = 0; i < entityList.Count; i++)
            //entityList[i].GetComponent<Entity>().Elapse();
            amnt -= entityList[i].GetComponent<Entity>().Elapse() ? 0 : 1;
        populationLeft = amnt;
        return amnt != 0;
    }

    NeuralNetwork SplitGenomes(NeuralNetwork parentA, NeuralNetwork parentB)
    {
        NeuralNetwork outNet = new NeuralNetwork(persistenceNetwork);
        //double[][][] outWeights = persistenceNetwork.weights;
        int secLength = 1;
        bool copyFromSide = false; // False => parentA    True => parentB
        for (int x = 0; x < parentA.weights.Length; x++)
        {
            for (int y = 0; y < parentA.weights[x].Length; y++)
            {
                for (int z = 0; z < parentA.weights[x][y].Length; z++, secLength--)
                {
                    bool isMutation = UnityEngine.Random.Range(0, 10) == 1;

                    if (isMutation)
                        outNet.weights[x][y][z] = outNet.FixedSingleMutate(outNet.weights[x][y][z]);
                    else
                    {
                        if (secLength <= 0)
                        {
                            secLength = UnityEngine.Random.Range(1, outNet.weights[x][y].Length);
                            copyFromSide = UnityEngine.Random.Range(0, 2) == 1;
                        }

                        if (copyFromSide == false)
                            outNet.weights[x][y][z] = parentA.weights[x][y][z];
                        else
                            outNet.weights[x][y][z] = parentB.weights[x][y][z];
                    }

                }
            }
        }
        //// Also pass on the mutatable variables
        //for (int x = 0; x < parentA.mutatableVariables.Length; x++)
        //{
        //    bool isMutation = UnityEngine.Random.Range(0, 100) == 0;

        //    copyFromSide = UnityEngine.Random.Range(0, 2) == 1;

        //    if (copyFromSide == false)
        //        outNet.mutatableVariables[x] = parentA.mutatableVariables[x];
        //    else
        //        outNet.mutatableVariables[x] = parentB.mutatableVariables[x];

        //    if (isMutation)
        //        outNet.mutatableVariables[x] = outNet.FixedSingleMutateMutVars(outNet.mutatableVariables[x]);
        //}

        return outNet;
    }

    void FindParents()
    {
        // Create new offspring to fill the population
        for (int i = 0; i < populationSize/* - populationSize / 10*/; i++) // 1/10th will be randomized
        {
            int numOfCandidates = UnityEngine.Random.Range(2, 5);

            int bestNet = 0;
            double bestScore = 100000d;

            // The indexes of the winning parents
            int parentA = UnityEngine.Random.Range(0, populationSize);
            int parentB = UnityEngine.Random.Range(0, populationSize);

            // Compare `numOfCandidates` number of candidates for parent A
            for (int j = 0; j < numOfCandidates; j++)
            {
                // Randomly select a network from the population
                int whichNet = UnityEngine.Random.Range(0, populationSize);
                // Compare this network with the best one randomly selected so far
                if (nets[whichNet].fitness < bestScore)
                {
                    bestScore = nets[whichNet].fitness;
                    bestNet = whichNet;
                }
            }
            parentA = bestNet;


            numOfCandidates = UnityEngine.Random.Range(2, 5);

            bestNet = 0;
            bestScore = 100000d;

            // Compare `numOfCandidates` number of candidates for parent B
            for (int j = 0; j < numOfCandidates; j++)
            {
                // Randomly select a network from the population
                int whichNet = UnityEngine.Random.Range(0, populationSize);
                // Compare this network with the best one randomly selected so far
                if (nets[whichNet].fitness < bestScore && whichNet != parentA)
                {
                    bestScore = nets[whichNet].fitness;
                    bestNet = whichNet;
                }
                else if (whichNet == parentA)
                {
                    j -= 1;
                    continue;
                }
            }
            parentB = bestNet;

            nets[i] = SplitGenomes(nets[parentA], nets[parentB]);
            nets[i].genome = nets[i].GenerateGenome();
        }

        //// This 1/10th will be randomized for diversity
        //for (int i = populationSize - populationSize / 10; i < populationSize; i++)
        //{
        //    nets[i].RandomizeWeights();
        //    nets[i].genome = nets[i].GenerateGenome();
        //}

        //// Keep the best 3 networks
        //for (int g = 0; g < topGenomes.Count; g++)
        //{
        //    nets[g + 1] = new NeuralNetwork(topGenomes[g]);
        //    //nets[i].CopyWeights(topGenomes[g].weights);
        //    //Array.Copy(topGenomes[g].mutatableVariables, nets[i].mutatableVariables, mutVarSize);
        //    nets[g + 1].genome = topGenomes[g].genome;
        //    nets[g + 1].Mutate();
        //    nets[g + 1].UpdateGenome();
        //    nets[g + 1].droppedNeurons = nets[g + 1].MutateDroppedNeurons(dropChance);
        //    nets[g + 1].mutatableVariables = nets[g + 1].MutateMutVars();
        //}

        for (int i = 0; i < populationSize; i++)
        {
            nets[i].isBest = false;
            nets[i].netID = i;
            nets[i].fitness = 0;
            nets[i].pendingFitness = 0;
            //nets[i].ResetGenome();
        }
        //nets[0] = new NeuralNetwork(persistenceNetwork);
        //nets[0].droppedNeurons = persistenceNetwork.droppedNeurons;
        //nets[0].mutatableVariables = persistenceNetwork.mutatableVariables;
        //nets[0].genome = persistenceNetwork.genome;
        //nets[0].isBest = true;
    }

    void Finalizer()
    {
        // Use the new system for parent finding and offspring
        FindParents();
        return;



        //// Create copies of top 3 genomes to replace the worst neural networks
        //for (int g = 0; g < topGenomes.Count; g++)
        //{
        //    for (int i = (int)(populationSize * 0.2 / topGenomes.Count) * g; i < (int)(populationSize * 0.2 / topGenomes.Count) * (g + 1); i++)
        //    {
        //        nets[i] = new NeuralNetwork(topGenomes[g]);
        //        //nets[i].CopyWeights(topGenomes[g].weights);
        //        //Array.Copy(topGenomes[g].mutatableVariables, nets[i].mutatableVariables, mutVarSize);
        //        nets[i].genome = topGenomes[g].genome;
        //        nets[i].UpdateGenome();
        //        nets[i].Mutate();
        //        //nets[i].MutateMutVars();
        //    }
        //}
        //// Create totally new neural networks with random weights
        //for (int i = (int)(populationSize * 0.2 / topGenomes.Count) * (topGenomes.Count + 1); i < (int)(populationSize * 0.5); i++)
        //{
        //    nets[i].CopyWeights(nets[i].RandomizeWeights());
        //    //nets[i].mutatableVariables = nets[i].RandomizeMutVars();
        //    nets[i].ResetGenome();
        //}
        //// Continue using the best 50% of neural networks and mutate them a bit
        //for (int i = (int)(populationSize * 0.5); i < populationSize - 1; i++)
        //{
        //    if (nets[i].genome.Substring(0, 8) == bestGenome.Substring(0, 8) && // If it is the same genome as the best
        //        Array.IndexOf(nets[i].letters, nets[i].genome[8]) < 5) // And if the mutation level is less than 5 away from the original
        //                                                               // Then randomize it to make population more diverse
        //    {
        //        //UnityEngine.Debug.Log("Resetting of:  " + i.ToString());
        //        nets[i].ResetGenome();
        //        nets[i].CopyWeights(nets[i].RandomizeWeights());
        //        //nets[i].mutatableVariables = nets[i].RandomizeMutVars();
        //    }
        //    else
        //    {
        //        nets[i].Mutate();
        //        //nets[i].mutatableVariables = nets[i].MutateMutVars(); // This causes problem in best NN
        //        nets[i].UpdateGenome();
        //        //UnityEngine.Debug.Log("Mutating Vars of:  " + i.ToString() + " , got: " + nets[i].mutatableVariables[0].ToString());
        //    }
        //}

        //// Hanbdle the best network
        ////Debug.Log("Best is index: " + (nets.Count - 1).ToString());
        ////Debug.Log("Vars are: " + (persistenceNetwork.mutatableVariables[0]).ToString());
        ////nets[nets.Count - 1] = new NeuralNetwork(persistenceNetwork);
        //nets[nets.Count - 1] = new NeuralNetwork(topGenomes[0]);
        ////nets[nets.Count - 1].neurons = persistenceNetwork.neurons;
        ////nets[nets.Count - 1].genome = bestGenome;
        ////Array.Copy(persistenceNetwork.mutatableVariables, nets[nets.Count - 1].mutatableVariables, mutVarSize);
        ////Debug.Log("Vars are: " + (nets[nets.Count - 1].mutatableVariables[0]).ToString());

        //// If any neural networks have an invalid genome, reset it and assign a genome
        //for (int i = 0; i < populationSize; i++)
        //{
        //    if (nets[i].genome.Trim() == "")
        //    {
        //        nets[i].ResetGenome();
        //        nets[i].CopyWeights(nets[i].RandomizeWeights());
        //        //nets[i].mutatableVariables = nets[i].RandomizeMutVars();
        //        Debug.LogWarning("Found broken genome " + i.ToString());
        //        nets[i].Mutate();
        //        nets[i].UpdateGenome();
        //    }
        //    nets[i].fitness = 0f;
        //    nets[i].pendingFitness = 0f;
        //    nets[i].isBest = false;
        //    nets[i].netID = i;

        //    // Save temp weights
        //    BinaryFormatter bf = new BinaryFormatter();
        //    using (FileStream fs = new FileStream("./Assets/dat/temp_weights.bin", FileMode.Create))
        //        bf.Serialize(fs, nets[i].weights);
        //    // Get hash of temp weights
        //    nets[i].weightsHash = ByteArrayToString(new MD5CryptoServiceProvider().ComputeHash(System.IO.File.ReadAllBytes("./Assets/dat/temp_weights.bin")));

        //}
        //nets[nets.Count - 1].isBest = true;
        ////Array.Copy(persistenceNetwork.mutatableVariables, bestMutVars, mutVarSize);
        ////bestMutVars = persistenceNetwork.mutatableVariables;



        // OLD SYSTEM:
        //
        // If using the optimizers, just create many versions of the best ever network and mutate slightly
        if (optimizeAndShrinkNet)
        {
            // Create copies of top 3 genomes to replace the worst neural networks
            for (int g = 0; g < topGenomes.Count; g++)
            {
                for (int i = (int)(populationSize * 0.2 / topGenomes.Count) * g; i < (int)(populationSize * 0.2 / topGenomes.Count) * (g + 1); i++)
                {
                    nets[i] = new NeuralNetwork(topGenomes[g]);
                    //nets[i].CopyWeights(topGenomes[g].weights);
                    //Array.Copy(topGenomes[g].mutatableVariables, nets[i].mutatableVariables, mutVarSize);
                    nets[i].genome = topGenomes[g].genome;
                    nets[i].Mutate();
                    nets[i].UpdateGenome();
                    nets[i].droppedNeurons = nets[i].MutateDroppedNeurons(dropChance);
                    nets[i].mutatableVariables = nets[i].MutateMutVars();
                }
            }
            for (int i = (int)(populationSize * 0.2 / topGenomes.Count) * (topGenomes.Count + 1); i < (int)(populationSize * 0.5); i++)
            {
                nets[i] = new NeuralNetwork(persistenceNetwork);
                nets[i].droppedNeurons = persistenceNetwork.droppedNeurons;
                nets[i].droppedNeurons = nets[i].MutateDroppedNeurons(dropChance);
                nets[i].genome = persistenceNetwork.genome;
                nets[i].mutatableVariables = nets[i].MutateMutVars();
                nets[i].UpdateGenome();
            }
            // Continue using the best 50% of neural networks and mutate them a bit
            for (int i = (int)(populationSize * 0.5); i < populationSize; i++)
            {
                nets[i] = new NeuralNetwork(nets[i]);
                nets[i].droppedNeurons = nets[i].MutateDroppedNeurons(dropChance);
                nets[i].UpdateGenome();
                nets[i].mutatableVariables = nets[i].MutateMutVars();
            }
        }
        // Otherwise, use normal distribution and randomization on population
        else
        {
            // Create copies of top 3 genomes to replace the worst neural networks
            for (int g = 0; g < topGenomes.Count; g++)
            {
                for (int i = (int)(populationSize * 0.2 / topGenomes.Count) * g; i < (int)(populationSize * 0.2 / topGenomes.Count) * (g + 1); i++)
                {
                    nets[i] = new NeuralNetwork(topGenomes[g]);
                    //nets[i].CopyWeights(topGenomes[g].weights);
                    //Array.Copy(topGenomes[g].mutatableVariables, nets[i].mutatableVariables, mutVarSize);
                    nets[i].genome = topGenomes[g].genome;
                    nets[i].Mutate();
                    nets[i].UpdateGenome();
                    nets[i].droppedNeurons = nets[i].MutateDroppedNeurons(dropChance);
                    nets[i].mutatableVariables = nets[i].MutateMutVars();
                }
            }
            // Create create totally new neural networks with random weights
            for (int i = (int)(populationSize * 0.2 / topGenomes.Count) * (topGenomes.Count + 1); i < (int)(populationSize * 0.5); i++)
            {
                nets[i] = new NeuralNetwork(persistenceNetwork);
                nets[i].weights = nets[i].RandomizeWeights();
                nets[i].droppedNeurons = nets[i].RandomizeDroppedNeurons(dropChance);
                nets[i].genome = nets[i].GenerateGenome();
                nets[i].mutatableVariables = nets[i].MutateMutVars();
            }
            // Continue using the best 50% of neural networks and mutate them a bit
            for (int i = (int)(populationSize * 0.5); i < populationSize; i++)
            {
                if (nets[i].genome.Substring(0, 8) == bestGenome.Substring(0, 8) && // If it is the same genome as the best
                    Array.IndexOf(nets[i].letters, nets[i].genome[8]) < 5 && // And if the mutation level is less than 5 away from the original
                    i < populationSize - 11)                                   // And it is not in the top 10
                                                                               // Then randomize it to make population more diverse
                {
                    nets[i].ResetGenome();
                    nets[i].CopyWeights(nets[i].RandomizeWeights());
                    nets[i].droppedNeurons = nets[i].RandomizeDroppedNeurons(dropChance);
                }
                else
                {
                    nets[i] = new NeuralNetwork(nets[i]);
                    nets[i].Mutate();
                    nets[i].droppedNeurons = nets[i].MutateDroppedNeurons(dropChance);
                    nets[i].UpdateGenome();
                    nets[i].mutatableVariables = nets[i].MutateMutVars();
                }
            }
        }
        for (int i = 0; i < populationSize; i++)
        {
            nets[i].isBest = false;
            nets[i].netID = i;
        }
        nets[0] = new NeuralNetwork(persistenceNetwork);
        nets[0].droppedNeurons = persistenceNetwork.droppedNeurons;
        nets[0].mutatableVariables = persistenceNetwork.mutatableVariables;
        nets[0].genome = persistenceNetwork.genome;
        nets[0].isBest = true;


    }

    private string ByteArrayToString(byte[] arrInput)
    {
        int i;
        StringBuilder sOutput = new StringBuilder(arrInput.Length);
        for (i = 0; i < arrInput.Length; i++)
        {
            sOutput.Append(arrInput[i].ToString("X2"));
        }
        return sOutput.ToString();
    }

    void InitEntityNeuralNetworks()
    {
        GatherPersistence();

        if (populationSize % 2 != 0)
            populationSize++;

        nets = new List<NeuralNetwork>();

        for (int i = 0; i < populationSize; i++)
        {
            // If no weights were loaded, create random network
            if (bestGenome == "")
            {
                NeuralNetwork net = new NeuralNetwork(layers, null);
                //Debug.Log("* Creating net: " + i + " of " + populationSize);

                net.layers = layers;
                net.mutVarSize = mutVarSize;
                net.ResetGenome();
                net.CopyWeights(net.RandomizeWeights());
                net.droppedNeurons = net.RandomizeDroppedNeurons(dropChance);
                //net.mutatableVariables = net.RandomizeMutVars();
                net.mutatableVariables[0] = 0.25f;

                nets.Add(net);
            }
            // Else load persistence weights
            else
            {
                NeuralNetwork net = new NeuralNetwork(persistenceNetwork);
                //Debug.Log("* Creating net: " + i + " of " + populationSize);

                net.layers = layers;
                net.mutVarSize = mutVarSize;
                net.genome = bestGenome;
                net.mutatableVariables = persistenceNetwork.mutatableVariables;

                //Array.Copy(persistenceNetwork.mutatableVariables, net.mutatableVariables, mutVarSize);

                nets.Add(net);
            }
        }
        //nets[0].isBest = true;

        if (bestGenome == "")
        {
            bestGenome = nets[0].GenerateGenome();
            persistenceNetwork.genome = bestGenome;
            persistenceNetwork.mutatableVariables[0] = 0.25f;
            persistenceNetwork.CopyWeights(persistenceNetwork.RandomizeWeights());
            persistenceNetwork.droppedNeurons = persistenceNetwork.RandomizeDroppedNeurons(dropChance);
            //persistenceNetwork.mutatableVariables = persistenceNetwork.RandomizeMutVars();
            //persistenceNetwork.RandomizeWeights();
        }

        //startup = false;
    }

    double[][] NormalizeData(double[][] input, double min, double max)
    {
        // Normalize the values between 0.0 and 1.0 based on min and max
        for (int i = 0; i < input.Length; i++)
        {
            for (int p = 0; p < input[i].Length; p++)
            {
                input[i][p] = (input[i][p] - min) / (max - min);
            }
        }

        return input;
    }

    double DeNormalize(double normalized, double min, double max)
    {
        return (normalized * (max - min) + min);
    }
    double Normalize(double unnormalized, double min, double max)
    {
        return (unnormalized - min) / (max - min);
    }

    void GetMinMax(double[][] input, out double min, out double max)
    {
        min = double.MaxValue;
        max = -double.MaxValue;

        // Get the minimum and maximum values first
        for (int i = 0; i < input.Length; i++)
        {
            for (int p = 0; p < input[i].Length; p++)
            {
                if (input[i][p] < min)
                    min = input[i][p];
                if (input[i][p] > max)
                    max = input[i][p];
            }
        }
    }

    void GatherPersistence()
    {
        try
        {
            int[] layersWithBiases = layers;
            persistenceNetwork = new NeuralNetwork(layersWithBiases, null);

            #region OLD LOADING SYSTEM
            // OLD LOADING SYSTEM
            //// Load weights data into `persistenceNetwork`
            //BinaryFormatter bf = new BinaryFormatter();
            //using (FileStream fs = new FileStream("./Assets/dat/WeightSave.bin", FileMode.Open))
            //    persistenceNetwork.weights = (double[][][])bf.Deserialize(fs);
            //// Load mutVar data into `persistenceNetwork`
            //BinaryFormatter bf2 = new BinaryFormatter();
            //using (FileStream fs2 = new FileStream("./Assets/dat/MutVars.bin", FileMode.Open))
            //    persistenceNetwork.mutatableVariables = (double[])bf2.Deserialize(fs2);
            //// Load dropped neurons data into `persistenceNetwork`
            //BinaryFormatter bf3 = new BinaryFormatter();
            //using (FileStream fs3 = new FileStream("./Assets/dat/DroppedNeurons.bin", FileMode.Open))
            //    persistenceNetwork.droppedNeurons = (bool[][])bf3.Deserialize(fs3);
            ////bestMutVars = persistenceNetwork.mutatableVariables;

            //// Get hash of best weights
            //bestHash = ByteArrayToString(new MD5CryptoServiceProvider().ComputeHash(System.IO.File.ReadAllBytes("./Assets/dat/WeightSave.bin")));
            #endregion

            // NEW LOADING SYSTEM

            // Load save data into persistence network
            SaveData sd = new SaveData();
            BinaryFormatter bf3 = new BinaryFormatter();
            using (FileStream fs3 = new FileStream("./Assets/dat/NetworkSaveData.bin", FileMode.Open))
                sd = (SaveData)bf3.Deserialize(fs3);
            persistenceNetwork.weights = sd.weights;
            persistenceNetwork.mutatableVariables = sd.mutVars;
            persistenceNetwork.droppedNeurons = sd.droppedNeurons;
            bestDroppedNeuronsAmnt = persistenceNetwork.CountDroppedNeurons();


            // Load metadata like best error and generation
            StreamReader sr = File.OpenText("./Assets/dat/WeightSaveMeta.mta");
            string firstLine = sr.ReadLine().Trim();
            currentGeneration = int.Parse(firstLine.Split('#')[0]) + 1;
            bestEverError = double.Parse(firstLine.Split('#')[1]);
            timeManager.offsetTime = int.Parse(firstLine.Split('#')[2]);
            bestGenome = firstLine.Split('#')[3];
            persistenceNetwork.genome = bestGenome;
            sr.Close();

        }
        catch (Exception)
        {
            Debug.LogWarning("Failed to load network data, possible mismatch in size?");
        }
    }

    static void Upload(float fitness)
    {
        File.Copy("./Assets/dat/WeightSave.dat", "./Assets/dat/" + fitness + "_WeightSave.dat");
        Debug.Log("* Copied \"./Assets/dat/WeightSave.dat\" to \"./Assets/dat/" + fitness + "_WeightSave.dat\"");
        File.Copy("./Assets/dat/WeightSaveMeta.mta", "./Assets/dat/" + fitness + "_WeightSaveMeta.mta");
        Debug.Log("* Copied \"./Assets/dat/WeightSaveMeta.mta\" to \"./Assets/dat/" + fitness + "_WeightSaveMeta.mta\"");

        // Upload weight save
        Debug.Log("* Uploading \"./Assets/dat/" + fitness + "_WeightSave.dat\" to http://achillium.us.to/digitneuralnet/");
        System.Net.WebClient Client = new System.Net.WebClient();
        Client.Headers.Add("enctype", "multipart/form-data");
        byte[] result = Client.UploadFile("http://achillium.us.to/digitneuralnet/uploadweights.php", "POST", "./Assets/dat/" + fitness + "_WeightSave.dat");
        string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);
        Debug.Log("* Uploaded \"./Assets/dat/" + fitness + "_WeightSave.dat\"");

        // Upload weight save meta
        Debug.Log("* Uploading \"./Assets/dat/" + fitness + "_WeightSaveMeta.mta\" to http://achillium.us.to/digitneuralnet/");
        System.Net.WebClient ClientTwo = new System.Net.WebClient();
        ClientTwo.Headers.Add("enctype", "multipart/form-data");
        byte[] resultTwo = ClientTwo.UploadFile("http://achillium.us.to/digitneuralnet/uploadweights.php", "POST", "./Assets/dat/" + fitness + "_WeightSaveMeta.mta");
        string sTwo = System.Text.Encoding.UTF8.GetString(resultTwo, 0, resultTwo.Length);
        Debug.Log("* Uploaded \"./Assets/dat/" + fitness + "_WeightSaveMeta.mta\"");

        File.Delete("./Assets/dat/" + fitness + "_WeightSave.dat");
        Debug.Log("* Deleted Copy at \"./Assets/dat/" + fitness + "_WeightSave.dat\"");
        File.Delete("./Assets/dat/" + fitness + "_WeightSaveMeta.mta");
        Debug.Log("* Deleted Copy at \"./Assets/dat/" + fitness + "_WeightSaveMeta.mta\"");

        Debug.Log("* Synced with server");
    }

    static void Download(string s)
    {
        System.Net.WebClient Client = new System.Net.WebClient();

        Debug.Log("* Downloading \"" + s + "_WeightSave.dat\" from http://achillium.us.to/digitneuralnet/" + s + "_WeightSave.dat");
        Client.DownloadFile(new Uri("http://achillium.us.to/digitneuralnet/" + s + "_WeightSave.dat"), @".\dat\temp_WeightSave.dat");
        Debug.Log("* Downloaded \"" + s + "_WeightSave.dat\"");
        Debug.Log("* Downloading \"" + s + "_WeightSaveMeta.mta\" from http://achillium.us.to/digitneuralnet/" + s + "_WeightSaveMeta.mta");
        Client.DownloadFile(new Uri("http://achillium.us.to/digitneuralnet/" + s + "_WeightSaveMeta.mta"), @".\dat\temp_WeightSaveMeta.mta");
        Debug.Log("* Downloaded \"" + s + "_WeightSaveMeta.mta\"");

        if (File.Exists("./Assets/dat/temp_WeightSave.dat"))
        {
            if (File.Exists("./Assets/dat/WeightSave.dat"))
                File.Delete("./Assets/dat/WeightSave.dat");
            File.Move("./Assets/dat/temp_WeightSave.dat", "./Assets/dat/WeightSave.dat");
        }
        if (File.Exists("./Assets/dat/temp_WeightSaveMeta.mta"))
        {
            if (File.Exists("./Assets/dat/WeightSaveMeta.mta"))
                File.Delete("./Assets/dat/WeightSaveMeta.mta");
            File.Move("./Assets/dat/temp_WeightSaveMeta.mta", "./Assets/dat/WeightSaveMeta.mta");
        }

        StreamReader sr = File.OpenText("./Assets/dat/WeightSaveMeta.mta");
        string firstLine = sr.ReadLine().Trim();
        string currentGen = firstLine.Split('#')[0];
        int currentGeneration = int.Parse(currentGen) + 1;
        sr.Close();
        StreamWriter persistence = new StreamWriter("./Assets/dat/WeightSaveMeta.mta");
        persistence.WriteLine((currentGeneration).ToString() + "#" + s);
        persistence.Close();

        Debug.Log("* Synced with server");
    }
}
