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
	private bool _isShowPlanesEnabled = true;
	private List<DetectedPlane> _allPlanes = new List<DetectedPlane>();

	private const string Tag = "NewBehaviourScript";

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
		Session.GetTrackables<DetectedPlane>(_allPlanes);
		
		
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
