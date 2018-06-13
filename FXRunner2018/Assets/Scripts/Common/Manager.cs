using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
	[System.Serializable]
	public class Section : System.Object
	{
		[Range(0,1)] public float start = 0f;
		[Range(0,1)] public float end = 1f;
		public int[] indexBlock;
		public int[] indexBigBlock;
	};
	
	[Header("UI")]
	public Text coinCounter;
	public Text timeCounter;
	public Image readySign;
	public Image goSign;
	public GameObject gameOver;
	public GameObject scoreFrame;
	public Text yourScore;
	public Text bestScoreCaption;
	public Text bestScore;
	public Text scoreMessage;
	public string messageBestScore = "You are the best!";
	public string messageNotBestScore = "Well done";
	public GameObject pausePanel;
	
	[Header("Level Configuration")]
	public float levelLength = 200f;
	public float sideLanes = 1f;
	public float laneWidth = 3f;
	public float objectStartZ = 20f;
	public float objectSpacing = 2f;
	public float turboAddSpeed = 100f;
	public float magnetTime = 4f;
	public Vector2 itemsInGroup = new Vector2(4f, 14f);
	public float speedMultiplier = 1f;
	public bool canSteerWhileJumping = false;
	
	[Header("Level Generation Probabilities")]
	[Tooltip("Seed for random generation. Set to zero to randomize the seed.")]
	public int seed = 0;
	[Space(10)]
	public float probPutBlock = 0.1f;
		public float probPutBigBlock = 0.2f;
	[Space(10)]
	public float probPutOil = 0.1f;
	[Space(10)]
	public float probPutItem = 0.5f;
		public float probPutCoin = 0.95f;
		public float probPutTurbo = 0.03f;
		public float probPutMagnet = 0.02f;
	public float probCoinsChangeLane = 0.1f;
	
	[Header("Effects")]
	public ParticleSystem particlesGetCoin;
	public ParticleSystem particlesGetMagnet;
	public ParticleSystem particlesGetTurbo;
	public ParticleSystem particlesOil;
	public GameObject splashes;
	public ParticleSystem confetti;
	
	[Header("Prefabs")]
	public GameObject[] prefabRunners;
	public GameObject prefabCoin;
	public GameObject prefabTurbo;
	public GameObject prefabMagnet;
	public GameObject prefabOil;
	public GameObject prefabGoal;
	public GameObject[] prefabBlock;
	public GameObject[] prefabBigBlock;
	public Section[] sections;
	
	[Header("Sounds")]
	public AudioClip audioReady;
	public AudioClip audioGo;
	public AudioClip audioTurn;
	public AudioClip audioJump;
	public AudioClip audioCoin;
	public AudioClip audioTurbo;
	public AudioClip audioMagnet;
	public AudioClip audioOil;
	public AudioClip audioGoal;
	public AudioClip audioCounting;
	public AudioClip audioScore;
	public AudioClip audioBestScore;
	public AnimationCurve enginePitch;
	
	[Header("Other")]
	public string nextScene;
	public string titleScene;
	public LayerMask groundLayer;
	
	public bool deleteScore = false;
	
	float runningTime = -8f;
	int coins = 0;
	int score = 0;
	
	bool finished = false;
	
	int itemsBudget = 0;
	
	public static Manager instance;
	
	AudioSource au;
	
	void Awake()
	{
		instance = this;
		
		au = GetComponent<AudioSource>();
		
		readySign.gameObject.SetActive(false);
		goSign.gameObject.SetActive(false);
		gameOver.SetActive(false);
		scoreFrame.gameObject.SetActive(false);
		scoreMessage.gameObject.SetActive(false);
		bestScoreCaption.gameObject.SetActive(false);
		bestScore.gameObject.SetActive(false);
	}
	
	void Start()
	{
		Time.timeScale = 1f;
		AudioListener.pause = false;

		if (deleteScore)
			PlayerPrefs.DeleteKey("BestOf" + SceneManager.GetSceneAt(0).name);
		
		Instantiate(prefabRunners[SelectionManager.runnerSelection], Vector3.zero, Quaternion.identity);
		GenerateLevel();
	}
	
	void Update()
	{
		if (runningTime > 2f)
		{
			goSign.gameObject.SetActive(false);
		}
		else if (runningTime > 0f)
		{
			goSign.gameObject.SetActive(true);
			readySign.gameObject.SetActive(false);
			Runner.instance.TriggerRunning ();
		}
		else if (runningTime > -2f)
		{
			readySign.gameObject.SetActive(true);
		}
		
		if (!finished)
		{
			runningTime += Time.deltaTime;
			timeCounter.text = Mathf.Floor(Mathf.Max(0f, runningTime)).ToString();
		}

		if (Input.GetButtonDown("Cancel"))
		{
			SetPause(Time.timeScale != 0f);
		}
	}
	
	Vector3 newPos;
	float itemLane = 0f;
	float itemHeight = 0f;
	void GenerateLevel()
	{
		if (seed == 0)
		{
			seed = (int)(Random.value * 36000f);
			print("Random seed: " + seed);
		}
		
		Random.InitState(seed);
		
		for (float z = objectStartZ; z < levelLength; z += objectSpacing)
		{
			float prob = Random.value;
			
			if ((prob -= probPutItem) <= 0f) // want to put an item?
			{
				// create a new items budget
				if (itemsBudget == 0)
					itemsBudget = (int)Random.Range(itemsInGroup.x, itemsInGroup.y);
			}
			else if ((prob -= probPutBlock) <= 0f) // want to put block?
			{
				if (Random.value < probPutBigBlock) // choose to put a big block?
				{
					// choose a location between two lanes
					float x = Random.Range(-(int)sideLanes, (int)sideLanes) + 0.5f;
					newPos = new Vector3(x * laneWidth, 0f, z);
					if (!GetColliderHere(newPos)) // get what collider is in the position
					{
						int i, sec = GetSectionHere(z);
						if (sec > -1)
							i = sections[sec].indexBigBlock[Random.Range(0, sections[sec].indexBigBlock.Length)];
						else
							i = Random.Range(0, prefabBigBlock.Length);
						
						Instantiate(prefabBigBlock[i], newPos, Quaternion.identity);
					}
				}
				else // or a small block?
				{
					// choose a location in the center of a lane
					float x = Random.Range(-(int)sideLanes, (int)sideLanes+1);
					newPos = new Vector3(x * laneWidth, 0f, z);
					if (!GetColliderHere(newPos)) // get what collider is in the position
					{
						int i, sec = GetSectionHere(z);
						if (sec > -1)
							i = sections[sec].indexBigBlock[Random.Range(0, sections[sec].indexBigBlock.Length)];
						else
							i = Random.Range(0, prefabBigBlock.Length);
						
						Instantiate(prefabBlock[i], newPos, Quaternion.identity);
					}
				}
			}
			else if ((prob -= probPutOil) <= 0f) // want to put an oil puddle?
			{
				float x = Random.Range(-(int)sideLanes, (int)sideLanes+1);
				newPos = new Vector3(x * laneWidth, 0f, z);
				if (!GetColliderHere(newPos)) // get what collider is in the position
					Instantiate(prefabOil, newPos, Quaternion.identity);
			}
			
			if (itemsBudget > 0) // put items according to budget
			{
				// what item will we instantiate?
				float itemProb = Random.value;
				if ((itemProb -= probPutCoin) < 0f)
				{
					float oldLane = itemLane;
					
					if (Random.value < probCoinsChangeLane) // want to change lane?
						itemLane = Mathf.Clamp(itemLane + Mathf.Sign(Random.Range(-1, 1)),
							-sideLanes, sideLanes); // one left or one right
					
					// preliminar new position for the item
					newPos = new Vector3(0.5f * (itemLane + oldLane) * laneWidth, 0f, z);
					if (PutObjectHere(prefabCoin, newPos))
						itemsBudget--;
				}
				else if ((itemProb -= probPutTurbo) < 0f)
				{
					// preliminar new position for the item
					newPos = new Vector3(Random.Range(-(int)sideLanes, (int)sideLanes+1) * laneWidth, 0f, z);
					if (PutObjectHere(prefabTurbo, newPos))
						itemsBudget--;
				}
				else if ((itemProb -= probPutMagnet) < 0f)
				{
					// preliminar new position for the item
					newPos = new Vector3(Random.Range(-(int)sideLanes, (int)sideLanes+1) * laneWidth, 0f, z);
					if (PutObjectHere(prefabMagnet, newPos))
						itemsBudget--;
				}
			}
			// if (itemsBudget > 0) // put items according to budget
			// {
			// 	float oldLane = itemLane;
			// 	float oldHeight = itemHeight;
				
			// 	if (Random.value < probCoinsChangeLane) // change lane?
			// 		itemLane = Mathf.Clamp(itemLane + Mathf.Sign(Random.Range(-1, 1)),
			// 			-sideLanes, sideLanes); // one left or one right
				
			// 	newPos = new Vector3(0.5f * (itemLane + oldLane) * laneWidth, 0f, z);
				
			// 	Collider col = GetColliderHere(newPos); // get what collider is in the position
				
			// 	if (!col || col.tag != "Wall") // is it not a wall?
			// 	{
			// 		if (!col) // nothing here? then just put the item
			// 			itemHeight = 0f;
			// 		else if (col.tag == "Block") // is it a block? put the item higher
			// 			itemHeight = 1f;
					
			// 		// adjust new height based on oldHeight to make it change more gradually
			// 		newPos.y = 0.5f * (itemHeight + oldHeight) * Player.instance.jumpHeight;
					
			// 		// now, what item will we instantiate?
			// 		float itemProb = Random.value;
			// 		if ((itemProb -= probPutCoin) < 0f)
			// 			Instantiate(prefabCoin, newPos, Quaternion.identity);
			// 		else if ((itemProb -= probPutTurbo) < 0f)
			// 			Instantiate(prefabTurbo, newPos, Quaternion.identity);
			// 		else if ((itemProb -= probPutMagnet) < 0f)
			// 			Instantiate(prefabMagnet, newPos, Quaternion.identity);
					
			// 		itemsBudget--;
			// 	}
			// }
		}
		
		Instantiate(prefabGoal, new Vector3(0f, 0f, levelLength), Quaternion.identity);
	}
	
	int GetSectionHere(float z)
	{
		// identify in what section we curently are
		float progress = z / levelLength;
		int sec = -1;
		for (int i = 0; i < sections.Length; ++i)
			if (sections[i].start <= progress && sections[i].end > progress)
			{
				sec = i;
				break;
			}
		
		return sec;
	}
	
	bool PutObjectHere(GameObject prefab, Vector3 pos)
	{
		// detect if we have to put the item higher, over a block
		Collider col = GetColliderHere(pos); // get what collider is in the position
		
		if (!col || col.tag != "Wall") // is it not a wall?
		{
			if (!col) // nothing here? then just put the item
				itemHeight = 0f;
			else if (col.tag == "Block") // is it a block? put the item higher
				itemHeight = Runner.instance.jumpHeight;
			
			// adjust new height based on oldHeight to make it change more gradually
			pos.y = itemHeight;
			
			if (GetColliderHere(pos) == null)
				Instantiate(prefab, pos, Quaternion.identity);
			
			return true;
		}
		
		return false;
	}
	
	public void AddCoin()
	{
		coins++;
		coinCounter.text = coins.ToString();
	}
	
	public static float EaseInOut(float t, float pow, bool clamped=false)
	{
		if (clamped)
			t = Mathf.Clamp01(t);
		
		return Mathf.Lerp(0f, 1f, Mathf.Pow(t, pow) / (Mathf.Pow(t, pow) + Mathf.Pow(1-t, pow)));
	}
	
	public static float EaseJump(float t, float pow)
	{
		// (1 - ( ( 2x-1 ) ^ 4 ) )
		return 1 - Mathf.Pow(2*t - 1, pow);
	}
	
	public bool GetFinished()
	{
		return finished;
	}
	
	public void Finished()
	{
		finished = true;
		StartCoroutine(ShowScore());
		score = coins * 10 - (int)runningTime;
		Runner.instance.TriggerFinished ();
	}
	
	IEnumerator ShowScore()
	{
		yield return new WaitForSeconds(2f);
		
		scoreFrame.SetActive(true);
		
		print("Your score: " + score);
		
		float shown = 2;
		while (shown < score)
		{
			yourScore.text = ((int)shown).ToString();
			shown *= 1.05f;
			au.PlayOneShot(audioCounting, 0.25f);
			yield return null;
		}
		yourScore.text = score.ToString();
		
		yield return new WaitForSeconds(1f);
		
		int best = PlayerPrefs.GetInt("BestOf" + SceneManager.GetSceneAt(0).name, 0);
		print("Best score in " + "BestOf" + SceneManager.GetSceneAt(0).name + ": " + best);
		
		if (score > best)
		{
			confetti.Play();
			scoreMessage.text = messageBestScore;
			PlayerPrefs.SetInt("BestOf" + SceneManager.GetSceneAt(0).name, score);
			best = score;
			print("New best score saved in " + "BestOf" + SceneManager.GetSceneAt(0).name);
			au.PlayOneShot(audioBestScore);
		}
		else
		{
			scoreMessage.text = messageNotBestScore;
			au.PlayOneShot(audioScore);
		}
		
		scoreMessage.gameObject.SetActive(true);
		bestScoreCaption.gameObject.SetActive(true);
		bestScore.gameObject.SetActive(true);
		bestScore.text = best.ToString();
	}
	
	Collider GetColliderHere(Vector3 point, float radius=1f)
	{
		Collider[] cols = Physics.OverlapSphere(point + Vector3.up * radius, radius,
			~groundLayer, QueryTriggerInteraction.Collide);
		
		if (cols.Length > 0)
			return cols[0];
		else
			return null;
	}
	
	public void GameOver()
	{
		gameOver.SetActive(true);
		timeCounter.gameObject.SetActive(false);
		coinCounter.gameObject.SetActive(false);
	}
	
	public void Running()
	{
		if (runningTime < 0f)
			runningTime = 0f;
	}
	
	public void LoadTitle()
	{
		SceneManager.LoadScene(titleScene);
	}
	
	public void ReloadLevel()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	
	public void LoadNext()
	{
		SceneManager.LoadScene(nextScene);
	}
	
	public void PlaySound(AudioClip clip, float volume=1f)
	{
		au.PlayOneShot(clip, volume);
	}

	public void SetPause(bool _pause)
	{
		Time.timeScale = _pause ? 0f : 1f;
		AudioListener.pause = _pause;
		pausePanel.SetActive(_pause);
	}
}
