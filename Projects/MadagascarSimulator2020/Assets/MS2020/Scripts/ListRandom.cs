using System.Collections.Generic;
using UnityEngine;

public sealed class ListRandom {
	/// <summary>
	/// https://answers.unity.com/questions/486626/how-can-i-shuffle-alist.html
	/// </summary>
	public static void shuffle<T>(List<T> list) {
		T t;

		int count = list.Count;

		for(int index = 0; index < count; index++) {
			int r = index + (int) (Random.value * (count - index));

			t = list[r];
			list[r] = list[index];
			list[index] = t;
		}
	}


	public static T select<T>(List<T> list) {
		return list[Random.Range(0, list.Count)];
	}
}
