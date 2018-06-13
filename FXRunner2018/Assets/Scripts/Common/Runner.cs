using UnityEngine;

public class Runner : MonoBehaviour
{
	public enum JumpMode { Parabollic, Eased, CurveDriven }
	public float lane = 0f;

	[Range(0, 6)]
	public int idleAnim = 0;
	[Range(0, 4)]
	public int finishAnim = 0;
	public float speed = 0f;
	public float maxSpeed = 150f;
	public float accel = 6f;
	public float turnDuration = 0.4f;
	public float slipDuration = 2f;
	public float jumpHeight = 3f;
	public float jumpDuration = 0.75f;
	public float slipDecel = 10f;
	public JumpMode jumpMode = JumpMode.Parabollic;
	public AnimationCurve jumpCurve;
	
	public float turboShake = 1.5f;
	public GameObject prefabDebris;
	public GameObject magneticField;
	
	Manager mgr;
	
	bool canTurn = true;
	float prevLane = 0f;
	float nextLane = 0f;
	float laneTime = 0f;
	
	float slipTime = 0f;
	
	float jumpTime = 1f;
	bool grounded = true;
	bool sliping = false;
	bool alive = true;
	float turbo = 0f;
	float magnetTime = 0f;

	bool inGame = false;
	bool finished = false;
	
	Camera cam;
	Vector3 camOffset;
	Quaternion camRot;
	float camFOV;
	float orbitTime = 0f;
	
	float inHorz = 0f;
	bool inJump = false;
	
	Vector3 dir, pos, camPos;
	
	Rigidbody rb;
	Animator anim;
	//AudioSource au;
	ParticleSystem part;
	
	public static Runner instance;
	
	void Awake()
	{
		instance = this;
		rb = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
		//au = GetComponent<AudioSource>();
		part = GetComponentInChildren<ParticleSystem>();

		anim.SetFloat ("idle", idleAnim);
	}
	
	void Start()
	{
		mgr = Manager.instance;
		
		cam = Camera.main;
		camOffset = cam.transform.position;
		camRot = cam.transform.rotation;
		camFOV = cam.fieldOfView;
		orbitTime = Time.time + 8f;
		
		maxSpeed *= mgr.speedMultiplier;
	}
	
	void Update()
	{
		if (!finished)
		{
			inHorz = Input.GetAxisRaw ("Horizontal");
			inJump = Input.GetAxisRaw ("Vertical") > 0f;
			
			GetTouchInput ();
		}

		if (orbitTime > Time.time)
		{
			if (inHorz != 0f || inJump)
			{
				orbitTime = 0f;
				inHorz = 0f;
				inJump = false;
				Input.ResetInputAxes ();
			}
			
			return;
		}

		mgr.Running();
		
		if (grounded) // on the ground
		{
			if (!part.isEmitting)
			{
				part.Emit (10);
				part.Play ();
			}

			if (sliping) // sliping? cannot control, just wait to stop sliping
			{
				if (slipTime < Time.time)
					sliping = false;
			}
			else // not sliping, we can control
			{
				HandleSteering();
				
				if (inJump) // handle jumping
				{
					jumpTime = 0f;
					mgr.PlaySound(mgr.audioJump);
				}
			}
		}
		else // not grounded? advance time in the jump
		{
			part.Stop ();

			if (mgr.canSteerWhileJumping)
				HandleSteering();
			
			jumpTime += Time.deltaTime;
		}
		
		ChangeLane();
	}
	
