using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class AppController : MonoBehaviour
{
	public Camera FirstPersonCamera;
	public GameObject DetectedPlanePrefab;
	private bool _showPlanes = true;
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
		foreach (var plane in _allPlanes)
		{
			if (plane.TrackingState == TrackingState.Tracking)
			{
				OnTogglePlanes(!_showPlanes);
				break;
			}
		}
	}
	
	public void OnTogglePlanes(bool flag) {
		_showPlanes = flag;
		foreach (GameObject plane in GameObject.FindGameObjectsWithTag ("plane")) {
			Renderer r = plane.GetComponent<Renderer> ();
			DetectedPlaneVisualizer t = plane.GetComponent<DetectedPlaneVisualizer>();
			r.enabled = flag;
			t.enabled = flag;
		}
	}

}
