using UnityEngine;
using System.Collections;

public class CameraControls : MonoBehaviour {

	public Transform target;
	
	
	public float interval = 5.0f; // bigger numbers increase interpolation speed

	// input controls
	public bool mouseControlsEnabled = false;
	public bool touchControlsEnabled = false;
	

	// pan
	private bool panning = false;
	public float panSpeed = 0.2f;
	public Vector3 center = new Vector3(0, 0.35f, 0);

	// rotate
	private bool rotating = false;
	public Vector3 angle = new Vector3(45, 45, 0);
	public Vector2 rotationSpeed = new Vector3(50, 25, 0);
	public float xAngleMin = -20;
	public float xAngleMax = 89;


	// zoom
	public float distance = 10;
	public float distanceMin = 1;
	public float distanceMax = 20;
	public float zoomSpeed = 15;


	void Start () {
		// warn if there is no target
		if(!target) {
			print("No camera target selected!");
			return;
		}
	}


	void Update () {
		setInputControls();
	}


	void LateUpdate () {
		if (!target) return;

		//get new pos and rot from increments
		// transform.RotateAround(Vector3.zero, Vector3.up, 20 * Time.deltaTime);
		Quaternion rotation = Quaternion.Euler(angle.x, angle.y, 0);
		
		//Vector3 c = rotating ? center : getDelta3d(center);
		Vector3 position =  rotation * new Vector3(0.0f, 0.0f, -distance) + target.position + getDelta3d(center);


		//position = getDelta3d(position);

		//interpolate camera to new pos and rotation
		float interval = 0;//this.interval;
		//if (rotating) interval = 25;

		if (interval != 0) {
			float time = Time.deltaTime;

			transform.position = Vector3.Slerp(transform.position, position, time * interval);
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, time * interval);

		} else {
			transform.position = position;
			transform.rotation = rotation;
		}
	}


	void setInputControls () {
		// middle button to pan
		if (Input.GetMouseButtonDown(2)) panning = true;
		if (Input.GetMouseButtonUp(2)) panning = false;
		if (panning) {
			center.x -= Input.GetAxis("Mouse X") * panSpeed;
			center.z -= Input.GetAxis("Mouse Y") * panSpeed;
			//center = getDelta3d(center);
		}

		// right button to rotate
		if (Input.GetMouseButtonDown(1)) rotating = true;
		if (Input.GetMouseButtonUp(1)) rotating = false;
		if (rotating) {
			angle.y += Input.GetAxis("Mouse X") * rotationSpeed.y;
			angle.x -= Input.GetAxis("Mouse Y") * rotationSpeed.x;
			angle.x = ClampAngle(angle.x, xAngleMin, xAngleMax);
		}

		// mouse wheel to zoom
		setDistance(Input.GetAxis("Mouse ScrollWheel"));
	}


	void setDistance (float mouseZ) {
		distance = Mathf.Clamp(distance - mouseZ * zoomSpeed, distanceMin, distanceMax);
	}


	float ClampAngle(float angle , float min , float max ) {
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}


	public Vector3 getDelta3d(Vector3 pos) {
		Vector3 cameraRelativeVector = Camera.main.transform.TransformDirection(pos.x, pos.z, pos.z);
		cameraRelativeVector.y = pos.y;
		return cameraRelativeVector;
	}


	//**********************************
	// Controls for device
	// *********************************

	/*private void setInputControls () {

		#if UNITY_EDITOR
			if (!mouseControlsEnabled) return;

			// -------------------------
			// control camera with mouse
			// -------------------------

			if (Input.GetButtonDown("Fire2")) rotating = true;
			if (Input.GetButtonUp("Fire2")) rotating = false;

			setDistance(Input.GetAxis("Mouse ScrollWheel"));

			if (rotating) {
				setRotation(
					Input.GetAxis("Mouse X"),
					Input.GetAxis("Mouse Y") * 0.5f
				);
			}

		#else
			if (!touchControlsEnabled) return;

			// --------------------------------------------------
			// control camera by pinching/rotating with 2 fingers
			// --------------------------------------------------

			if (Input.touchCount == 2) {
				rotating = true;

				// Zoom the camera while pinching with 2 fingers

				if (Input.GetTouch(1).phase == TouchPhase.Began) {
					pinchLength = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
				}

				if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved) {
					deltaLength = (Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) - pinchLength) * 0.01f;
					pinchLength = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);

					// set camera distance
					setDistance(deltaLength);
				}

				// Rotate camera by moving with 2 fingers

				for (int i = 0; i < Input.touchCount; ++i) {
					Touch touch = Input.GetTouch(i);
					if (touch.phase == TouchPhase.Began) rotating = true;
					if (touch.phase == TouchPhase.Ended) rotating = false;

					if (rotating) {
						setRotation(
							touch.deltaPosition.x * 0.05f,
							touch.deltaPosition.y * 0.05f
						);
					}
				}

			} else if (Input.touchCount == 2) {
				rotating = false;
			}
		#endif
	}*/

}


