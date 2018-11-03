using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;

// Set up touch input propagation while using Instant Preview in the editor.
#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class AppController : MonoBehaviour
{
	public Camera FirstPersonCamera;
	public GameObject DetectedPlanePrefab;
	public GameObject AimPrefab;
	public GameObject ReferencePlanPrefab;
	private bool _isShowPlanesEnabled = true;
	private List<DetectedPlane> _allPlanes = new List<DetectedPlane>();
	private GameObject _aim = null;
	private Anchor _aimAnchor = null;
	private DetectedPlane _currentPlane = null;
	private string _cameraPosition;
	private string _hitTextureCoord;
	private string _hitPoint;
	private GameObject _referencePlan;
	public GUIStyle buttonBlue;

	/// <summary>
	/// The rotation in degrees need to apply to model when the Point model is placed.
	/// </summary>
	private const float KModelRotation = 180.0f;


	private const string Tag = "AppControllerTag";

	// Use this for initialization
	void Start () {
		Debug.Log(Tag + "-> Start()");
//		Screen.fullScreen = false;
	}
	
	// Update is called once per frame
	void Update () {

		if (OnBackButtonClicked())
		{
			return;
		}

		if (Screen.fullScreen)
		{
			Screen.fullScreen = false;
		}
		
		Touch touch;
		if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
		{
			return;
		}

		// get all planes
		Session.GetTrackables<DetectedPlane>(_allPlanes);
		// select current plane
		_selectPlane(touch);


		// enable or disable a visualisation of found planes
		// FIXME (it need to remove or enable) 
//		foreach (var plane in _allPlanes)
//		{
//			if (plane.TrackingState == TrackingState.Tracking)
//			{
//				//	revers (!bool)
//				OnTogglePlanes(!_isShowPlanesEnabled);
//				break;
//			}
//		}
	}

	private bool OnBackButtonClicked()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
				activity.Call<bool>("moveTaskToBack", true);
			}
			else
			{
				Application.Quit();
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Enable or disable rendering for all planes.
	/// </summary>
	public void OnTogglePlanes(bool flag) {
		_isShowPlanesEnabled = flag;
		foreach (GameObject plane in GameObject.FindGameObjectsWithTag ("plane")) {
			Renderer r = plane.GetComponent<Renderer> ();
			DetectedPlaneVisualizer t = plane.GetComponent<DetectedPlaneVisualizer>();
			r.enabled = flag;
			t.enabled = flag;
		}
	}

	private void _selectPlane(Touch touch)
	{
		// RayCast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {

	            if (Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
		                hit.Pose.rotation * Vector3.up) < 0)
	            {
		            Debug.LogError("back rayCast");
		            return;
	            }
	            
	            
                // Use hit pose and camera pose to check if hitTest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) && _currentPlane == null)
                {
	                _currentPlane = (DetectedPlane) hit.Trackable;
	                _aimAnchor = Session.CreateAnchor(hit.Pose);
	                

	                if (_referencePlan == null)
	                {
		                // Instantiate Plan model at the hit pose.
		                _referencePlan = Instantiate(ReferencePlanPrefab, _aimAnchor.transform.position,
			                _aimAnchor.transform.rotation);
		                // Make Plan model a child of the anchor.
		                _referencePlan.transform.parent = _aimAnchor.transform;
	                }
	                
	                var session = GameObject.Find("ARCore Device")
		                .GetComponent<ARCoreSession>(); 
	                session.SessionConfig.PlaneFindingMode = DetectedPlaneFindingMode.Disabled; 
//	                session.OnEnable();
                }              
            }
	}

	/// <summary>
	/// Physics will be updated here.
	/// </summary>
	private void FixedUpdate()
	{
		_UpdateAim();
	}

	private void _UpdateAim()
	{
		// make new aim
		if (_aimAnchor != null && _aim == null)
		{
			// Instantiate Aim model at the hit pose.
			_aim = Instantiate(AimPrefab, _aimAnchor.transform.position, _aimAnchor.transform.rotation);

			// Compensate for the Aim rotation facing away from the raycast (i.e. camera).
//			_aim.transform.Rotate(0, KModelRotation, 0, Space.Self);

			// Make Aim model a child of the anchor.
//			_aim.transform.parent = _aimAnchor.transform;  
			
			return;
		}

		// update position
		if (_aimAnchor != null && _aim != null)
		{
			Ray ray = new Ray(FirstPersonCamera.transform.position, FirstPersonCamera.transform.forward);
			
			RaycastHit raycastHit;
			if (Physics.Raycast(ray, out raycastHit))
			{
				if (raycastHit.collider.name.Contains("ReferencePlane"))
				{
					_aim.transform.position = raycastHit.point;
				// Movement speed in units/sec.
//					float speed = 0.8F;
//					Vector3 startPosition = _aim.transform.position;
//					Vector3 endPosition = raycastHit.point;
//					_aim.transform.position = Vector3.Lerp (startPosition, endPosition, speed);
//					_aim.transform.Rotate(0, KModelRotation, 0, Space.Self); // compensate rotate
//					Debug.Log("startPosition, endPosition" + startPosition + ", " + endPosition);
				}
				else
				{
					var lastY = _aim.transform.position.y;
					Vector3 newPos = new Vector3(raycastHit.point.x, lastY, raycastHit.point.z);
					_aim.transform.position = newPos;
				}
			}
		}
	}
	
//	void OnGUI () {
//		if (GUI.Button (new Rect (25, 25, 300, 100), "Button")) {
//			// This code is executed when the Button is clicked
//		}
//	}

	public void OnClickButtonDelete()
	{
		// Destroy the Aim
		if (_aim != null)
		{
			Destroy(_aim);
			_aim = null;
		}

		// destroy the Aim's Anchor
		if (_aimAnchor != null)
		{
			Destroy(_aimAnchor);
			_aimAnchor = null;
		}

		// destroy the Reference Plan
		if (_referencePlan != null)
		{
			Destroy(_referencePlan);
			_referencePlan = null;
		}
		
		// destroy the Current Plan
		_currentPlane = null;
		
		var session = GameObject.Find("ARCore Device")
			.GetComponent<ARCoreSession>(); 
		session.SessionConfig.PlaneFindingMode = DetectedPlaneFindingMode.HorizontalAndVertical; 
	    session.OnEnable();
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		OnClickButtonDelete();
	}
}
