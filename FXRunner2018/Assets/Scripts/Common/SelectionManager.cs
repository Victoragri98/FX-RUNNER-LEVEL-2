using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelData : System.Object
{
    public GameObject prefabLevelModel;
    public string sceneName;
};

public class SelectionManager : MonoBehaviour
{
	public GameObject[] prefabRunners;
	public LevelData[] levelsData;
	public GameObject prefabPlatform;
	public Transform runnersPlatformsParent;
	public Transform levelsPlatformsParent;
	
	new public Transform camera;
	public Transform camPosRunnerSelection;
	public Transform camPosLevelSelection;
	
	public bool rotateRunners = true, rotateLevels = true;
	public float rotSpeed = 90f;
	
	public AudioClip audioScroll;
	public AudioClip audioSelect;
	
	int selectionStep = 1; // 1: runner, 2: level
	
	GameObject[] runners;
	GameObject[] levels;
	
	Vector3 runnerPos;
	Vector3 levelPos;
	
	bool loading = false;
	
	public static int runnerSelection = 0;
	public static int levelSelection = 0;
	
	AudioSource au;
	
	void Start()
	{
		au = GetComponent<AudioSource>();
		
		camera.position = camPosRunnerSelection.position;
		camera.rotation = camPosRunnerSelection.rotation;
		
		runnerPos = runnersPlatformsParent.position;
		levelPos = levelsPlatformsParent.position;
		
		runners = new GameObject[prefabRunners.Length];
		Transform transPlatform;
		
		for (int i = 0; i < runners.Length; ++i)
		{
			transPlatform = Instantiate(prefabPlatform,
				runnersPlatformsParent.position + Vector3.up * i * -3f,
				Quaternion.identity).transform;
			transPlatform.SetParent(runnersPlatformsParent);
			
			runners[i] = Instantiate(prefabRunners[i],
				transPlatform.position, Quaternion.identity);
			runners[i].transform.SetParent(transPlatform);
			runners [i].transform.Rotate (0f, 180f, 0f);
			Destroy(runners[i].GetComponent<Runner>());
			Destroy(runners[i].GetComponent<Rigidbody>());
			Destroy(runners[i].GetComponent<AudioSource>());
		}
		
		levels = new GameObject[levelsData.Length];
		
		for (int i = 0; i < levels.Length; ++i)
		{
			transPlatform = Instantiate(prefabPlatform,
				levelsPlatformsParent.position + Vector3.up * i * -3f,
				Quaternion.identity).transform;
			transPlatform.SetParent(levelsPlatformsParent);
			
			levels[i] = Instantiate(levelsData[i].prefabLevelModel,
				transPlatform.position, Quaternion.identity);
			levels[i].transform.SetParent(transPlatform);
		}
		
		runnerSelection = 0;
		levelSelection = 0;
	}
	
	void Update()
	{
		switch (selectionStep)
		{
			case 1:
				if (rotateRunners)
					for (int i = 0; i < runners.Length; ++i)
						runners[i].transform.Rotate(0f, Time.deltaTime * rotSpeed, 0f);
				
				runnersPlatformsParent.position = Vector3.Lerp(runnersPlatformsParent.position,
					runnerPos, Time.deltaTime * 4f);
				
				camera.position = Vector3.Lerp(camera.position,
					camPosRunnerSelection.position,
					Time.deltaTime * 4f);
				
				break;
			case 2:
				if (rotateLevels)
					for (int i = 0; i < levels.Length; ++i)
						levels[i].transform.Rotate(0f, Time.deltaTime * 180f, 0f);
				
				levelsPlatformsParent.position = Vector3.Lerp(levelsPlatformsParent.position,
					levelPos, Time.deltaTime * 4f);
				
				camera.position = Vector3.Lerp(camera.position,
					camPosLevelSelection.position,
					Time.deltaTime * 4f);
				
				break;
		}
	}
	
	public void Scroll(int dir)
	{
		au.PlayOneShot(audioScroll);
		
		switch (selectionStep)
		{
			case 1:
				runnerSelection = Mathf.Clamp(runnerSelection + dir, 0, runners.Length-1);
				runnerPos.y = runnerSelection * 3f;
				break;
			case 2:
				levelSelection = Mathf.Clamp(levelSelection + dir, 0, levels.Length-1);
				levelPos.y = levelSelection * 3f;
				break;
		}
	}
	
	public void MakeSelection()
	{
		au.PlayOneShot(audioSelect);
		
		switch (selectionStep)
		{
			case 1:
				selectionStep = 2;
				break;
			case 2:
				if (!loading)
				{
					loading = true;
					Invoke("LoadNext", audioSelect.length);
				}
				break;
		}
	}
	
	void LoadNext()
	{
		SceneManager.LoadScene(levelsData[levelSelection].sceneName);
	}
}
