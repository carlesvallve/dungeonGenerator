using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;


public class Entity : MonoBehaviour {

	private World world;

	private Transform body;
	private Transform torax;
	private Transform head;
	private Transform armR;
	private Transform armL;
	private Transform hip;
	private Transform legR;
	private Transform legL;

	private List<Vector2> path;
	private Sequence moveSequence;

	private bool moving = false;
	private float speed = 0.4f;
	

	// ************************************************************
	// Init
	// ************************************************************

	public void init (World world, Transform parent, Vector3 pos, Vector3 rot) {
		this.world = world;
		
		transform.parent = parent.transform;
		transform.localPosition = pos;
		transform.localEulerAngles = rot;

		getParts();

		DOTween.defaultEaseType = Ease.Linear; //InOutQuad;
	}


	private void getParts () {
		body = transform.Find("Body");
		torax = transform.Find("Body/Torax");
		head = transform.Find("Body/Torax/Head");
		armR = transform.Find("Body/Torax/ArmR");
		armL = transform.Find("Body/Torax/ArmL");
		hip = transform.Find("Body/Hip");
		legR = transform.Find("Body/Hip/LegR");
		legL = transform.Find("Body/Hip/LegL");
	}


	// ************************************************************
	// Move
	// ************************************************************

	private void renderPath () {
		for(int i = 0; i < path.Count; i++) {
			Tile tile = world.dungeon.getTileAtPos(new Vector3(path[i].x, 0, path[i].y));
			Transform cube = tile.obj.transform.Find("Cube");
			cube.localScale = new Vector3(0.5f, 0.2f, 0.8f);
			cube.gameObject.renderer.material.color = Color.yellow;
		}
	}


	public void moveTo (Vector3 goal) {
		// get path
		path = world.astar.SearchPath(transform.position, goal);
		if (path.Count == 0) { return; }
		renderPath();

		// create move sequence
		moveSequence = DOTween.Sequence();
		for(int i = 0; i < path.Count; i++) { addStep(i); }
		moveSequence.PrependCallback(startMove);
		moveSequence.AppendCallback(endMove);
		
		// if enough angle diff, rotate ent and delay sequence by rotation time
		Vector3 vec = new Vector3(path[0].x, 0, path[0].y) - transform.position;
		float angle = Vector3.Angle(transform.forward, vec);
		if (angle >= 45) {
			moveSequence.PrependInterval(speed / 2);
			transform.DOLookAt(new Vector3(path[0].x, 0, path[0].y), speed / 2, AxisConstraint.Y);
		}
	}


	private void startMove () {
		moving = true;
		animStep(1);
	}


	private void addStep(int i) {
		Vector3 pos = new Vector3(path[i].x, 0, path[i].y);
		moveSequence.Append(transform.DOLocalMove(pos, speed).OnComplete(()=>endStep(i, pos)));
		moveSequence.Join(transform.DOLookAt(pos, speed / 2, AxisConstraint.Y));
	}


	private void endStep (int i, Vector3 pos) {
		//print("step" + i + " -> " + (path.Count - 1));
		//if (i < path.Count - 1) { jump(); }
		/*if (i < path.Count) {
			Vector3 nextPos = path[i + 1];
			Tile tile = world.dungeon.getTileAtPos(new Vector3(nextPos.x, 0, nextPos.z));
			print (tile.id);
			if (tile.id == TileType.DOOR) {
				print ("DOOR");
			}
		}*/
	}


	private void endMove () {
		moving = false;
	}


	private void animStep (int d) {
		if (!moving) return;

		float t = speed / 3;

		DOTween.Sequence()
			// move forward
			.Append(legR.DOLocalRotate(new Vector3(45 * d, 0, 0), t))
			.Join(legL.DOLocalRotate(new Vector3(-45 * d, 0, 0), t))

			.Join(armR.DOLocalRotate(new Vector3(-45 * d, 0, 0), t))
			.Join(armL.DOLocalRotate(new Vector3(45 * d, 0, 0), t))

			.Join(torax.DOLocalRotate(new Vector3(0, -10 * d, 0), t))
			.Join(head.DOLocalRotate(new Vector3(0, 20 * d, 0), t))

			// return
			.Append(legR.DOLocalRotate(new Vector3(0, 0, 0), t))
			.Join(legL.DOLocalRotate(new Vector3(0, 0, 0), t))

			.Join(armR.DOLocalRotate(new Vector3(0, 0, 0), t))
			.Join(armL.DOLocalRotate(new Vector3(0, 0, 0), t))

			.Join(torax.DOLocalRotate(new Vector3(0, 0, 0), t))
			.Join(head.DOLocalRotate(new Vector3(0, 0, 0), t))

			// initiate next step
			.OnComplete(()=>animStep(d == 1 ? -1 : 1));
	}


	// ************************************************************
	// Jump
	// ************************************************************

	//private float jumpForce = 9f;

	/*private void jump () {
		// reset box physics
		body.rigidbody.velocity = Vector3.zero;
		body.rigidbody.angularVelocity = Vector3.zero;

		//body.rigidbody.Sleep();
		//body.position = Vector3.zero;

		// make box jump
		//Audio.play("audio/Squish", 0.5f, Random.Range(1.0f, 2.0f));
		body.rigidbody.AddForce( new Vector3(0, jumpForce * body.rigidbody.mass, 0), ForceMode.Impulse);
	}*/


	/*void Update () {
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
	}*/
}
