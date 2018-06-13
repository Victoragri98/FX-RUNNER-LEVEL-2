using UnityEngine;
 
// [ExecuteInEditMode]
public class CurveController : MonoBehaviour
{
	public bool executeInEditMode = false;
	
	public Vector2 curveMinMax = new Vector2(50f, 50f);
	public Transform curveOrigin;
	public float falloff = 0f;
	public float adaptSpeed = 0.01f;
	public float timeBetweenCurves = 5f;
	
	Vector2 newCurve;
	Vector2 oldCurve;
	Vector2 bendCurve;
	float timeToCurve = 0f;
	float t = 0f;
	
	int bendAmountId;
	int bendOriginId;
	int bendFalloffId;

	void Start()
	{
		bendAmountId = Shader.PropertyToID("_BendAmount");
		bendOriginId = Shader.PropertyToID("_BendOrigin");
		bendFalloffId = Shader.PropertyToID("_BendFalloff");
		
		bendCurve = new Vector2(Random.Range(-curveMinMax.x, curveMinMax.x),
								Random.Range(-curveMinMax.y, curveMinMax.y));
	}
	
	void Update()
	{
		if (executeInEditMode || Application.isPlaying)
		{
			if (!Manager.instance.GetFinished() && Runner.instance.IsAlive())
			{
				if (timeToCurve < Time.time)
				{
					oldCurve = bendCurve;
					newCurve = new Vector2(	Random.Range(-curveMinMax.x, curveMinMax.x),
											Random.Range(-curveMinMax.y, curveMinMax.y));
					
					timeToCurve = Time.time + timeBetweenCurves;
					t = 0f;
				}
				else
				{
					t += adaptSpeed * Runner.instance.speed * Time.deltaTime;
					
					bendCurve = Vector2.Lerp(oldCurve, newCurve, Manager.EaseInOut(t, 2f, true));
				}
			}
						
			Shader.SetGlobalVector(bendAmountId, bendCurve);
			Shader.SetGlobalVector(bendOriginId, curveOrigin.position);
			Shader.SetGlobalFloat(bendFalloffId, falloff);
		}
		else
			Shader.SetGlobalVector(bendAmountId, Vector2.zero);
	}
	
	void OnDisable()
	{
		Shader.SetGlobalVector(bendAmountId, Vector2.zero);
	}
}
