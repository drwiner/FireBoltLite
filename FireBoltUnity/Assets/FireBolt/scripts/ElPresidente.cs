using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.IO;
using System.Collections;
using Assets.scripts;
using System.Collections.Generic;
using System;
using Impulse.v_1_336;
using UintT = Impulse.v_1_336.Intervals.Interval<Impulse.v_1_336.Constants.ValueConstant<uint>, uint>;
using UintV = Impulse.v_1_336.Constants.ValueConstant<uint>;
using CM = CinematicModel;

/// <summary>
/// commander-in-chief of cinematic realization.
/// </summary>
public class ElPresidente : MonoBehaviour {

    //where we keep our datas from a given load
    FireBoltActionList actorActionList;
    CameraActionList cameraActionList;
    FireBoltActionList discourseActionList;

    //where we copy our datas when our datas should be doing stuff
    FireBoltActionList executingActorActions; 
    FireBoltActionList executingCameraActions;
    FireBoltActionList executingDiscourseActions;

    public static string timestampFormat = "HH:mm:ss.FF";
    
    private float lastTickLogged;
    public Text debugText;
	public float myTime;
    public Slider whereWeAt;
    public static readonly ushort MILLIS_PER_FRAME = 5;
    private Story<UintV, UintT, IIntervalSet<UintV, UintT>> story;

    public static ElPresidente Instance;

    private AssetBundle actorsAndAnimations = null;
    private AssetBundle terrain = null;
    private bool initialized = false;
    private bool initNext = false;
    private bool initTriggered = false;

    public static GameObjectRegistry createdGameObjects;

    public List<ProCamsLensDataTable.FOVData> lensFovData;

    private bool keyframesGenerated = false;
    public bool KeyframesGenerated
    {
        get { return keyframesGenerated; }
    }

    private CM.CinematicModel cinematicModel = null;
    public CM.CinematicModel CinematicModel { get { return cinematicModel; } }

    InputSet currentInputSet=null;
    DateTime storyPlanLastReadTimeStamp = DateTime.Now;
    bool reloadStoryPlan = false;
    
    DateTime cameraPlanLastReadTimeStamp = DateTime.Now;    
    bool reloadCameraPlan = false;
    
    DateTime cinematicModelPlanLastReadTimeStamp = DateTime.Now;
    bool reloadCinematicModel = false;

    DateTime actorsAndAnimationsBundleLastReadTimeStamp = DateTime.Now;
    bool reloadActorsAndAnimationsBundle = false;

    DateTime terrainBundleLastReadTimeStamp = DateTime.Now;
    bool reloadTerrainBundle = false;

    bool generateKeyframes = true;

    /// <summary>
    /// story time.  controlled by discourse actions
    /// </summary>
    public static float currentStoryTime;

    /// <summary>
    /// time as relates to playback scrubbing
    /// </summary>
    public static float currentDiscourseTime;

    public float CurrentStoryTime { get { return currentStoryTime; } }
    public float CurrentDiscourseTime { get { return currentDiscourseTime; } }
    public uint EndDiscourseTime {
        get { return cameraActionList != null ? cameraActionList.EndDiscourseTime : 0; }}

    //number of milliseconds to advance the story and discourse time on update
    private uint? timeUpdateIncrement;
    private bool generateVideoFrames;
    private uint videoFrameNumber;

    private bool implicitActorCreation;

    void Start()
    {
        Instance = this;
        if (whereWeAt != null)
        {
            whereWeAt.gameObject.AddComponent<SliderManager>();
        }
        ProCamsLensDataTable.Instance.LoadData();
        lensFovData = ProCamsLensDataTable.Instance.GetFilmFormat("35mm 16:9 Aperture (1.78:1)").GetLensKitData(0)._fovDataset;
    }

    /// <summary>
    /// look up tightest lens for loaded kit that has a larger vertical fov than requested
    /// this way we don't lose anything that was in the frame...though it's not as precise
    /// as we had wanted.
    /// </summary>
    /// <param name="targetVerticalFov">vertical fov calculated to frame the target</param>
    /// <returns>lens kit index to apply to cameraBody</returns>
    public int GetLensIndex(float targetVerticalFov)
    {
        int lensIndex = 0;
        //iterates over the entire array of lenses always.  it has 15 elements, so it doesn't make much difference
        for (int i = 0; i < lensFovData.Count; i++ )
        {
            if (lensFovData[i]._unityVFOV > targetVerticalFov)
            {
                lensIndex = i;
            }
        }
        return lensIndex;
    }

