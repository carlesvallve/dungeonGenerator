using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/*
TODO:
- walls on corners -> OK!
- light on avatar -> OK!
- grid and mouse interaction -> OK
- astar pathfinding -> OK

- doors on room entrances

- multiple prefab types
- FOV visibility system
- lights on rooms

- stairs and diferent height levels 
- start and end spots that lead to new levels
*/

// DungeonGenerator class. Singleton.
public class DungeonGenerator : MonoSingleton <DungeonGenerator> {
	
	// Dungeon Parameters
	public int MAP_WIDTH = 64;
	public int MAP_HEIGHT = 64;
	
	// Room Parameters
	public int ROOM_MAX_SIZE = 24;
	public int ROOM_MIN_SIZE = 4;
	public int ROOM_WALL_BORDER = 1;
	public bool ROOM_UGLY_ENABLED = true; // used to eliminate ugly zones
	public float ROOM_MAX_RATIO = 5.0f;   // used to eliminate ugly zones
	
	// QuadTree Generation Parameters
	public int MAX_DEPTH = 10;
	public int CHANCE_STOP = 5;
	public int SLICE_TRIES = 10;

	// Corridor Generation Parameters
	public int CORRIDOR_WIDTH = 2;
	
	// Tilemap
	public Tile[,] tiles;
	
	// Prefabs and Instance Management
	public GameObject containerRooms;
	public GameObject prefabFloor;
	public GameObject prefabWall;
	public GameObject prefabDoor;
	public GameObject meshCombiner;
	
	// The Random Seed
	public int seed = -1;
	
	// QuadTree for dungeon distribution
	public QuadTree quadTree;
	
	// List of rooms
	public List<Room> rooms;
	
	// Auxiliar vars
//	private GameObject floor;
	private bool debugToTexture = false; // in case we want to export textures to illustreate our process
	private Texture2D dungeonTexture;
	
	// On Awake
	public override void Init() {
		// Initialize the tilemap
		tiles = new Tile[MAP_HEIGHT,MAP_WIDTH];
		for (int i = 0; i < MAP_HEIGHT; i++) {
			for (int j = 0; j < MAP_WIDTH; j++) {
				tiles[i,j] = new Tile(TileType.EMPTY);
			}
		}
		
		// Init QuadTree
		quadTree = new QuadTree(new AABB(new XY(MAP_WIDTH/2.0f,MAP_HEIGHT/2.0f),new XY(MAP_WIDTH/2.0f, MAP_HEIGHT/2.0f)));

		// List of rooms
		rooms = new List<Room>();
		
	}
	
	
	// Clean everything
	public void ResetDungeon() {
		// Disable player
		//player.SetActive(false);
		
		// Reset tilemap
		for (int i = 0; i < MAP_HEIGHT; i++) 
			for (int j = 0; j < MAP_WIDTH; j++) 
				tiles[i,j] = new Tile(TileType.EMPTY);
		
		// Reset QuadTree
		quadTree = new QuadTree(new AABB(new XY(MAP_WIDTH/2.0f,MAP_HEIGHT/2.0f),new XY(MAP_WIDTH/2.0f, MAP_HEIGHT/2.0f)));
		
		// Reset rooms
		rooms.Clear();
		
		// Destroy tile GameObjects
		foreach (Transform t in containerRooms.transform) GameObject.Destroy(t.gameObject);

		// Destroy Player
		//GameObject.Destroy(Player);
	}
	
