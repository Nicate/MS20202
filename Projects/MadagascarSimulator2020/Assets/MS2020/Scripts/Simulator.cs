using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulator : MonoBehaviour {
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

	public float startRange = 1.0f;

	private Capital[] capitals;
	private City[] cities;
	private Port[] ports;
	private Airport[] airports;

	private List<Capital> activeCapitals;
	private List<City> activeCities;
	private List<Port> activePorts;
	private List<Airport> activeAirports;

	private List<Vehicle> boats;
	private List<Vehicle> planes;


	private void Awake() {
		capitals = capitalsRoot.GetComponentsInChildren<Capital>();
		cities = citiesRoot.GetComponentsInChildren<City>();
		ports = portsRoot.GetComponentsInChildren<Port>();
		airports = airportsRoot.GetComponentsInChildren<Airport>();

		activeCapitals = new List<Capital>();
		activeCities = new List<City>();
		activePorts = new List<Port>();
		activeAirports = new List<Airport>();

		boats = new List<Vehicle>();
		planes = new List<Vehicle>();
	}

	private void Start() {
		listActiveLandmarks(capitals, activeCapitals);
		listActiveLandmarks(cities, activeCities);
		listActiveLandmarks(ports, activePorts);
		listActiveLandmarks(airports, activeAirports);

		StartCoroutine(makeVehicles());
	}

	private void Update() {
		simulateVehicles(boats);
		simulateVehicles(planes);
	}


	private void listActiveLandmarks<T>(T[] landmarks, List<T> activeLandmarks) where T : Landmark {
		foreach(T landmark in landmarks) {
			if(landmark.activated) {
				activeLandmarks.Add(landmark);
			}
		}
	}

	private T pickRandomActiveLandmark<T>(List<T> activeLandmarks) where T : Landmark {
		return activeLandmarks[Random.Range(0, activeLandmarks.Count)];
	}


	private IEnumerator makeVehicles() {
		while(true) {
			if(activePorts.Count > 0) {
				boats.Add(createVehicle(cruiseShipPrefab, boatsRoot, pickRandomActiveLandmark(activePorts)));
				boats.Add(createVehicle(merchantVesselPrefab, boatsRoot, pickRandomActiveLandmark(activePorts)));
			}

			if(activeAirports.Count > 0) {
				planes.Add(createVehicle(jumboJetPrefab, planesRoot, pickRandomActiveLandmark(activeAirports)));
				planes.Add(createVehicle(cargoPlanePrefab, planesRoot, pickRandomActiveLandmark(activeAirports)));
			}

			yield return new WaitForSeconds(0.5f);
		}
	}

	private Vehicle createVehicle(Vehicle prefab, Transform parent, Landmark target) {
		Vector2 radial = project(target.model.transform.position) - project(parent.position);
		float distance = radial.magnitude;

		float angle = 0.5f * target.incidenceAngle * Mathf.Deg2Rad;
		float arcExtent = Mathf.Atan2(Mathf.Tan(angle) * (startRange - distance), startRange);
		float halfAngle = Mathf.Atan2(radial.y, radial.x);
		float startAngle = Random.Range(halfAngle - arcExtent, halfAngle + arcExtent);

		Vector2 position = startRange * new Vector3(Mathf.Cos(startAngle), Mathf.Sin(startAngle));
		Vector2 direction = Vector3.Normalize(radial - position);

		Vehicle vehicle = Instantiate(prefab, parent);
		
		vehicle.target = target;
		vehicle.incoming = true;
		vehicle.direction = reproject(direction);

		vehicle.transform.localPosition = reproject(position);
		vehicle.transform.localRotation = Quaternion.LookRotation(vehicle.direction, Vector3.up);

		return vehicle;
	}

	private void simulateVehicles<T>(List<T> vehicles) where T : Vehicle {
		// Iterate a copy since we may remove some.
		foreach(T vehicle in vehicles.ToArray()) {
			Vector3 position = vehicle.transform.localPosition;

			position += vehicle.direction * vehicle.speed * Time.deltaTime;

			// Bounce if we traveled beyond the target.
			if(vehicle.incoming) {
				Vector2 stride = project(vehicle.target.model.transform.position) - project(vehicle.transform.parent.position) - project(position);

				if(Vector3.Dot(Vector3.Normalize(stride), vehicle.direction) < 0.0f) {
					vehicle.incoming = false;
					vehicle.direction = -vehicle.direction;

					position += 2.0f * reproject(stride);
				}
			}

			vehicle.transform.localPosition = position;
			vehicle.transform.localRotation = Quaternion.LookRotation(vehicle.direction, Vector3.up);
			
			if(position.magnitude > startRange) {
				vehicles.Remove(vehicle);

				Destroy(vehicle.gameObject);
			}
		}
	}


	private Vector2 project(Vector3 vector) {
		return new Vector2(vector.x, vector.z);
	}
	
	private Vector3 reproject(Vector2 vector) {
		return new Vector3(vector.x, 0.0f, vector.y);
	}
}