using UnityEngine;
using System.Collections;

public class Grapple : MonoBehaviour {
	
	public float holdTime = 1.2f;
	
	public float range= .1f;
	public float rate = .1f;
	public float returnRate = .01f;
	public float staleRate = 2;
	
	Player owner;
	
	
	public GameObject startThing;
	public Vector3 destinationVector;
	
	bool setup = false;
	float hold = 0.0f;
	
	int state = 0;
	float timeHeld = 0;
	
	
	float distance = 0;
	bool returning = false;
	
	bool grabbed = false;
	int IDbar;
	
	Vector3 returningFrom;
	
	Vector2 sizer;
	Vector2 launcher;
	
	
	public void passInfo(float Range, float Rate, float ReturnRate, GameObject StartLoc, Vector3 endTarget, float HoldTime, Player me)
	{
		range = Range;
		rate = Rate;
		startThing = StartLoc;
		destinationVector = endTarget;
		setup = true;
		holdTime = HoldTime;
		returnRate = ReturnRate;
		
		owner = me;
		
		destinationVector.z = startThing.transform.position.z;
		
		
		
	}
	
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
		
		
		
		if(!setup)
		{
			//holding pattern
			hold += Time.deltaTime;
			
			if(hold >= staleRate)
			{
				Object.Destroy(this.gameObject);
			}
		}
		
		else
		{
			Vector3 tempDir = destinationVector;
			
			//Vector3 rateVector = (tempDir-this.transform.position);
			Vector3 rateVector = tempDir;
			rateVector.z = owner.transform.position.z;
			
			
			
			sizer.x = rateVector.x;
			sizer.y = rateVector.y;
			
			
			launcher.x = this.transform.position.x;
			launcher.y = this.transform.position.y;
			
			
			
			if((sizer-launcher).magnitude > range)
			{
				//Debug.Log("A" + rateVector.ToString());
				
				sizer = ((sizer-launcher).normalized * range) + launcher;
			
				
				rateVector.x = sizer.x;
				rateVector.y = sizer.y;
				//Debug.Log("B" + rateVector.ToString());
			}

			
			if(!grabbed)
			{
				if(state == 0)
				{
					distance += rate * Time.deltaTime;
					this.transform.position = Vector3.Lerp(startThing.transform.position, rateVector, distance);
					
					if( distance >= 1)
					{
						
						returning = true;
						distance = 0;
						returningFrom = this.transform.position;
						
						state = 1;
					}
				}
				else if(state == 1)
				{
					timeHeld += Time.deltaTime;
					if(timeHeld >= holdTime)
					{
						state = 2;	
					}
				}
				
				else if (state == 2)
				{
					//Debug.Log(destinationVector.ToString());
					distance += returnRate * Time.deltaTime;
					//Debug.Log("flipped!");
					this.transform.position = Vector3.Lerp( returningFrom, startThing.transform.position,  distance);
					
					if( distance >= 1)
					{
						owner.retracted();
						Destroy(this.gameObject);
					}
				}
				
				
			}
			else
			{
				//actually not much to snag
			}
				
		}
		
	
	}
	
	public void snagged(int barID)
	{
		Debug.Log("snagged");
		IDbar = barID;
		grabbed = true;
		owner.grabbed(this.transform.position, this);
	}
	
}
