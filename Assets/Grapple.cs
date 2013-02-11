using UnityEngine;
using System.Collections;

public class Grapple : MonoBehaviour {

	public float range= .1f;
	public float rate = .1f;
	public float returnRate = .1f;
	public float staleRate = 2;
	
	public GameObject startThing;
	public Vector3 destinationVector;
	
	bool setup = false;
	float hold = 0.0f;
	
	float distance = 0;
	bool returning = false;
	
	
	public void passInfo(float Range, float Rate, GameObject StartLoc, Vector3 endTarget)
	{
		range = Range;
		rate = Rate;
		startThing = StartLoc;
		destinationVector = endTarget;
		setup = true;
		
		
		
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
				Object.Destroy(this);
			}
		}
		
		else
		{
			Vector3 startLocation = startThing.transform.position;
			
			if(!returning)
			{
				distance += rate;
				this.transform.position = Vector3.Lerp(this.transform.position, destinationVector, distance);
				
				if( distance >= 1)
				{
					returning = true;
					distance = 0;
				}
			}
			else
			{
				distance += rate;
				this.transform.position = Vector3.Lerp( destinationVector, this.transform.position,  distance);
				
				if( distance >= 1)
				{
					Object.Destroy(this);
				}
			}
				
		}
		
	
	}
}