	// Generate a new dungeon with the given seed
	public void GenerateDungeon(int seed) {
		Debug.Log ("Generating QuadTree");
			
		// Clean
		ResetDungeon ();
		
		// Place a temporary floor to see progress
//		floor = GameObject.Instantiate(prefabFloor01,new Vector3(MAP_WIDTH/2,-0.5f,MAP_HEIGHT/2), Quaternion.identity) as GameObject;
//		floor.transform.localScale = new Vector3(MAP_WIDTH,1,MAP_HEIGHT);
		
		// Generate QuadTree
		GenerateQuadTree (ref quadTree);
		
		// Export texture
		Texture2D quadTreeTexture = quadTree.QuadTreeToTexture();
//		floor.renderer.material.mainTexture = quadTree.QuadTreeToTexture();
		TextureToFile(quadTreeTexture,seed + "_quadTree");

		Debug.Log ("Generating Rooms");
		
		// Generate Rooms
		GenerateRooms (ref rooms, quadTree);
		
		// Export texture
		dungeonTexture = DungeonToTexture();
//		floor.renderer.material.mainTexture = dungeonTexture;
		TextureToFile(dungeonTexture,seed + "_rooms");
		
		Debug.Log ("Generating Corridors");
		
		// Generate Corridors
		GenerateCorridors ();
		
		// Export texture
		dungeonTexture = DungeonToTexture();
//		floor.renderer.material.mainTexture = dungeonTexture;
		TextureToFile(dungeonTexture,seed + "_corridors");
		
		
		Debug.Log ("Generating Walls");
		
		GenerateWalls();

		Debug.Log ("Generating Doors");
		
		GenerateDoors();
		
		// Export texture
		dungeonTexture = DungeonToTexture();
//		floor.renderer.material.mainTexture = dungeonTexture;
		TextureToFile(dungeonTexture,seed + "_walls");
		
		Debug.Log ("Generating GameObjects, this may take a while..");
		
		// Instantiate prefabs
		GenerateGameObjects(quadTree);
			
		// Create Player

		// Place Player
		/*int r = Random.Range(0,rooms.Count-1);
		Room room = rooms[r];
		player.SetActive(true);
		player.transform.position = new Vector3(room.boundary.center.x,1.0f,room.boundary.center.y);*/
		
//		GameObject.DestroyImmediate(floor);
		
	}


	// *************************************************************
	// Generate Dungeon Features
	// *************************************************************
	
	// Generate the quadtree system
	void GenerateQuadTree(ref QuadTree _quadTree) {
		_quadTree.GenerateQuadTree(seed);
	}
	
	// Generate the list of rooms and dig them
	public void GenerateRooms(ref List<Room> _rooms, QuadTree _quadTree) {
		// Childless node
		if (_quadTree.northWest == null && _quadTree.northEast == null && _quadTree.southWest == null && _quadTree.southEast == null) {
			_rooms.Add(GenerateRoom(_quadTree));
			return;
		}
		
		// Recursive call
		if (_quadTree.northWest != null) GenerateRooms (ref _rooms,_quadTree.northWest);
		if (_quadTree.northEast != null) GenerateRooms (ref _rooms,_quadTree.northEast);
		if (_quadTree.southWest != null) GenerateRooms (ref _rooms,_quadTree.southWest);
		if (_quadTree.southEast != null) GenerateRooms (ref _rooms,_quadTree.southEast);
	}
	
	// Generate a single room
	public Room GenerateRoom(QuadTree _quadTree) {
		// Center of the room
		XY roomCenter = new XY();
		roomCenter.x = Random.Range(ROOM_WALL_BORDER + _quadTree.boundary.Left() + ROOM_MIN_SIZE/2.0f, _quadTree.boundary.Right() - ROOM_MIN_SIZE/2.0f - ROOM_WALL_BORDER);
		roomCenter.y = Random.Range(ROOM_WALL_BORDER + _quadTree.boundary.Bottom() + ROOM_MIN_SIZE/2.0f, _quadTree.boundary.Top() - ROOM_MIN_SIZE/2.0f - ROOM_WALL_BORDER);		
		
		// Half size of the room
		XY roomHalf = new XY();
		
		float halfX = (_quadTree.boundary.Right() - roomCenter.x - ROOM_WALL_BORDER);
		float halfX2 =(roomCenter.x - _quadTree.boundary.Left() - ROOM_WALL_BORDER);
		if (halfX2 < halfX) halfX = halfX2;
		if (halfX > ROOM_MAX_SIZE/2.0f) halfX = ROOM_MAX_SIZE/2.0f;
		
		float halfY = (_quadTree.boundary.Top() - roomCenter.y - ROOM_WALL_BORDER);
		float halfY2 =(roomCenter.y - _quadTree.boundary.Bottom() - ROOM_WALL_BORDER);
		if (halfY2 < halfY) halfY = halfY2;
		if (halfY > ROOM_MAX_SIZE/2.0f) halfY = ROOM_MAX_SIZE/2.0f;
		
		roomHalf.x = Random.Range((float)ROOM_MIN_SIZE/2.0f,halfX);
		roomHalf.y = Random.Range((float)ROOM_MIN_SIZE/2.0f,halfY);

		// Eliminate ugly zones
		if (ROOM_UGLY_ENABLED == false) {
			float aspect_ratio = roomHalf.x / roomHalf.y;
			if (aspect_ratio > ROOM_MAX_RATIO || aspect_ratio < 1.0f/ROOM_MAX_RATIO) return GenerateRoom(_quadTree); 
		}
		
		// Create AABB
		AABB randomAABB = new AABB(roomCenter, roomHalf);
		
		// Dig the room in our tilemap
		DigRoom (randomAABB.BottomTile(), randomAABB.LeftTile(), randomAABB.TopTile()-1, randomAABB.RightTile()-1);
		
		// Return the room
		return new Room(randomAABB,_quadTree);
	}
	
