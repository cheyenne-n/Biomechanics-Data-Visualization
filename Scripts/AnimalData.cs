using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IVLab.Plotting;
using System.IO;
using UnityEngine.UI;

///<summary> 
///This class controls the load data screen, marker and trial selection, and contains methods that return filepaths and information for the data from selected trials
///</summary>
public class AnimalData : MonoBehaviour
{
    private const int TotalNumMarkers = 59;
    TMPro.TMP_InputField fileInput; //input field for filepath to directory containing dta for one individual
    GameObject loadCanvas; //canvas for the load screen
    Toggle togglePrefab; //prefab for a toggle

    string measurementStr = "MeasurementData"; //name of folder containing measurement data
    string markerStr = "Markers"; //name of folder containing marker data
    string markerIDStr = "MarkerID";//name of folder containing marker ID
    string poseStr = "Poses"; //name of folder containing Pose data

    string[] markerNames; //array of marker names to be displayed on the toggles

    SortedDictionary<int, string> measurementDict = new SortedDictionary<int, string>(); //trial ID number and file path for measurement csvs;
    SortedDictionary<int, string> markerDataDict = new SortedDictionary<int, string>(); //trial ID number and file path for marker csvs;
    SortedDictionary<int, string> poseDict = new SortedDictionary<int, string>(); //trial ID number and file path for directories of poses for one trial;

    Toggle[] togArray = new Toggle[TotalNumMarkers]; //array of toggles used for marker selection
    bool[] markerSelect = new bool[TotalNumMarkers]; //array of bool values that are true if the toggle for the marker at that index is switched on

    List<Toggle> trialTogList = new List<Toggle>(); //list of toggles for trial selection
    List<bool> trialSelect = new List<bool>(); //a list of boolean values that indicate whether the trial toggle is on, the list is ordered from lowest to highest trial number

    List<Toggle> poseTogList = new List<Toggle>(); //list of toggles for pose selection
    List<bool> poseSelect = new List<bool>(); //a list of boolean values that indicate whether the pose toggle is on, the list is ordered from lowest to highest trial number


    ///<summary> 
    ///The Start array initializes the markerSelect array with false bool values
    ///</summary>
    void Start()
    {

        //initializes markerSelect array
        for (int i = 0; i < 59; ++i)
        {
            markerSelect[i] = false;
        }

    }
    ///<summary> 
    ///This method initializes the input field, canvas, and toggle prefav
    ///</summary>
    public void setData(TMPro.TMP_InputField input, GameObject canvas, Toggle tog)
    {
        fileInput = input;
        loadCanvas = canvas;
        togglePrefab = tog;
    }


    ///<summary> 
    ///This method passes the filepath from the input field to helper methods that will instantiate toggles on the load screen
    ///</summary>
    public void loadFilePath()
    {
        string filePath = fileInput.text; //gets path from input field
        getPoses(filePath); //getPoses must be called first so poseDict can be initialized for measurementData method
        getMeasurementData(filePath);
        getMarkerData(filePath);
        getMarkerNames(filePath);
    }

    ///<summary> 
    ///This method initializes a dictionary of trial id numbers and filepaths to measurement csvs. It also creates trial toggles for each trial and pose toggles for each trial that has pose data
    ///</summary>
    private void getMeasurementData(string filePath) //gets measurement data and prints trial menu
    {

        string measurementPath = filePath + "/" + measurementStr; //gets filepath to directory of measurement csvs

        DirectoryInfo measurementDir = new DirectoryInfo(measurementPath);

        FileInfo[] measurementFiles = measurementDir.GetFiles("*.*");

        foreach (FileInfo f in measurementFiles)
        {
            //gets trial ID number from file name
            string name = f.ToString();
            int startPosition = (name.Length - 22);
            string trialStr = name.Substring(startPosition, 2);
            int trialNum = int.Parse(trialStr);

            //adds trial ID number and filepath to measurement csv to measurementDict
            measurementDict.Add(trialNum, name);


        }

        Vector3 togPos = new Vector3(-281, 135, 0); //initial position for trial toggles
        Vector3 poseTogPos = new Vector3(-225, 135, 0); //iniital position for pose toggles

        foreach (var item in measurementDict.Keys)
        {

            Toggle tog = Instantiate(togglePrefab);
            tog.transform.SetParent(loadCanvas.transform);
            tog.GetComponent<RectTransform>().anchoredPosition = togPos;

            togPos += new Vector3(0, -26, 0); //increments toggle position

            tog.GetComponentInChildren<Text>().text = "Trial " + item;
            tog.onValueChanged.AddListener(delegate { trialTogs(); });

            trialTogList.Add(tog);
            trialSelect.Add(false);


            if (poseDict.ContainsKey(item)) //entered if there are poses for that trial
            {
                Toggle poseTog = Instantiate(togglePrefab); //creates a pose toggle next to the trial toggle for trials that have pose data
                poseTog.transform.SetParent(loadCanvas.transform);
                poseTog.GetComponent<RectTransform>().anchoredPosition = poseTogPos;

                poseTog.GetComponentInChildren<Text>().text = "";
                poseTog.onValueChanged.AddListener(delegate { poseTogs(); });

                poseTogList.Add(poseTog);
                poseSelect.Add(false);

            }
            poseTogPos += new Vector3(0, -26, 0); //increments pose toggle position

        }
    }