	void FixedUpdate()
	{
		if (finished)
		{
			speed = Mathf.Max(0f, speed - 20f * Time.deltaTime); // reduce speed
		}
		else if (slipTime > Time.time) // slipìng?
		{
			speed = Mathf.Max(0f, speed - slipDecel * Time.deltaTime); // reduce speed
		}
		else if (grounded && orbitTime < Time.time) // not slipping, grounded and cam not orbiting?
		{
			turbo = Mathf.Max(0f, turbo - Time.deltaTime * 40f);
			speed = Mathf.Min(maxSpeed, speed + turbo + accel * Time.deltaTime); // accelerate
		}
		
		// au.pitch = mgr.enginePitch.Evaluate(speed/maxSpeed);
		
		if (jumpTime < jumpDuration) // jumping?
		{
			grounded = false;
			switch (jumpMode)
			{
				case JumpMode.Parabollic:
					pos.y = (1f - Mathf.Pow(2f * (jumpTime / jumpDuration) - 1f, 2f)) * jumpHeight;
					break;
				case JumpMode.Eased:
					pos.y = Manager.EaseJump(jumpTime / jumpDuration, 4f) * jumpHeight;
					break;
				case JumpMode.CurveDriven:
					pos.y = jumpCurve.Evaluate(jumpTime / jumpDuration) * jumpHeight;
					break;
			}
		}
		else
		{
			grounded = true;
			pos.y = 0f;
		}
		
		if (magnetTime < Time.time)
		{
			magneticField.SetActive(false);
		}
		
		anim.SetBool("grounded", grounded);
		
		pos.x = lane * mgr.laneWidth; // moving to the sides
		pos.z += speed * Time.deltaTime; // moving forward
		
		rb.MovePosition(pos); // update rigidbody position
		
		// update camera position
		if (/*!mgr.GetFinished() && */cam)
		{
			if (orbitTime > Time.time)
			{
				cam.transform.RotateAround(transform.position, Vector3.up, Time.deltaTime * 45f);
				camPos = cam.transform.position;
				camPos.y = 3f;
				cam.transform.position = camPos;
			}
			else
			{
				camPos = pos;
				camPos.x *= 0.75f;
				camPos.y = 0f;
				camPos += camOffset;
				
				cam.transform.position = Vector3.Lerp(cam.transform.position, camPos, 10f * Time.deltaTime);
				cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, camFOV + turbo * 0.25f, Time.deltaTime * 5f);
				
				if (turbo > 0f)
				{
					cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation,
						camRot * Quaternion.Euler(Mathf.Sin(Time.time * 40f) * turboShake,
							Mathf.Cos(Time.time * 30f) * turboShake, 0f),
						turbo * 0.005f);
				}
				else
				{
					cam.transform.rotation = camRot;
				}
			}
		}
	}
	
	void HandleSteering()
	{
		if (canTurn && inHorz != 0f) // handle steering
		{
			prevLane = Mathf.Round(lane);
			nextLane = Mathf.Clamp(prevLane + Mathf.Sign(inHorz),
				-mgr.sideLanes, mgr.sideLanes);
			
			if (nextLane != prevLane) // changing lane?
			{
				canTurn = false; // don't let steer again until turn is completed
				laneTime = 0f;
				
				if (grounded) // only animate steering if grounded
				{
					if (inHorz > 0f)
						anim.SetTrigger("turn_right");
					else
						anim.SetTrigger("turn_left");
				}
				
				mgr.PlaySound(mgr.audioTurn);
			}
		}
	}
	
	void OnTriggerEnter(Collider col)
	{
		if (col.tag == "Coin")
		{
			mgr.AddCoin();
			Destroy(col.gameObject);
			mgr.particlesGetCoin.transform.position = col.bounds.center;
			mgr.particlesGetCoin.Play();
			mgr.PlaySound(mgr.audioCoin, 0.5f);
		}
		else if (col.tag == "Turbo")
		{
			turbo = mgr.turboAddSpeed;
			Destroy(col.gameObject);
			mgr.particlesGetTurbo.transform.position = col.bounds.center;
			mgr.particlesGetTurbo.Play();
			mgr.PlaySound(mgr.audioTurbo);
		}
		else if (col.tag == "Magnet")
		{
			magneticField.SetActive(true);
			magnetTime = Time.time + mgr.magnetTime;
			Destroy(col.gameObject);
			mgr.particlesGetMagnet.transform.position = col.bounds.center;
			mgr.particlesGetMagnet.Play();
			mgr.PlaySound(mgr.audioMagnet);
		}
		else if (col.tag == "Block" || col.tag == "Wall")
		{
			col.isTrigger = false;
			gameObject.SetActive(false);
			Instantiate(prefabDebris, transform.position, transform.rotation);
			alive = false;
			mgr.GameOver();
		}
		else if (!sliping && col.tag == "Oil")
		{
			slipTime = Time.time + slipDuration;
			sliping = true;
			anim.SetTrigger("slip");
			// mgr.splashes.SetActive(true);
			mgr.particlesOil.transform.position = col.bounds.center;
			mgr.particlesOil.Play();
			mgr.PlaySound(mgr.audioOil);
		}
		else if (col.tag == "Goal")
		{
			mgr.Finished();
			magneticField.SetActive(false);
			mgr.PlaySound(mgr.audioGoal);
		}
	}

	int touchStatus = 0;
	float beginPosX;
	float deltaPosX;
	void GetTouchInput()
	{
		if (Time.timeScale == 0f)
		{
			touchStatus = 0;
			return;
		}
		
		if (touchStatus == 0 && Input.GetMouseButtonDown(0))
		{
			beginPosX = Input.mousePosition.x;
			touchStatus = 1;
		}

		if (touchStatus == 1 && Input.GetMouseButtonUp(0))
		{
			touchStatus = 0;
			deltaPosX = Input.mousePosition.x - beginPosX;
			
			if (Mathf.Abs(deltaPosX) < 20f)
			{
				inJump = true;
			}
			else
			{
				inHorz = deltaPosX;
			}
		}
	}
	
	void ChangeLane()
	{
		laneTime = laneTime + Time.deltaTime / turnDuration;
		// laneTime = laneTime + turnSpeed * Time.deltaTime;
		
		lane = Mathf.Lerp(prevLane, nextLane, Manager.EaseInOut(laneTime, 2f, true));
		
		if (laneTime >= 1f)
			canTurn = true;
	}
	
	public bool IsAlive()
	{
		return alive;
	}

	public void TriggerRunning()
	{
		if (!inGame)
		{
			inGame = true;
			anim.SetTrigger ("in_game");
		}
	}

	public void TriggerFinished()
	{
		if (!finished)
		{
			finished = true;
			anim.SetTrigger ("finished");
			anim.SetFloat("victory", finishAnim);
		}
	}
}
