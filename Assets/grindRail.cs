using UnityEngine;
using System.Collections;

public class grindRail : MonoBehaviour {

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
			temp.overBar(true);
			Debug.Log("Trigger Head");
		}
		if(other.tag == "grabber")
		{
			temp.handOverBar(true);
			Debug.Log("Trigger Hand");
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		Player temp;
		temp = (Player)(GameObject.Find("ourHero").GetComponent("Player"));
		if(other.tag == "Player")
		{
			temp.overBar(true);
		}
		else if(other.tag == "grabber")
		{
			temp.handOverBar(false);
		}
	}
	
	
}