	void GenerateCorridors() {
		quadTree.GenerateCorridors();
	}

	// Generate walls when there's something near
	public void GenerateWalls() {
		// Place walls
		for (int y = 0; y < MAP_HEIGHT; y++) {
			for (int x = 0; x < MAP_WIDTH; x++) {
				bool room_near = false;
				if (IsPassable(x,y)) continue;
				if (x > 0) if (IsPassable(x - 1, y)) room_near = true;
				if (x < MAP_HEIGHT - 1) if (IsPassable(x + 1, y)) room_near = true;
				if (y > 0) if (IsPassable(x, y - 1)) room_near = true;
				if (y < MAP_WIDTH - 1) if (IsPassable(x, y + 1)) room_near = true;
				if (room_near) SetWall(x, y);
			}
		}

		// place wall corners
		for (int y = 0; y < MAP_HEIGHT; y++) {
			for (int x = 0; x < MAP_WIDTH; x++) {
				if (IsWallCorner(x, y)) SetWallCorner(x, y);
			}
		}
	}


	public void GenerateDoors() {
		// generate doors
		for (int y = 1; y < MAP_HEIGHT - 1; y++) {
			for (int x = 1; x < MAP_WIDTH - 1; x++) {
				Tile tile = tiles[x, y];
				if (tile.id != TileType.CORRIDOR) continue;

				if (tiles[x, y - 1].id == TileType.WALL || tiles[x, y + 1].id == TileType.WALL) {
					if (tiles[x + 1, y].id == TileType.ROOM && tiles[x - 1, y].id == TileType.CORRIDOR) {
						SetDoor(x, y);
					}

					if (tiles[x - 1, y].id == TileType.ROOM && tiles[x + 1, y].id == TileType.CORRIDOR) {
						SetDoor(x, y);
					}
				}

				if (tiles[x - 1, y].id == TileType.WALL || tiles[x + 1, y].id == TileType.WALL) {
					if (tiles[x, y + 1].id == TileType.ROOM && tiles[x, y - 1].id == TileType.CORRIDOR) {
						SetDoor(x, y);
					}

					if (tiles[x, y - 1].id == TileType.ROOM && tiles[x, y + 1].id == TileType.CORRIDOR) {
						SetDoor(x, y);
					}
				}
			}
		}

		// remove bad doors
		for (int y = 1; y < MAP_HEIGHT - 1; y++) {
			for (int x = 1; x < MAP_WIDTH - 1; x++) {
				Tile tile = tiles[x, y];
				if (tile.id != TileType.DOOR) continue;

				if ((tiles[x, y - 1].id == TileType.ROOM && tiles[x, y + 1].id == TileType.CORRIDOR) ||
					(tiles[x, y + 1].id == TileType.ROOM && tiles[x, y - 1].id == TileType.CORRIDOR)) {

					if (tiles[x - 1, y].id == TileType.CORRIDOR || tiles[x + 1, y].id == TileType.CORRIDOR) {
						tile.id = TileType.CORRIDOR;
					}
				}

				if ((tiles[x - 1, y].id == TileType.ROOM && tiles[x + 1, y].id == TileType.CORRIDOR) ||
					(tiles[x + 1, y].id == TileType.ROOM && tiles[x - 1, y].id == TileType.CORRIDOR)) {

					if (tiles[x, y - 1].id == TileType.CORRIDOR || tiles[x, y + 1].id == TileType.CORRIDOR) {
						tile.id = TileType.CORRIDOR;
					}
				}
			}
		}
	}


	// *************************************************************
	// Read tilemap and instantiate GameObjects
	// *************************************************************
	
