using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Simulator : MonoBehaviour {
	public UserInterface userInterface;

	public Transform country;

	public Transform capitalsRoot;
	public Transform citiesRoot;
	public Transform portsRoot;
	public Transform airportsRoot;

	public Transform boatsRoot;
	public Transform planesRoot;

	public CruiseShip cruiseShipPrefab;
	public MerchantVessel merchantVesselPrefab;
	public JumboJet jumboJetPrefab;
	public CargoPlane cargoPlanePrefab;

	public ParticleSystem spawnEffectPrefab;
	
	public int startCapitals = 1;
	public int startCities = 1;
	public int startPorts = 1;
	public int startAirports = 1;
	
	public float monthDuration = 1.0f;

	public int capitalThreshold = 1;
	public int cityThreshold = 1;
	public int portThreshold = 1;
	public int airportThreshold = 1;
	
	public float thresholdProgression = 0.0f;

	public float startRange = 1.0f;
	
	public float selectionRange = 1.0f;

	public string wonMessage = "";
	public string lostMessage = "";

	private Capital[] capitals;
	private City[] cities;
	private Port[] ports;
	private Airport[] airports;

	private List<Capital> inactiveCapitals;
	private List<City> inactiveCities;
	private List<Port> inactivePorts;
	private List<Airport> inactiveAirports;

	private List<Capital> activeCapitals;
	private List<City> activeCities;
	private List<Port> activePorts;
	private List<Airport> activeAirports;

	private List<Vehicle> vehicles;

	private bool infected;
	private int month;
	private long gdp;

	private bool over;
	private bool won;
	private bool lost;


	private void Awake() {
		capitals = capitalsRoot.GetComponentsInChildren<Capital>();
		cities = citiesRoot.GetComponentsInChildren<City>();
		ports = portsRoot.GetComponentsInChildren<Port>();
		airports = airportsRoot.GetComponentsInChildren<Airport>();

		inactiveCapitals = new List<Capital>();
		inactiveCities = new List<City>();
		inactivePorts = new List<Port>();
		inactiveAirports = new List<Airport>();

		activeCapitals = new List<Capital>();
		activeCities = new List<City>();
		activePorts = new List<Port>();
		activeAirports = new List<Airport>();

		vehicles = new List<Vehicle>();
	}

	private void Start() {
		categorizeLandmarks(capitals, inactiveCapitals, activeCapitals);
		categorizeLandmarks(cities, inactiveCities, activeCities);
		categorizeLandmarks(ports, inactivePorts, activePorts);
		categorizeLandmarks(airports, inactiveAirports, activeAirports);

		activateRandomLandmarks(inactiveCapitals, activeCapitals, startCapitals, false);
		activateRandomLandmarks(inactiveCities, activeCities, startCities, false);
		activateRandomLandmarks(inactivePorts, activePorts, startPorts, false);
		activateRandomLandmarks(inactiveAirports, activeAirports, startAirports, false);

		infected = false;
		month = 0;
		gdp = 0L;

		updateInfected();
		updateMonth();
		updateGDP();

		over = false;
		won = false;
		lost = false;

		StartCoroutine(startTiming(monthDuration));
	}

	private void Update() {
		Vector2 cursor = project(userInterface.getPointOnPlane(Input.mousePosition, country.position.y));

		List<Transfer> activeTransfers = new List<Transfer>();
		activeTransfers.AddRange(activePorts);
		activeTransfers.AddRange(activeAirports);

		foreach(Transfer activeTransfer in activeTransfers) {
			activeTransfer.icon.selected = false;
		}

		Transfer transfer = findNearestLandmark(activeTransfers, cursor, selectionRange);

		if(!over) {
			if(transfer != null) {
				transfer.icon.selected = true;

				if(Input.GetMouseButtonDown(0)) {
					transfer.toggleAvailability();
				}
			}

			if(Input.GetKeyDown(KeyCode.Z)) {
				shutDownPorts();
			}

			if(Input.GetKeyDown(KeyCode.X)) {
				shutDownAirports();
			}

			if(Input.GetKeyDown(KeyCode.Space)) {
				shutDownEverything();
			}
		}

		simulateVehicles();
		
		if(!over) {
			activateMissingLandmarks(inactiveCapitals, activeCapitals, startCapitals, capitalThreshold);
			activateMissingLandmarks(inactiveCities, activeCities, startCities, cityThreshold);
			activateMissingLandmarks(inactivePorts, activePorts, startPorts, portThreshold);
			activateMissingLandmarks(inactiveAirports, activeAirports, startAirports, airportThreshold);
		}

		updateGDP();
	}


	private void categorizeLandmarks<T>(T[] landmarks, List<T> inactiveLandmarks, List<T> activeLandmarks) where T : Landmark {
		foreach(T landmark in landmarks) {
			if(landmark.activated) {
				activeLandmarks.Add(landmark);
			}
			else {
				inactiveLandmarks.Add(landmark);
			}
		}
	}

	private void activateRandomLandmarks<T>(List<T> inactiveLandmarks, List<T> activeLandmarks, int amount, bool spawnEffect) where T : Landmark {
		int cap = Mathf.Min(inactiveLandmarks.Count, amount);

		for(int index = 0; index < cap; index += 1) {
			T landmark = pickRandomLandmark(inactiveLandmarks);

			if(landmark != null) {
				landmark.activate();

				inactiveLandmarks.Remove(landmark);
				activeLandmarks.Add(landmark);

				// Here comes the poorly planned part.
				if(landmark is Port) {
					StartCoroutine(startCreatingVehicles(new Vehicle[]{ cruiseShipPrefab, merchantVesselPrefab }, boatsRoot, landmark as Transfer, activePorts.Count));
				}
				else if(landmark is Airport) {
					StartCoroutine(startCreatingVehicles(new Vehicle[]{ jumboJetPrefab, cargoPlanePrefab }, planesRoot, landmark as Transfer, activeAirports.Count));
				}

				if(spawnEffect) {
					Instantiate(spawnEffectPrefab, landmark.model.transform.position, Quaternion.identity);
				}
			}
		}
	}

	private void activateMissingLandmarks<T>(List<T> inactiveLandmarks, List<T> activeLandmarks, int start, int threshold) where T : Landmark {
		// Provide an early bonus that falls off following a geometric progression.
		// We have to calculate this one by one if we want to avoid using the Lambert W-function :P
		// But avoid looping infinitely due to there being no more landmarks left to activate!
		int current = activeLandmarks.Count - start;

		float reference = current + 2.0f - (1.0f - Mathf.Pow(thresholdProgression, current + 2.0f)) / (1.0f - thresholdProgression);
		
		while(inactiveLandmarks.Count > 0 && gdp > reference * threshold) {
			activateRandomLandmarks(inactiveLandmarks, activeLandmarks, 1, true);

			current = activeLandmarks.Count - start;

			reference = current + 2.0f - (1.0f - Mathf.Pow(thresholdProgression, current + 2.0f)) / (1.0f - thresholdProgression);
		}
	}

	private T findNearestLandmark<T>(List<T> landmarks, Vector2 point, float range) where T : Landmark {
		float minimumDistance = float.MaxValue;
		T nearestLandmark = null;

		foreach(T landmark in landmarks) {
			float currentDistance = Vector2.Distance(project(landmark.model.transform.position), point);

			if(currentDistance < minimumDistance) {
				minimumDistance = currentDistance;
				nearestLandmark = landmark;
			}
		}

		if(minimumDistance > range) {
			return null;
		}
		else {
			return nearestLandmark;
		}
	}

	private T pickRandomLandmark<T>(List<T> landmarks) where T : Landmark {
		if(landmarks.Count > 0) {
			return landmarks[Random.Range(0, landmarks.Count)];
		}
		else {
			return null;
		}
	}


	private void exposeActiveCities(Vehicle vehicle) {
		Vector2 sourcePosition = project(vehicle.target.model.transform.position);
								
		foreach(City city in activeCities) {
			float distance = Vector2.Distance(sourcePosition, project(city.model.transform.position));
			float chance = vehicle.chanceOfInfection * vehicle.target.getInfectionRate(distance);

			if(!city.infected && Random.value < chance) {
				city.infect();

				infected = true;

				updateInfected();

				StartCoroutine(startInfectingCitiesAndCapitals(city));

				// Only infect one city at a time. This is visually less jarring.
				return;
			}
		}
	}
	
	private void exposeActiveCitiesAndCapitals(City city) {
		Vector2 sourcePosition = project(city.model.transform.position);
		
		foreach(Capital capital in activeCapitals) {
			float distance = Vector2.Distance(sourcePosition, project(capital.model.transform.position));
			float chance = city.chanceOfInfection * city.getInfectionRate(distance);

			if(!capital.infected && Random.value < chance) {
				capital.infect();

				infected = true;

				updateInfected();

				// Game over, man! GAME OVER!
				gameOver(false);
				
				// Since we only allow one infection at a time, prioritize capitals over cities.
				// Otherwise cities might protect you just because there are so many of them.
				return;
			}
		}
								
		foreach(City activeCity in activeCities) {
			float distance = Vector2.Distance(sourcePosition, project(activeCity.model.transform.position));
			float chance = city.chanceOfInfection * city.getInfectionRate(distance);

			if(!activeCity.infected && Random.value < chance) {
				activeCity.infect();

				infected = true;

				updateInfected();

				StartCoroutine(startInfectingCitiesAndCapitals(activeCity));

				// Only infect one city at a time. This is visually less jarring.
				return;
			}
		}
	}


	public IEnumerator startInfectingCitiesAndCapitals(City city) {
		float firstInterval = pickRandomInterval(city.infectionInterval);

		yield return new WaitForSeconds(firstInterval);

		while(!over) {
			exposeActiveCitiesAndCapitals(city);

			float interval = pickRandomInterval(city.infectionInterval);

			yield return new WaitForSeconds(interval);
		}
	}


	private IEnumerator startCreatingVehicles(Vehicle[] prefabs, Transform parent, Transfer transfer, int numberOfTransfers) {
		while(!lost) {
			Vehicle prefab = pickRandomVehiclePrefab(prefabs);

			if(prefab != null) {
				createVehicle(prefab, parent, transfer);
			}

			float interval = pickRandomInterval(transfer.interval);

			yield return new WaitForSeconds(interval);
		}
	}

	private Vehicle createVehicle(Vehicle prefab, Transform parent, Transfer target) {
		Vector2 radial = project(target.model.transform.position) - project(parent.position);
		float distance = radial.magnitude;

		float angle = 0.5f * target.incidenceAngle * Mathf.Deg2Rad;
		float arcExtent = Mathf.Atan2(Mathf.Tan(angle) * (startRange - distance), startRange);
		float halfAngle = Mathf.Atan2(radial.y, radial.x);
		float startAngle = Random.Range(halfAngle - arcExtent, halfAngle + arcExtent);

		Vector2 position = startRange * new Vector3(Mathf.Cos(startAngle), Mathf.Sin(startAngle));
		Vector2 direction = Vector3.Normalize(radial - position);

		Vehicle vehicle = Instantiate(prefab, parent);

		vehicle.name = prefab.name;
		
		vehicle.target = target;
		vehicle.incoming = true;
		vehicle.direction = reproject(direction);

		vehicle.transform.localPosition = reproject(position);
		vehicle.transform.localRotation = Quaternion.LookRotation(vehicle.direction, Vector3.up);

		vehicles.Add(vehicle);

		return vehicle;
	}

	private void simulateVehicles() {
		// Iterate a copy since we may remove some.
		foreach(Vehicle vehicle in vehicles.ToArray()) {
			Vector3 position = vehicle.transform.localPosition;
			Vector3 direction = vehicle.direction;

			Vector3 forward = vehicle.direction * vehicle.speed * Time.deltaTime;

			position += forward;

			// Bounce if we traveled beyond the target.
			if(vehicle.incoming) {
				if(vehicle.waitForTarget) {
					if(vehicle.target.available) {
						Vector2 stride = project(vehicle.target.model.transform.position) - project(vehicle.transform.parent.position) - project(position);
						
						if(Vector2.Dot(stride.normalized, project(vehicle.direction)) < 0.1f) {
							vehicle.incoming = false;

							vehicle.direction = -vehicle.direction;

							position += 2.0f * reproject(stride);
							direction = vehicle.direction;

							if(!over) {
								increaseGDP(vehicle.revenueMean, vehicle.revenueDeviation);

								if(vehicle.bad) {
									exposeActiveCities(vehicle);
								}
							}
						}

						vehicle.stopWaiting();
					}
					else {
						Vector3 waitPosition = vehicle.waitRange * -vehicle.direction;

						Vector2 stride = project(vehicle.target.model.transform.position) - project(vehicle.transform.parent.position) + project(waitPosition) - project(position);
						
						// Include 0 in the dot check, since the (safe) normalized version of the stride may be the zero vector.
						if(Vector2.Dot(stride.normalized, project(vehicle.direction)) < 0.1f) {
							// Don't place it back where it was if that could make it move again.
							if(stride.magnitude < forward.magnitude) {
								position += reproject(stride);
							}
							else {
								position -= forward;
							}

							// Playfully look to the side.
							direction = Quaternion.FromToRotation(Vector3.forward, Vector3.left) * vehicle.direction;

							vehicle.startWaiting();
						}
					}
				}
				else {
					Vector2 stride = project(vehicle.target.model.transform.position) - project(vehicle.transform.parent.position) - project(position);

					if(Vector2.Dot(stride.normalized, project(vehicle.direction)) < 0.1f) {
						vehicle.incoming = false;

						if(vehicle.target.available) {
							vehicle.direction = -vehicle.direction;

							position += 2.0f * reproject(stride);
							direction = vehicle.direction;
							
							if(!over) {
								increaseGDP(vehicle.revenueMean, vehicle.revenueDeviation);

								if(vehicle.bad) {
									exposeActiveCities(vehicle);
								}
							}
						}
					}
				}
			}

			vehicle.transform.localPosition = position;
			vehicle.transform.localRotation = Quaternion.LookRotation(direction, Vector3.up);
			
			if(position.magnitude > startRange) {
				vehicles.Remove(vehicle);

				Destroy(vehicle.gameObject);
			}
		}
	}


	private Vehicle pickRandomVehiclePrefab(Vehicle[] prefabs) {
		if(prefabs.Length > 0) {
			float totalWeight = 0.0f;

			foreach(Vehicle weightedPrefab in prefabs) {
				totalWeight += weightedPrefab.weight;
			}

			float cumulativeWeight = 0.0f;

			List<float> weights = new List<float>(prefabs.Length);

			foreach(Vehicle weightedPrefab in prefabs) {
				cumulativeWeight += weightedPrefab.weight / totalWeight;

				weights.Add(cumulativeWeight);
			}

			float value = Random.value;

			Vehicle prefab = prefabs[prefabs.Length - 1];

			for(int index = 0; index < weights.Count; index += 1) {
				float weight = weights[index];

				if(value < weight) {
					prefab = prefabs[index];

					break;
				}
			}

			return prefab;
		}
		else {
			return null;
		}
	}

	private float pickRandomInterval(float mean) {
		// Generate a exponentially distributed interval so we can model a Poisson process, but
		// keep it from being extremely small or large, and avoid taking the logarithm of zero.
		float value;
		float interval;

		do {
			value = Random.value;

			interval = -Mathf.Log(1.0f - value) * mean;
		}
		while(value == 1.0f || interval < 0.25f * mean || interval > 1.75 * mean);

		return interval;
	}


	private IEnumerator startTiming(float duration) {
		yield return new WaitForSeconds(duration);

		while(!over) {
			if(month > 10) {
				gameOver(true);
			}
			else {
				month += 1;

				updateMonth();

				yield return new WaitForSeconds(duration);
			}
		}
	}

	private void updateInfected() {
		userInterface.setInfected(infected);
	}

	private void updateMonth() {
		userInterface.setMonth(month);
	}


	private void increaseGDP(int mean, int deviation) {
		// Never lose GDP.
		gdp += Mathf.Max(0, Mathf.RoundToInt(mean + deviation * GaussianRandom.value));
	}

	private void updateGDP() {
		userInterface.setGDP(gdp);
	}


	private Vector2 project(Vector3 vector) {
		return new Vector2(vector.x, vector.z);
	}
	
	private Vector3 reproject(Vector2 vector) {
		return new Vector3(vector.x, 0.0f, vector.y);
	}


	private void gameOver(bool successful) {
		over = true;
		won = successful;
		lost = !successful;

		if(won) {
			foreach(Port port in activePorts) {
				if(!port.available) {
					port.toggleAvailability();
				}
			}
			
			foreach(Airport airport in activeAirports) {
				if(!airport.available) {
					airport.toggleAvailability();
				}
			}

			userInterface.setMessage(wonMessage);
		}

		if(lost) {
			foreach(Vehicle vehicle in vehicles) {
				if(vehicle.incoming) {
					vehicle.incoming = false;

					if(vehicle.waitForTarget) {
						vehicle.direction = -vehicle.direction;

						vehicle.stopWaiting();
					}
				}
			}

			userInterface.setMessage(lostMessage);
		}
	}


	public void shutDownPorts() {
		if(!over) {
			foreach(Port port in activePorts) {
				port.shutDown();
			}
		}
	}

	public void shutDownAirports() {
		if(!over) {
			foreach(Airport airport in activeAirports) {
				airport.shutDown();
			}
		}
	}

	public void shutDownEverything() {
		shutDownPorts();
		shutDownAirports();
	}


	public void restart() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	
	public void exit() {
		Application.Quit();
	}
}