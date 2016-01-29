using UnityEngine;
using System.Collections;

public class ClickDiverterSwitchRight : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnMouseDown(){
		if(gameObject.transform.parent.GetComponent<DiverterLeftC>().diverteron)
		{
			gameObject.transform.parent.GetComponent<DiverterLeftC>().diverteron=false;
			//gameObject.renderer.material.color = Color.red;
		}
		else
		{
			gameObject.transform.parent.GetComponent<DiverterLeftC>().diverteron=true;
			//gameObject.renderer.material.color = Color.green;
		}	
	}
		
}

