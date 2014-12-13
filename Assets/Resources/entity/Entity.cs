using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Entity : MonoBehaviour {

	private World world;
	private Transform parent;
	public Transform body;

	private List<Vector2> path;
	private Sequence moveSequence;


	public void init (World world, Transform parent, Vector3 pos, Vector3 rot) {
		this.world = world;
		this.body = transform.Find("Body");

		transform.parent = parent.transform;
		transform.localPosition = pos;
		transform.localEulerAngles = rot;

		DOTween.defaultEaseType = Ease.Linear;
	}


	public void moveTo (Vector3 goal) {
		path = world.astar.SearchPath(transform.position, goal);
		world.astar.PrintPath(path);

		if (path.Count == 0) {
			return;
		}


		// Create move Sequence
		moveSequence = DOTween.Sequence();
		for(int i = 0; i < path.Count; i++) {
			addStep(i);
		}

		// rotate ent and delay sequence by rotation time
		moveSequence.PrependInterval(0.15f);
		transform.DOLookAt(new Vector3(path[0].x, 0, path[0].y), 0.15f, AxisConstraint.Y);
	}


	private void addStep(int i) {
		// get pos at path step
		Vector3 pos = new Vector3(path[i].x, 0, path[i].y);
		
		// move
		moveSequence.Append(
			transform.DOLocalMove(pos, 0.3f)
				.OnComplete(()=>endStep(i, pos))
		);

		// rotate
		moveSequence.Join(transform.DOLookAt(pos, 0.15f, AxisConstraint.Y));
	}


	private void endStep (int i, Vector3 pos) {
		print("step" + i + " -> " + pos);

		/*if (i < path.Count) {
			Vector3 nextPos = path[i + 1];
			Tile tile = world.dungeon.getTileAtPos(new Vector3(nextPos.x, 0, nextPos.z));
			print (tile.id);

			if (tile.id == TileType.DOOR) {
				print ("DOOR");
			}
		}*/
	}


	void Update () {
		snapToGround();
	}


	private void snapToGround () {
		RaycastHit hit;
		Vector3 pos = new Vector3(transform.position.x, transform.position.y + 5, transform.position.z);
		if (Physics.Raycast(pos, -Vector3.up * 100, out hit)) {
			if (hit.transform != transform) {
				transform.position = new Vector3(
					transform.position.x,
					hit.point.y, // + 0.01f,
					transform.position.z
				);
			}
		}
	}
}
