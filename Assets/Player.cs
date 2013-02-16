//#define DEBUG

using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
	const int TOP = 1;
	const int LEFT = 2;
	const int RIGHT = 4;
	const int BOTTOM = 8;
	
	int allCollisions = 0;
	
	public Transform launcher;
	
	//these numbers are bullshit, by the way.
	public ParticleSystem emitter;
	public float JumpForce = 1f;
	public float DownForce = .1f;
	public float upGrav = 9.8f;
	public float downGrav = 9.8f;
	public float goSpeed = .5f;
	public float mouseSpeedControlMax = 10f;
	public GameObject handLocation;
	public float grappleSpeed = .6f;
	public float grappleReturnSpeed = .6f;
	public float grappleRange =  15;
	public float grappleFloatTime = .8f;
	public float turnRate = .5f;
	
	int lastBar = 0; //the player can't grab the last bar they grappled until they are falling
	int prospectiveBar = -1;
	
	enum state {  JUMPING, FALLING, SETTOJUMP, GRAPPLED, PULLING, TURNING};
	enum secondary {NONE,SHOT, RELEASED};
	
	state now = state.SETTOJUMP;
	state last;
	secondary state2 = secondary.NONE;
	
	
	Grapple grabby;
	Vector3 grappleAnchor;
	float length = 0;
	Vector3 grappleOld;
	bool deployed = false;
	
	Vector3 currentMovement;
	
	bool facingRight = true;
	bool oldFacing = true;
	Vector3 tempRotation;
	
	bool nearBar = false;
	
	bool onGround;
	public bool isGrounded
	{
		get{
			return onGround;
		}
	}

	Vector3 oldHandLoc;
	
	Transform barStart;
	Transform barEnd;
	
	int mouselayermask;
	
	// Use this for initialization
	void Start () {
		last = now;
		currentMovement =new Vector3(0,0,0);
		
		//log old hand local location
		oldHandLoc = handLocation.transform.localPosition;
		
		mouselayermask = 1 << LayerMask.NameToLayer("mousePlane");
		Debug.Log(mouselayermask.ToString());
			
	}
	
	
	// Update is called once per frame
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		
		CapsuleCollider physicall = (CapsuleCollider)this.GetComponent("CapsuleCollider");
		
		
		Ray mouseLine = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit stuff;
		
		//Debug.DrawRay(mouseLine.origin, mouseLine.direction, Color.red);
		
		//Debug.Log("Grabber is layer :" + (LayerMask.NameToLayer("grabber").ToString())+ " from " + Camera.mainCamera.ToString() + "originating"
		//	+ mouseLine.origin.ToString());
		
		//try to get the direction of movement from the mouse
		Vector2 screenHere = Camera.mainCamera.WorldToScreenPoint(this.transform.position);
		
		float mouseVector = Input.mousePosition.x - screenHere.x;
		

		
		//check for click action
		if(Input.GetMouseButtonDown(0) && !deployed)// && (now == state.SETTOJUMP || now == state.GRAPPLED))
		{	
			
			//check for hit
			if(Physics.Raycast(mouseLine, out stuff, Mathf.Infinity, mouselayermask))
			{
				//check of type
				if(stuff.transform.tag == "mousePlane")
				{
					//deploy grapple hook
					Transform temp = (Transform)Object.Instantiate(launcher);
					grabby = (Grapple)temp.GetComponent("Grapple");
					grabby.passInfo(grappleRange, grappleSpeed, grappleReturnSpeed, handLocation, stuff.point, grappleFloatTime, this);
					
					deployed = true;
#if DEBUG
					Debug.Log(stuff.transform.tag.ToString());
#endif
				}
					
			}
			
		}
		
		
		
		
		if(Mathf.Abs(mouseVector) > mouseSpeedControlMax)
		{
			//cap the the distance 
			mouseVector = mouseSpeedControlMax * (mouseVector/Mathf.Abs(mouseVector));
		}
		
		//scale the speed based of of the distance
		float speedScale = mouseVector/mouseSpeedControlMax;
		
		currentMovement.x = goSpeed * speedScale;
		
		oldFacing = facingRight;
		if(currentMovement.x > 0)
		{
			facingRight = true;
			if(oldFacing != facingRight)
				now = state.TURNING;
		}
		else if (currentMovement.x < 0)
		{
			facingRight = false;
			if(oldFacing != facingRight)
				now = state.TURNING;
		}
		
		
		//the mechanics of jumping
		//yes, the implication is that you can't turn while jumping
		if(now == state.TURNING)
		{
			currentMovement.x = 0;
			if(this.transform.localEulerAngles.y < 90 && facingRight)
			{
				now = state.FALLING;
			}
			else if(this.transform.localEulerAngles.y > 90 && !facingRight)
			{
				now = state.FALLING;
			}
		}
		else if(now == state.JUMPING)
		{
			if(now != last)
			{
				//start of jump, apply force
				currentMovement.y = JumpForce; 
				
			}
			else
			{	//starting to fallllll
				float newSpeed = Time.deltaTime * -upGrav + currentMovement.y;
				if (newSpeed <= 0)
				{
					now = state.FALLING;
				}
				//bumped head, fall.
				
				else if ((allCollisions & TOP)  > 0)
				{
#if DEBUG
					Debug.Log("Bonked Head, falling");
#endif
					newSpeed = 0;
					now = state.FALLING;
				}
				
				currentMovement.y = newSpeed;
#if DEBUG
				Debug.Log("Jumping at: " + currentMovement.y.ToString());
#endif
				
			}
		}
		
		else if ( now == state.FALLING)
		{	
			
			float newSpeed = Time.deltaTime * -downGrav + currentMovement.y;
			//started falling recently, this bit reduces airtime
			if (now != last)
			{
				newSpeed = -DownForce;
			}
			
			//hitting the ground is a good time to stop falling
			//if(isGrounded)
			if(((CharacterController)this.GetComponent("CharacterController")).isGrounded)
			{
				now = state.SETTOJUMP;
				newSpeed = 0;
			}
			currentMovement.y = newSpeed;
#if DEBUG
			Debug.Log("Falling at: " + currentMovement.y.ToString());
#endif
			
			//once away from the bar, allow for bar grab
			if(!nearBar && state2 == secondary.RELEASED)
			{
#if DEBUG
				Debug.Log("...and Reset!");
#endif
				state2 = secondary.NONE;
			}
		}
		
		else if( now == state.GRAPPLED)
		{
			//for testing purposes...
			//assuming lateral movement
			Vector3 slope = barEnd.transform.position - barStart.transform.position;
			slope.Normalize();
			currentMovement = currentMovement.x * slope;
#if DEBUG
			Debug.Log("GRABBED");
#endif
			if( Input.GetMouseButtonDown(1))
			{
#if DEBUG
				Debug.Log("released!");
#endif
				state2 = secondary.RELEASED;
				now = state.FALLING;
			}
			
			//end of the bar!
			if(transform.position.x > barEnd.transform.position.x || transform.position.x < barStart.position.x)
			{
				now = state.FALLING;
			}
			
			lastBar = prospectiveBar;
		}
		
		else if(now == state.SETTOJUMP)
		{
			state2 = secondary.NONE;
			if (!((CharacterController)this.GetComponent("CharacterController")).isGrounded)
			{
				now = state.FALLING;
			}
			
		}
		
		if (now == state.JUMPING && prospectiveBar == lastBar)
		{
			//do nothing, basically	
		}
		else if(now == state.JUMPING || now == state.FALLING)
		{
			//should I grab a bar?  Where should I stick if I do?
			if(nearBar && state2 != secondary.RELEASED && now != state.GRAPPLED)
			{
#if DEBUG
				Debug.Log("latched...");
#endif
				
				Vector3 tempstuff = new Vector3(0,0,0); 
				tempstuff = Vector3.Lerp(barStart.position, barEnd.position, (this.transform.position.x - barStart.transform.position.x)/(barEnd.transform.position.x - barStart.transform.position.x));
				tempstuff -= transform.Find("handLocation").transform.position - this.transform.position;
				
				//Debug.Log("at: " + this.transform.position.ToString() + " Warping to: " + tempstuff.ToString()); 
				this.transform.position = tempstuff;
				
				//move the hand to line up with the bar.
				//TEST: make it go to start
				
				//change the state to recognize the gwapplin'
				now = state.GRAPPLED;
			}
		}
		else if(now == state.PULLING)
		{
			if(last != state.PULLING)
			{
				grappleOld = this.transform.position;	
			}
			
			length += grappleSpeed * Time.deltaTime;
			
			this.transform.position = Vector3.Lerp(grappleOld, grappleAnchor, length);
			
			if( length >= 1)
			{
				length = 0;
				now = state.FALLING;
				this.retracted();
				Destroy(grabby.gameObject);
				
			}
		}
		
		
		
		if(facingRight && this.transform.localEulerAngles.y != 0)
		{
			this.transform.RotateAroundLocal(Vector3.up, -turnRate * Time.deltaTime);
			if ((this.transform.localEulerAngles.y > 350 || this.transform.localEulerAngles.y < 10) )
			{
				tempRotation = this.transform.localEulerAngles;
				tempRotation.y = 0;
				this.transform.localEulerAngles = tempRotation;
				Debug.Log("derp" + this.gameObject.transform.localEulerAngles.ToString());
			}
		}
		else if (!facingRight && !(this.transform.localEulerAngles.y > 170 && this.transform.localEulerAngles.y < 190))
		{
			this.transform.RotateAroundLocal(Vector3.up, turnRate * Time.deltaTime);
			if (this.transform.localEulerAngles.y > 170 && this.transform.localEulerAngles.y < 190)
			{
				tempRotation = this.transform.localEulerAngles;
				tempRotation.y = 180;
				this.transform.localEulerAngles = tempRotation;
				Debug.Log(this.gameObject.transform.localEulerAngles.ToString());
			}
		}
		
		
				
		
		
		//this.transform.position += currentMovement * Time.deltaTime;
		CharacterController mover = (CharacterController)this.GetComponent("CharacterController");
		
		if(!(now == state.PULLING))
		mover.Move(currentMovement * Time.deltaTime);
