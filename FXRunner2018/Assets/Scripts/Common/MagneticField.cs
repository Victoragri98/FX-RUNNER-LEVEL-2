using UnityEngine;

public class MagneticField : MonoBehaviour
{
	public float radius = 6f;
	
	void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(transform.position, radius);
	}
	
	Rigidbody rb;
	Collider[] col;
	void FixedUpdate()
	{
		col = Physics.OverlapSphere(transform.position, radius,
			Physics.AllLayers, QueryTriggerInteraction.Collide);
		
		for (int i = 0; i < col.Length; ++i)
		{
			if (col[i].tag == "Coin")
			{
				rb = col[i].attachedRigidbody;
				rb.MovePosition(rb.position + (transform.position - rb.position).normalized *
					Time.deltaTime * Runner.instance.speed * 2f);
			}
		}
	}
}
