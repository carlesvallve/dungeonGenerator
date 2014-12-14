using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour {

	public TouchControls touchControls;
	public TouchLayer touchLayer;

	public DungeonGenerator dungeon;
	public Astar astar;

	public CameraControls cam;

	private Entity entity;
	
	private GameObject cube1;
	private GameObject cube2;


	void Start () {
		dungeon = GameObject.Find("Dungeon").GetComponent<DungeonGenerator>();
		cam = Camera.main.GetComponent<CameraControls>();

		cube1 = GameObject.Find("Cube1");
		cube2 = GameObject.Find("Cube2");

		initTouchControls();
	}

	
	void Update () {
		if (Input.GetKeyDown("space")) generateDungeon();
		if (Input.GetKeyDown("1")) locateCubesAtRandom();
		if (Input.GetKeyDown("2")) changeCameraTarget();
		if (Input.GetKeyDown("3")) changeCameraView();
	}


	private void generateDungeon () {
		// Generate a new Seed
		dungeon.seed = System.DateTime.Now.Millisecond*1000 + System.DateTime.Now.Minute*100;
		Random.seed = dungeon.seed;
		
		// Generate Dungeon
		Debug.Log ("Dungeon Generation Started with seed " + dungeon.seed);
		dungeon.GenerateDungeon(dungeon.seed);
		//dungeon.logGrid();
		//dungeon.logRooms();

		// initialize astar pathfinder
		astar = new Astar(dungeon.tiles);

		// create entity hero
		if (entity) Destroy(entity.gameObject);
		Vector3 pos = dungeon.getRandomPosInDungeon();
		Vector3 rot = new Vector3(0, new int[] { 0, 90, 180, 270 }[Random.Range(0, 4)], 0);
		entity = createEntity(pos, rot);
		
		// set camera target
		cam.target = entity.transform;
	}


	private Entity createEntity (Vector3 pos, Vector3 rot) {
		GameObject obj = (GameObject)Instantiate(Resources.Load("entity/Ent"));
		Entity entity = obj.GetComponent<Entity>();
		entity.init(this, this.transform, pos, rot);

		return entity;
	}


	private void locateCubesAtRandom () {
		cube1.transform.position = new Vector3(Random.Range(0, dungeon.MAP_WIDTH), 0.5f, Random.Range(0, dungeon.MAP_HEIGHT));
		cube2.transform.position = new Vector3(Random.Range(0, dungeon.MAP_WIDTH), 0.5f, Random.Range(0, dungeon.MAP_HEIGHT));
	}


	private void changeCameraTarget () {
		if (cam.target == cube1.transform) {
			cam.target = cube2.transform;
		} else {
			cam.target = cube1.transform;
		}

		cam.interpolateTo (Vector3.zero, new Vector3(Random.Range(10, 90), Random.Range(0, 360), 0), Random.Range(10, 80));
	}


	private void changeCameraView () {
		cam.interpolateTo (Vector3.zero, new Vector3(Random.Range(10, 90), Random.Range(0, 360), 0), Random.Range(10, 80));
	}


	// *****************************************************
	// Gestures
	// *****************************************************

	private void initTouchControls() {
		// register touch events
		touchControls = GameObject.Find("TouchControls").GetComponent<TouchControls>();

		touchLayer = touchControls.getLayer("grid");
		touchLayer.onPress += onTouchPress;
		touchLayer.onRelease += onTouchRelease;
		touchLayer.onMove += onTouchMove;
		touchLayer.onSwipe += onTouchSwipe;
	}


	private Vector3 pressPos;
	public void onTouchPress (TouchEvent e) {
		//print("press " + e.activeTouch.getPos3d(Camera.main));
		pressPos = e.activeTouch.getPos3d(Camera.main);
	}


	public void onTouchRelease (TouchEvent e) {
		//print("release " + e.activeTouch.getPos3d(Camera.main));

		Vector3 pos = e.activeTouch.getPos3d(Camera.main);
		if ((pressPos - pos).magnitude > 0.5f) return;

		Transform obj = e.activeTouch.getObject3d(Camera.main);
		if (!obj) return;

		//Tile tile = dungeon.getTileAtPos(obj.transform.position);
		//print(obj + " " + tile.id);

		entity.moveTo(obj.transform.position);
	}


	public void onTouchMove (TouchEvent e) {
		//print("move " + e.activeTouch.getPos3d(Camera.main));
	}


	public void onTouchSwipe (TouchEvent e) {
		//print ("swipe " + e.activeTouch.getVelocity3d(Camera.main));
	}
}
