using UnityEngine;
using System.Collections;

//[System.Serializable]

public class Tile {

	// to avoid unity 4.5 "exceded depth" warnings
	[System.NonSerialized]

	// Tile Types
	public const int TILE_EMPTY = 0;
	public const int TILE_ROOM = 1;
	public const int TILE_WALL = 2;
	public const int TILE_CORRIDOR = 3;

	public const int TILE_WALLCORNER = 4;

	public const int TILE_DOOR = 5;

	public GameObject obj;
	
	// Tile ID
	public int id;
	
	public Tile ( int _id ) {
		this.id = _id;
	}
}
