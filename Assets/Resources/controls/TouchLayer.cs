public class TouchLayer {
	public delegate void TouchAction(TouchEvent touchEvent);
    public event TouchAction onPress;
    public event TouchAction onRelease;
    public event TouchAction onMove;
    public event TouchAction onSwipe;

    public string name;

    private bool isEnabled = true;
    private bool stopPropagation = false;

    public TouchLayer(string name) {
        this.name = name;
    }

    public void press(TouchEvent e) {
    	if (isEnabled) {
            if (onPress != null) onPress(e);
            if (stopPropagation) e.setHandled();
        }
    }


    public void release(TouchEvent e) {
        if (isEnabled) {
            if (onRelease != null) onRelease(e);
            if (stopPropagation) e.setHandled();
        }
    }


    public void move(TouchEvent e) {
        if (isEnabled) {
            if (onMove != null) onMove(e);
            if (stopPropagation) e.setHandled();
        }
    }


    public void swipe(TouchEvent e) {
        if (isEnabled) {
            if (onSwipe != null) onSwipe(e);
            if (stopPropagation) e.setHandled();
        }
    }


    public void allowPropagation() {
        stopPropagation = false;
    }


    public void forbidPropagation() {
        stopPropagation = true;
    }


    public void enable() {
        isEnabled = true;
    }


    public void disable() {
        isEnabled = false;
    }
}
