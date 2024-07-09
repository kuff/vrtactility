using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Logger : MonoBehaviour
{
    private const bool DO_LOGGING = true;
    private const int LOGGING_FREQUENCY = 2;    //(?)

    private static readonly List<IEnumerable> _logQueue = new List<IEnumerable>();
    private static readonly IList[] _previousLogs = new IList[8];

    //private OVRPlugin.HandState _handState = new OVRPlugin.HandState();

    private static readonly string _logFileName = $"/LOG_MOVE_ME_{System.DateTime.Now:HHmmss-ffff}.csv";
    private static float _pointOfLastWrite = -1f;
    private static bool _fileSystemOperationInProgress = false;
    private static int _frameCounter = 0;
    private static float _frameCountTimestamp = -1f;
    private static int _invokesSinceLastWrite = LOGGING_FREQUENCY - 1;

    // private OVRPlugin.HandState _hsLeft = new OVRPlugin.HandState();
    // private OVRPlugin.HandState _hsRight = new OVRPlugin.HandState();
    //private Transform _mainCameraTransform;


    public enum LogType
    {
        // Continuous/Event/Static
        // Overall
        FPS,                  //  0 C
        LowMemory,            //  1 E
        QuitApp,              //  2 E
        HeadPos,              //  3 C
        HandPos,              //  4 C - dominant hand? Maybe select R/L in the calibration
        Focus,                //  5 E
        // Grabbing
        ForceThresholds,      //  6 C
        GraspForce,           //  7 C
        CurrentForceLevel,    //  8
        NormalVectorHand,     //  9 C!!!
        // Trial
        TargetForceLevel,     //  10 S
        TrialIndex,           //  11 S
        CubePositions,        //  12 S
        Time,                 //  13 E   // start->0 //isGrasped->1 //isReleased->2 //isDestroyed->3 //end->4    (THIS IS MORE IMPORTANT IN THE VALIDATION)
        Progress,             //  14 C
        ScenarioStatus,       //  15 E   // success -> 0 // drop -> 1 // destroy -> 2 // lossOfGrab -> 3
        // Stimulation
        Amplitude,            //  16 C
        Frequency,            //  17 C
        ActivePads,           //  18 C
        // LoadScene
        SceneIndex,           //  19 E
        StimModality          //  20 S  
    }

    protected void Start()
    {
        //Log(LogType.Time);  //(?)
    }

    protected void Update()
    {
#pragma warning disable CS0162
        if (!DO_LOGGING) return;
#pragma warning restore CS0162

        // Update OVR Hand States
        //OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandLeft, ref _hsLeft);
        //OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandRight, ref _hsRight);
        ////
        //// Log player hands- and head data
        ////Log(LogType.LeftHand, listData: GetSortedValues(_hsLeft));
        //Log(LogType.HandPos, listData: GetSortedValues(_hsRight));
        //Log(LogType.HeadPos, listData: GetSortedValues(_mainCameraTransform));

        // Determine if FPS is to be logged in this frame
        _frameCounter++;
        _frameCountTimestamp += Time.deltaTime;
        if (_frameCountTimestamp is -1 or > 1)
        {
            // Log FPS count and reset
            Log(LogType.FPS, listData: new List<int> { _frameCounter }, ignorePrevious: true);

            _frameCounter = 0;
            _frameCountTimestamp = 0;

            // Write data to disc after x seconds
            _invokesSinceLastWrite++;
            if (_invokesSinceLastWrite >= LOGGING_FREQUENCY && !_fileSystemOperationInProgress)
            {
                WriteToDisc();
                _invokesSinceLastWrite = 0;
            }
        }
    }

    private static IEnumerable<object> GetSortedValues(OVRPlugin.HandState inputHandState)
    {
        // Hardcoded value return for HandState objects
        var result = new List<object>
        {
            inputHandState.Status,
            inputHandState.HandConfidence,
            inputHandState.FingerConfidences,
            inputHandState.RootPose,
            inputHandState.PointerPose,
            inputHandState.BoneRotations,
            inputHandState.HandScale,
            inputHandState.Pinches,
            inputHandState.PinchStrength,
            inputHandState.RequestedTimeStamp,
            inputHandState.SampleTimeStamp
        };
        return result;
    }

    private static IEnumerable<object> GetSortedValues(Transform inputTransform)
    {
        // Hardcoded value return for Transform objects
        var result = new List<object>
        {
            inputTransform.position,
            inputTransform.rotation,
            inputTransform.forward
        };
        return result;
    }

    protected void OnEnable()
    {
        Application.lowMemory += LogLowMemory;
        Application.quitting += OnApplicationQuit;
    }

    private static void LogLowMemory()
    {
        Log(LogType.LowMemory);
    }

    private void OnApplicationQuit()
    {
        OnApplicationFocus(true);
    }


    private void OnApplicationFocus(bool hasFocus)
    {
        Log(LogType.Focus, listData: new List<bool> { hasFocus });
    }

    private void OnApplicationPause(bool pauseStatus)   //(?)
    {
        // Provide end-of-file signifier and log the remaining data from memory before quitting
        // NOTE: We do this in the pause event because the quit event on Android is unreliable
        Log(LogType.QuitApp, ignorePrevious: true);
        WriteToDisc();
    }

    public static void LogSceneChange(int SceneIndex, int TargetForceLevel)   //to call when the next cube appears
    {
        Log(LogType.TrialIndex, new int[] { SceneIndex }, true);
        Log(LogType.TargetForceLevel, new int[] { TargetForceLevel }, true);
    }

    public static void LogForceThresholds(float[] forceThrs)   //to call when the next cube appears
    {
        Log(LogType.ForceThresholds, new float[] { forceThrs[0], forceThrs[1], forceThrs[2], forceThrs[3], forceThrs[4] }, true);
    }

    public static void LogForce(float graspForce, float progress, int currentForceLevel)   //to call Continuosly
    {
        Log(LogType.GraspForce, new float[] { graspForce }, true);
        Log(LogType.Progress, new float[] { progress }, true);
        Log(LogType.CurrentForceLevel, new int[] { currentForceLevel }, true);
    }

    public static void LogScenarioState(int scenarioState)   
    {
        Log(LogType.ScenarioStatus, new int[] { scenarioState }, true);
    }

    public static void LogTimeEvent(int taskEvent)   //to call Continuosly
    {
        Log(LogType.Time, new int[] { taskEvent }, true);
    }

    public static void SceneIndex(int currentSceneIndex)   //to call Continuosly
    {
        Log(LogType.SceneIndex, new int[] { currentSceneIndex }, true);
    }

    public static void LogStimModality(string stimModality)   //to call when the next cube appears
    {
        Log(LogType.StimModality, new string[] { stimModality }, true);
    }

    public static void Log(LogType type, IEnumerable listData = default, bool ignorePrevious = false)
    {
        var data = listData?.Cast<object>().ToList();  // Cast input data to list

        // Check to make sure provided data is unique from previous log
        if (!ignorePrevious && _previousLogs[(int)type] is not null && data is not null && data.Count == _previousLogs[(int)type].Count)
        {
            try
            {
                //if (type is LogType.HandPos)
                //{
                //    // Handle hand comparisons differently from other data by looking at orientation specifically
                //    var compDataNew = (OVRPlugin.Posef)data[3];
                //    var compDataOld = (OVRPlugin.Posef)_previousLogs[(int)type][3];
                //    var orientation1 = compDataNew.Orientation;
                //    var orientation2 = compDataOld.Orientation;
                //    if (orientation1.Equals(orientation2)) return;
                //}
                //else
                //{
                //    // Compare other objects by turning them to json and comparing the strings
                //    // TODO: Come up with a more performant approach to this
                //    var obj1 = JsonUtility.ToJson(data);
                //    var obj2 = JsonUtility.ToJson(_previousLogs[(int)type]);
                //    if (obj1 == obj2) return;
                //}
            }
            catch
            {
                // Ignored...
            }
        }

        // Define the beginning of the log string with LogType and timestamp
        var baseString = "" + (int)(Time.realtimeSinceStartup * 10000) + " " + (int)type + " ";

        // Parse log command
        switch (type)
        {
            case LogType.FPS:
                _logQueue.Add(baseString + data![0]);
                break;
            case LogType.Focus:
                _logQueue.Add(baseString + ((bool)data![0] ? 1 : 0));
                break;
            case LogType.LowMemory:
                _logQueue.Add(baseString);
                break;
            case LogType.QuitApp:
                _logQueue.Add(baseString);
                break;
            case LogType.ForceThresholds:
                _logQueue.Add(baseString + data![0]);
                _logQueue.Add(baseString + data![1]);
                _logQueue.Add(baseString + data![2]);
                _logQueue.Add(baseString + data![3]);
                _logQueue.Add(baseString + data![4]);
                break;
            case LogType.GraspForce:
                _logQueue.Add(baseString + (float)data![0]);
                break;
            case LogType.CurrentForceLevel:
                _logQueue.Add(baseString + data![0]);
                break;
            case LogType.Progress:
                _logQueue.Add(baseString + (float)data![0]);
                break;
            case LogType.Time:
                _logQueue.Add(baseString + data![0]);// + "," + DateTime.Now.ToString("HH:mm:ss:fff"));
                break;
            case LogType.TargetForceLevel:
                _logQueue.Add(baseString + data![0]);   
                break;
            case LogType.TrialIndex:
                _logQueue.Add(baseString + data![0]);
                break;
            case LogType.CubePositions:
                _logQueue.Add(baseString + data![0] + data![1]);
                break;
            case LogType.ScenarioStatus:
                _logQueue.Add(baseString + data![0]);
                break;
            case LogType.SceneIndex:
                _logQueue.Add(baseString + data![0]);
                break;
            case LogType.StimModality:
                _logQueue.Add(baseString + data![0]);
                break;
                //default:
                //    throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        string BuildRecursively(IEnumerable input)
        {
            var result = "";  // What will become the end result string
            var e = input.GetEnumerator();
            while (e.MoveNext())
            {
                var elem = e.Current;

                // Handle predictable values of elem
                switch (elem)
                {

                    case null:
                        elem = "n";
                        break;
                }

                // Parse and save elem
                if (elem is ICollection or IList) result += "< " + BuildRecursively((IEnumerable)elem) + "> ";
                else result += "" + elem + " ";
            }

            return result;
        }
    }

    private static bool WriteToDisc()
    {
        _fileSystemOperationInProgress = true;
#if UNITY_EDITOR
        var path = "C:\\Users\\Eleonora Vendrame\\VR_task_data_recording" + _logFileName;  //Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + _logFileName;
#else  // NOTE: ASSUMING RELEASE BUILDS RUN ON DEVICE
        var path = Application.persistentDataPath + _logFileName;
#endif
        try
        {
            // Open log file or create one and write log entries as lines
            using (var sw = File.AppendText(path))
            {
                foreach (var list in _logQueue)
                    sw.WriteLine(list.ToString());
            }

            // Clear log queue and return
            _logQueue.Clear();
            _fileSystemOperationInProgress = false;
            _pointOfLastWrite = Time.realtimeSinceStartup;
            return true;
        }
        // NOTE: Currently errors are not handled beyond this
        catch (InvalidDataException e)
        {
            Debug.LogError("Target log path exists but is read-only\n" + e);
        }
        catch (PathTooLongException e)
        {
            Debug.LogError("Target log path name may be too long\n" + e);
        }
        catch (IOException e)
        {
            Debug.LogError("The disk may be full\n" + e);
        }

        // TODO: revert log file if write operations fail...

        _fileSystemOperationInProgress = false;
        _pointOfLastWrite = Time.realtimeSinceStartup;
        return false;
    }
}
