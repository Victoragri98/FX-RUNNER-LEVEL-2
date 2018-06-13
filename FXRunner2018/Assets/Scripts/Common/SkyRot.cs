using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyRot : MonoBehaviour
{
	public Material mat;
	public float initialRot = 0f;
	public float speed = 1f;
	
	void Update()
	{
		mat.SetFloat("_Rotation", initialRot + speed * Time.time);
		print((initialRot + speed * Time.time));
	}
}
