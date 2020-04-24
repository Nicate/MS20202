using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class Importer : MonoBehaviour {
	#if UNITY_EDITOR

	[UnityEditor.CustomEditor(typeof(Importer))]
	public class ImportButton : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			if(GUILayout.Button("Load File")) {
				(target as Importer).load();
			}
		}

	}

	#endif


	public GameObject prefab;

	public string fileName;

	
	#if UNITY_EDITOR

	private void load() {
		if(File.Exists(fileName)) {
			List<GameObject> gameObjects = new List<GameObject>();

			foreach(Transform child in transform) {
				gameObjects.Add(child.transform.gameObject);
			}

			foreach(GameObject gameObject in gameObjects) {
				gameObject.transform.SetParent(null);
				DestroyImmediate(gameObject);
			}

			string[] lines = File.ReadAllLines(fileName);

			foreach(string line in lines) {
				string[] numbers = line.Split(',');

				float x = float.Parse(numbers[0], CultureInfo.InvariantCulture);
				float z = float.Parse(numbers[1], CultureInfo.InvariantCulture);

				Vector3 position = new Vector3(x, 0.0f, z);
				Quaternion rotation = Quaternion.LookRotation(position.normalized, Vector3.up);

				GameObject instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
				
				instance.name = string.Format("{0} ({1}, {2})", prefab.name, x, z);
				instance.transform.SetPositionAndRotation(position, rotation);
			}
		}
	}

	#endif
}