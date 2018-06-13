using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TouchToLoad : MonoBehaviour
{
	public Text textTouch;
	public string nextScene;
	public bool canExit = true;
	
	bool loading = false;
	
	AudioSource au;
	
	void Start()
	{
		au = GetComponent<AudioSource>();
		Time.timeScale = 1f;
		AudioListener.pause = false;
	}
	
	void Update()
	{
		if (loading)
		{
			textTouch.enabled = (Mathf.Round(Time.time*10) % 2 == 0);
		}

		if (Input.GetButtonDown("Cancel"))
			Application.Quit();
	}
	
	void LoadNextScene()
	{
		SceneManager.LoadScene(nextScene);
	}

	public void ButtonPress()
	{
		if (!loading)
		{
			loading = true;
			au.Play();
			Invoke("LoadNextScene", au.clip.length);
		}
	}
}
