using UnityEngine;

public class NotInWebGL : MonoBehaviour {
	private void Start() {
		#if UNITY_WEBGL

		gameObject.SetActive(false);

		#endif
	}
}