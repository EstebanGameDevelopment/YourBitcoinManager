/// <summary>
/// write by 52cwalk,if you have some question ,please contract lycwalk@gmail.com
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class DeviceCameraController : MonoBehaviour {

	public enum CameraMode
	{
		FACE_C,
		DEFAULT_C,
		NONE
	}

	[HideInInspector]
	public WebCamTexture cameraTexture;
    string CAMERA_PERMISSION = "android.permission.CAMERA";
    
    private bool isPlay = false;
	GameObject e_CameraPlaneObj;
	bool isCorrected = false;
	float screenVideoRatio = 1.0f;
    
	public bool isPlaying
	{
		get{
			return isPlay;
		}
	}

	// Use this for initialization  
	void Awake()  
	{
        StartWork();
        e_CameraPlaneObj = transform.Find ("CameraPlane").gameObject;
	}
    
	// Update is called once per frame  
	void Update()  
	{  
		if (isPlay) {  
			if(e_CameraPlaneObj.activeSelf)
			{
				e_CameraPlaneObj.GetComponent<Renderer>().material.mainTexture = cameraTexture;
			}
		}


		if (cameraTexture != null && cameraTexture.isPlaying) {
			if (cameraTexture.width > 200 && !isCorrected) {
				correctScreenRatio();
			}
		}

	}
    

	/// <summary>
	/// Stops the work.
	/// when you need to leave current scene ,you must call this func firstly
	/// </summary>
	public void StartWork()
	{

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!AndroidPermissionsManager.IsPermissionGranted(CAMERA_PERMISSION))
        {
            requestCameraPermissions();
            return;
        }
#endif

        if (cameraTexture == null)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE
            cameraTexture = new WebCamTexture();
#elif UNITY_EDITOR_OSX
			cameraTexture = new WebCamTexture(640,480);  
#elif UNITY_IOS
			cameraTexture = new WebCamTexture(640,480);
#elif UNITY_ANDROID
			cameraTexture = new WebCamTexture(640,480);
#else
			cameraTexture = new WebCamTexture(); 
#endif
        }
        
        if (this.cameraTexture != null && !this.cameraTexture.isPlaying)
        {
            this.cameraTexture.Play();
        }
        else if(this.cameraTexture == null)
        {
            this.cameraTexture.Play();
        }
        isPlay = true;
    }

    public void StopWork()
    {
        isPlay = false;
        if (this.cameraTexture != null && this.cameraTexture.isPlaying)
        {
            this.cameraTexture.Stop();
            Destroy(this.cameraTexture);
            this.cameraTexture = null;
        }
    }

    /// <summary>
    /// Corrects the screen ratio.
    /// </summary>
    void correctScreenRatio()
	{
		int videoWidth = 640;
		int videoHeight = 480;
		int ScreenWidth = 640;
		int ScreenHeight = 480;

		float videoRatio = 1;
		float screenRatio = 1;

		if (cameraTexture != null) {
			videoWidth = cameraTexture.width;
			videoHeight = cameraTexture.height;
		}
		videoRatio = videoWidth * 1.0f / videoHeight;
		ScreenWidth = Mathf.Max (Screen.width, Screen.height);
		ScreenHeight = Mathf.Min (Screen.width, Screen.height);
		screenRatio = ScreenWidth * 1.0f / ScreenHeight;

		screenVideoRatio = screenRatio / videoRatio;
		isCorrected = true;

		if (e_CameraPlaneObj != null) {
			e_CameraPlaneObj.GetComponent<CameraPlaneController>().correctPlaneScale(screenVideoRatio);
		}
	}

	public float getScreenVideoRatio()
	{
		return screenVideoRatio;
	}

    

    void requestCameraPermissions()
    {
        AndroidPermissionsManager.RequestPermission(new[] { CAMERA_PERMISSION }, new AndroidPermissionCallback(
            grantedPermission =>
            {
                StartCoroutine(waitTimeToOpenCamera());
                // The permission was successfully granted, restart the change avatar routine
            },
            deniedPermission =>
            {
                // The permission was denied
            },
            deniedPermissionAndDontAskAgain =>
            {
                // The permission was denied, and the user has selected "Don't ask again"
                // Show in-game pop-up message stating that the user can change permissions in Android Application Settings
                // if he changes his mind (also required by Google Featuring program)
            }));
    }
    
    IEnumerator waitTimeToOpenCamera()
    {
        yield return new WaitForSeconds(0.2f);
        StartWork();
    }
    

}


