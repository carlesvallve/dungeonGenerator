using UnityEngine;
using System.Collections;

//[System.Serializable]

public class Room {

	// to avoid unity 4.5 "exceded depth" warnings
	[System.NonSerialized] 

	public AABB boundary;
	public QuadTree quadtree;
	

	public Room (AABB b) {
		boundary = b;
	}

	
	public Room (AABB b, QuadTree q) {
		boundary = b;
		quadtree = q;
		quadtree.room = this;
	}
	
}
