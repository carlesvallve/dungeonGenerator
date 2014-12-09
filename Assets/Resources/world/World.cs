using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour {

	public DungeonGenerator dungeon;

	CameraControls cam;
	GameObject cube1;
	GameObject cube2;


	void Start () {
		dungeon = GameObject.Find("Dungeon").GetComponent<DungeonGenerator>();

		cam = Camera.main.GetComponent<CameraControls>();

		cube1 = GameObject.Find("Cube1");
		cube2 = GameObject.Find("Cube2");
	}

	
	void Update () {
		if (Input.GetKeyDown("space")) {
			generateDungeon();
		}

		if (Input.GetKeyDown("1")) {
			locateCubesAtRandom();
		}

		if (Input.GetKeyDown("2")) {
			interpolateCamera();
		}
	}


	private void generateDungeon () {
			// Generate a new Seed
			dungeon.seed = System.DateTime.Now.Millisecond*1000 + System.DateTime.Now.Minute*100;
			Random.seed = dungeon.seed;
			
			// Generate Dungeon
			Debug.Log ("Dungeon Generation Started");
			dungeon.GenerateDungeon(dungeon.seed);

			locateCubesAtRandom();

			// initialize grid
			//initGrid();

			// create player at the center of a random room
			/*Room room = dungeon.rooms[Random.Range(0, dungeon.rooms.Count - 1)];
			Vector3 pos = new Vector3(Mathf.Round(room.boundary.center.x), 0, Mathf.Round(room.boundary.center.y));
			player = createPlayer(pos);

			// create some monsters
			monsters = createMonsters(80);*/
	}


	private void locateCubesAtRandom () {
		cube1.transform.position = new Vector3(Random.Range(0, dungeon.MAP_WIDTH), 0.6f, Random.Range(0, dungeon.MAP_HEIGHT));
		cube2.transform.position = new Vector3(Random.Range(0, dungeon.MAP_WIDTH), 0.6f, Random.Range(0, dungeon.MAP_HEIGHT));
	}


	private void interpolateCamera () {
		if (cam.target == cube1.transform) {
			cam.target = cube2.transform;
		} else {
			cam.target = cube1.transform;
		}

		cam.rotating.angle = new Vector3(Random.Range(10, 60), Random.Range(-360, 360), 0);
		cam.zooming.distance = Random.Range(10, 80);
		cam.panning.position = Vector3.zero;
	}


	/*private void initGrid () {
		// init grid cells for astar calculations
        Grid.InitEmpty(dungeon.MAP_WIDTH, dungeon.MAP_HEIGHT);
        print ("Initializing grid " + Grid.xsize + "," + Grid.ysize);

        // set walkability map
        for (int y = 0; y < Grid.ysize; y++) {
			for (int x = 0; x < Grid.xsize; x++) {
				// set rooms and corridor cells to walkable
				Tile tile = dungeon.tiles[x, y];
				if (tile.id == Tile.TILE_ROOM || tile.id == Tile.TILE_CORRIDOR) {
					Grid.setWalkable(y, x, true);
				} else {
					Grid.setWalkable(y, x, false);
				}
			}
		}
	}

	// *****************************************************
	// Gestures
	// *****************************************************

	/*private void initTouchControls() {
		// register touch events
		touchControls = GameObject.Find("TouchControls").GetComponent<TouchControls>();

		touchLayer = touchControls.getLayer("grid");
		touchLayer.onPress += onTouchPress;
		touchLayer.onRelease += onTouchRelease;
		touchLayer.onMove += onTouchMove;
		touchLayer.onSwipe += onTouchSwipe;
	}


	public void onTouchPress (TouchEvent e) {
		// Vector3 pos = e.activeTouch.getPos3d(Camera.main);
		Transform obj = e.activeTouch.getObject3d(Camera.main);
		if (!obj) return;

		player.moveTo(new Vector3(obj.position.x, 0, obj.position.z));

		// get tile at current path node
		//Tile tile = dungeon.tiles[(int)path[0].y, (int)path[0].x];
		//Vector3 pos = tile.obj.transform.position + Vector3.up * tile.obj.transform.lossyScale.y / 2;
		//pos += Vector3.up * transform.lossyScale.y / 2;;
	}


	public void onTouchRelease (TouchEvent e) {
		//print("release " + e.activeTouch.getPos3d(Camera.main));
	}


	public void onTouchMove (TouchEvent e) {
		//print("move " + e.activeTouch.getPos3d(Camera.main));
	}


	public void onTouchSwipe (TouchEvent e) {
		//print ("swipe " + e.activeTouch.getVelocity3d(Camera.main));
	}*/
}
