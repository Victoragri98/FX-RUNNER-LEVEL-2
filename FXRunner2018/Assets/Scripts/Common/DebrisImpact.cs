using UnityEngine;

public class DebrisImpact : MonoBehaviour
{
	public float breakForce = 10f;
	public Vector3 hitOffset = Vector3.forward;
	
	void Start()
	{
		Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rb in rbs)
		{
			rb.AddExplosionForce(breakForce, transform.position + transform.TransformVector(hitOffset),
				1f, breakForce * 0.5f, ForceMode.Impulse);
		}
	}
}