#if DEBUG
		Debug.Log(currentMovement.ToString() + " , " + this.transform.position.ToString());
#endif
		//physicall.velocity = currentMovement * Time.deltaTime;
		//Debug.Log("actually movintg: " + (physicall.position + currentMovement * Time.deltaTime).ToString());
		
		//physicall.AddForce(currentMovement);
				
		
		last = now;
		

			
	}
	

	/// <summary>
	/// Lets the player know it could latch onto a bar.
	/// </summary>
	/// <param name='state'>
	/// True if over a bar, False if not
	/// </param>
	/// <param name='Start'>
	/// The location of the start of the bar
	/// </param>
	/// <param name='End'>
	/// The location of th end of the bar
	/// </param>
	/// <param name='barID'>
	/// unityID of bar object
	/// </param>
	public void overBar(bool state, Transform Start, Transform End, int barID)
	{
		nearBar = state;
		barStart = Start;
		barEnd = End;
		prospectiveBar = barID;
	}
	
	void OnTriggerExit(Collider other)
	{
		

	}
	
	/// <summary>
	/// Raises the collision enter event. Non-Functional
	/// </summary>
	/// <param name='collision'>
	/// Collision.
	/// </param>
	void OnCollisionEnter(Collision collision)
	{
		allCollisions = 0;
		onGround = false;
		Debug.Log("=========================parsing " + collision.contacts.Length.ToString() + "contacts=====================");
		foreach (ContactPoint i in collision.contacts)
		{
			if(i.otherCollider.tag != "grabbable")
			{
				float angle = Mathf.Atan2(i.point.y - transform.position.y, i.point.x - transform.position.x);
				if(angle >= 60 && angle <120)
				{
					allCollisions = allCollisions | TOP;
				}
				else if(angle >= 120 && angle < 240)
				{
					allCollisions = allCollisions | LEFT;
				}
				else if (angle >= 240 && angle < 300)
				{
					allCollisions = allCollisions | BOTTOM;
					if(i.otherCollider.tag == "ground")
					{
						onGround = true;
					}
				}
				else if (angle >= 300 && angle < 60)
				{
					allCollisions = allCollisions | RIGHT;
				}
			}
		}
	}
	
	/// <summary>
	/// The grapple Hook will call this function to let the player know where to get pulled towards.  Could also concievably yank the player around
	/// HL barnacle style
	/// </summary>
	/// <param name='destination'>
	/// Point of attraction
	/// </param>
	/// <param name='thing'>
	/// Grappling hook that is doing the pulling
	/// </param>
	public void grabbed(Vector3 destination, Grapple thing)
	{
		now = state.PULLING;
		grappleAnchor = destination;
		grabby = thing;
	}
	
	/// <summary>
	/// Calling this function frees the player to launch a new grappling hook
	/// </summary>
	public void retracted()
	{
		deployed = false;
	}
}
