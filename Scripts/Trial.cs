using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IVLab.Plotting;
using IVLab.Utilities;
using UnityEngine.UI;

///<summary> 
///This class instantiates the markers for one trial and contains methods to highlight and update the markers
///</summary>
public class Trial : Manager3D
{
    private const int MaxNumFrames = 1800; //maximum number of frames for a trial

    private string fileName; //file path to csv containing marker data

    private int numMarkers; //number of markers selected for this trial

    private Vector3 scaleChange = new Vector3(-0.75f, -0.75f, -0.75f); //vector to change the scale of the marker spheres

    private GameObject[] colorSpheres;  //array of sphere prefabs to be instantiated

    private Color[] colorArray;   //array of colors corresponding to GameObject array

    //parents for sphere and NaN objects
    private GameObject sphereParent; //parents for sphere  objects
    private GameObject NaNParent; //parents for NaN sphere objects

    //highlighted and default materials
    private Material defMat, highlightMat;

    //array of lists for spheres and their renderers
    private List<GameObject>[] dataObjects;
    private List<MeshRenderer>[] renderers;

    private Camera cam;  //camera for 3D view
    DataManager dataManager; //dataManager object for measurement data

    private bool enableClick = false; //true if click selection is enabled
    private bool enableBrush = false; //true if brush selection is enabled
    private bool enableSlider = false; //true if time slider selection is enabled

    private bool[] markerSelection; //an array containing values that are true if the marker at that index is selected

    private int trialNum; //number representing the order of the trial corresponding to these poses, ex. the first trial created would be 0, the second  trial created would be 1
    private int offset = MaxNumFrames;  //offset used for going between linked indices for poses for one trial and data table of all measurement data
    int lastFrame; //the height of the data table of marker values i.e. the index one greater than the last index for this trail
    int maxTrialIndex; //this is the max value for the index on the large data table of measurement data that starts the next trial after this one
    Slider slide; //time slider

    private bool altSelectMode = false; //true of alternate select mode for time slider should be used
    private int newStartVal; //the starting index used by the alternate select mode for the time slider


    ///<summary> 
    ///This method initializes the fileName, cam, dataMAnager, numMarkers,defat, hilightMat, and markerSelection variables
    ///</summary>
    public void setData(string trialCSV, bool[] markerSelection, int numSelected, Camera cam, DataManager manager, Material def, Material highlighted)
    {
        fileName = trialCSV;
        this.cam = cam;
        dataManager = manager;
        numMarkers = numSelected;
        defMat = def;
        highlightMat = highlighted;
        this.markerSelection = markerSelection;
    }

    ///<summary> 
    ///This method initializes parent game objectes for the spheres and NaN spheres
    ///</summary>
    public void setContainer(GameObject sphere, GameObject
        NaN)
    {
        sphereParent = sphere;
        NaNParent = NaN;

    }

    ///<summary> 
    ///This method initializes the color array for the markers
    ///</summary>
    public void setColor(Color[] colors)
    {
        colorArray = colors;

    }

    ///<summary> 
    ///This method initializes the trialNum and timeSlide variables
    ///</summary>
    public void setLinking(int trialNum, Slider timeSlide)
    {
        this.trialNum = trialNum;
        slide = timeSlide;
        maxTrialIndex = (trialNum * offset) + offset; //this is the index that starts the next trial
    }


    public void renderSpheres()
    {
        //initializes list of sphere and renderer arrays
        dataObjects = new List<GameObject>[numMarkers];
        renderers = new List<MeshRenderer>[numMarkers];

        for (int i = 0; i < numMarkers; ++i)
        {
            dataObjects[i] = new List<GameObject>();
            renderers[i] = new List<MeshRenderer>();
        }


        // creates array of prefab spheres
        colorSpheres = new GameObject[numMarkers];


        //creates prefab spheres and adds them to arrays
        for (int i = 0; i < numMarkers; ++i)
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            temp.transform.localScale += scaleChange;
            var renderer = temp.GetComponent<Renderer>();
            renderer.material.SetColor("_Color", colorArray[i]);//gets color for sphere
            BoxCollider boxCollider = temp.AddComponent<BoxCollider>();
            colorSpheres[i] = temp; // adds sphere to array
        }

        int columnNum = 0;  //column of the csv that is being accessed
        int selectedCount = 0; //keeps track of how many marker trails have been instantiated

        //accesses csv to instantiates points for each marker
        DataTable dataTable = new DataTable(fileName, false, false);
        lastFrame = dataTable.Height;