    /// <summary>
    /// wrapper for default args Init to use from UI button as default args methods are not visible in UI click event assignment in inspector
    /// </summary>
    /// <param name="a"></param>
    [Obsolete("write your own script to interact with Init(InputSet, uint?, bool, bool")]
    public void Init(float a)
    {
        Init(null, null, true);
    }

    /// <summary>
    /// re Initialize using default paths and only reloading files when 
    /// they are updated
    /// </summary>
    /// <param name="generateKeyframes">toggles keyframe generation.  
    /// Keyframe generation runs the entire story through and may take 
    /// a large amount of time to initialize.</param>
    [Obsolete("write your own script to interact with Init(InputSet, uint?, bool, bool")]
    public void Init(bool generateKeyframes)
    {
        Debug.Log(string.Format("reload keyframes[{0}]",generateKeyframes));
        Init(null, null, false, generateKeyframes);
    }

    /// <summary>
    /// Use this method to start FireBolt.
    /// </summary>
    /// <param name="newInputSet">specify input file locations in an InputSet.  accepts null and 
    /// uses the default paths and names.</param>
    /// <param name="timeUpdateIncrement">optional. defaults null.  specifies number of milliseconds
    /// to force the execution to jump everyframe.  Uses Time.deltaTime if null.</param>
    /// <param name="forceFullReload">optional, defaults false.  true ignores timestamps on input files
    /// and reloads the whole shebang...almost like you would expect.</param>
    /// <param name="generateKeyframes">optional default false. makes keyframes for display over scrubber.
    /// locks down the UI for some time at startup to execute whole cinematic once all speedy like</param>
    public void Init(InputSet newInputSet=null, uint? timeUpdateIncrement=null, bool forceFullReload=false, 
        bool generateKeyframes=false, bool generateVideoFrames=false, bool implicitActorCreation=false)
    {
        this.timeUpdateIncrement = timeUpdateIncrement;
        this.generateKeyframes = generateKeyframes;
        this.generateVideoFrames = generateVideoFrames;
        this.implicitActorCreation = implicitActorCreation;

        if (generateVideoFrames)
        {
            // Find the canvase game object.
            GameObject canvasGO = GameObject.Find("Canvas");

            // Get the canvas component from the game object.
            Canvas canvas = canvasGO.GetComponent<Canvas>();

            // Toggle the canvas display off.
            canvas.enabled = false;

        }
        if (whereWeAt == null) //if there is no slider to display them on, don't generate keyframes
        {
            this.generateKeyframes = false;
        }

        //if we didn't get handed one, generate an input set with the default paths
        if (newInputSet == null) 
        {
            newInputSet = new InputSet();
        }

        //don't have to do file modification timestamp compares.  just set all the load flags to reload everything
        if (forceFullReload || currentInputSet == null)
        {
            reloadStoryPlan = true;
            reloadCameraPlan = true;
            reloadCinematicModel = true;
            reloadActorsAndAnimationsBundle = true;
            reloadTerrainBundle = true;
        }
        else //we actually should figure out what's changed so we can reload only those required inputs
        {
            reloadStoryPlan = requiresReload(currentInputSet.StoryPlanPath, newInputSet.StoryPlanPath, storyPlanLastReadTimeStamp);
            reloadCameraPlan = requiresReload(currentInputSet.CameraPlanPath, newInputSet.CameraPlanPath, cameraPlanLastReadTimeStamp);
            reloadCinematicModel = requiresReload(currentInputSet.CinematicModelPath, newInputSet.CinematicModelPath, cinematicModelPlanLastReadTimeStamp);
            reloadActorsAndAnimationsBundle = requiresReload(currentInputSet.ActorsAndAnimationsBundlePath, newInputSet.ActorsAndAnimationsBundlePath, actorsAndAnimationsBundleLastReadTimeStamp);
            reloadTerrainBundle = requiresReload(currentInputSet.TerrainBundlePath, newInputSet.TerrainBundlePath, terrainBundleLastReadTimeStamp);
        }

        if(createdGameObjects!=null)createdGameObjects.Destroy("InstantiatedObjects");
        if (reloadTerrainBundle&&createdGameObjects!=null) createdGameObjects.Destroy("Terrain");
        initialized = false;
        initTriggered = true;
        currentInputSet = newInputSet;
    }