	void GenerateGameObjects(QuadTree _quadtree) {
		// If it's an end quadtree, read every pos and make a chunk of combined meshes
		if (_quadtree.HasChildren() == false) {
			GameObject container = GameObject.Instantiate(meshCombiner) as GameObject;
			for (int row = _quadtree.boundary.BottomTile(); row <= _quadtree.boundary.TopTile()-1; row++) {
				for (int col = _quadtree.boundary.LeftTile(); col <= _quadtree.boundary.RightTile()-1; col++) {
					TileType id = tiles[row,col].id;
					// floors
					if (id == TileType.ROOM || id == TileType.CORRIDOR || id == TileType.DOOR) {
						GameObject floor = createFloor(container, row, col);
						tiles[row,col].obj = floor; // record gameobject in tile
					}
					// walls
					else if (id == TileType.WALL || id == TileType.WALLCORNER) {
						GameObject wall = createWall(container, row, col);
						tiles[row,col].obj = wall; // record gameobject in tile
					}
					// doors
					if (id == TileType.DOOR) {
						GameObject door = createDoor(container, row, col);
						tiles[row,col].obj = door; // record gameobject in tile
					}
				}
			}
			container.transform.parent = containerRooms.transform;
		} else {
			GenerateGameObjects(_quadtree.northWest);
			GenerateGameObjects(_quadtree.northEast);
			GenerateGameObjects(_quadtree.southWest);
			GenerateGameObjects(_quadtree.southEast);
		}
	}


	private GameObject createFloor (GameObject container, int row, int col) {
		GameObject floor = GameObject.Instantiate(prefabFloor,new Vector3(col, 0.0f, row),Quaternion.identity) as GameObject;
		floor.transform.parent = container.transform;
		floor.transform.localScale = new Vector3(1, Random.Range(0.1f, 0.3f), 1);

		float h = 0.01f;
		floor.transform.localScale = new Vector3(1, h, 1);
		floor.transform.localPosition = new Vector3(col, 0, row); // h / 2


		// colored rooms and corridors (note that this will generate too many draw calls)
		GameObject cube = floor.transform.Find("Cube").gameObject;
		
		Tile tile = tiles[row, col];
		if (tile.id == TileType.ROOM) {
			cube.renderer.material.color = Color.red;
		}
		if (tile.id == TileType.CORRIDOR) {
			cube.renderer.material.color = Color.cyan;
		}

		return floor;
	}


	private GameObject createWall (GameObject container, int row, int col) {
		GameObject wall = GameObject.Instantiate(prefabWall,new Vector3(col, 0.0f, row),Quaternion.identity) as GameObject;
		wall.transform.parent = container.transform;

		float h = 1.0f;
		wall.transform.localScale = new Vector3(1, h, 1);
		wall.transform.localPosition = new Vector3(wall.transform.position.x, 0, wall.transform.position.z); // h / 2

		return wall;
	}


	private GameObject createDoor (GameObject container, float row, float col) {
		GameObject door = GameObject.Instantiate(prefabDoor,new Vector3(col, 0.0f, row),Quaternion.identity) as GameObject;
		door.transform.parent = container.transform;

		float h = 1f;
		door.transform.localScale = new Vector3(0.9f, h, 0.9f);
		door.transform.localPosition = new Vector3(door.transform.position.x, 0, door.transform.position.z); // h / 2

		return door;
	}


	// *************************************************************
	// Walkability grid
	// *************************************************************

	public void logGrid () {
		print("Grid " + MAP_WIDTH + ", " + MAP_HEIGHT);

		string str = "";
		for (int y = 0; y < MAP_HEIGHT; y++) {
			
			for (int x = 0; x < MAP_WIDTH; x++) {
				Tile tile = tiles[x, y];
				str += tile.getWalkable() ? "1" : "0"; //id;
			}
			str += "\n";
		}

		print (str);
	}

	
	// *************************************************************
	// Helper Methods
	// *************************************************************

	public bool IsEmpty(int row, int col) { 
		return tiles[row,col].id == TileType.EMPTY; 
	}
	

	public bool IsPassable(int row, int col) { 
		return 
			tiles[row,col].id == TileType.ROOM || 
			tiles[row,col].id == TileType.CORRIDOR ||
			tiles[row,col].id == TileType.DOOR; 
	}

	
	public bool IsPassable(XY xy) { 
		return IsPassable((int) xy.y, (int) xy.x);
	}


	public bool IsWallCorner(int row, int col) { 
		if (tiles[row, col].id != TileType.EMPTY) return false;
		if (row > 0  && col > 0 && tiles[row - 1, col].id == TileType.WALL && tiles[row, col - 1].id == TileType.WALL && tiles[row - 1, col - 1].id != TileType.WALL) return true;
		if (row > 0  && col < MAP_HEIGHT - 1 && tiles[row - 1, col].id == TileType.WALL && tiles[row, col + 1].id == TileType.WALL && tiles[row - 1, col + 1].id != TileType.WALL) return true;
		if (row < MAP_HEIGHT - 1  && col > 0 && tiles[row + 1, col].id == TileType.WALL && tiles[row, col - 1].id == TileType.WALL && tiles[row + 1, col - 1].id != TileType.WALL) return true;
		if (row < MAP_HEIGHT - 1  && col < MAP_HEIGHT - 1 && tiles[row + 1, col].id == TileType.WALL && tiles[row, col + 1].id == TileType.WALL && tiles[row + 1, col + 1].id != TileType.WALL) return true;
		return false;
	}
	
