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


public class TileNeighbours {
	public Tile n = null;
	public Tile s = null;
	public Tile w = null;
	public Tile e = null;
}


//[System.Serializable]

public class Tile {

	public TileType id;
	public GameObject obj;

	public Room room;

	public Color color = Color.white;


	public Tile ( TileType id ) {
		this.id = id;
	}


	public bool getWalkable () {
		switch (id) {
		case TileType.ROOM:
			return true;
		case TileType.CORRIDOR:
			return true;
		case TileType.DOOR:
			return true;
		default:
			return false;
		}
	}


	public bool isWall () {
		return id == TileType.WALL || id == TileType.WALLCORNER || id == TileType.DOOR;
	}

	public bool isEmpty () {
		return id == TileType.EMPTY;
	}
}