    private bool requiresReload(string oldPath, string newPath, DateTime lastRead)
    {
        if(string.Compare(oldPath, newPath)==0 && //same file
           getFileLastModifiedTime(newPath) < lastRead)//file was last modified before the last time we read it.
        {
            return false;
        }
        return true;
    }

    private DateTime getFileLastModifiedTime(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError(string.Format("file[{0}] cannot be found for FireBolt load. This will crash in a sec :)", path));            
        }
        return File.GetLastWriteTime(path);        
    }

    /// <summary>
    /// does the actual re-init work.  should only be called after destroy has had time to process.  
    /// currently this involves an elaborate set of bools to keep up with engine execution
    /// </summary>
    private void init()
    {

        createdGameObjects = new GameObjectRegistry();
        createdGameObjects.Add("Rig", GameObject.Find("Rig"));//get the camera where we can find it quickly
        GameObject proCam = GameObject.Find("Pro Cam");
        createdGameObjects.Add(proCam.name, proCam);
        if (reloadStoryPlan)
        {
            loadStructuredImpulsePlan(currentInputSet.StoryPlanPath);
            Debug.Log(string.Format("loading story plan[{0}] @ [{1}].  last read [{2}]", 
                                    currentInputSet.StoryPlanPath, DateTime.Now.ToString(timestampFormat), storyPlanLastReadTimeStamp.ToString(timestampFormat)));
            storyPlanLastReadTimeStamp = DateTime.Now;
        }

        if (reloadCinematicModel)
        {
            cinematicModel = CM.Parser.Parse(currentInputSet.CinematicModelPath); 
            Debug.Log(string.Format("loading cinematic model[{0}] @ [{1}].  last read [{2}]",
                                     currentInputSet.CinematicModelPath, DateTime.Now.ToString(timestampFormat), storyPlanLastReadTimeStamp.ToString(timestampFormat)));
            cinematicModelPlanLastReadTimeStamp = DateTime.Now;
        }

        if (actorsAndAnimations != null && reloadActorsAndAnimationsBundle)
            actorsAndAnimations.Unload(true);


        if (reloadActorsAndAnimationsBundle)
        {
            //actorsAndAnimations = AssetBundle.LoadFromFile(currentInputSet.ActorsAndAnimationsBundlePath);
            //Debug.Log(string.Format("loading actors bundle[{0}] @ [{1}].  last read [{2}]",
            //             currentInputSet.ActorsAndAnimationsBundlePath, DateTime.Now.ToString(timestampFormat), storyPlanLastReadTimeStamp.ToString(timestampFormat)));
            //actorsAndAnimationsBundleLastReadTimeStamp = DateTime.Now;
        }            

        if (terrain != null && reloadTerrainBundle)
            terrain.Unload(true);

        if (reloadTerrainBundle)
        {
           // terrain = AssetBundle.LoadFromFile(currentInputSet.TerrainBundlePath);
            //Debug.Log(string.Format("loading terrain bundle[{0}] @ [{1}].  last read [{2}]",
            //                        currentInputSet.TerrainBundlePath, DateTime.Now.ToString(timestampFormat), storyPlanLastReadTimeStamp.ToString(timestampFormat)));
            //terrainBundleLastReadTimeStamp = DateTime.Now;
            //instantiateTerrain();
        }  

        if (reloadStoryPlan || reloadActorsAndAnimationsBundle || reloadCinematicModel)
        {        
            actorActionList = ActorActionFactory.CreateStoryActions(story, cinematicModel, implicitActorCreation);
            Debug.Log(string.Format("upstream components reloaded, rebuilding actor action queue @ [{0}].",
                                    DateTime.Now.ToString(timestampFormat)));
        }

        if (reloadStoryPlan || reloadCameraPlan)
        {            
            CameraActionFactory.CreateActions(story, cinematicModel, currentInputSet.CameraPlanPath,  out cameraActionList, out discourseActionList);
            Debug.Log(string.Format("upstream components reloaded, rebuilding camera action queue @ [{0}].",
                                    DateTime.Now.ToString(timestampFormat)));
            cameraPlanLastReadTimeStamp = DateTime.Now;
        }

        currentDiscourseTime = 0;
        currentStoryTime = 0;
        actorActionList.NextActionIndex = 0;
        cameraActionList.NextActionIndex = 0;
        discourseActionList.NextActionIndex = 0;

        executingActorActions = new FireBoltActionList(new ActionTypeComparer());
        executingCameraActions = new FireBoltActionList(new ActionTypeComparer());
        executingDiscourseActions = new FireBoltActionList(new ActionTypeComparer());
        GameObject instantiatedObjects = new GameObject("InstantiatedObjects");
        instantiatedObjects.transform.SetParent((GameObject.Find("FireBolt") as GameObject).transform);
        createdGameObjects.Add(instantiatedObjects.name, instantiatedObjects);

        if (generateKeyframes) 
        {
            // Call the screenshot coroutine to create keyframe images for scrubbing.
            StartCoroutine(CreateScreenshots());
        }       

        initialized = true;
        initNext = false;
        initTriggered = false;

        reloadActorsAndAnimationsBundle = false;
        reloadCameraPlan = false;
        reloadCinematicModel = false;
        reloadStoryPlan = false;
        reloadTerrainBundle = false;
    }

    private void instantiateTerrain()
    {
        var terrainPrefab = terrain.LoadAsset(cinematicModel.Terrain.TerrainFileName);
        if (!terrainPrefab)
        {
            Debug.Log(string.Format("terrain [{0}] not found in asset bundle", cinematicModel.Terrain.TerrainFileName));
        }
        Vector3 v;
        cinematicModel.Terrain.Location.TryParseVector3(out v);
        var t = Instantiate(terrainPrefab,v,Quaternion.identity)as GameObject;
        t.name = "Terrain";
        t.transform.SetParent(GameObject.Find("FireBolt").transform,true);
        createdGameObjects.Add(t.name, t);
    }

    private void loadStructuredImpulsePlan(string storyPlanPath)
    {
        Debug.Log("begin story plan xml load");
        var xml = Impulse.v_1_336.Xml.Story.LoadFromFile(storyPlanPath);
        Debug.Log("end story plan xml load");
        var factory = Impulse.v_1_336.StoryParsingFactories.GetUnsignedIntergerIntervalFactory();
        Debug.Log("begin story plan parse");
        story = factory.ParseStory(xml, false);//TODO true! get crackin with that validation, colin!
        Debug.Log("end story plan parse");
    }

    public AssetBundle GetActiveAssetBundle()
    {
        if (actorsAndAnimations == null)
        {
            Debug.Log("attempting to load from asset bundle before it is set. " +
                      "use ElPresidente.SetActiveAssetBundle() to load an asset bundle");
            return null;
        }
        return actorsAndAnimations;
    }



    public bool IsPaused()
    {
        return Time.timeScale == 0f;
    }

    /// <summary>
    /// suspends/resumes execution 
    /// </summary>
    public void PauseToggle()
    {
        if (Time.timeScale < float.Epsilon)
            Time.timeScale = 1f;
        else
            Time.timeScale = 0f;
    }

    public void speedToggle()
    {
        Time.timeScale = 1f;
        //Time.timeScale = (Time.timeScale + 1f) % 4;        
    }

    public void setTime(float targetPercentComplete)
    {
        if (cameraActionList == null)
            goToDiscourseTime(0);

        else if (Mathf.Abs(targetPercentComplete * cameraActionList.EndDiscourseTime - currentDiscourseTime) > MILLIS_PER_FRAME)
        {
            goToDiscourseTime(targetPercentComplete * cameraActionList.EndDiscourseTime);                
        }            
    }

    private void updateCurrentTime()
    {
        if (timeUpdateIncrement.HasValue)
        {
            currentStoryTime += timeUpdateIncrement.Value;
            currentDiscourseTime += timeUpdateIncrement.Value;
        }
        else
        {
            currentStoryTime += Time.deltaTime * 1000;
            currentDiscourseTime += Time.deltaTime * 1000;
        }        
    }

    void Update()
    {
        if (!initialized && initNext)
            init();
        else if (!initialized)
            return;

        updateCurrentTime();
        
        if (debugText != null)
            debugText.text = currentDiscourseTime.ToString() + " : " + currentStoryTime.ToString();
        if (whereWeAt && currentDiscourseTime < cameraActionList.EndDiscourseTime)
            whereWeAt.value = currentDiscourseTime / cameraActionList.EndDiscourseTime;
        myTime = currentStoryTime;
        logTicks();

        updateFireBoltActions(discourseActionList, executingDiscourseActions, currentDiscourseTime);
        updateFireBoltActions(actorActionList, executingActorActions, currentStoryTime);
        updateFireBoltActions(cameraActionList, executingCameraActions, currentDiscourseTime);
    }

    void LateUpdate()
    {
        if (!initialized && initTriggered)
        {
            initNext = true;
            return;
        }
        else if (!initialized)
            return;

        executingDiscourseActions.ExecuteList(ElPresidente.currentDiscourseTime);
        executingActorActions.ExecuteList(ElPresidente.currentStoryTime);
        executingCameraActions.ExecuteList(ElPresidente.currentDiscourseTime);

        if (generateVideoFrames)
        {
            // Initialize the render texture and texture 2D.
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
            Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

            // Render the texture.
            Camera.main.targetTexture = rt;
            Camera.main.Render();

            // Read the rendered texture into the texture 2D.
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

            // Clean everything up.
            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            Destroy(screenShot);

            // Save the texture 2D as a PNG.
            byte[] bytes = screenShot.EncodeToPNG();
            File.WriteAllBytes(@".screens/" + videoFrameNumber + ".png", bytes);
            videoFrameNumber++;

            // Quit if we have passed the end of the discourse.
            if (cameraActionList.EndDiscourseTime < currentDiscourseTime)
                Application.Quit();
        }
    }

    /// <summary>
    /// appends and removes from the executing lists based on the time and the last action
    /// appended from the full list of actions
    /// </summary>
    /// <param name="actions">action list of either story or discourse actions.  both will be update each frame</param>
    /// <param name="executingActions">executing list for story or discourse actions.  should match first parameter</param>
    /// <param name="referenceTime">current story or discourse time. also should match the lists</param>
    void updateFireBoltActions(FireBoltActionList actions, FireBoltActionList executingActions, float referenceTime)
    {
        //stop things that should be finished now and remove them from the executing list
        List<FireBoltAction> removeList = new List<FireBoltAction>();
        foreach (FireBoltAction action in executingActions)
        {
            if (actionComplete(action,referenceTime) || action.StartTick() > referenceTime)
            {
                action.Stop();
                removeList.Add(action);
            }
        }
        foreach (FireBoltAction action in removeList)
        {
            executingActions.Remove(action);
        }

        //go through the full action list until the next action shouldn't have started yet
        while (actions.NextActionIndex < actions.Count && actions[actions.NextActionIndex].StartTick() <= referenceTime) 
        {
            FireBoltAction action = actions[actions.NextActionIndex];
            actions.NextActionIndex++;
            if (action.Init()) 
			{
                if (actionComplete(action, referenceTime))
                    action.Skip();                //if the new action should have already completed, run the skip on it
                else
                    executingActions.Add(action);                     
			}
        }
    }
    
    void rewindFireBoltActions(FireBoltActionList actions, float time)
    {
        int currentIndex = actions.NextActionIndex-1;//next action was pointed to...next action!
        actions.NextActionIndex = 0;
        //find the point to which we should back up
        while (actions.NextActionIndex < actions.Count &&
               actions[actions.NextActionIndex].EndTick() < time) //this is bad.  actions are ordered by startTick, and we are searching by endTick...there will be some small issues
        {
            actions.NextActionIndex++;
        }

        //walk backward, undoing actions until current action is actually before nextAction
        while (currentIndex >= actions.NextActionIndex)
        {
            actions[currentIndex].Undo();
            currentIndex--;
        }
        if (actions.Count > actions.NextActionIndex)
            Debug.Log("rewind to " + actions.NextActionIndex + ": " + actions[actions.NextActionIndex]);
        else
            Debug.Log("rewind to action #" + actions.NextActionIndex);
    }

    void fastForwardFireBoltActions(FireBoltActionList actions, float targetTime, FireBoltActionList executingActions, float currentTime)
    {
        List<FireBoltAction> removeList = new List<FireBoltAction>();
        foreach (FireBoltAction action in executingActions)
        {
            if (actionComplete(action, currentTime))
            {
                action.Skip();
                removeList.Add(action);
            }
        }
        foreach (FireBoltAction action in removeList)
        {
            executingActions.Remove(action);
        }
        while (actions.NextActionIndex < actions.Count && actions[actions.NextActionIndex].StartTick() <= targetTime) 
        {
            FireBoltAction action = actions[actions.NextActionIndex];
            actions.NextActionIndex++;
            if (action.Init())
            {
                if (!actionComplete(action, currentTime))
                    executingActions.Add(action);
                else
                    action.Skip();
            }
        }
    }

	public void goToStoryTime(float time)
	{
        if (time < 0)
            time = 0;
        Debug.Log ("goto story " + time);
		if (time < currentStoryTime)
        {
            currentStoryTime = time;
            rewindFireBoltActions(actorActionList, currentStoryTime);
        }
        else
        {
            currentStoryTime = time;
            fastForwardFireBoltActions(actorActionList, currentStoryTime, executingActorActions, currentStoryTime);
        }
		currentStoryTime = time;
	}

    public void goToDiscourseTime(float time)
    {
        if (time < 0)
            time = 0;
        Debug.Log("goto discourse " + time);
        lastTickLogged = time;
        if (time < currentDiscourseTime)
        {
            currentDiscourseTime = time;
            rewindFireBoltActions(discourseActionList, currentDiscourseTime);
            rewindFireBoltActions(cameraActionList, currentDiscourseTime);
            
        }
        else
        {
            currentDiscourseTime = time;
            fastForwardFireBoltActions(discourseActionList, currentDiscourseTime, executingDiscourseActions, currentDiscourseTime);
            fastForwardFireBoltActions(cameraActionList, currentDiscourseTime, executingCameraActions, currentDiscourseTime);
        }
        currentDiscourseTime = time;
    }

    public void scaleTime(float scale)
    {
        Time.timeScale = scale;
    }

	public void goToRel(float time)
	{
		goToStoryTime(currentStoryTime + time);
	}

    void logTicks()
    {
        if (currentDiscourseTime - lastTickLogged > 1000)
        {
            Debug.Log(currentDiscourseTime + " : " + currentStoryTime);
            lastTickLogged = currentDiscourseTime;
        }
    }

    bool actionComplete(FireBoltAction action, float  referenceTime)
    {
        return action.EndTick() < referenceTime;
    }

    /// <summary>
    /// Creates a series of screenshots to be used as keyframes for scrubbing.
    /// </summary>
    private IEnumerator CreateScreenshots ()
    {
        // Find the canvase game object.
        GameObject canvasGO = GameObject.Find("Canvas");

        // Get the canvas component from the game object.
        Canvas canvas = canvasGO.GetComponent<Canvas>();

        // Toggle the canvas display off.
        canvas.enabled = false;

        // Store the main camera's default settings.
        CameraClearFlags defClearFlags = Camera.main.clearFlags;
        Color defBackgroundColor = Camera.main.backgroundColor;
        int defCullingMask = Camera.main.cullingMask;

        // Make the main camera display a black screen while the system iterates through the keyframes.
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = Color.black;
        Camera.main.cullingMask = 0;

        int pic = 1;

        // Loop through discourse time at intervals of 5%.
        for (float i = 0; i < 100; i = i + 5)
        {
            // Set the time based on the current loop.
            setTime(i / 100);

            // Allow the frame to process.
            yield return new WaitForEndOfFrame();

            // Initialize the render texture and texture 2D.
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
            Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

            // Create a new camera object and position it where the main camera is.
            GameObject testCameraGO = new GameObject();
            testCameraGO.transform.position = Camera.main.transform.position;
            testCameraGO.transform.rotation = Camera.main.transform.rotation;
            Camera test = testCameraGO.AddComponent<Camera>();

            // Render the texture.
            test.targetTexture = rt;
            test.Render();

            // Read the rendered texture into the texture 2D and reset the camera.
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            test.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            Destroy(testCameraGO);

            // Save the texture 2D as a PNG.
            byte[] bytes = screenShot.EncodeToPNG();
            File.WriteAllBytes(@".screens/" + pic++ + ".png", bytes);
        }

        // Reset the main camera to its default configuration.
        Camera.main.clearFlags = defClearFlags;
        Camera.main.backgroundColor = defBackgroundColor;
        Camera.main.cullingMask = defCullingMask;

        // Toggle the canvas display back on.
        canvas.enabled = true;

        // Reset the time to zero.
        setTime(0);

        // Set that the keyframes have been generated.
        keyframesGenerated = true;

        //UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }
}