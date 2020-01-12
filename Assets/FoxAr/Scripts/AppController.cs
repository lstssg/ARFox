using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;
#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif
public class AppController : MonoBehaviour {

    public Camera FirstPersonCamera;
    public GameObject prefab;
    private bool mIsQuitting = false;
    private const float mModelRotation = 180.0f;
    // Use this for initialization
    void Start () {
        OnCheckDevice();
    }
	
	// Update is called once per frame
	void Update () {
        UpdateApplicationLifecycle();

        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }
        Debug.Log("射线击中了！");
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.PlaneWithinBounds;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            if ((hit.Trackable is DetectedPlane) && Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("射线击中了DetectedPlane的背面！");
            }
            else
            {
                var FoxObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);
                FoxObject.transform.Rotate(-90.0f, mModelRotation, 0, Space.Self);
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                FoxObject.transform.parent = anchor.transform;
            }
        }
    }
    /// <summary>
    /// 检查设备
    /// </summary>
    private void OnCheckDevice()
    {
        if(Session.Status == SessionStatus.ErrorSessionConfigurationNotSupported)
        {
            ShowAndroidToastMessage("ARCore在本机上不受支持或配置错误！");
            mIsQuitting = true;
            Invoke("DoQuit", 0.5f);
        }
        else if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            ShowAndroidToastMessage("AR应用的运行需要使用摄像头，现无法获取到摄像头授权信息，请允许使用摄像头！");
            mIsQuitting = true;
            Invoke("DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            ShowAndroidToastMessage("ARCore运行时出现错误，请重新启动本程序！");
            mIsQuitting = true;
            Invoke("DoQuit", 0.5f);
        }
    }
    /// <summary>
    /// 管理应用的生命周期
    /// </summary>
    private void UpdateApplicationLifecycle()
    {
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (mIsQuitting)
        {
            return;
        }
    }
    /// <summary>
    /// 退出程序
    /// </summary>
    private void DoQuit()
    {
        Application.Quit();
    }
    /// <summary>
    /// 弹出信息提示
    /// </summary>
    /// <param name="message">要弹出的信息</param>
    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
