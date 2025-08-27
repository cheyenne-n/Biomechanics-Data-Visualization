using IVLab.OBJImport;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using IVLab.Plotting;
using IVLab.Utilities;
using UnityEngine.UI;

///<summary> 
///This class instantiates the skeletal poses for one trial and contains methods for their selection and updating
///</summary>
public class PoseSet : LinkedData
{

    private const int MaxNumFrames = 1800; //maximum number of frames for a trial
    List<int> poseFrames = new List<int>(); // a list of frames for which there are poses

    private string filePath;  // file path to a directory containing pose objs for one trial

    private GameObject[] dataObjects = new GameObject[MaxNumFrames]; //array of pose Game Objects
    private MeshRenderer[] renderers = new MeshRenderer[MaxNumFrames]; //array of Mesh Renderers for pose Game Objects

    private Camera cam; //camera used for 3D view

    DataManager dataManager; //data manager object that controls measurement data

    Dictionary<int, List<MeshRenderer>> poseMap = new Dictionary<int, List<MeshRenderer>>(); // dictionary of  indices (frame number - 1) and mesh renderers for pose Game Objects

    Dictionary<GameObject, int> objectToIndex = new Dictionary<GameObject, int>(); // dictionary of pose Game Objects and indices (frame number - 1) 

    bool initialized = false; //true if PoseSet object has been initialized
    bool enableClick = false; //true if click selection mode is entered
    bool enableBrush = false; //true if brush selection mode is entered


    // Start is called before the first frame update
    private int trialNum; //number representing the order of the trial corresponding to these poses, ex. the first trial created would be 0, the second  trial created would be 1
    private int offset = MaxNumFrames; //offset used for going between linked indices for poses for one trial and data table of all measurement data

    Color highlightedColor = new Color32(55, 189, 238, 255); //color of the highlighted bones

    GameObject trialBin; //Game object that is the parent of all poses for a particular trial



    void Start()
    {


    }


    ///<summary> 
    ///This method sets values for trialNum, camera, and poseParent. trialNum is a number representing the order of the trial, ex. the first trial created would be 0
    ///</summary
    public void setTrial(int num, Camera cam, GameObject poseParent)
    {
        trialNum = num;
        this.cam = cam;
        trialBin = poseParent;

    }

    ///<summary> 
    ///This method initializes dataManager and filePath and
    ///</summary
    public void loadFile(string fileString, DataManager manager)
    {

        dataManager = manager;
        filePath = fileString;

        DirectoryInfo dir = new DirectoryInfo(filePath);
        FileInfo[] info = dir.GetFiles("*.*");

        foreach (FileInfo f in info) //iterates through every pose obj in the directory
        {

            //gets frame number from file name
            string name = f.ToString();
            int startPosition = (name.Length - 8);
            string frameNum = name.Substring(startPosition, 4);
            int frame = int.Parse(frameNum);

            poseFrames.Add(frame);

            List<MeshRenderer> curRenderer = new List<MeshRenderer>(); //list of renderers for the children of every pose Game Object

            GameObject newBone = new OBJLoader().Load(f.ToString()); //creates an obj for one pose


            //transforms the pose to the correct set of coordinates
            Matrix4x4 matrix = Matrix4x4.TRS(newBone.transform.position, newBone.transform.rotation, newBone.transform.localScale);

            Matrix4x4 newMatrix = XROMMCoordinates.ToUnity(matrix);

            Vector3 newPosition = newMatrix.GetColumn(3);
            Quaternion newRotation = newMatrix.rotation;
            Vector3 newScale = newMatrix.lossyScale;

            newBone.transform.position = newPosition;
            newBone.transform.rotation = newRotation;
            newBone.transform.localScale = newScale;

            newBone.transform.SetParent(trialBin.transform); //sets the poses parent

            for (int i = 0; i < newBone.transform.childCount; ++i) //iterates through every child of the pose game object
            {
                MeshRenderer childRenderer = newBone.transform.GetChild(i).GetComponent<MeshRenderer>(); //gets Mesh Renderer
                BoxCollider collider = newBone.transform.GetChild(i).gameObject.AddComponent<BoxCollider>(); //adds box collider
                curRenderer.Add(childRenderer);
            }
            BoxCollider boxCollider = newBone.AddComponent<BoxCollider>(); //adds box collider to pose Game Object

            poseMap.Add((frame - 1), curRenderer); //adds index and rendererer to poseMap
            objectToIndex.Add(newBone, (frame - 1)); //adds pose Game Object and index to objectToIndex

        }
        initialized = true;
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
        if (enableBrush)
        {
            brushSelection();

        }

    }

