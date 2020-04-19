using UnityEngine;

public class Landmark : MonoBehaviour {
	public GameObject model;

	public float incidenceAngle = 90.0f;

	public bool activated = false;


	protected virtual void Start() {
		if(!activated) {
			model.SetActive(false);
		}
	}

	protected virtual void Update() {
		
	}


	public virtual void activate() {
		activated = true;

		model.SetActive(true);
	}
}