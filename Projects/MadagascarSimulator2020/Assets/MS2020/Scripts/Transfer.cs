using UnityEngine;

public class Transfer : Landmark {
	public float interval = 1.0f;

	public bool available = true;


	protected override void Start() {
		base.Start();

		updateAvailability();
	}

	protected override void Update() {
		base.Update();
	}


	public void toggleAvailability() {
		available = !available;

		updateAvailability();
	}

	public void updateAvailability() {
		displayIcon(!available);
	}
}