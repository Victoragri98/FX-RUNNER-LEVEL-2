using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebLink : MonoBehaviour
{
	public void OpenURL(string _url)
	{
		Application.OpenURL(_url);
	}
};