    ///<summary> 
    ///This method initializes a dictionary of trial id numbers and filepaths to marker data csvs
    ///</summary>
    private void getMarkerData(string filePath)
    {
        string markerPath = filePath + "/" + markerStr; //filepath to directory of marker data csvs
        DirectoryInfo markerDir = new DirectoryInfo(markerPath);
        FileInfo[] markerFiles = markerDir.GetFiles("*.*");

        foreach (FileInfo f in markerFiles)
        {

            //gets trial ID number from filepath
            string name = f.ToString();
            int startPosition = (name.Length - 12);
            string trialStr = name.Substring(startPosition, 2);
            int trialNum = int.Parse(trialStr);

            //adds trial ID number and filepath to marker dat csv to markerDataDict
            markerDataDict.Add(trialNum, name);
        }

    }

    ///<summary> 
    ///This method initializes a dictionary of trial id numbers and filepaths to directories of pose objs for each trial 
    ///</summary>
    /// <remarks>
    /// <b>Must</b> be called before <see cref="AnimalData.getMeasurementData(string)()"/>.
    /// </remarks>
    private void getPoses(string filePath)
    {

        string posePath = filePath + "/" + poseStr; //creates filepath to directory of pose directories
        DirectoryInfo poseDir = new DirectoryInfo(posePath);
        DirectoryInfo[] poseDirs = poseDir.GetDirectories("*.*");

        foreach (DirectoryInfo f in poseDirs)
        {

            //gets trial ID number from filepath
            string name = f.ToString();
            int startPosition = (name.Length - 7);
            string trialStr = name.Substring(startPosition, 2);
            int trialNum = int.Parse(trialStr);

            //adds to a dictionary of trial id numbers and filepaths to directories of pose objs
            poseDict.Add(trialNum, name);
        }
    }

    ///<summary> 
    ///This method gets the marker names from the marker ID file and creates toggles for each markers 
    ///</summary>
    private void getMarkerNames(string filePath)
    {

        string IDPath = filePath + "/" + markerIDStr; //creates path to directory containing marker ID file
        DirectoryInfo markerIDDir = new DirectoryInfo(IDPath);
        FileInfo[] IDfile = markerIDDir.GetFiles("*.*");

        //creates a data table from the marker ID file and gets the marker names
        foreach (FileInfo f in IDfile)
        {
            DataTable dataTable = new DataTable(f.ToString(), true, false);

            for (int i = 0; i <= dataTable.Height - 1; ++i)
            {
                markerNames = dataTable.RowNames; //copies array of row names in to markerNames

            }
        }

        Vector3 togPos = new Vector3(-175, 135, 0); //initial position for marker toggles

        for (int i = 0; i < markerNames.Length; ++i)
        {
            if ((i % 11 == 0) && (i != 0)) //starts a new column of markers
            {
                togPos += new Vector3(100, +286, 0); //starts new column
            }

            Toggle tog = Instantiate(togglePrefab);
            tog.transform.SetParent(loadCanvas.transform);
            tog.GetComponent<RectTransform>().anchoredPosition = togPos;

            togPos += new Vector3(0, -26, 0); //increments y position of toggles

            tog.GetComponentInChildren<Text>().text = markerNames[i];
            tog.onValueChanged.AddListener(delegate { markerArray(); });
            togArray[i] = tog; //adds toggle to array
        }
    }

