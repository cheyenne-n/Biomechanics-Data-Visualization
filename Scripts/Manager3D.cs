using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IVLab.Plotting;
using UnityEngine.UI;


///<summary> 
///This class creates the load screen for data and controls the creation of trial and pose objects
///</summary>
public class Manager3D : LinkedData
{


    public const int totalNumMarkers = 59; //total number of markers
    [SerializeField] Camera cam; //camera for 3D view
    [SerializeField] DataManager dataManager; //data manager object for 2D measurement dat
    [SerializeField] public Material defaultMat, highlightedMat;//highlighted and default materials                                                                 
    [SerializeField] private GameObject nanParent; //parent for NaN spheres
    [SerializeField] private Slider timeSlider; //time slider
    [SerializeField] private Canvas bottomBar; //menu bar for gameplay

    private bool[] markerSelection; //array of bool values that are true if the corresponding marker should be visualized

    private bool initialized = false; //indicates that files have been loaded;


    [SerializeField] TMPro.TMP_InputField input; //input field for file path on load screen
    [SerializeField] GameObject canvas; //canvas for load screen
    [SerializeField] Toggle tog; //prefab toggle for load screen

    GameObject loadObj; //game object used for load screen;
    AnimalData loadScreen; //AnimalData object that controls the load screen

    List<Trial> trialList = new List<Trial>(); //list of Trial objectes that have been created
    List<PoseSet> poseList = new List<PoseSet>(); //list of Pose objects that have been created

    //colors used for the cluster plot colors
    Color[] clusterColorArray = new Color[100];
    private Color orange3 = new Color32(255, 149, 5, 255); //yellow orange
    private Color purple2 = new Color32(153, 95, 163, 255); //purpureus
    private Color green4 = new Color32(15, 96, 23, 255); //pea green
    private Color pink3 = new Color32(247, 163, 153, 255); //salmon
    private Color blue1 = new Color32(47, 35, 146, 255); //salmon


    List<GameObject> trialParents = new List<GameObject>();
    private Color groundColor = new Color(0.4056604f, 0.3775958f, 0.3775958f);


    // Start is called before the first frame update
    ///<summary> 
    ///This Start creates a loadScreen using an AnimalData object and sets the input field, canvas, and prefab toggle
    ///</summary>
    void Start()
    {
        loadObj = new GameObject();
        loadScreen = loadObj.AddComponent<AnimalData>(); //creates a load screen that will get file names and information
        loadScreen.setData(input, canvas, tog); //initializes loadScreen
    }

    ///<summary> 
    ///This method is called by the begin button and creates the ground plane and Trial and PosesSet objects;
    ///</summary>
    public void beginVis()
    {

        if (initialized) //checks if data is initialized
        {

            //creates ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Vector3 groundScale = new Vector3(10f, 10f, 10f);
            ground.transform.localScale += groundScale;
            var groundRenderer = ground.GetComponent<Renderer>();
            groundRenderer.material.SetColor("_Color", groundColor);
            ground.transform.position = new Vector3(0f, -1.21f, 0f);
            ground.name = "ground";

            //sets load canvas to be inactive
            canvas.SetActive(false);



            SortedDictionary<int, string> measurementD = loadScreen.getMeasurement(); //gets dictionary of trial ID numbers and filepaths to measurement csvs
            DataTable[] tableArray = new DataTable[measurementD.Count];//creates an array of data tables created from each measurement file

            int numTrialsSelected = measurementD.Count; //number of trials that have been selected
            float[] trialNames = new float[numTrialsSelected]; //array of trial IDs

            //initializes array of trial ID nums
            int nameCount = 0;
            foreach (var item in measurementD.Keys)
            {
                trialNames[nameCount] = (float)item;

                ++nameCount;
            }


            //Initializes array of data tables
            int count = 0;
            foreach (var v in measurementD)
            {

                DataTable table = new DataTable(v.Value, false, false);
                tableArray[count] = table;
                ++count;
            }



            //Creates queue of colors for cluster plot
            Queue<Color> clusterPalette = new Queue<Color>();
            clusterPalette.Enqueue(purple2);
            clusterPalette.Enqueue(orange3);
            clusterPalette.Enqueue(green4);
            clusterPalette.Enqueue(pink3);
            clusterPalette.Enqueue(blue1);

            for (int i = 0; i < 100; ++i)
            {
                Color temp = clusterPalette.Dequeue();
                clusterColorArray[i] = temp;
                clusterPalette.Enqueue(temp);
            }


            //creates a combined trial table using array of data tables and array of trial ID names
            DataTable combinedTrialTable = PlottingUtilities.ClusterDataTables(tableArray, trialNames, clusterColorArray, "Trial");

            dataManager.DataTable = combinedTrialTable;

            markerSelection = loadScreen.getMarkerSelection(); //gets array of bool values corresponding to selected markers



            int numMarkersSelected = 0; //number of markers that have been selected


            foreach (var v in markerSelection)
            {
                if (v == true)
                {
                    ++numMarkersSelected; //incrementes variable if a marker has been selected
                }
            }


            SortedDictionary<int, string> markerD = loadScreen.getMarkers(); //gets dictionary of trial ID numbers and filepaths to marker data csvs
            SortedDictionary<int, string> poseD = loadScreen.getPoseData(); //gets dictionary of trial ID numbers and filepaths to directories of pose objs


            int trialCount = 0;  //number representing the order of the trials created, ex. the first trial created would be 0, the second  trial created would be 1
            foreach (var v in markerD)
            {
                GameObject obj = new GameObject();
                Trial run = obj.AddComponent<Trial>();

                run.setData(v.Value, markerSelection, numMarkersSelected, cam, dataManager, defaultMat, highlightedMat); //sets data for the trial

                GameObject trialPar = new GameObject(); //creates parent for trial

                run.setContainer(trialPar, nanParent);
                trialPar.name = "Trial " + v.Key;
                trialParents.Add(trialPar);

                GameObject palette = new GameObject();
                ColorTheme colorGenerator = palette.AddComponent<ColorTheme>();
                run.setColor(colorGenerator.getColorArray(trialCount)); //sets color palette for trial

                run.setLinking(trialCount, timeSlider); //sets linking

                run.renderSpheres(); //calls method to create spheres

                ++trialCount;
                trialList.Add(run); //adds to list of trials

            }


            int trialPoseCount = 0; //number representing the order of the trial corresponding to these poses, ex. the first trial created would be 0, the second  trial created would be 1

            foreach (var v in markerD.Keys)
            {

                if (poseD.ContainsKey(v)) //checks if poses exist for this trial
                {

                    GameObject obj = new GameObject();
                    PoseSet poses = obj.AddComponent<PoseSet>();
                    poses.setTrial(trialPoseCount, cam, trialParents[trialPoseCount]); //sets trial info for pose object
                    poses.loadFile(poseD[v], dataManager); //loads filepath and dataManager for pose object
                    poseList.Add(poses); //add to list of poses 

                }

                ++trialPoseCount;
            }

        }
    }

