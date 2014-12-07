using UnityEngine;
using System.Collections.Generic;

public class TouchEvent {

	public Dictionary<int, TouchData> touches;
	public TouchData activeTouch;
	private bool handled;


	public TouchEvent(Dictionary<int, TouchData> touches, TouchData activeTouch) {
		this.touches = touches;
		this.activeTouch = activeTouch;
		this.handled = false;
	}


	public void setHandled() {
		this.handled = true;
	}


	public bool isHandled() {
		return this.handled;
	}
}
