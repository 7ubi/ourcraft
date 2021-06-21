using UnityEngine;
using System.Collections;

public class DestroyEffect : MonoBehaviour
{
	private float _time;
	
	void Update ()
	{
		_time += Time.deltaTime;

		if (_time >= 3)
		{
			Destroy(transform.gameObject);
		}
	}
}
