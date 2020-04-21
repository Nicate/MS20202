using UnityEngine;

public class Icon : MonoBehaviour {
	public SpriteRenderer visibleImage;
	public SpriteRenderer selectedImage;

	public float selectionScale = 1.0f;

	
	protected internal bool visible = false;
	protected internal bool selected = false;


	private void Start() {
		updateRotation();
		updateScale();

		updateImage();
	}

	private void LateUpdate() {
		updateRotation();
		updateScale();

		updateImage();
	}


	private void updateRotation() {
		Camera viewer = Camera.main;

		transform.rotation = viewer.transform.rotation;
		transform.rotation *= Quaternion.FromToRotation(Vector3.up, Vector3.back);
	}
	
	private void updateScale() {
		float scale = 1.0f;

		if(selected) {
			scale *= selectionScale;
		}

		transform.localScale = new Vector3(scale, scale, scale);
	}


	private void updateImage() {
		if(visibleImage != null) {
			visibleImage.gameObject.SetActive(visible);
		}

		if(selectedImage != null) {
			selectedImage.gameObject.SetActive(!visible && selected);
		}
	}
}