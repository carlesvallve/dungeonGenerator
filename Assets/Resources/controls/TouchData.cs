using UnityEngine;

public class TouchData {
	public bool active = false;
	public int id;

	public Vector2 startPos;
	public Vector2 endPos;
	public Vector2 deltaPos;

	public float startTime;
	public float endTime;
	public float deltaTime;

	public Vector2 lastPos;
	public Vector2 relativeDeltaPos;

	public Transform transform;
	public GameObject gameObject;

	public float swipeForce = 0.1f;
	public float maxDeltaTime = 0.5f;
	public float maxDeltaPos = 48.0f;


	public TouchData(int id) {
		this.id = id;
	}


	public bool isSwipe() {
		return deltaPos.magnitude >= maxDeltaPos && deltaTime <= maxDeltaTime;
	}


	public void setInitialData(float x, float y) {
		this.startPos = new Vector2(x, y);
		this.endPos = new Vector2(x, y);
		this.lastPos = new Vector2(x, y);
		this.deltaPos = Vector2.zero;
	
		this.startTime = Time.time;
		this.endTime = Time.time;
		this.deltaTime = 0;
	}


	public void setData(float x, float y) {
		this.endPos = new Vector2(x, y);

		this.deltaPos = this.endPos - this.startPos;

		this.relativeDeltaPos = this.endPos - this.lastPos;
		this.lastPos = this.endPos;

		this.endTime = Time.time;
		this.deltaTime = this.endTime - this.startTime;
	}


	public Transform getObject3d(Camera camera) {
		Ray ray = Camera.main.ScreenPointToRay(this.endPos);
		RaycastHit hit = new RaycastHit();
	
		if (Physics.Raycast(ray, out hit, 100)) {
			return hit.transform;
		}

		return null;
	}


	public Vector3 getPos3d(Camera camera) {
		Ray ray = camera.ScreenPointToRay(this.endPos);
		RaycastHit hit = new RaycastHit();
	
		if (Physics.Raycast(ray, out hit, 1000)) {
			 return hit.point;
		}

		return new Vector3(0, -1000, 0);
	}


	public Vector3 getDelta3d(Camera camera) {
		Vector2 pos = this.deltaPos;
		Vector3 cameraRelativeVector = camera.transform.TransformDirection(pos.x, pos.y, pos.y);
		cameraRelativeVector.y = 0;
		return cameraRelativeVector;
	}


	public Vector3 getVelocity3d(Camera camera) {
		return this.swipeForce * (this.getDelta3d(camera) / this.deltaTime);
	}
}
