using UnityEngine;
using System.Collections;


// Tile Types
public enum TileType {
	EMPTY = 0,
	ROOM = 1,
	WALL = 2,
	CORRIDOR = 3,
	WALLCORNER = 4,
	DOOR = 5
}

//[System.Serializable]

public class Tile {

	// to avoid unity 4.5 "exceded depth" warnings
	[System.NonSerialized]

	

	/*public const int TILE_EMPTY = 0;
	public const int TILE_ROOM = 1;
	public const int TILE_WALL = 2;
	public const int TILE_CORRIDOR = 3;
	public const int TILE_WALLCORNER = 4;
	public const int TILE_DOOR = 5;*/

	public GameObject obj;

	private bool walkable = true;
	
	// Tile ID
	public TileType id;
	
	public Tile ( TileType _id ) {
		this.id = _id;
	}


	public bool getWalkable () {
		return true;
	}


	public void setWalkable (bool walkable) {
		this.walkable = walkable;
	}
}
