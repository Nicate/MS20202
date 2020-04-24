using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {
	public GameObject[] gameObjectsToShowInMenu;
	public GameObject[] gameObjectsToShowInHowToPlay;

	private bool howToPlaying;


	private void Start() {
		howToPlaying = false;

		updateGameObjects();
	}


	public void start() {
		SceneManager.LoadScene("Madagascar");
	}
	
	public void exit() {
		Application.Quit();
	}


	public void toggleHowToPlay() {
		howToPlaying = !howToPlaying;

		updateGameObjects();
	}


	private void updateGameObjects() {
		foreach(GameObject gameObject in gameObjectsToShowInMenu) {
			gameObject.SetActive(!howToPlaying);
		}

		foreach(GameObject gameObject in gameObjectsToShowInHowToPlay) {
			gameObject.SetActive(howToPlaying);
		}
	}
}