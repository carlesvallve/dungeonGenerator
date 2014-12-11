using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {

	public Transform body;

	public void init (Transform parent, Vector3 pos) {

		transform.parent = parent.transform;
		transform.localPosition = pos;

		//body.eulerAngles = new Vector3(0, 180, 0);
	}
}
