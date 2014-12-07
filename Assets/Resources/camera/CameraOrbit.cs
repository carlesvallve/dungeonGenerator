using UnityEngine;
using System.Collections;

public class CameraOrbit : MonoBehaviour {

	//public CameraFade scriptFade;

	public Transform target;
	public Vector3 center = new Vector3(0, 0.35f, 0);
	public Vector3 angle = new Vector3(45, 45, 0);
	public float distance = 10;

	public float distanceMin = 1;
	public float distanceMax = 20;

	public float yMin = -20;
	public float yMax = 89;

	public float xSpeed = 25;
	public float ySpeed = 50;
	public float zSpeed = 15;

	public float x = 0;
	public float y = 0;

	public float interval = 5.0f; // bigger numbers increase interpolation speed

	// input controls
	public bool mouseControlsEnabled = false;
	public bool touchControlsEnabled = false;
	private bool rotating = false;

	#if !UNITY_EDITOR
		private float pinchLength = 0f;
		private float deltaLength = 0f;
	#endif


	void Start () {
		// warn if there is no target
		if(!target) {
			print("No camera target selected!");
			return;
		}

		//angle increments
		x = angle.y;
		y = angle.x;

		// camera fade-in
		/*scriptFade = gameObject.GetComponent<CameraFade>();
		if(!scriptFade) {
			print("CameraFade script is required in order to fade the scene!");
			return;
		}
		//StartCoroutine(FadeIn(Color.white, 1.5f, 1.0f));*/
	}


	/*public IEnumerator FadeIn (Color color, float time, float delay) {
		scriptFade.SetScreenOverlayColor(color);
		yield return new WaitForSeconds(delay);
		scriptFade.StartFade(new Color(0, 0, 0, 0), time);
	}
	

	public IEnumerator FadeOut (Color color, float time, float delay) {
		yield return new WaitForSeconds(delay);
		scriptFade.StartFade(color, time);
	}*/


	public void setTarget (Transform target) {
		if (!target) target = new GameObject().transform;
		this.target = target;
	}


	void setDistance (float mouseZ) {
		distance = Mathf.Clamp(distance - mouseZ * zSpeed, distanceMin, distanceMax);
	}


	void setRotation (float mouseX, float mouseY) {
		x += mouseX * xSpeed;
		y -= mouseY * ySpeed;
		y = ClampAngle(y, yMin, yMax);
		//interval = 100;

		// update angle prop for debug purposes
		angle.x = y;
		angle.y = x;
	}


	void Update () {
		if (!target) return;
		
		// manage interactive control input
		setInputControls();

		//get new pos and rot from increments
		Quaternion rotation = Quaternion.Euler(y, x, 0);
		Vector3 position = rotation *
			new Vector3(0.0f, 0.0f, -distance) +
			target.position + center;

		//interpolate camera to new pos and rotation
		float interval = this.interval;
		if (rotating) interval = 25;

		if (interval != 0) {
			float time = Time.deltaTime;
			//transform.position = Vector3.MoveTowards(transform.position, position, time * interval);
			transform.position = Vector3.Slerp(transform.position, position, time * interval);
			transform.rotation = Quaternion.Slerp (transform.rotation, rotation, time * interval);
		} else {
			transform.position = position;
			transform.rotation = rotation;
		}
	}


	private float ClampAngle(float angle , float min , float max ) {
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}


	private void setInputControls () {

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
	}

}


