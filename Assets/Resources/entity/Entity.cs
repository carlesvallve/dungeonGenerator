using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Entity : MonoBehaviour {

	private World world;
	private Transform parent;
	public Transform body;

	public void init (World world, Transform parent, Vector3 pos, Vector3 rot) {
		this.world = world;
		this.parent = parent;

		transform.parent = parent.transform;
		transform.localPosition = pos;
		transform.localEulerAngles = rot;
	}


	public void moveTo (Vector3 pos) {
		List<Vector2> path = world.astar.SearchPath(transform.position, pos);
		world.astar.PrintPath(path);
	}
}
