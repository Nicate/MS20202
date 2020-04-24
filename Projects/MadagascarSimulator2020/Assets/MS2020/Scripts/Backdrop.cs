using UnityEngine;

public class Backdrop : MonoBehaviour {
	public RectTransform userInterfaceElement;

	public UserInterface userInterface;

	public Transform basis;


	private void Start() {
		updatePosition();
	}

	private void LateUpdate() {
		updatePosition();
	}


	private void updatePosition() {
		transform.position = userInterface.getPointOnPlane(userInterfaceElement.position, basis.position.y);
	}
}