using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour {
	public Image warning;

	public TextMeshProUGUI date;
	public TextMeshProUGUI extra;
	public TextMeshProUGUI score;

	public Camera viewer;


	private void Start() {
		
	}

	private void Update() {
		
	}


	public void setInfected(bool infected) {
		warning.gameObject.SetActive(infected);
	}


	public void setMonth(int month) {
		string[] names = new string[]{
			"January",
			"February",
			"March",
			"April",
			"May",
			"June",
			"July",
			"August",
			"September",
			"October",
			"November",
			"December"
		};

		date.text = names[month];
	}

	public void overrideMonth(string message) {
		date.text = message;
	}

	public void setBonus(float bonus) {
		// Just for the fun of it.
		if(bonus > 1.1f) {
			bonus = 10.0f;
		}

		extra.text = string.Format(CultureInfo.InvariantCulture, "+{0:#0%}", bonus);
	}

	public void setGDP(long gdp) {
		score.text = string.Format(CultureInfo.InvariantCulture, "${0:#,0}", gdp);
	}


	public Vector3 getPointOnPlane(Vector3 point) {
		Ray ray = viewer.ScreenPointToRay(point);
		Plane plane = new Plane(Vector3.up, 0.0f);

		float distance;

		if(plane.Raycast(ray, out distance)) {
			return ray.GetPoint(distance);
		}
		else {
			return Vector3.zero;
		}
	}
}