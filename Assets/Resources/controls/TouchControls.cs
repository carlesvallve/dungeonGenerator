using UnityEngine;
using System;
using System.Collections.Generic;


public class TouchControls : MonoBehaviour {

	private Dictionary<string, TouchLayer> mappedLayers = new Dictionary<string, TouchLayer>();
	private List<TouchLayer> sortedLayers = new List<TouchLayer>();
	private Dictionary<int, TouchData> touches = new Dictionary<int, TouchData>();

	public List<string> layers = new List<string>() { "background" };  // used as configuration


	void Awake() {
		// pre-create layers for all sorting layers (in reverse, layer at the bottom of the list has first dibs on events)
		for (int i = 0; i < layers.Count; i++) {
			this.addLayer(layers[i]);
		}
	}


	public TouchLayer addLayer(string name) {
		if (mappedLayers.ContainsKey(name)) {
			return mappedLayers[name];
		}

		TouchLayer layer = new TouchLayer(name);

		mappedLayers.Add(name, layer);
		sortedLayers.Insert(0, layer);

		return layer;
	}


	public bool hasLayer(string name) {
		return mappedLayers.ContainsKey(name);
	}


	public TouchLayer getLayer(string name) {
		return mappedLayers[name];
	}


	public TouchLayer getLayerAt(int index) {
		return sortedLayers[index];
	}


	private void press(TouchData activeTouch, float x, float y) {
		
		activeTouch.active = true;
		activeTouch.setInitialData(x, y);

		TouchEvent e = new TouchEvent(touches, activeTouch);

		for (int i = 0; i < sortedLayers.Count && !e.isHandled(); i++) {
			sortedLayers[i].press(e);
		}
	}


	private void release(TouchData activeTouch, float x, float y) {
		activeTouch.setData(x, y);
		activeTouch.active = false;

		TouchEvent e = new TouchEvent(touches, activeTouch);
		bool isSwipe = activeTouch.isSwipe();

		for (int i = 0; i < sortedLayers.Count && !e.isHandled(); i++) {
			//print("releasing on layer " + sortedLayers[i].name);
			if (isSwipe) {
				sortedLayers[i].swipe(e);
			} else {
				sortedLayers[i].release(e);
			}
		}
	}


	private void move(TouchData activeTouch, float x, float y) {
		activeTouch.setData(x, y);

		if (activeTouch.relativeDeltaPos.magnitude > 0) {
			TouchEvent e = new TouchEvent(touches, activeTouch);

			for (int i = 0; i < sortedLayers.Count && !e.isHandled(); i++) {
				sortedLayers[i].move(e);
			}
		}
	}


	// *****************************************************
	// Touch Detection
	// *****************************************************

	private TouchData getFinger(int id) {
		if (touches.ContainsKey(id)) {
			return touches[id];
		}

		TouchData touchData = new TouchData(id);

		touches.Add(id, touchData);

		return touchData;
	}


	private void delFinger(int id) {
		touches.Remove(id);
	}


	void LateUpdate () {
		if (touches == null) return;

		TouchData touchData;

		#if UNITY_EDITOR

			touchData = getFinger(0);

			if (Input.GetButtonDown("Fire1")) {
				press(touchData, Input.mousePosition.x, Input.mousePosition.y);
			} else if (Input.GetButtonUp("Fire1")) {
				release(touchData, Input.mousePosition.x, Input.mousePosition.y);
			} else if (touchData.active) {
				move(touchData, Input.mousePosition.x, Input.mousePosition.y);
			}

		#else

			for (int i = 0; i < Input.touchCount; i++) {
				Touch touch = Input.GetTouch(i);
				int fingerId = touch.fingerId;

				// print("finger test: " + fingerId);

				touchData = getFinger(fingerId);

				if (touch.phase == TouchPhase.Began) {
					press(touchData, touch.position.x, touch.position.y);
				} else if (touch.phase == TouchPhase.Ended) {
					release(touchData, touch.position.x, touch.position.y);
				} else if (touch.phase == TouchPhase.Moved) {
					move(touchData, touch.position.x, touch.position.y);
				}
			}

		#endif
	}
}

