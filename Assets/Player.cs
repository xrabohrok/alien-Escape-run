using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
	public CharacterMotor motor;
	public ParticleEmitter emitter;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if (motor.grounded)
			motor.inputJump = false;
		
		Ray mouseLine = Camera.mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit stuff;
		
		Debug.DrawRay(mouseLine.origin, mouseLine.direction, Color.red);
		
		//Debug.Log("Grabber is layer :" + (LayerMask.NameToLayer("grabber").ToString())+ " from " + Camera.mainCamera.ToString() + "originating"
		//	+ mouseLine.origin.ToString());
		
		//check for click action
		if(Input.GetMouseButtonDown(0))
		{
			//check for hit
			if(Physics.Raycast(mouseLine, out stuff, Mathf.Infinity))
			{
				//check of type
				if(stuff.transform.tag == "grabbable")
				{
					motor.inputJump = true;
					Debug.Log(stuff.transform.tag.ToString());
				}
				
			}
		}
		
		
		
	}
}
