using System.Collections;
using UnityEngine;

public class Vehicle : MonoBehaviour {
	public float weight = 1.0f;
	public float speed = 1.0f;

	public bool waitForTarget = false;
	public float minimumWaitRange = 1.0f;
	public float maximumWaitRange = 1.0f;
	public float minimumWaitTime = 1.0f;
	public float maximumWaitTime = 1.0f;

	public int revenueMean = 0;
	public int revenueDeviation = 1;

	public bool bad = false;
	public float chanceOfInfection = 0.0f;

	
	protected internal Transfer target;
	protected internal bool incoming;
	protected internal Vector3 direction;
	protected internal float waitRange;
	protected internal float waitTime;


	private Coroutine waitCoroutine;


	protected virtual void Start() {
		waitRange = Random.Range(minimumWaitRange, maximumWaitRange);
		waitTime = Random.Range(minimumWaitTime, maximumWaitTime);
	}

	protected virtual void Update() {
		
	}


	public void startWaiting() {
		if(waitCoroutine == null) {
			waitCoroutine = StartCoroutine(wait(waitTime));
		}
	}
	
	public void stopWaiting() {
		if(waitCoroutine != null) {
			StopCoroutine(waitCoroutine);

			waitCoroutine = null;
		}
	}

	private IEnumerator wait(float time) {
		yield return new WaitForSeconds(time);

		incoming = false;

		direction = -direction;
	}
}