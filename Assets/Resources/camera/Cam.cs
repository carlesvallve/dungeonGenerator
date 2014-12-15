using UnityEngine;
using System.Collections;

public class Cam : MonoBehaviour {

	public Transform target;
	public Vector3 panning = new Vector3(0, 0.35f, 0);
	public Vector3 angle = new Vector3(55, -45, 0);
	public float distance = 20f;

	private bool isRotating;


	void Start () {
		if(!target) {
			Debug.LogError("A camera target is required!");
			return;
		}

		setTarget(target);
	}


	public void setTarget (Transform target) {
		this.target = target;
		transform.rotation = Quaternion.Euler(angle.x, angle.y, 0);
		transform.position =  transform.rotation * new Vector3(0.0f, 0.0f, -distance) + target.position + panning;
	}

	
	void Update () {
		#if UNITY_EDITOR
			if (Input.GetMouseButtonDown(0)) isRotating = true;
			if (Input.GetMouseButtonUp(0)) isRotating = false;
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
}
