using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


//Source: https://dev.to/matthewodle/simple-ui-element-dragging-script-in-unity-c-450p
public class UIElementDragger : EventTrigger {

	private bool dragging;
	[SerializeField]
	public bool enableDragging = true;

	public void Update() {
		if (enableDragging && dragging) {
			transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
	}

	public override void OnPointerDown(PointerEventData eventData) {
		if (enableDragging)
			dragging = true;
	}

	public override void OnPointerUp(PointerEventData eventData) {
		if (enableDragging)
			dragging = false;
	}

}