    ///<summary> 
    ///This method is called whenever the value of a marker toggle is changed and intializes an array that contains a value of true if the corresponding marker toggle is on
    ///</summary>
    private void markerArray()
    {


        for (int i = 0; i < togArray.Length; ++i) //iterates through array of marker toggles
        {

            if (togArray[i].isOn)
            {
                markerSelect[i] = true;
            }
            else
            {
                markerSelect[i] = false;
            }
        }

    }

    ///<summary> 
    ///This method is called whenever the value of a trial toggle is changed and intializes an array that contains a value of true if the corresponding trial toggle is on
    ///</summary>
    private void trialTogs()
    {
        for (int i = 0; i < trialTogList.Count; ++i)
        {

            if (trialTogList[i].isOn)
            {
                trialSelect[i] = true;
            }
            else
            {
                trialSelect[i] = false;
            }
        }

    }


    ///<summary> 
    ///This method is called whenever the value of a pose toggle is changed and intializes an array that contains a value of true if the corresponding pose toggle is on
    ///</summary>
    private void poseTogs()
    {
        for (int i = 0; i < poseTogList.Count; ++i)
        {

            if (poseTogList[i].isOn)
            {
                poseSelect[i] = true;
            }
            else
            {
                poseSelect[i] = false;
            }
        }

    }

    ///<summary> 
    ///This method returns a dictionary of trial ID numbers and filepaths to measurement csvs for only the selected trials
    ///</summary>
    public SortedDictionary<int, string> getMeasurement()
    {
        setMeasurementDict(); //removes unselected trials from measurementDict

        return measurementDict;

    }
    // Update is called once per frame

    ///<summary> 
    ///This method an array that contains true is the marker at that index has been selected
    ///</summary>
    public bool[] getMarkerSelection()
    {
        return markerSelect;
    }


    ///<summary> 
    ///This method returns a dictionary of trial ID numbers and filepaths to marker data csvs for only the selected trials
    ///</summary>
    public SortedDictionary<int, string> getMarkers()
    {
        setMarkerDict(); //removes unselected trials from markerDataDict

        return markerDataDict;
    }

    ///<summary> 
    ///This method returns a dictionary of trial ID numbers and filepaths to directories of pose objs for each trial 
    ///</summary>
    public SortedDictionary<int, string> getPoseData()
    {

        setPoseDict();  //removes unselected pose trials from poseDict

        return poseDict;
    }


    ///<summary> 
    ///This method removes data from unselected trials from measurementDict
    ///</summary>
    private void setMeasurementDict()
    {
        int count = 0;
        List<int> itemsToRemove = new List<int>(); //list of keys to be removed from the dictionary

        foreach (var item in measurementDict.Keys)
        {
            if (trialSelect[count] == false) //indicates that this trial is not selected
            {
                itemsToRemove.Add(item);
            }
            ++count;
        }

        //removes all keys in itemsToRemove
        foreach (var v in itemsToRemove)
        {
            measurementDict.Remove(v);

        }

    }


    ///<summary> 
    ///This method removes data from unselected trials from markerDataDict
    ///</summary>
    private void setMarkerDict()
    {

        int count = 0;
        List<int> itemsToRemove = new List<int>(); //list of keys to be removed from the dictionary


        foreach (var item in markerDataDict.Keys)
        {
            if (trialSelect[count] == false) //indicates that this trial is not selected
            {
                itemsToRemove.Add(item);
            }
            ++count;
        }

        //removes all keys in itemsToRemove
        foreach (var v in itemsToRemove)
        {
            markerDataDict.Remove(v);

        }

    }

    ///<summary> 
    ///This method removes data from unselected trials from poseDict
    ///</summary>
    private void setPoseDict()
    {

        int count = 0;
        List<int> itemsToRemove = new List<int>(); //list of keys to be removed from the dictionary


        foreach (var item in poseDict.Keys)
        {
            if (poseSelect[count] == false) //indicates that poses from this trial are not selected
            {
                itemsToRemove.Add(item);
            }
            ++count;
        }

        //removes all keys in itemsToRemove
        foreach (var v in itemsToRemove)
        {
            poseDict.Remove(v);

        }

    }
    void Update()
    {

    }
}
