using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[System.Serializable]

public class Room {

	// to avoid unity 4.5 "exceded depth" warnings
	[System.NonSerialized] 

	public AABB boundary;
	public QuadTree quadtree;

	public List<Tile> tiles;
	
	public int id;

	public Room (int id, AABB b) {
		boundary = b;

		this.id = id;
		tiles = new List<Tile>();
	}

	
	public Room (int id, AABB b, QuadTree q) {
		boundary = b;
		quadtree = q;
		quadtree.room = this;

		this.id = id;
		tiles = new List<Tile>();
	}
	
}
