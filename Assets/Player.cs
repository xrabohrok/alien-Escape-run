using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
	const int TOP = 1;
	const int LEFT = 2;
	const int RIGHT = 4;
	const int BOTTOM = 8;
	
	int allCollisions = 0;
	
	
	public ParticleSystem emitter;
	public float JumpForce = 1f;
	public float DownForce = .1f;
	public float upGrav = 9.8f;
	public float downGrav = 9.8f;
	public float goSpeed = .5f;
	public GameObject handLocation;
	
	enum state {  JUMPING, FALLING, SETTOJUMP, GRAPPLED};
	enum secondary {NONE,SHOT, RELEASED};
	
	state now = state.SETTOJUMP;
	state last;
	secondary state2 = secondary.NONE;
	
	Vector3 currentMovement;
	
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
	
	// Use this for initialization
	void Start () {
		last = now;
		currentMovement =new Vector3(0,0,0);
		
		//log old hand local location
		oldHandLoc = handLocation.transform.localPosition;
			
	}
	
	
	// Update is called once per frame
	void Update () {
		
		CapsuleCollider physicall = (CapsuleCollider)this.GetComponent("CapsuleCollider");
		
		currentMovement.x = goSpeed;

		
		Ray mouseLine = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit stuff;
		
		//Debug.DrawRay(mouseLine.origin, mouseLine.direction, Color.red);
		
		//Debug.Log("Grabber is layer :" + (LayerMask.NameToLayer("grabber").ToString())+ " from " + Camera.mainCamera.ToString() + "originating"
		//	+ mouseLine.origin.ToString());
		
		
		//should I grab a bar?  Where should I stick if I do?
		if(nearBar && state2 != secondary.RELEASED && now != state.GRAPPLED)
		{
			Debug.Log("latched...");
			Vector3 movement = handLocation.transform.position - barStart.transform.position;
			currentMovement +=movement;
			
			//move the hand to line up with the bar.
			//TEST: make it go to start
			
			//change the state to recognize the gwapplin'
			now = state.GRAPPLED;
		}
		
		//check for click action
		if(Input.GetMouseButtonDown(0) && (now == state.SETTOJUMP || now == state.GRAPPLED))
		{
			//check for hit
			if(Physics.Raycast(mouseLine, out stuff, Mathf.Infinity))
			{
				//check of type
				if(stuff.transform.tag == "grabbable")
				{
					//jump
					now = state.JUMPING;
					Debug.Log(stuff.transform.tag.ToString());
				}
				
			}
		}
		
		//the mechanics of jumping
		if(now == state.JUMPING)
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
					Debug.Log("Bonked Head, falling");
					newSpeed = 0;
					now = state.FALLING;
				}
				
				currentMovement.y = newSpeed;
				Debug.Log("Jumping at: " + currentMovement.y.ToString());
				
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
			Debug.Log("Falling at: " + currentMovement.y.ToString());
			
			if(!nearBar && state2 == secondary.RELEASED)
			{
				Debug.Log("...and Reset!");
				state2 = secondary.NONE;
			}
		}
		
		else if( now == state.GRAPPLED)
		{
			//for testing purposes...
			//assuming lateral movement
			currentMovement.y = 0;
			currentMovement.z = 0;
			
			Debug.Log("GRABBED");
			if( Input.GetMouseButtonDown(1))
			{
				Debug.Log("released!");
				state2 = secondary.RELEASED;
				now = state.FALLING;
			}
			
			//end of the bar!
			if(transform.position.x > barEnd.transform.position.x)
			{
				now = state.FALLING;
			}
		}
		
		else if(now == state.SETTOJUMP)
		{
			state2 = secondary.NONE;
			
		}
		
		
		
		//this.transform.position += currentMovement * Time.deltaTime;
		CharacterController mover = (CharacterController)this.GetComponent("CharacterController");
		mover.Move(currentMovement * Time.deltaTime);
		
		Debug.Log(currentMovement.ToString() + " , " + this.transform.position.ToString());
		//physicall.velocity = currentMovement * Time.deltaTime;
		//Debug.Log("actually movintg: " + (physicall.position + currentMovement * Time.deltaTime).ToString());
		
		//physicall.AddForce(currentMovement);
				
		
		last = now;
		

			
	}
	

	
	public void overBar(bool state, Transform Start, Transform End)
	{
		nearBar = state;
		barStart = Start;
		barEnd = End;
	}
	
	void OnTriggerExit(Collider other)
	{
		
		if(other.tag == "grabbable")
		{
			Debug.Log("OUT");
		}	
	}
	
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
}
