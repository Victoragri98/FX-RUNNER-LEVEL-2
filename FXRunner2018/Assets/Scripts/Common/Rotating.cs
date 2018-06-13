using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotating : MonoBehaviour
{
	public float speed = 180f;
	public bool worldSpace = true;
	public float phase = 0f;
	
	void Update()
	{
		transform.Rotate(0f, phase + speed * Time.deltaTime, 0f, worldSpace ? Space.World : Space.Self);
	}
}
