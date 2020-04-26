using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour {
	public Image warning;

	public TextMeshProUGUI date;
	public TextMeshProUGUI overlay;
	public TextMeshProUGUI score;

	public Camera viewer;


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

	public void setMessage(string message) {
		overlay.text = message;
	}

	public void setGDP(long gdp) {
		score.text = string.Format(CultureInfo.InvariantCulture, "${0:#,0}", gdp);
	}


	public Vector3 getPointOnPlane(Vector3 point, float height) {
		Ray ray = viewer.ScreenPointToRay(point);
		Plane plane = new Plane(Vector3.up, -height);

		float distance;

		if(plane.Raycast(ray, out distance)) {
			return ray.GetPoint(distance);
		}
		else {
			return Vector3.zero;
		}
	}
}