        for (int n = 0; n < totalNumMarkers; ++n) //iterates through all possible markers and checks if they should be displayed
        {
            int valueCheck = columnNum / 3;

            if (markerSelection[valueCheck]) //entered if this marker should be displayed
            {

                GameObject point = colorSpheres[selectedCount]; //gets the correct prefab sphere

                //instantiates each point for one marker
                for (int i = 0; i <= dataTable.Height - 1; ++i)
                {

                    int tempNum = columnNum;

                    //gets x,y, and z values
                    float xPos = dataTable.Data(i, tempNum); //[tempNum][i];
                    ++tempNum;
                    float yPos = dataTable.Data(i, tempNum);// Data[tempNum][i];
                    ++tempNum;
                    float zPos = dataTable.Data(i, tempNum); //Data[tempNum][i];


                    if (!(float.IsNaN(xPos)) && !(float.IsNaN(yPos)) && !(float.IsNaN(zPos))) //entered if there are no NaNs
                    {

                        Vector3 position = new Vector3(xPos, yPos, zPos);

                        Vector3 transformed = XROMMCoordinates.ToUnity(position); //transforms vector to correct coordinates

                        GameObject temp = Instantiate(point, transformed, Quaternion.identity) as GameObject; //instantiates sphere
                        temp.transform.SetParent(sphereParent.transform); //sets parent
                        dataObjects[selectedCount].Add(temp); //adds to array of data objects
                        renderers[selectedCount].Add(temp.GetComponent<MeshRenderer>()); //adds to array of renderers

                    }
                    else //entered if there is a NaN
                    {

                        GameObject temp = Instantiate(point, new Vector3(100, 100, 100), Quaternion.identity) as GameObject;
                        temp.transform.SetParent(NaNParent.transform);
                        dataObjects[selectedCount].Add(temp);
                        renderers[selectedCount].Add(temp.GetComponent<MeshRenderer>());
                        temp.SetActive(false);
                    }
                }
                ++selectedCount;
            }
            columnNum += 3; //moves to the columns containing data for the next marker
        }


