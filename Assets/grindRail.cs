using UnityEngine;
using System.Collections;

public class grindRail : MonoBehaviour {
	
	public GameObject start;
	public GameObject end;
	

	
	// Use this for initialization
	void Start () {
	

	}
	
	// Update is called once per frame
	void Update () {
	

	}
	
	void OnTriggerEnter(Collider other)
	{
		Player temp;
		temp = (Player)(GameObject.Find("ourHero").GetComponent("Player"));
		Debug.Log("Rail Triggered");
		if(other.tag == "Player")
		{	
			temp.overBar(true, start.transform, end.transform, this.GetInstanceID());
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		Player temp;
		temp = (Player)(GameObject.Find("ourHero").GetComponent("Player"));
		if(other.tag == "Player")
		{
			temp.overBar(false, start.transform, end.transform, this.GetInstanceID());
		}
	}
	
	void OnDrawGizmos(){
		if (start != null)
		{
			Gizmos.DrawWireSphere(start.transform.position, .6f);
		}
		
		if (end != null)
		{
			Gizmos.DrawWireSphere(end.transform.position, .6f);
		}
		
		if(end != null && start != null)
		{
			Gizmos.DrawRay(start.transform.position, end.transform.position - start.transform.position);
		}
	}
	
	
}