    ///<summary> 
    ///This method is called by load button and calls on the load screen to load the file path
    ///</summary>
    public void loadAnimalData()
    {
        loadScreen.loadFilePath();
        initialized = true;
    }


    ///<summary> 
    ///This method overrides the UpdateDataPoint method in LinkedIndices
    ///</summary
    public override void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes linkedAttributes)
    {


        foreach (var v in trialList) //calls update method for every Trial created
        {
            v.UpdateDataPoint(index, linkedAttributes);
        }

        foreach (var v in poseList) //calls update method for every PoseSet created
        {
            v.UpdateDataPoint(index, linkedAttributes);
        }


    }

    ///<summary> 
    ///This method enables click selection  for every Trial and PoseSet and disables all other selection methods
    ///</summary
    public void ClickSelection()
    {
        foreach (var v in trialList)
        {
            v.enableClickSelection();
        }
        foreach (var v in poseList)
        {
            v.enableClickSelection();
        }

    }

    ///<summary> 
    ///This method enables brush selection  for every Trial and PoseSet and disables all other selection methods
    ///</summary
    public void BrushSelection()
    {
        foreach (var v in trialList)
        {
            v.enableBrushSelection();
        }
        foreach (var v in poseList)
        {
            v.enableBrushSelection();
        }

    }

    ///<summary> 
    ///This method enables time slider selectionfor every Trial and PoseSet and disables all other selection methods
    ///</summary
    public void sliderSelection()
    {
        foreach (var v in trialList)
        {
            v.enableSlideSelection();
        }

    }

    ///<summary> 
    ///This method is called by the separate trials button and wil causes the markers and poses for a trial to separate from other trials
    ///</summary
    public void separateTrials()
    {
        Vector3 translationVal = new Vector3(5, 0, 0);

        if (trialParents.Count % 2 == 0) //even number of trials
        {

            int midpoint = trialParents.Count / 2;

            for (int i = 0; i < midpoint; ++i) //trials before midpoint are translated in negative direction
            {

                trialParents[i].transform.position -= (translationVal * (i + 1));


            }
            int count = 0;
            for (int i = midpoint; i < trialParents.Count; ++i) //trials after midpoint are translated in positive direction
            {

                trialParents[i].transform.position += (translationVal * (count));

                ++count;
            }
        }
        else //odd number of trials
        {
            int midpoint = trialParents.Count / 2;

            for (int i = 0; i < midpoint; ++i)  //trials before midpoint are translated in negative direction
            {

                trialParents[i].transform.position -= (translationVal * (i + 1));


            }

            int count = 1;
            for (int i = midpoint + 1; i < trialParents.Count; ++i) //trials after midpoint are translated in positive direction
            {

                trialParents[i].transform.position += (translationVal * (count));

                ++count;
            }
        }




    }

    ///<summary> 
    ///This method is called by the reverse button and does the opposite of separateTrials method
    ///</summary
    public void unSeparateTrials()
    {
        Vector3 translationVal = new Vector3(5, 0, 0);

        if (trialParents.Count % 2 == 0) //even number of trials
        {

            int midpoint = trialParents.Count / 2;

            for (int i = 0; i < midpoint; ++i) //trials before midpoint are translated in positive direction
            {

                trialParents[i].transform.position += (translationVal * (i + 1));


            }
            int count = 0;
            for (int i = midpoint; i < trialParents.Count; ++i) //trials after midpoint are translated in negative directiona
            {

                trialParents[i].transform.position -= (translationVal * (count));

                ++count;
            }
        }
        else
        {
            int midpoint = trialParents.Count / 2;

            for (int i = 0; i < midpoint; ++i) //trials before midpoint are translated in positive direction
            {

                trialParents[i].transform.position += (translationVal * (i + 1));



            }

            int count = 1;
            for (int i = midpoint + 1; i < trialParents.Count; ++i) //trials after midpoint are translated in negative direction
            {

                trialParents[i].transform.position -= (translationVal * (count));

                ++count;
            }
        }

    }




    // Update is called once per frame
    void Update()
    {


    }
}
