using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerActivator : MonoBehaviour
{
	public GameObject objectToActivate;
	public GameObject objectToDeactivate;
	
	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject == Runner.instance.gameObject)
		{
			if (objectToActivate)
				objectToActivate.SetActive(true);
			
			if (objectToDeactivate)
				objectToDeactivate.SetActive(false);
			
			Destroy(gameObject);
		}
	}
}
