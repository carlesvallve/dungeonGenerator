using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FramesPerSecond : MonoBehaviour {

	public int FPS = 60;
    public bool showFPS = true;

	private Text label;
    private int m_fps;
    private int tframe = 0;
    

	void Awake () {
		// get text component
		label = GetComponent<Text>();

		// make the game run to specified fps
		Application.targetFrameRate = FPS;
		tframe = 0;
	}
	
	
	void Update () {
		// update is called once per frame
		tframe++;
		if (tframe == FPS) {
			tframe = 0;
			m_fps = (int)(1 / Time.deltaTime);
			label.text = "fps " + m_fps;
		}
	}
}

