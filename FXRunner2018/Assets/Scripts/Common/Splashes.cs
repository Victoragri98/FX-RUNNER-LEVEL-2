using UnityEngine;
using UnityEngine.UI;

public class Splashes : MonoBehaviour
{
	public Image[] img;
	public float awayFromCenter = 0.4f;
	public float upOffset = 0.25f;
	
	Vector3 scale = Vector3.one;
	Color color = Color.white;
	float splashing = 0f;
	
	void OnEnable()
	{
		scale.y = 1f;
		color = Color.white;
		color.a = 0f;
		splashing = 0f;
		
		float range = Screen.width * awayFromCenter;
		
		for (int i = 0; i < img.Length; ++i)
		{
			img[i].rectTransform.anchoredPosition = new Vector2(
				Random.Range(-range, range),
				Random.Range(-range, range) + Screen.height * upOffset
			);
		}
	}
	
	void Update ()
	{
		if (splashing < 1f)
		{
			color.a = splashing;
			scale.y = (1.5f + Mathf.Pow(splashing-1, 2f) - Mathf.Pow(splashing-1.1f, 6f)) * 0.66f;
			scale.x = scale.y;
			splashing += Time.deltaTime * 4f;
		}
		else
		{
			scale.y += Time.deltaTime * 0.4f;
			color.a -= Time.deltaTime * 0.2f;
			
			if (color.a <= 0f)
				gameObject.SetActive(false);
		}
		
		for (int i = 0; i < img.Length; ++i)
		{
			img[i].transform.localScale = scale;
			img[i].color = color;
		}
	}
}