	public void SetWall(int row, int col) {
		tiles[row,col].id = TileType.WALL;
	}

	public void SetWallCorner(int row, int col) {
		tiles[row,col].id = TileType.WALLCORNER;
	}

	public void SetDoor(int row, int col) {
		tiles[row,col].id = TileType.DOOR;
	}

	// Dig a room, placing floor tiles
	public void DigRoom(int row_bottom, int col_left, int row_top, int col_right) {
		// Out of range
		if ( row_top < row_bottom ) {
		    int tmp = row_top;
		    row_top = row_bottom;
		    row_bottom = tmp;
		}
		
		// Out of range
		if ( col_right < col_left ) {
		    int tmp = col_right;
		    col_right = col_left;
		    col_left = tmp;
		}
		
		if (row_top > MAP_HEIGHT-1) return;
		if (row_bottom < 0) return;
		if (col_right > MAP_WIDTH-1) return;
		if (col_left < 0) return;
		
		// Dig floor
	    for (int row = row_bottom; row <= row_top; row++) 
	        for (int col = col_left; col <= col_right; col++) 
	            DigRoom (row,col);
	}
	
	public void DigRoom(int row, int col) {
		 tiles[row,col].id = TileType.ROOM;
	}
	
	public void DigCorridor(int row, int col) {
		if (tiles[row,col].id != TileType.ROOM) {
			tiles[row,col].id = TileType.CORRIDOR;
		}
	}
	
	public void DigCorridor(XY p1, XY p2) {
		int row1 = Mathf.RoundToInt(p1.y);
		int row2 = Mathf.RoundToInt(p2.y);
		int col1 = Mathf.RoundToInt(p1.x);
		int col2 = Mathf.RoundToInt(p2.x);
		
		DigCorridor(row1,col1,row2,col2);
	}
	
	public void DigCorridor(int row1, int col1, int row2, int col2) {		
		if (row1 <= row2) {
			for (int col = col1; col < col1 + CORRIDOR_WIDTH; col++)
				for (int row = row1; row <= row2; row++)
					DigCorridor(row,col);
		} else {
			for (int col = col1; col < col1 + CORRIDOR_WIDTH; col++)
				for (int row = row2; row <= row1; row++)
					DigCorridor(row,col);
		}
		
		if (col1 <= col2) {
			for (int row = row2; row < row2 + CORRIDOR_WIDTH; row++)
				for (int col = col1; col <= col2; col++)
					DigCorridor(row,col);
		} else {
			for (int row = row2; row < row2 + CORRIDOR_WIDTH; row++)
				for (int col = col2; col <= col1; col++)
					DigCorridor(row2,col);
		}
	}


	// *************************************************************
	// Paint textures for debug purposes
	// *************************************************************

	Texture2D DungeonToTexture() {
		if (!debugToTexture) return null;

		Texture2D texOutput = new Texture2D((int) (MAP_WIDTH), (int) (MAP_HEIGHT),TextureFormat.ARGB32, false);
		PaintDungeonTexture(ref texOutput);
		texOutput.filterMode = FilterMode.Point;
		texOutput.wrapMode = TextureWrapMode.Clamp;
		texOutput.Apply();
		return texOutput;
	}

	void PaintDungeonTexture(ref Texture2D t) {
		if (!debugToTexture) return;

		for (int i = 0; i < MAP_WIDTH; i++) for (int j = 0; j < MAP_HEIGHT; j++) {
			switch (tiles[j,i].id) {
			case TileType.EMPTY:
				t.SetPixel(i,j,Color.black);
				break;
			case TileType.ROOM:
				t.SetPixel(i,j,Color.white);
				break;
			case TileType.CORRIDOR:
				t.SetPixel(i,j,Color.grey);
				break;
			case TileType.WALL:
				t.SetPixel(i,j,Color.blue);
				break;
			}
		}
	}
	
	// Export a texture to a file
	public void TextureToFile(Texture2D t, string filename) {
		if (!debugToTexture) return;

		byte[] bytes = t.EncodeToPNG();
		FileStream myFile = new FileStream(Application.dataPath + "/Resources/Generated/" + filename + ".png",FileMode.OpenOrCreate,System.IO.FileAccess.ReadWrite);
		myFile.Write(bytes,0,bytes.Length);
		myFile.Close();
	}
	
}