        //makes the prefab spheres not visible
        for (int i = 0; i < colorSpheres.Length; ++i)
        {
            colorSpheres[i].SetActive(false);
        }

    }
    void Start()
    {
    }

    ///<summary> 
    //This method enables click selection and disables all other selection methods
    ///</summary
    public void enableClickSelection()
    {
        enableClick = true;
        enableBrush = false;
        enableSlider = false;
    }

    ///<summary> 
    //This method enables brush selection and disables all other selection methods
    ///</summary
    public void enableBrushSelection()
    {
        enableBrush = true;
        enableClick = false;
        enableSlider = false;
    }

    ///<summary> 
    //This method enables time slider selection and disables all other selection methods
    ///</summary
    public void enableSlideSelection()
    {
        enableBrush = false;
        enableClick = false;
        enableSlider = true;
    }



    ///<summary> 
    ///This method controls the click selection mode for the marker spheres
    ///</summary
    void clickSelection()
    {
        if (Input.GetMouseButtonDown(0)) //entered if the user clicks
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                int hitSphereIndex = -1;
                for (int i = 0; i < numMarkers; ++i)
                {
                    for (int j = 0; j < dataObjects[i].Count; ++j) // iterates through every marker sphere
                    {
                        if (hit.collider.gameObject == dataObjects[i][j]) //checks if hit object is a marker sphere
                        {
                            hitSphereIndex = j; //stores index of sphere
                        }
                    }
                    if (hitSphereIndex != -1) //entered if a marker was hit
                    {
                        int temp = hitSphereIndex + (trialNum * offset);  //calculates the index used for linked indices
                        dataManager.LinkedIndices[temp].Highlighted = true;
                        break;
                    }
                }

            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl)) //clears selection with control key
        {
            for (int i = 0; i < numMarkers; ++i)
            {
                for (int j = 0; j < dataObjects[i].Count; ++j)  // iterates through every marker sphere
                {
                    int temp = j + (trialNum * offset);  //calculates the index used for linked indices
                    dataManager.LinkedIndices[temp].Highlighted = false;
                }
            }
        }

    }

    ///<summary> 
    ///This method controls the brush selection mode for the marker spheres
    ///</summary
    void brushSelection()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl)) //clears selection with control key
        {
            for (int i = 0; i < numMarkers; ++i)
            {
                for (int j = 0; j < dataObjects[i].Count; ++j) // iterates through every marker sphere
                {
                    int temp = j + (trialNum * offset); //calculates the index used for linked indices
                    dataManager.LinkedIndices[temp].Highlighted = false;
                }
            }
        }
        else if (Input.GetKey(KeyCode.LeftAlt)) //enables selection
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                for (int i = 0; i < numMarkers; ++i)
                {
                    for (int j = 0; j < dataObjects[i].Count; ++j) // iterates through every marker sphere
                    {
                        if (hit.collider.gameObject == dataObjects[i][j]) //checks if hit object is a marker sphere
                        {
                            int temp = j + (trialNum * offset); //calculates the index used for linked indices
                            dataManager.LinkedIndices[temp].Highlighted = true;
                        }
                    }
                }
            }
        }

    }


    ///<summary> 
    ///The Update method checks if various selection modes are enabled and then calls the correct selection method
    ///</summary
    void Update()
    {


        if (enableClick)
        {
            clickSelection();

        }
        else if (enableBrush)
        {
            brushSelection();
        }

        if (enableSlider)
        {
            sliderSelect();
        }


    }

    private void Awake()
    {

    }


    ///<summary> 
    ///This method controls the time slider selection mode
    ///</summary
    public void sliderSelect()
    {

        if (Input.GetKeyDown(KeyCode.LeftControl) || (slide.value == -1)) //entered if control key is pressed or when the slider is at initial value of -1; clears highlited selection
        {
            for (int i = 0; i < numMarkers; ++i)
            {
                for (int j = 0; j < offset; ++j)
                {
                    int temp = j + (trialNum * offset); //calculates the index used for linked indices
                    dataManager.LinkedIndices[temp].Highlighted = false;
                }
            }
            slide.value = 0;
            altSelectMode = false;
            enableSlider = false;
        }
        else if (Input.GetKeyDown(KeyCode.LeftAlt)) //activates alternative selection mode to select from middle of time slider
        {
            if (slide.value != -1)
            {
                altSelectMode = true;
                newStartVal = (int)slide.value + (trialNum * offset); //calculates new start value for indices

                //unhighlights frames before marked point
                for (int j = (trialNum * offset); j < newStartVal; ++j) //unhighlights all indices before new start value
                {
                    dataManager.LinkedIndices[j].Highlighted = false;

                }
            }
        }
        else
        {
            if (slide.value != -1)
            {
                int sliderValue = (int)slide.value;
                int temp = sliderValue + (trialNum * offset);  //calculates the index used for linked indices

                if (altSelectMode) //entered when in alt select mode
                {

                    for (int j = newStartVal; j < temp; ++j)
                    {
                        dataManager.LinkedIndices[j].Highlighted = true;

                    }
                    for (int j = temp + 1; j < maxTrialIndex; ++j)
                    {
                        dataManager.LinkedIndices[j].Highlighted = false;
                    }

                }
                else //entered when in regular select mode
                {

                    for (int j = (trialNum * offset); j <= temp; ++j)
                    {
                        dataManager.LinkedIndices[j].Highlighted = true;

                    }
                    for (int j = temp + 1; j < maxTrialIndex; ++j)
                    {
                        dataManager.LinkedIndices[j].Highlighted = false;
                    }
                }
            }
        }
    }

    ///<summary> 
    ///This method overrides the UpdateDataPoint method in LinkedIndice
    ///</summary
    public override void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes linkedAttributes)
    {
        // Perform different actions according to the current state of linked indices


        index -= trialNum * offset; //changes the index from linked indices into the index used for a trial

        if ((index >= 0) && (index < lastFrame))
        { //checks that index is within the bounds of the marker data (some marker data csvs may not have 1800 frames of data)

            // If this data point is masked . . .
            if (linkedAttributes.Masked)  //entered if data point is masked
            {
                for (int i = 0; i < numMarkers; ++i)
                {
                    renderers[i][index].enabled = false;
                }
            }
            // If this data point is highlighted . . .
            else if (linkedAttributes.Highlighted)
            {

                for (int i = 0; i < numMarkers; ++i)
                {
                    renderers[i][index].enabled = true;
                    renderers[i][index].material = highlightMat; //changes marker spheres material to the highlighted material
                }
            }
            // Otherwise . . .
            else
            {
                for (int i = 0; i < numMarkers; ++i)
                {
                    renderers[i][index].enabled = true;
                    renderers[i][index].material = defMat;
                    renderers[i][index].material.SetColor("_Color", colorArray[i]); //turns marker sphere back to its original color

                }
            }
        }
    }
}