    ///<summary> 
    ///This method enables click selection and disables all other selection methods
    ///</summary
    public void enableClickSelection()
    {
        enableClick = true;
        enableBrush = false;
    }

    ///<summary> 
    ///This method enables brush selection and disables all other selection methods
    ///</summary
    public void enableBrushSelection()
    {
        enableClick = false;
        enableBrush = true;
    }


    ///<summary> 
    ///This method controls the click selection mode for the poses
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

                foreach (GameObject pose in objectToIndex.Keys)
                {
                    for (int i = 0; i < pose.transform.childCount; ++i) //iterates through the children of each pose objext
                    {
                        if (hit.collider.gameObject == pose.transform.GetChild(i).gameObject) //checks if hit object is a pose
                        {
                            hitSphereIndex = objectToIndex[pose]; //stores index of pose
                        }
                    }

                }


                if (hitSphereIndex != -1) //entered if a pose was hit
                {
                    int temp = hitSphereIndex + (trialNum * offset); //calculates the index used for linked indices
                    dataManager.LinkedIndices[temp].Highlighted = true;

                }
            }

        }
        else if (Input.GetKeyDown(KeyCode.LeftControl)) //clears selection with control key
        {
            foreach (int frame in poseMap.Keys)
            {
                int temp = frame + (trialNum * offset);  //calculates the index used for linked indices
                dataManager.LinkedIndices[temp].Highlighted = false;
            }

        }

    }

    ///<summary> 
    ///This method controls the brush selection mode for the poses
    ///</summary
    void brushSelection()
    {

        if (Input.GetKeyDown(KeyCode.LeftControl)) //clears selection
        {
            foreach (int frame in poseMap.Keys)
            {
                int temp = frame + (trialNum * offset); //calculates the index used for linked indices
                dataManager.LinkedIndices[temp].Highlighted = false;
            }

        }
        else if (Input.GetKey(KeyCode.LeftAlt)) //enables selection
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                foreach (GameObject pose in objectToIndex.Keys)
                {
                    for (int i = 0; i < pose.transform.childCount; ++i)  //iterates through the children of each pose objext
                    {

                        if (hit.collider.gameObject == pose.transform.GetChild(i).gameObject) //entered if a pose was hit
                        {
                            int temp = objectToIndex[pose] + (trialNum * offset); //calculates the index used for linked indices
                            dataManager.LinkedIndices[temp].Highlighted = true;
                        }
                    }

                }
            }
        }

    }


    ///<summary> 
    ///This method overrides the UpdateDataPoint method in LinkedIndices
    ///</summary
    public override void UpdateDataPoint(int index, LinkedIndices.LinkedAttributes linkedAttributes)

    {

        index -= trialNum * offset; //changes the index from linked indices into the index used for a trial
        if ((index >= 0) && (index < MaxNumFrames))
        {

            if (initialized) //checks that the poses have been initialized
            {

                if (linkedAttributes.Masked) //entered if data point is masked
                {

                    if (poseMap.TryGetValue(index, out List<MeshRenderer> renderList)) //checks if a pose exists for this index
                    {
                        for (int j = 0; j < renderList.Count; ++j) //iterates through the renderers for each of a poses's children
                        {
                            renderList[j].enabled = false;
                        }
                    }

                }
                // If this data point is highlighted . . .
                else if (linkedAttributes.Highlighted)
                {

                    if (poseMap.TryGetValue(index, out List<MeshRenderer> renderList)) //checks if a pose exists for this index
                    {

                        for (int j = 0; j < renderList.Count; ++j) //iterates through the renderers for each of a poses's children
                        {
                            renderList[j].enabled = true;
                            renderList[j].material.color = highlightedColor; //changes pose to the highlighted color
                        }

                    }

                }
                // Otherwise . . .
                else
                {
                    if (poseMap.TryGetValue(index, out List<MeshRenderer> renderList)) //checks if a pose exists for this index
                    {

                        for (int j = 0; j < renderList.Count; ++j)
                        {
                            renderList[j].enabled = true;
                            renderList[j].material.color = Color.white; //returns pose to original color
                        }
                    }
                }
            }
        }
    }

}
