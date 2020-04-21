using UnityEngine;

/// <summary>
/// https://stackoverflow.com/questions/5817490/implementing-box-mueller-random-number-generator-in-c-sharp
/// </summary>
public sealed class GaussianRandom {
	private static bool phase = false;

	private static float u;
	private static float v;

	private static float factor;


	public static float value {
		get {
			float z;

			if(phase) {
				z = v * factor;
			}
			else {
				float s;

				do {
					u = 2.0f * Random.value - 1.0f;
					v = 2.0f * Random.value - 1.0f;

					s = u * u + v * v;
				}
				while(s >= 1.0f);

				factor = Mathf.Sqrt(-2.0f * Mathf.Log(s) / s);

				z = u * factor;
			}

			phase = !phase;

			return z;
		}
	}
}
