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

	public TileType id;
	public GameObject obj;


	public Tile ( TileType id ) {
		this.id = id;
	}


	public bool getWalkable () {
		switch (id) {
		case TileType.ROOM:
			return true;
		case TileType.CORRIDOR:
			return true;
		default:
			return false;
		}
	}
}
