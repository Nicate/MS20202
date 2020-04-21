using UnityEngine;

public class City : Landmark {
	public float chanceOfInfection = 0.0f;
	public float infectionInterval = 1.0f;


	protected internal bool infected;


	protected override void Start() {
		base.Start();
	}

	protected override void Update() {
		base.Update();
	}
	
	
	public void infect() {
		infected = true;

		displayIcon(true);
	}
}