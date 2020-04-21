using UnityEngine;

public class Capital : Landmark {
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