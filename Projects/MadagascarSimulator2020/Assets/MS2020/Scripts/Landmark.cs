using UnityEngine;

public class Landmark : MonoBehaviour {
	public GameObject model;
	public Icon icon;

	public float incidenceAngle = 90.0f;

	public bool activated = false;

	public float minimumInfectionRange = 1.0f;
	public float maximumInfectionRange = 1.0f;


	protected virtual void Start() {
		if(!activated) {
			displayModel(false);
		}
	}

	protected virtual void Update() {
		
	}
	
	protected void displayModel(bool display) {
		model.SetActive(display);
	}

	protected void displayIcon(bool display) {
		icon.visible = display;
	}


	public virtual void activate() {
		activated = true;

		displayModel(true);
	}


	public float getInfectionRate(float range) {
		return 1.0f - Mathf.Clamp01((range - minimumInfectionRange) / (maximumInfectionRange - minimumInfectionRange));
	}
}