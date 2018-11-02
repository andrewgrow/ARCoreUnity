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
	private bool _isShowPlanesEnabled = true;
	private List<DetectedPlane> _allPlanes = new List<DetectedPlane>();
	private GameObject _aim = null;
	private Anchor _aimAnchor = null;
	private DetectedPlane _currentPlane = null;
	private string _cameraPosition;
	private string _hitTextureCoord;
	private string _hitPoint;

	/// <summary>
	/// The rotation in degrees need to apply to model when the Point model is placed.
	/// </summary>
	private const float KModelRotation = 180.0f;


	private const string Tag = "AppControllerTag";

	// Use this for initialization
	void Start () {
		Debug.Log(Tag + "-> Start()");
	}
	
	// Update is called once per frame
	void Update () {
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
		            return;
	            }
	            
	            
                // Use hit pose and camera pose to check if hitTest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) && _currentPlane == null)
                {
	                _currentPlane = (DetectedPlane) hit.Trackable;
	                _aimAnchor = Session.CreateAnchor(hit.Pose);
	                
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
			// Instantiate Andy model at the hit pose.
			_aim = Instantiate(AimPrefab, _aimAnchor.transform.position, _aimAnchor.transform.rotation);

			// Compensate for the Aim rotation facing away from the raycast (i.e. camera).
//			_aim.transform.Rotate(0, KModelRotation, 0, Space.Self);

			// Make Point model a child of the anchor.
			_aim.transform.parent = _aimAnchor.transform;  
			
			return;
		}

		// update position
		if (_aimAnchor != null && _aim != null)
		{
//			_cameraPosition = Tag 
//			                  + " FirstPersonCamera.transform.position = " 
//			                  + FirstPersonCamera.transform.position
				;
//			Debug.Log(_cameraPosition);
//			StartCoroutine(_log(_cameraPosition));
			
//			Ray ray = FirstPersonCamera.ScreenPointToRay(FirstPersonCamera.transform.position + FirstPersonCamera.transform.forward);
			Ray ray = new Ray(FirstPersonCamera.transform.position, FirstPersonCamera.transform.forward);
//			RaycastHit hit;

			_cameraPosition = "new Ray(" + ray.origin + " -> " + ray.direction + ")";

			TrackableHit hit;
			TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
			                                  TrackableHitFlags.FeaturePointWithSurfaceNormal;

//			if (Frame.Raycast(FirstPersonCamera.transform.position, FirstPersonCamera.transform.forward, out hit,
//				500, raycastFilter))
			float centerX = 0.5f * Screen.width;
			float centerY = 0.5f * Screen.height;
			
			if (Frame.Raycast(centerX, centerY, raycastFilter, out hit))
			{
				_hitPoint = hit.Pose.position.ToString();
//				_hitPoint = hit.point.ToString();
				_aim.transform.position = hit.Pose.position;
			}
			
//			if (Frame.Raycast(ray, out hit))
//			{
//				_hitTextureCoord = hit.textureCoord.ToString();
//				_hitPoint = hit.point.ToString();
//			}
			
			
			
			Debug.Log("" 
			          +_cameraPosition
//			          + ", "+ "_hitTextureCoord = " + _hitTextureCoord
			          + ", _hitPoint = " + _hitPoint
			          );
		}
	}

	IEnumerator _log(string s)
	{
		for (;;)
		{
			Debug.Log(s);
			yield return new WaitForSeconds(.25f);
		}
	}
}
