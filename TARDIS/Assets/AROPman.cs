using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine.UI;

using maxstAR;

public class AROPman : ARBehaviour
{

    [SerializeField]
    private Text startBtnText = null, setBtn = null;
    public GameObject model1,model2,model3,b1,b2;
    public Image i1, i2;
    private int index = 0,size=0;
    private Vector3 touchToWorldPosition = Vector3.zero;
    private Vector3 touchSumPosition = Vector3.zero;
    private bool findSurfaceDone = false;
    private bool setposition = false;
    private ApiAiModule a;
    private InstantTrackableBehaviour instantTrackable = null;
    private CameraBackgroundBehaviour cameraBackgroundBehaviour = null;
    private Vector3 tp;
    void Awake()
    {
        Init();

        cameraBackgroundBehaviour = FindObjectOfType<CameraBackgroundBehaviour>();
        if (cameraBackgroundBehaviour == null)
        {
            Debug.LogError("Can't find CameraBackgroundBehaviour.");
            return;
        }
    }

    void Start()
    {
        instantTrackable = FindObjectOfType<InstantTrackableBehaviour>();
        if (instantTrackable == null)
        {
            return;
        }

        instantTrackable.OnTrackFail();
        StartCamera();

        TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_INSTANT);
        SensorDevice.GetInstance().Start();

        // For see through smart glass setting
        if (ConfigurationScriptableObject.GetInstance().WearableType == WearableCalibration.WearableType.OpticalSeeThrough)
        {
            WearableManager.GetInstance().GetDeviceController().SetStereoMode(true);

            CameraBackgroundBehaviour cameraBackground = FindObjectOfType<CameraBackgroundBehaviour>();
            cameraBackground.gameObject.SetActive(false);
        }
        a = transform.GetComponent<ApiAiModule>();
        var b = i1.color;
        b.a = 1f;
        i1.color = b;
        i1.CrossFadeAlpha(1f, 0f, true);
        b = i2.color;
        b.a = 1f;
        i2.color = b;
        i2.CrossFadeAlpha(1f, 0f, true);
    }

    void Update()
    {
        if (instantTrackable == null)
        {
            return;
        }
        if (Math.Abs(index) == 0)
        {
            model1.SetActive(true); model2.SetActive(false); model3.SetActive(false);

        }
        else if (Math.Abs(index) == 1)
        {
            model1.SetActive(false); model2.SetActive(true); model3.SetActive(false);

        }
        else
        {
            model1.SetActive(false); model2.SetActive(false); model3.SetActive(true);
        }
        TrackingState state = TrackerManager.GetInstance().UpdateTrackingState();

        cameraBackgroundBehaviour.UpdateCameraBackgroundImage(state);

        TrackingResult trackingResult = state.GetTrackingResult();

        if (trackingResult.GetCount() == 0)
        {
            instantTrackable.OnTrackFail();
            return;
        }
          if (Input.touchCount > 0 && setposition == false)
        {
            UpdateTouchDelta(Input.GetTouch(0).position);
        }
        else if (Input.touchCount > 0 && setposition == true && findSurfaceDone == true && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            StartChatbot();
        }
        Trackable trackable = trackingResult.GetTrackable(0);
        Matrix4x4 poseMatrix = trackable.GetPose() * Matrix4x4.Translate(touchSumPosition);
        instantTrackable.OnTrackSuccess(trackable.GetId(), trackable.GetName(), poseMatrix);
    }

    private void UpdateTouchDelta(Vector2 touchPosition)
    {
        switch (Input.GetTouch(0).phase)
        {
            case TouchPhase.Began:
                touchToWorldPosition = TrackerManager.GetInstance().GetWorldPositionFromScreenCoordinate(touchPosition);
                break;

            case TouchPhase.Moved:
                Vector3 currentWorldPosition = TrackerManager.GetInstance().GetWorldPositionFromScreenCoordinate(touchPosition);
                touchSumPosition += (currentWorldPosition - touchToWorldPosition);
                touchToWorldPosition = currentWorldPosition;
                break;
        }
    }
    private void StartChatbot()
    {
        a.StartNativeRecognition();
    }
    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SensorDevice.GetInstance().Stop();
            TrackerManager.GetInstance().StopTracker();
            StopCamera();
        }
        else
        {
            StartCamera();
            TrackerManager.GetInstance().StartTracker(TrackerManager.TRACKER_TYPE_INSTANT);
            SensorDevice.GetInstance().Start();
        }
    }

    void OnDestroy()
    {
        SensorDevice.GetInstance().Stop();
        TrackerManager.GetInstance().StopTracker();
        TrackerManager.GetInstance().DestroyTracker();
        StopCamera();
    }

    public void OnClickStart()
    {
        if (!findSurfaceDone)
        {
            
            TrackerManager.GetInstance().FindSurface();
            if (startBtnText != null)
            {
                i1.CrossFadeAlpha(0, 0.0f, false);
                i2.CrossFadeAlpha(0, 0.0f, false);
                b1.SetActive(false);
                b2.SetActive(false);
                startBtnText.text = "Stop Tracking";
            }
            findSurfaceDone = true;
            touchSumPosition = Vector3.zero;
        }
        else
        {
            TrackerManager.GetInstance().QuitFindingSurface();
            if (startBtnText != null)
            {
                i1.CrossFadeAlpha(1, 0.0f, false);
                i2.CrossFadeAlpha(1, 0.0f, false);
                b1.SetActive(true);
                b2.SetActive(true);
                startBtnText.text = "Start Tracking";
            }
            findSurfaceDone = false;
        }
    }
    public void SetPositionTrue()
    {
        setposition = !setposition;
        setBtn.text = !setposition ? "Fix" : "UnFix";
    }
    public void indexinc()
    {
        index = Math.Abs(index + 1) % size;
        Debug.Log(index);
    }
    public void indexdec()
    {
        index = Math.Abs(index - 1) % size;
        Debug.Log(index);
    }
}
