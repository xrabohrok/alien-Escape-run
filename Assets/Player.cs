using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
	public ParticleSystem emitter;
	public float JumpForce = 1f;
	public float DownForce = .1f;
	public float upGrav = 9.8f;
	public float downGrav = 9.8f;
	
	enum state {JUMPING, FALLING, SETTOJUMP, GRAPPLED};
	enum secondary {NONE,SHOT, RELEASED};
	
	state now = state.SETTOJUMP;
	state last;
	secondary state2 = secondary.NONE;
	
	Vector3 currentMovement;
	
	bool barRenewal = true;  //if the user voluntarily releases, the alien won't regrab until clear of the bar
	//true means he will grab the bar
	
	public bool nearBar = false;
	public bool bodyBar = false;
	bool readyToGrab = false;
	
	// Use this for initialization
	void Start () {
		last = now;
		currentMovement =new Vector3(0,0,0);
		
	}
	
	// Update is called once per frame
	void Update () {
		
		CharacterController physicall = (CharacterController)this.GetComponent("CharacterController");
		
		Ray mouseLine = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit stuff;
		
		//Debug.DrawRay(mouseLine.origin, mouseLine.direction, Color.red);
		
		//Debug.Log("Grabber is layer :" + (LayerMask.NameToLayer("grabber").ToString())+ " from " + Camera.mainCamera.ToString() + "originating"
		//	+ mouseLine.origin.ToString());
		
		//check for click action
		if(Input.GetMouseButtonDown(0) && now == state.SETTOJUMP)
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
				physicall.Move(currentMovement);
			}
			else
			{	//starting to fallllll
				float newSpeed = Time.deltaTime * -upGrav + currentMovement.y;
				if (newSpeed <= 0)
				{
					now = state.FALLING;
				}
				//bumped head, fall.
				else if ((physicall.collisionFlags & CollisionFlags.Above) > 0)
				{
					Debug.Log("Bonked Head, falling");
					newSpeed = 0;
					now = state.FALLING;
				}
				currentMovement.y = newSpeed;
				physicall.Move(currentMovement);
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
			if(physicall.isGrounded)
			{
				now = state.SETTOJUMP;
			}
			currentMovement.y = newSpeed;
			physicall.Move(currentMovement);
		}
				
		
		last = now;
		
	}
	
	public void handOverBar(bool state)
	{
		nearBar = state;
	}
	
	public void overBar(bool state)
	{
		bodyBar = state;
	}
}
