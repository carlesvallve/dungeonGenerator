using UnityEngine;
using System.Collections;

public class Cam : MonoBehaviour {

	public TouchControls touchControls;
	public TouchLayer touchLayer;

	public Transform target;
	public Vector3 panning = new Vector3(0, 0.35f, 0);
	public Vector3 angle = new Vector3(55, -45, 0);
	public float distance = 20f;

	private bool isRotating;
	//public bool isPanning;


	void Start () {
		if(!target) {
			Debug.LogError("A camera target is required!");
			return;
		}

		setTarget(target);

		initTouchControls();
	}


	public void setTarget (Transform target) {
		this.target = target;
		transform.rotation = Quaternion.Euler(angle.x, angle.y, 0);
		transform.position =  transform.rotation * new Vector3(0.0f, 0.0f, -distance) + target.position + panning;
	}

	
	void Update () {
		#if UNITY_EDITOR
			if (Input.GetMouseButtonDown(1)) isRotating = true;
			if (Input.GetMouseButtonUp(1)) isRotating = false;
			if (isRotating) {
				angle.y += Input.GetAxis("Mouse X") * 5.0f;
				angle.x -= Input.GetAxis("Mouse Y") * 5.0f;
			}
			distance -= Input.GetAxis("Mouse ScrollWheel") * 3.0f;
		#endif

		Quaternion rotation = Quaternion.Euler(angle.x, angle.y, 0);
		Vector3 position =  rotation * new Vector3(0.0f, 0.0f, -distance) + target.position + panning;

		float interval = 5.0f;
		transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * interval);
		transform.rotation = Quaternion.Lerp (transform.rotation, rotation, Time.deltaTime * interval);
	}


	// *****************************************************
	// Camera Gestures
	// *****************************************************

	private void initTouchControls() {
		// register touch events
		touchControls = GameObject.Find("TouchControls").GetComponent<TouchControls>();
		touchLayer = touchControls.getLayer("grid");
		//touchLayer.onPress += onTouchPress;
		//touchLayer.onRelease += onTouchRelease;
		touchLayer.onMove += onTouchMove;
		//touchLayer.onSwipe += onTouchSwipe;
	}


	public void onTouchPress (TouchEvent e) {
		//print("press " + e.activeTouch.getPos3d(Camera.main));
	}


	public void onTouchRelease (TouchEvent e) {
		//print("release " + e.activeTouch.getPos3d(Camera.main));
	}


	public void onTouchMove (TouchEvent e) {
		// translate camera on 1 finger move
		Vector2 d = e.activeTouch.relativeDeltaPos;
		//print(d.magnitude);
		if (d.magnitude >= 1f) {
			Vector3 direction = this.camera.transform.TransformDirection(d * 0.02f);
			panning.x -= direction.x;
			panning.z -= direction.z;
		}	
	}


	public void onTouchSwipe (TouchEvent e) {
		//print ("swipe " + e.activeTouch.getVelocity3d(Camera.main));
	}
}
