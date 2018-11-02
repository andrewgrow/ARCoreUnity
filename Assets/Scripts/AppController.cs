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
                else
                {
	                if (_aim == null)
	                {
		                // Choose the Aim model for the Trackable that got hit.
		                GameObject prefab = AimPrefab;

		                // Instantiate Andy model at the hit pose.
		                _aim = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

		                // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
		                _aim.transform.Rotate(0, KModelRotation, 0, Space.Self);

		                // Create an anchor to allow ARCore to track the hitPoint as understanding of the physical
		                // world evolves.
//		                _aimAnchor = hit.Trackable.CreateAnchor(hit.Pose);

		                // Make Point model a child of the anchor.
		                _aim.transform.parent = _aimAnchor.transform;       
	                }
                }
            }
	}

	/// <summary>
	/// Physics will be updated here.
	/// </summary>
	private void FixedUpdate()
	{
		
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

}
