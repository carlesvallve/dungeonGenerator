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
	public Color color = Color.white;

	public Room (int id, AABB b) {
		boundary = b;

		this.id = id;
		this.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		this.tiles = new List<Tile>();
	}

	
	public Room (int id, AABB b, QuadTree q) {
		boundary = b;
		quadtree = q;
		quadtree.room = this;

		this.id = id;
		this.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		this.tiles = new List<Tile>();
	}
	
}
