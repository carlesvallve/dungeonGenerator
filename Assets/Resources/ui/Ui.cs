using UnityEngine;
using System.Collections;

public class Ui : MonoBehaviour {

	private World world;


	void Start () {
		world = GameObject.Find("World").GetComponent<World>();
	}
	
	
	public void generateDungeon () {
		world.generateDungeon();
	}
}
