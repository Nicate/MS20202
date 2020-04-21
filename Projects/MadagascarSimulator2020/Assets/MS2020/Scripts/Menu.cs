using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {
	public GameObject backdrop;
	public GameObject foredrop;

	private bool howToPlaying = false;


	public void start() {
		SceneManager.LoadScene("Madagascar");
	}
	
	public void exit() {
		Application.Quit();
	}


	public void toggleHowToPlay() {
		howToPlaying = !howToPlaying;

		backdrop.SetActive(!howToPlaying);
		foredrop.SetActive(howToPlaying);
	}
}